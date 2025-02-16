const { app, BrowserWindow, ipcMain } = require('electron');
const path = require('path');

let navWindow;
let browserWindow;

function createWindows() {
    // Create the navigation window
    navWindow = new BrowserWindow({
        width: 1200,
        height: 55,
        webPreferences: {
            preload: path.join(__dirname, 'preload-nav.js'),
            contextIsolation: true,
            enableRemoteModule: false,
        },
    });

    navWindow.loadFile('nav.html');

    navWindow.setMenuBarVisibility(false);
    // Create the browser window
    browserWindow = new BrowserWindow({
        width: 1200,
        height: 600,
        webPreferences: {
            preload: path.join(__dirname, 'preload-browser.js'),
            contextIsolation: true,
            enableRemoteModule: false,
        },
    });

    browserWindow.setMenuBarVisibility(false);

    browserWindow.loadURL('https://www.google.com'); // Default URL

    // Position the windows side by side
    const { width, height } = require('electron').screen.getPrimaryDisplay().workAreaSize;
    navWindow.setPosition(0, 0);
    browserWindow.setPosition(0, 100);

    // Handle navigation commands from the navigation window
    ipcMain.on('navigate', (event, url) => {
      if (!/^https?:\/\//i.test(url)) {
        url = 'http://' + url;
      }
        browserWindow.loadURL(url);
    });

    ipcMain.on('go-back', () => {
        browserWindow.webContents.goBack();
    });

    ipcMain.on('go-forward', () => {
        browserWindow.webContents.goForward();
    });

    ipcMain.on('reload', () => {
        browserWindow.webContents.reload();
    });

    ipcMain.on('go-home', () => {
        const homeUrl = `file://${path.join(__dirname, 'index.html')}`;
        browserWindow.loadURL(homeUrl);
    });

    browserWindow.webContents.on('did-navigate', () => {
      const currentUrl = browserWindow.webContents.getURL();
      navWindow.webContents.executeJavaScript(`document.getElementById('url-input').value = '${currentUrl}';`);
    });
    
    navWindow.on('close', () => {
      if (browserWindow) {
          browserWindow.close();
      }
    });

    // Open DevTools (optional)
    //navWindow.webContents.openDevTools({ mode: 'detach' });
    //browserWindow.webContents.openDevTools({ mode: 'detach' });
}

app.whenReady().then(createWindows);

app.on('window-all-closed', () => {
    if (process.platform !== 'darwin') {
        app.quit();
    }
});

app.on('activate', () => {
    if (BrowserWindow.getAllWindows().length === 0) {
        createWindows();
    }
});