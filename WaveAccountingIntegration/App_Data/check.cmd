@echo off
setlocal enabledelayedexpansion

timeout 5




cd C:\Users\todor.peykov\Dropbox\SyncTargets\Code\C#\Web\Automations\WaveAccountingIntegration\App_Data


forfiles /p C:\Users\todor.peykov\Dropbox\SyncTargets\Code\C#\Web\Automations\WaveAccountingIntegration\App_Data /m RequestHeaders.txt /c "cmd /c echo @fsize" > c:\users\todor.peykov\temp.txt

set count=0

for /f "tokens=*" %%x in (c:\users\todor.peykov\temp.txt) do (
    set /a count+=1
    set var[!count!]=%%x
)
echo %var[1]%

if %var[1]% gtr 1500 echo "filesize is more than 1500"

if %var[1]% lss 1500 echo "filesize is less than 1500"
if %var[1]% lss 1500 del RequestHeaders.txt
if %var[1]% lss 1500 copy RequestHeaders_backup.txt RequestHeaders.txt


del C:\Users\todor.peykov\Dropbox\SyncTargets\Code\C#\Web\Automations\WaveAccountingIntegration\App_Data\RequestHeaders_backup.txt


