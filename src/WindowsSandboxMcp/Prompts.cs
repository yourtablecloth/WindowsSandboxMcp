internal static class Prompts
{
    public static readonly string McpServerPrompt = """
        This MCP server provides programmatic control over Windows Sandbox on Windows 11 24H2 or later.

        IMPORTANT REQUIREMENTS:
        - This server ONLY works on Windows 11 24H2 or later
        - Windows Sandbox feature must be enabled on the system
        - Only one sandbox instance can run at a time

        AVAILABLE CAPABILITIES:
  
        1. Sandbox Lifecycle Management
        - Start new Windows Sandbox instances with customizable configurations
        - Stop running sandbox instances
        - Check if a sandbox is currently running

        2. Configuration Options (when starting sandbox)
        - Enable/disable vGPU, networking, audio/video input
        - Configure memory allocation (in MB)
        - Enable/disable printer, clipboard redirection
        - Specify startup commands to execute on logon
        - Map host folders to sandbox (supports multiple folders)
      
        3. Command Execution
        - Execute commands inside the running sandbox
        - Choose execution context (ExistingLogin or System)
        - Specify working directory for commands
            
        4. Shared Folders
        - Add shared folders to running sandbox dynamically
        - Control read/write permissions
        - Specify custom sandbox paths for mapped folders
  
        5. Session Management
        - Open remote session windows to access sandbox
        - Automatic connection and readiness detection
    
        6. Network Information
        - Retrieve IP addresses and network configuration of running sandbox

        USAGE GUIDELINES:
        - Always check if a sandbox is running before starting a new one
        - Wait for sandbox to be ready before executing commands
        - The server automatically manages sandbox connections and sessions
        - Folder paths should be absolute paths on the host system
        - Multiple folders can be mapped using comma-separated paths when starting sandbox
          
        TYPICAL WORKFLOW:
        1. Start a sandbox with desired configuration
        2. Wait for it to be ready (handled automatically)
        3. Execute commands or add shared folders as needed
        4. Stop the sandbox when finished
        """;
}