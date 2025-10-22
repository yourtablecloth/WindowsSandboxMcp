internal static class Prompts
{
    public static readonly string McpServerPrompt = """
        This MCP server provides programmatic control over Windows Sandbox on Windows 11 24H2 or later.

        CRITICAL SYSTEM REQUIREMENTS:
        - This server EXCLUSIVELY operates on Windows 11 24H2 or later versions
        - Windows Sandbox feature must be enabled on the target system
        - Only one sandbox instance can execute simultaneously

        AVAILABLE CAPABILITIES:

        1. Sandbox Lifecycle Management
        - Initialize new Windows Sandbox instances with customizable configurations
        - Terminate running sandbox instances
        - Verify if a sandbox is currently active

        2. Configuration Options (when initializing sandbox)
        - Enable/disable vGPU, networking, audio/video input
        - Configure memory allocation (in MB)
        - Enable/disable printer, clipboard redirection
        - Define startup commands to execute on logon
        - Map host directories to sandbox (supports multiple directories)

        3. Command Execution
        - Execute commands within the active sandbox environment
        - Select execution context (ExistingLogin or System)
        - Specify working directory for command operations

        4. Shared Directories
        - Add shared directories to active sandbox dynamically
        - Control read/write access permissions
        - Specify custom sandbox paths for mapped directories

        5. Session Management
        - Launch remote session windows to access sandbox interface
        - Automatic connection establishment and readiness detection

        6. Network Information
        - Retrieve IP addresses and network configuration of active sandbox

        WELL-KNOWN PROGRAMS IN SANDBOX:
        The following programs are available in the Windows Sandbox by default:
        - Windows Version Lookup: C:\Windows\System32\winver.exe
        - Command Line Console: C:\Windows\System32\cmd.exe
        - Windows Explorer: C:\Windows\System32\explorer.exe
        - Task Manager: C:\Windows\System32\Taskmgr.exe
        - Windows PowerShell (Legacy): C:\Windows\System32\powershell.exe
        - Microsoft Remote Desktop: C:\Windows\System32\mstsc.exe
        - Control Panel: C:\Windows\System32\control.exe
        - Microsoft Edge Web Browser (Chromium-based): C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe

        OPERATIONAL GUIDELINES:
        - Always verify if a sandbox is active before initializing a new instance
        - Wait for sandbox readiness confirmation before executing commands
        - The server automatically manages sandbox connections and sessions
        - Directory paths must be absolute paths on the host system
        - Multiple directories can be mapped using comma-separated paths when initializing sandbox
        - Use absolute paths when executing programs to ensure correct resolution
        - Reference the well-known programs list above for accurate program paths in the sandbox

        TOOL SELECTION GUIDELINES:
        - Analyze available tools systematically to select the optimal tool for each task
        - Use command execution tools for running programs and executables within the sandbox
        - Use dedicated web/URL tools for opening web pages or URLs in the browser
        - Each tool serves a specific purpose - align the tool selection with your intended operation

        TYPICAL WORKFLOW:
        1. Verify if a sandbox is currently active
        2. Initialize a sandbox with desired configuration (if none is active)
        3. Wait for sandbox readiness confirmation (handled automatically)
        4. Execute commands or add shared directories as needed
        5. Terminate the sandbox when operations are complete
        """;
}