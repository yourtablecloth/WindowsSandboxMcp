using ModelContextProtocol.Server;
using System.ComponentModel;

namespace WindowsSandboxMcp.Tools;

[McpServerToolType]
public sealed class WindowsSandboxTools
{
    [McpServerTool, Description("Starts a new Windows Sandbox. Only one sandbox can run at a time.")]
    public async Task<string> StartSandbox(
        [Description("Whether to enable vGPU")] bool? enableVGpu = null,
        [Description("Whether to enable networking")] bool? enableNetworking = null,
        [Description("Whether to enable audio input")] bool? enableAudioInput = null,
        [Description("Whether to enable video input")] bool? enableVideoInput = null,
        [Description("Whether to enable protected client")] bool? enableProtectedClient = null,
        [Description("Whether to enable printer redirection")] bool? enablePrinterRedirection = null,
        [Description("Whether to enable clipboard redirection")] bool? enableClipboardRedirection = null,
        [Description("Memory size in MB")] int? memoryInMB = null,
        [Description("Command to execute on logon")] string? logonCommand = null)
    {
        // Check if a sandbox is already running
        var existingSandboxId = await WindowsSandbox.GetSingleSandboxIdAsync();
        if (!string.IsNullOrEmpty(existingSandboxId))
            return "A sandbox is already running. Please stop the existing sandbox before starting a new one.";

        var config = new WindowsSandboxConfiguration
        {
            EnableVGpu = enableVGpu,
            EnableNetworking = enableNetworking,
            EnableAudioInput = enableAudioInput,
            EnableVideoInput = enableVideoInput,
            EnableProtectedClient = enableProtectedClient,
            EnablePrinterRedirection = enablePrinterRedirection,
            EnableClipboardRedirection = enableClipboardRedirection,
            MemoryInMB = memoryInMB,
            LogonCommand = logonCommand
        };

        var sandboxId = await WindowsSandbox.StartSandboxAsync(config);

        if (string.IsNullOrEmpty(sandboxId))
            return "Failed to start sandbox.";

        _ = WindowsSandbox.ConnectToSandboxAsync(sandboxId);
        await WindowsSandbox.WaitUntilSandboxReadyAsync(sandboxId, TimeSpan.FromSeconds(1d));

        return "Sandbox started successfully.";
    }

    [McpServerTool, Description("Executes a command in the running Sandbox.")]
    public async Task<string> ExecuteInSandbox(
        [Description("Command to execute")] string command,
        [Description("Execution context (ExistingLogin or System)")] string runAs = "ExistingLogin",
        [Description("Working directory")] string? workingDirectory = null)
    {
        try
        {
            var sandboxId = await WindowsSandbox.GetSingleSandboxIdAsync();
            if (string.IsNullOrEmpty(sandboxId))
                return "No running sandbox found. Please start a sandbox first.";

            if (!WindowsSandbox.GetActiveSandboxRemoteSession(sandboxId))
            {
                _ = WindowsSandbox.ConnectToSandboxAsync(sandboxId);
                await WindowsSandbox.WaitUntilSandboxReadyAsync(sandboxId, TimeSpan.FromSeconds(1d));
            }

            var runAsContext = runAs.Equals("System", StringComparison.OrdinalIgnoreCase)
                ? SandboxRunningContext.System
                : SandboxRunningContext.ExistingLogin;

            _ = WindowsSandbox.ExecuteInSandboxAsync(sandboxId, command, runAsContext, workingDirectory);

            return "Command executed in sandbox successfully.";
        }
        catch (Exception ex)
        {
            return $"Error executing command in sandbox: {ex.Message}";
        }
    }

    [McpServerTool, Description("Stops the running Sandbox.")]
    public async Task<string> StopSandbox()
    {
        var sandboxId = await WindowsSandbox.GetSingleSandboxIdAsync();
        if (string.IsNullOrEmpty(sandboxId))
            return "No running sandbox found.";

        await WindowsSandbox.StopSandboxAsync(sandboxId);
        return "Sandbox stopped successfully.";
    }

    [McpServerTool, Description("Adds a shared folder to the running Sandbox.")]
    public async Task<string> AddSharedFolder(
        [Description("Host directory path")] string hostDirectoryPath,
        [Description("Absolute path in Sandbox")] string? sandboxAbsolutePath = null,
        [Description("Whether to allow write access")] bool? allowWrite = null)
    {
        var sandboxId = await WindowsSandbox.GetSingleSandboxIdAsync();
        if (string.IsNullOrEmpty(sandboxId))
            return "No running sandbox found. Please start a sandbox first.";

        await WindowsSandbox.AddSharedFolderAsync(sandboxId, hostDirectoryPath, sandboxAbsolutePath, allowWrite);
        return $"Shared folder added to sandbox: {hostDirectoryPath}";
    }

    [McpServerTool, Description("Opens a window that accesses the currently running Windows Sandbox session.")]
    public async Task<string> OpenSandboxRemoteSessionWindowAsync()
    {
        var sandboxId = await WindowsSandbox.GetSingleSandboxIdAsync();
        if (string.IsNullOrEmpty(sandboxId))
            return "No running sandbox found. Please start a sandbox first.";

        await WindowsSandbox.ConnectToSandboxAsync(sandboxId);
        return "Sandbox remote session window opened.";
    }

    [McpServerTool, Description("Gets network information for the running Sandbox.")]
    public async Task<string> GetSandboxNetwork()
    {
        var sandboxId = await WindowsSandbox.GetSingleSandboxIdAsync();
        if (string.IsNullOrEmpty(sandboxId))
            return "No running sandbox found. Please start a sandbox first.";

        var networks = await WindowsSandbox.GetSandboxNetworkAsync(sandboxId);

        if (!networks.Any())
            return "No network information found for the sandbox.";

        var result = string.Join("\n", networks.Select(n =>
            $"IPv4: {n.IPv4Address}"));

        return $"Network information:\n{result}";
    }

    [McpServerTool, Description("Checks if a Sandbox is currently running.")]
    public async Task<string> IsSandboxRunning()
    {
        var sandboxId = await WindowsSandbox.GetSingleSandboxIdAsync();

        if (string.IsNullOrEmpty(sandboxId))
            return "No sandbox is currently running.";

        return "A sandbox is currently running.";
    }
}
