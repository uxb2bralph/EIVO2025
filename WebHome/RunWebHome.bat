set ASPNETCORE_WEBROOT=C:\Users\USER\Desktop\Project\IFS-EIVO03\eIVOGo  rem 11405 網頁排版第1順序路徑
SET ASPNETCORE_ENVIRONMENT=Development
SET ASPNETCORE_HOSTINGSTARTUPASSEMBLIES=Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation
SET LAUNCHER_PATH=bin\Debug\net9.0\WebHome.exe
rem "c:\Program Files\IIS Express\iisexpress.exe" /site:WebHome /config:C:\Project\Github\beyond-fitness\WebHome\.vs\WebHome\config\applicationhost.config /apppool:"WebHome AppPool"
rem robocopy ..\WebDev\bin\Debug\net9.0 bin\Debug\net9.0 /S
Start "WebHome" bin\Debug\net9.0\WebHome.exe --urls="http://*:5000;https://*:5443"