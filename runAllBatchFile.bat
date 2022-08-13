set pathMSBuild="%PROGRAMFILES(x86)%\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin"
@ECHO OFF

%pathMSBuild%\msbuild.exe ".\Middleware1\Middleware1.sln" /p:Configuration=Debug /p:Platform="Any CPU" /t:Restore;Build
%pathMSBuild%\msbuild.exe ".\Middleware2\Middleware2.sln" /p:Configuration=Debug /p:Platform="Any CPU" /t:Restore;Build
%pathMSBuild%\msbuild.exe ".\Middleware3\Middleware3.sln" /p:Configuration=Debug /p:Platform="Any CPU" /t:Restore;Build
%pathMSBuild%\msbuild.exe ".\Middleware4\Middleware4.sln" /p:Configuration=Debug /p:Platform="Any CPU" /t:Restore;Build
%pathMSBuild%\msbuild.exe ".\Middleware5\Middleware5.sln" /p:Configuration=Debug /p:Platform="Any CPU" /t:Restore;Build

cd ".\Network"
start Network.exe

cd /D "%~dp0"
cd ".\Middleware1\bin\Debug\net5.0-windows\"
start Middleware1.exe

cd /D "%~dp0"
cd ".\Middleware2\bin\Debug\net5.0-windows\"
start Middleware2.exe

cd /D "%~dp0"
cd ".\Middleware3\bin\Debug\net5.0-windows\"
start Middleware3.exe

cd /D "%~dp0"
cd ".\Middleware4\bin\Debug\net5.0-windows\"
start Middleware4.exe

cd /D "%~dp0"
cd ".\Middleware5\bin\Debug\net5.0-windows\"
start Middleware5.exe