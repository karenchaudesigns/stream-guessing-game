const puppeteer = require('puppeteer');

(async () => {
    const browser = await puppeteer.launch({
        args: ['--autoplay-policy=no-user-gesture-required']
    });
    const page = await browser.newPage();
    page.on('console', msg => console.log('PAGE LOG:', msg.text()));
    await page.goto('http://localhost:8000/test_audio_fallback_3.html');
    await new Promise(r => setTimeout(r, 1000));
    await browser.close();
})();
