set pathMSBuild="%PROGRAMFILES(x86)%\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin"
@ECHO OFF

%pathMSBuild%\msbuild.exe ".\Middleware5.sln" /p:Configuration=Debug /p:Platform="Any CPU" /t:Restore;Build

cd ".\bin\Debug\net5.0-windows\"
start Middleware5.exe