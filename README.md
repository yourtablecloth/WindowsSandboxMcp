# Windows Sandbox MCP Server

A Model Context Protocol (MCP) server that enables control of Windows Sandbox through MCP.

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

#### 2. Configure Claude Desktop

Open the `claude_desktop_config.json` file located at:

`%AppData%\Claude\claude_desktop_config.json`

Configure it as follows:

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

### With VS Code

#### 1. Install the MCP Extension

Install the official MCP extension for VS Code from the marketplace:

- Extension Name: **MCP Servers**
- Extension ID: `modelcontextprotocol.vscode-mcp`

Or search for "MCP" in the VS Code Extensions view (Ctrl+Shift+X).

#### 2. Build the Project

```bash
dotnet build -c Release
```

#### 3. Configure MCP Settings

Open VS Code settings (Ctrl+,) and search for "MCP" or edit your `settings.json` directly:

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

#### 4. Reload VS Code

After saving the settings, reload VS Code (Ctrl+Shift+P → "Developer: Reload Window") or restart VS Code.

#### 5. Use with GitHub Copilot Chat

Once configured, you can use the Windows Sandbox tools in GitHub Copilot Chat by mentioning them:

- Type `@workspace` in Copilot Chat to access workspace context
- The MCP tools will be available for Copilot to use when helping you

## Testing

### Testing with Claude Desktop

You can ask Claude Desktop questions like:

- "Start a new Windows Sandbox"
- "Execute notepad in the sandbox"
- "Get network information for the sandbox"
- "Stop the sandbox"
- "Add a shared folder to the sandbox"
- "Is a sandbox currently running?"

### Testing with VS Code (GitHub Copilot Chat)

You can ask GitHub Copilot questions like:

- "Can you start a Windows Sandbox for me?"
- "Execute notepad.exe in the sandbox"
- "What's the network configuration of the running sandbox?"
- "Please stop the sandbox"
- "Add C:\Temp as a shared folder in the sandbox"

## Troubleshooting

### General

#### If `dotnet run --project` doesn't work or multiple clients can't connect simultaneously

The `dotnet run --project` approach may have limitations when multiple MCP clients try to connect at the same time, as each client may trigger a separate build process.

**Solution:** Build and use the executable directly

1. Build the project in Release mode:

   ```bash
   dotnet publish -c Release -o ./publish
   ```

2. Update your MCP client configuration to use the executable directly:

   **For Claude Desktop** (`claude_desktop_config.json`):

   ```json
   {
     "mcpServers": {
       "windows-sandbox": {
         "command": "D:\\projects\\WindowsSandboxMcp\\publish\\WindowsSandboxMcp.exe"
       }
     }
   }
   ```

   **For VS Code** (`settings.json`):

   ```json
   {
     "mcp.servers": {
       "windows-sandbox": {
         "command": "D:\\projects\\WindowsSandboxMcp\\publish\\WindowsSandboxMcp.exe"
       }
     }
   }
   ```

   Replace `D:\\projects\\WindowsSandboxMcp` with your actual project path.

3. Restart your MCP client

This approach allows multiple clients to use the same compiled executable without conflicts.

### Claude Desktop

#### If the server is not visible in Claude Desktop

1. Verify the JSON syntax in `claude_desktop_config.json`
2. Ensure the project path is absolute
3. Completely close and restart Claude Desktop

#### Viewing Claude Desktop Logs

```powershell
type %USERPROFILE%\AppData\Roaming\Claude\logs\mcp*.log
```

### VS Code

#### If MCP tools are not available

1. Verify the MCP extension is installed and enabled
2. Check that the `settings.json` configuration is correct
3. Ensure the project path is absolute
4. Reload the VS Code window (Ctrl+Shift+P → "Developer: Reload Window")
5. Check the VS Code Output panel (View → Output) and select "MCP" from the dropdown

#### Viewing VS Code Logs

1. Open VS Code Output panel (Ctrl+Shift+` or View → Output)
2. Select "MCP" from the dropdown menu
3. Look for connection and error messages

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
