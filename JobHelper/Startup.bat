"F:\Download\WinSCP-5.19.5-Portable\WinSCP.exe" /log="F:\Project\temp\WinSCP.log" /ini=nul /script="F:\Project\temp\script.txt"
timeout /T 3
JobHelper 1
decrypt.bat
timeout /T 3
JobHelper 2
prepare.bat
timeout /T 30
JobHelper 3
retrieve.bat
