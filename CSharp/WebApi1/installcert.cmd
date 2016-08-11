setx -m TestVariable "MyValue" > %TEMP%\1.log
powershell.exe -ExecutionPolicy Bypass -Command ".\installcert.ps1" > %TEMP%\2.log