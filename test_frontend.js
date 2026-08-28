const { WebSocketServer } = require('ws');
const puppeteer = require('puppeteer');

(async () => {
  // Start WebSocket server on port 8080
  const wss = new WebSocketServer({ port: 8080 });
  wss.on('connection', (ws) => {
    ws.on('message', (message) => {
      const data = JSON.parse(message);
      if (data.request === 'GetGlobalVariables') {
        // Send initial recap state
        ws.send(JSON.stringify({
          id: data.id,
          status: 'ok',
          variables: [{ name: 'guessing-game_state', value: 'recap' }]
        }));
      } else if (data.request === 'GetUserVariables') {
        ws.send(JSON.stringify({
          id: data.id,
          status: 'ok',
          users: {}
        }));
      }
    });
  });

  const browser = await puppeteer.launch({ args: ['--no-sandbox', '--disable-setuid-sandbox'] });
  const page = await browser.newPage();

  const path = require('path');
  const indexUrl = `file://${path.join(__dirname, 'index.html')}?ws=127.0.0.1:8080`;

  await page.goto(indexUrl, { waitUntil: 'networkidle2' });

  // Wait for the websocket to connect and update the state
  await new Promise(resolve => setTimeout(resolve, 1000));

  // Check the game state text
  const stateText = await page.$eval('#game-state-text', el => el.innerText);
  console.log(`Game state text: ${stateText}`);

  // Check the game state indicator color
  const indicatorClassName = await page.$eval('#game-state-indicator', el => el.className);
  console.log(`Indicator class name: ${indicatorClassName}`);

  if (stateText === 'ROUND RECAP' && indicatorClassName.includes('bg-blue-400')) {
      console.log('Frontend logic verified successfully!');
  } else {
      console.error('Frontend logic verification failed!');
      process.exit(1);
  }

  await browser.close();
  wss.close();
})();
