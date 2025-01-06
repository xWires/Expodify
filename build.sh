dotnet publish Expodify.GUI -o bin/gui/win-x64 -r win-x64 -f net8.0
dotnet publish Expodify.GUI -o bin/gui/linux-x64 -r linux-x64 -f net8.0
dotnet publish Expodify.GUI -o bin/gui/osx-arm64 -r osx-arm64 -f net8.0

dotnet publish Expodify.CLI -o bin/cli/win-x64 -r win-x64 -f net8.0
dotnet publish Expodify.CLI -o bin/cli/linux-x64 -r linux-x64 -f net8.0
dotnet publish Expodify.CLI -o bin/cli/osx-arm64 -r osx-arm64 -f net8.0
