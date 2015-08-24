rem Use this command line to compile with Roslyn
rem "C:\Program Files (x86)\MSBuild\12.0\bin\amd64\msbuild" Source\ALinq.sln /property:Configuration=Release  /p:VisualStudioVersion=14.0 /tv:14.0

"C:\Program Files (x86)\MSBuild\12.0\bin\amd64\msbuild" Source\ALinq.sln /property:Configuration=Release  /p:VisualStudioVersion=14.0
md Packages
cd Source\ALinq
..\.nuget\nuget pack -OutputDirectory ..\..\Packages -Properties Configuration=Release
cd ..\..