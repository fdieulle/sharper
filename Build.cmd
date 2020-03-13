@echo off
echo "Install sharper package from %cd%"

set RSCRIPT="%R_HOME%\bin\RScript.exe"
%RSCRIPT% -e devtools::document()

set R="%R_HOME%\bin\R.exe"
REM %R% CMD REMOVE "sharper"
%R% CMD INSTALL "%cd%" --preclean

pause