@echo @off

SET CUR_DIR=%~d0%~p0
SET R_HOME=%R_HOME%
SET RTOOLS=C:\Program Files\R\Rtools
SET VS_VC_BUILD="C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\VC\Auxiliary\Build"

REM Prepare VS project for x86

SET OUT_DIR=%CUR_DIR%\ClrHost\libs\x86
IF NOT EXIST %OUT_DIR% mkdir %OUT_DIR%

REM Generate R.def export file from R.dll library
call "%RTOOLS%\mingw_32\bin\gendef.exe" - "%R_HOME%\bin\i386\R.dll" > %OUT_DIR%\R.def
REM Load Visual Studio dev tools environment
call %VS_VC_BUILD%\vcvars32.bat
REM generate the linkable library
call lib /nologo /def:%OUT_DIR%\R.def /out:%OUT_DIR%\rdll.lib /machine:x86

REM Prepare VS project for x64

SET OUT_DIR=%CUR_DIR%\ClrHost\libs\x64
IF NOT EXIST %OUT_DIR% mkdir %OUT_DIR%

REM Generate R.def export file from R.dll library
call "%RTOOLS%\mingw_64\bin\gendef.exe" - "%R_HOME%\bin\x64\R.dll" > %OUT_DIR%\R.def
REM Load Visual Studio dev tools environment
call %VS_VC_BUILD%\vcvars64.bat
REM generate the linkable library
call lib /nologo /def:%OUT_DIR%\R.def /out:%OUT_DIR%\rdll.lib /machine:x64

REM pause


