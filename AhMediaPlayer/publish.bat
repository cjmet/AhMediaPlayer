del /s /q ..\..\Packages\tmp\
dotnet clean
dotnet publish -f net8.0-windows10.0.19041.0 -c Release -p:RuntimeIdentifierOverride=win10-x64  -p:WindowsPackageType=None -p:WindowsAppSDKSelfContained=true  --output ..\..\Packages\tmp
