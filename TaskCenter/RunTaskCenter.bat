set ASPNETCORE_WEBROOT=C:\Project\Github\EIVO2025\TaskCenter\wwwroot
SET ASPNETCORE_ENVIRONMENT=Development
SET ASPNETCORE_HOSTINGSTARTUPASSEMBLIES=Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation
SET LAUNCHER_PATH=bin\Debug\net9.0\TaskCenter.exe
rem "c:\Program Files\IIS Express\iisexpress.exe" /site:WebHome /config:C:\Project\Github\beyond-fitness\WebHome\.vs\WebHome\config\applicationhost.config /apppool:"WebHome AppPool"
rem robocopy ..\WebDev\bin\Debug\net9.0 bin\Debug\net9.0 /S
Start "TaskCenter" bin\Debug\net9.0\TaskCenter.exe --urls="http://*:5050;https://*:6443" --verbose