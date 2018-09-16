@prompt $$$S
@echo off

for %%f in (tests\*.y) do (
	echo .\bin\Debug\yanp.exe %%f
	start /B .\bin\Debug\yanp.exe %%f -b . -V %%f.txt -c %%f.csv -t nothing
)

exit /B 0
