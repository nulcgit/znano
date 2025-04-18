cd /D %0%\..\..
set ZNANO=%cd%
mkdir temp data apps
powershell -Command "Start-Process cmd -Verb RunAs -ArgumentList '/c setx -m ZNANO %cd% & setx -m PATH \"%cd%\assets;%PATH%\"'"
powershell -Command "Start-Process powershell -Verb RunAs -ArgumentList '& {Set-ExecutionPolicy -ExecutionPolicy Unrestricted -Force};'"
set PATH=%PATH%;%cd%\assets
wget -O temp\runtime.exe https://builds.dotnet.microsoft.com/dotnet/WindowsDesktop/8.0.15/windowsdesktop-runtime-8.0.15-win-x64.exe
start /w temp\runtime.exe /install /quiet /norestart
copy assets\mstart.cmd "C:\Users\%USERNAME%\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup\mstart.cmd"
rd /s /q temp
mkdir temp
shutdown /r /f /t 60
