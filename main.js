const { app, BrowserWindow, ipcMain } = require('electron');
const path = require('path');

let navWindow;
let browserWindow;

function createWindows() {
    const { width, height } = require('electron').screen.getPrimaryDisplay().workAreaSize;

    navWindow = new BrowserWindow({
        width: width,
        height: 55,
        webPreferences: {
            preload: path.join(__dirname, 'preload-nav.js'),
            contextIsolation: true,
            enableRemoteModule: false,
        },
    });

    navWindow.loadFile('nav.html');

    navWindow.setMenuBarVisibility(false);
    navWindow.setPosition(0, 0);

    ipcMain.on('navigate', (event, url) => {
        if (!browserWindow || browserWindow.isDestroyed()) {
            browserWindow = new BrowserWindow({
                width: width,
                height: height - 110,
                webPreferences: {
                    preload: path.join(__dirname, 'preload-browser.js'),
                    contextIsolation: true,
                    enableRemoteModule: false,
                },
            });

            browserWindow.setMenuBarVisibility(false);
            navWindow.setPosition(0, 0);
            browserWindow.setPosition(0, 110);

        }
        if (url === 'about:blank' || url === '') {
            url = 'https://google.com';
        }
        if (!/^https?:\/\//i.test(url)) {
            url = 'http://' + url;
        }
        browserWindow.loadURL(url);
        browserWindow.webContents.on('did-create-window', (childWindow) => {
            childWindow.webContents.on('will-navigate', (e, url) => {
                e.preventDefault();
                childWindow.close();
                browserWindow.loadURL(url);
            })

        })

        browserWindow.webContents.on('did-navigate', () => {
            const currentUrl = browserWindow.webContents.getURL();
            navWindow.webContents.executeJavaScript(`document.getElementById('url-input').value = '${currentUrl}';`);
        });
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
        ipcMain.emit('navigate');
        const homeUrl = `file://${path.join(__dirname, 'index.html')}`;
        browserWindow.loadURL(homeUrl);
    });

    navWindow.on('close', () => {
        if (!browserWindow || browserWindow.isDestroyed()) {
        } else browserWindow.close();
    });
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