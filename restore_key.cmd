@setlocal
@echo off

sig\crypt.exe decrypt sig\sns key.snk
sig\crypt.exe enablesigning src\IsabelDb\IsabelDb.csproj ..\..\key.snk
sig\crypt.exe enablesigning src\IsabelDb.Test\IsabelDb.Test.csproj ..\..\key.snk
sig\crypt.exe enablesigning src\IsabelDb.Browser\IsabelDb.Browser.csproj ..\..\key.snk
