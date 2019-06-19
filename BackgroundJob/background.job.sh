@echo off
title Install Background Job
echo Installing Background Job!
sc.exe create backgroundjob binPath= "Full Path" DisplayName= "Background Job" start= auto
sc.exe start backgroundjob
echo installed Background Job!
ping google.com -t
pause