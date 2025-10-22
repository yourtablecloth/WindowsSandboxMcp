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
        [Description("Command to execute on logon")] string? logonCommand = null,
        [Description("Comma-separated list of host folder paths to map to the sandbox. Example: C:\\Temp,D:\\Documents")] string? mappedFolders = null)
    {
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

        // Parse mapped folders from comma-separated string
        if (!string.IsNullOrWhiteSpace(mappedFolders))
        {
            var folderPaths = mappedFolders.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var folderPath in folderPaths)
            {
                if (!string.IsNullOrWhiteSpace(folderPath))
                {
                    config.MappedFolders.Add(new MappedFolderConfiguration
                    {
                        HostFolderPath = folderPath,
                        SandboxFolderAbsolutePath = null,
                        ReadOnly = null
                    });
                }
            }
        }

        if (!WindowsSandbox.CanUseSandboxCli())
            return "Windows Sandbox CLI is not available on this system.";

        // Check if a sandbox is already running
        var existingSandboxId = await WindowsSandbox.GetSingleSandboxIdAsync().ConfigureAwait(false);
        if (!string.IsNullOrEmpty(existingSandboxId))
            return "A sandbox is already running. Please stop the existing sandbox before starting a new one.";

        var (sandboxId, error) = await WindowsSandbox.StartSandboxAsync(config).ConfigureAwait(false);

        if (!string.IsNullOrEmpty(error))
            return $"Failed to start sandbox: {error}";

        if (string.IsNullOrEmpty(sandboxId))
            return "Failed to start sandbox.";

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
            if (!WindowsSandbox.CanUseSandboxCli())
                return "Windows Sandbox CLI is not available on this system.";

            var sandboxId = await WindowsSandbox.GetSingleSandboxIdAsync().ConfigureAwait(false);
            if (string.IsNullOrEmpty(sandboxId))
                return "No running sandbox found. Please start a sandbox first.";

            if (!WindowsSandbox.GetActiveSandboxRemoteSession(sandboxId))
            {
                _ = WindowsSandbox.ConnectToSandboxAsync(sandboxId).ConfigureAwait(false);
                await WindowsSandbox.WaitUntilSandboxReadyAsync(sandboxId, TimeSpan.FromSeconds(1d)).ConfigureAwait(false);
            }

            var runAsContext = runAs.Equals("System", StringComparison.OrdinalIgnoreCase)
                ? SandboxRunningContext.System
                : SandboxRunningContext.ExistingLogin;

            _ = WindowsSandbox.ExecuteInSandboxAsync(sandboxId, $"cmd.exe /c start {command}", runAsContext, workingDirectory).ConfigureAwait(false);
            return "Command execution request sent in sandbox.";
        }
        catch (Exception ex)
        {
            return $"Error executing command in sandbox: {ex.Message}";
        }
    }

    [McpServerTool, Description("Stops the running Sandbox.")]
    public async Task<string> StopSandbox()
    {
        if (!WindowsSandbox.CanUseSandboxCli())
            return "Windows Sandbox CLI is not available on this system.";

        var sandboxId = await WindowsSandbox.GetSingleSandboxIdAsync().ConfigureAwait(false);
        if (string.IsNullOrEmpty(sandboxId))
            return "No running sandbox found.";

        var error = await WindowsSandbox.StopSandboxAsync(sandboxId).ConfigureAwait(false);

        if (!string.IsNullOrEmpty(error))
            return $"Failed to stop sandbox: {error}";

        return "Sandbox stopped successfully.";
    }

    [McpServerTool, Description("Adds a shared folder to the running Sandbox.")]
    public async Task<string> AddSharedFolder(
        [Description("Host directory path")] string hostDirectoryPath,
        [Description("Absolute path in Sandbox")] string? sandboxAbsolutePath = null,
        [Description("Whether to allow write access")] bool? allowWrite = null)
    {
        if (!WindowsSandbox.CanUseSandboxCli())
            return "Windows Sandbox CLI is not available on this system.";

        var sandboxId = await WindowsSandbox.GetSingleSandboxIdAsync().ConfigureAwait(false);
        if (string.IsNullOrEmpty(sandboxId))
            return "No running sandbox found. Please start a sandbox first.";

        var error = await WindowsSandbox.AddSharedFolderAsync(sandboxId, hostDirectoryPath, sandboxAbsolutePath, allowWrite).ConfigureAwait(false);

        if (!string.IsNullOrEmpty(error))
            return $"Failed to add shared folder: {error}";

        return $"Shared folder added to sandbox: {hostDirectoryPath}";
    }

    [McpServerTool, Description("Opens a window that accesses the currently running Windows Sandbox session.")]
    public async Task<string> OpenSandboxRemoteSessionWindowAsync()
    {
        if (!WindowsSandbox.CanUseSandboxCli())
            return "Windows Sandbox CLI is not available on this system.";

        var sandboxId = await WindowsSandbox.GetSingleSandboxIdAsync().ConfigureAwait(false);
        if (string.IsNullOrEmpty(sandboxId))
            return "No running sandbox found. Please start a sandbox first.";

        _ = WindowsSandbox.ConnectToSandboxAsync(sandboxId).ConfigureAwait(false);
        return "Sandbox remote session window opened.";
    }

    [McpServerTool, Description("Gets network information for the running Sandbox.")]
    public async Task<string> GetSandboxNetwork()
    {
        if (!WindowsSandbox.CanUseSandboxCli())
            return "Windows Sandbox CLI is not available on this system.";

        var sandboxId = await WindowsSandbox.GetSingleSandboxIdAsync().ConfigureAwait(false);
        if (string.IsNullOrEmpty(sandboxId))
            return "No running sandbox found. Please start a sandbox first.";

        var (networks, error) = await WindowsSandbox.GetSandboxNetworkAsync(sandboxId).ConfigureAwait(false);

        if (!string.IsNullOrEmpty(error))
            return $"Failed to get network information: {error}";

        if (!networks.Any())
            return "No network information found for the sandbox.";

        var result = string.Join("\n", networks.Select(n => $"IPv4: {n.IPv4Address}"));

        return $"Network information:\n{result}";
    }

    [McpServerTool, Description("Checks if a Sandbox is currently running.")]
    public async Task<string> IsSandboxRunning()
    {
        if (!WindowsSandbox.CanUseSandboxCli())
            return "Windows Sandbox CLI is not available on this system.";

        var sandboxId = await WindowsSandbox.GetSingleSandboxIdAsync().ConfigureAwait(false);

        if (string.IsNullOrEmpty(sandboxId))
            return "No sandbox is currently running.";

        return "A sandbox is currently running.";
    }

    [McpServerTool, Description("Checks if the Sandbox remote session window is currently opened.")]
    public async Task<string> IsRemoteSessionWindowOpened()
    {
        if (!WindowsSandbox.CanUseSandboxCli())
            return "Windows Sandbox CLI is not available on this system.";

        var sandboxId = await WindowsSandbox.GetSingleSandboxIdAsync().ConfigureAwait(false);
        if (string.IsNullOrEmpty(sandboxId))
            return "No running sandbox found. Please start a sandbox first.";

        var isOpened = WindowsSandbox.GetActiveSandboxRemoteSession(sandboxId);
        return isOpened ? "The sandbox remote session window is currently opened." : "The sandbox remote session window is not opened.";
    }
}
