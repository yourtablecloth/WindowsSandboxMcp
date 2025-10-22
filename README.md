# Windows Sandbox MCP

[![Publish](https://github.com/yourtablecloth/WindowsSandboxMcp/actions/workflows/publish.yml/badge.svg)](https://github.com/yourtablecloth/WindowsSandboxMcp/actions/workflows/publish.yml)
[![NuGet version](https://img.shields.io/nuget/v/WindowsSandboxMcp.svg)](https://www.nuget.org/packages/WindowsSandboxMcp/)

A .NET tool to manage Windows Sandbox instances through the Model-View-Controller (MCP) protocol.

[![Demo](https://img.youtube.com/vi/2cIWJsQDlSM/maxresdefault.jpg)](https://www.youtube.com/watch?v=2cIWJsQDlSM)

> **⚠️ EXPERIMENTAL PROJECT**
>
> This project is currently in experimental stage. It may contain bugs and limitations. We welcome feedback and testing, but **use in production environments is not recommended** at this time.
>
> Please report issues and provide feedback to help improve the project!

## Requirements

> **⚠️ IMPORTANT:** This tool requires **Windows 11 24H2 or later**. It does NOT work on Windows 10 or earlier versions of Windows 11.

This MCP server depends on the new Windows Sandbox architecture and the `wsb.exe` CLI tool, which are only available in Windows 11 24H2 and later versions.

- [**Windows 11 24H2 or later**](https://www.microsoft.com/en-us/software-download/windows11) (required)
- [.NET 10.0 SDK](https://dot.net/)
- [Windows Sandbox](https://learn.microsoft.com/en-us/windows/security/application-security/application-isolation/windows-sandbox/windows-sandbox-install)

## Build

```bash
dotnet build
```

## Usage

This MCP server can be used with any MCP-compatible client, including Claude Desktop and VS Code.

### With Claude Desktop

#### 1. Build the Project

```bash
dotnet build -c Release
```

#### 2. Configuration for AI Client

##### 2.1. Claude Desktop

Open the `claude_desktop_config.json` file located at:

`%AppData%\Claude\claude_desktop_config.json`

For everyday use, configure it as follows:

```json
{
  "mcpServers": {
    "windows-sandbox": {
      "command": "dnx",
      "args": [
        "WindowsSandboxMcp",
        "--yes",
      ]
    }
  }
}
```

For development, configure it as follows:

```json
{
  "mcpServers": {
    "windows-sandbox": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "D:\\projects\\WindowsSandboxMcp\\src\\WindowsSandboxMcp\\WindowsSandboxMcp.csproj"
      ]
    }
  }
}
```

##### 2.2. Visual Studio Code

Open VS Code settings (Ctrl+,) and search for "MCP" or edit your `settings.json` directly:

For everyday use, configure it as follows:

```json
{
  "servers": {
    "windows-sandbox": {
      "type": "stdio",
      "command": "dnx",
      "args": ["WindowsSandboxMcp", "--yes"]
    }
  }
}
```

For development, configure it as follows:

```json
{
  "servers": {
    "windows-sandbox": {
      "type": "stdio",
      "command": "dotnet",
      "args": ["run", "--project", "D:\\projects\\WindowsSandboxMcp\\src\\WindowsSandboxMcp\\WindowsSandboxMcp.csproj"]
    }
  }
}
```

You must use an absolute path. You can check the absolute path of the project using the `pwd` command in PowerShell.


You must use an absolute path. You can check the absolute path of the project using the `pwd` command in PowerShell.

#### 3. Restart Claude Desktop

After saving the configuration file, **completely close** Claude Desktop and restart it.

Right-click the Claude icon in the system tray and select "Exit"

## Available Features

This MCP server provides programmatic control over Windows Sandbox through the following capabilities:

- **Sandbox Lifecycle Management** - Start and stop Windows Sandbox instances with custom configurations
- **Command Execution** - Execute commands and scripts inside the sandbox environment
- **Shared Folders** - Map host directories to the sandbox for file sharing
- **Network Information** - Retrieve network configuration and IP addresses
- **Session Management** - Connect to and monitor sandbox sessions
- **Status Monitoring** - Check if a sandbox is currently running

> **Note:** The available tools and their parameters may change as the project evolves. Use your MCP client's tool discovery features to see the current list of available tools and their specifications.

## Usage

You can ask Claude Desktop questions like:

- "Start a new Windows Sandbox"
- "Execute notepad in the sandbox"
- "Get network information for the sandbox"
- "Stop the sandbox"
- "Add a shared folder to the sandbox"
- "Is a sandbox currently running?"

## Technical Details

### Architecture

This MCP server uses the new Windows Sandbox architecture introduced in Windows 11 24H2, which provides programmatic control through the `wsb.exe` CLI tool.

**Dependencies:**

- **ModelContextProtocol** (v0.4.0-preview.3) - Core MCP implementation
- **Microsoft.Extensions.Hosting** (v10.0.0) - Application hosting infrastructure
- **System.Management** (v10.0.0) - WMI integration for process management
- **wsb.exe** - Windows Sandbox CLI tool (built-in on Windows 11 24H2+)

### Project Structure

- `Program.cs` - Entry point and MCP server initialization
- `WindowsSandbox.cs` - High-level Windows Sandbox API
- `WindowsSandboxCliRaw.cs` - Low-level CLI wrapper
- `Tools/WindowsSandboxTools.cs` - MCP tool definitions
- `Models/` - Configuration and data models

### Key Features

- Single sandbox instance management (prevents multiple sandboxes running simultaneously)
- Automatic connection and readiness detection
- Remote session window management
- WMI-based process detection for active sessions

## Contributing

This is an experimental project and we welcome contributions! If you encounter any issues or have suggestions for improvements, please:

- Open an issue on GitHub
- Submit a pull request
- Share your feedback and test results

Your contributions help make this project better for everyone!

## License

This project is licensed under the Apache License 2.0 - see the [LICENSE](LICENSE) file for details.
