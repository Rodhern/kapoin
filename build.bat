@ECHO.
@ECHO Build Kapoin dlls with Fake and MSBuild.

.paket\paket.bootstrapper.exe
.paket\paket.exe install

@ECHO OFF
if errorlevel 1 (
  exit /b %errorlevel%
)
@ECHO.
@ECHO ON

packages\FAKE\tools\Fake.exe .\output\build.fsx
