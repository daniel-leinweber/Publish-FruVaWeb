# Publish-Web

CLI tool to publish a .NET Web project with a predefined configuration.

<!--toc:start-->
- [Publish-Web](#publish-web)
  - [Usage](#usage)
    - [Build executable](#build-executable)
    - [Publish](#publish)
    - [Run the executable](#run-the-executable)
<!--toc:end-->

## Usage

### Build executable

In the project directory, run:

```bash
dotnet build -c Release
```

### Publish

**Windows:**

```powershell
dotnet publish -r win-x64 -p:PublishSingleFile=true --self-contained true
```

**Linux:**

```bash
dotnet publish -r linux-x64 -p:PublishSingleFile=true --self-contained true
```

### Run the executable

**Windows:**

```powershell
Publish-Web.exe --configuration <DEV | PROD> --user <UserName> --projectDir <PathToTheFruVaWebProject>
```

**Linux:**

```powershell
./Publish-Web --configuration <DEV | PROD> --user <UserName> --projectDir <PathToTheFruVaWebProject>
```
