for /f "tokens=1 delims=" %%a in ('dir /b /s *.sln') do (
	%windir%\microsoft.net\framework\v4.0.30319\msbuild /m "%%a" /t:rebuild /p:Configuration=Release
	call "%%a.clean.bat"
)

@PAUSE