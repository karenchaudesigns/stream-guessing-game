const { WebSocketServer } = require('ws');
const puppeteer = require('puppeteer');

(async () => {
  // Start WebSocket server on port 8080
  const wss = new WebSocketServer({ port: 8080 });
  wss.on('connection', (ws) => {
    ws.on('message', (message) => {
      const data = JSON.parse(message);
      if (data.request === 'GetGlobals' || data.request === 'GetGlobalVariables') {
        // We merged all scripts so we must return variables in the format expected by the integrated app
        ws.send(JSON.stringify({
          id: data.id,
          status: 'ok',
          variables: {
            'guessing-game_state': { value: 'recap' },
            'guessing-game_winner': { value: 'TestUser' },
            'guessing-game_winningWord': { value: 'TESTWORD' },
            'guessing-game_currentGuest': { value: 'GuestUser' }
          }
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
  await new Promise(resolve => setTimeout(resolve, 2000));

  // Check the title of clue which indicates state in new UI
  const clueTitleText = await page.$eval('#clueTitle', el => el.innerText);
  console.log(`Clue title text: ${clueTitleText}`);

  // Check announcement
  const winnerText = await page.$eval('#winner-text', el => el.innerText);
  console.log(`Winner text: ${winnerText}`);

  // In the new code, recap state sets the clue title to "Round Over!"
  if (clueTitleText.toLowerCase().includes('round over') && winnerText.toLowerCase().includes('testuser')) {
      console.log('Frontend logic verified successfully!');
  } else {
      console.error('Frontend logic verification failed!');
      process.exit(1);
  }

  await browser.close();
  wss.close();
})();
