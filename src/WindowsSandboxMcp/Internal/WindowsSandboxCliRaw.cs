using System.Diagnostics;
using System.Text.Json;
using WindowsSandboxMcp.Models;

internal static class WindowsSandboxCliRaw
{
    public static string InferSandboxCliPath() => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Microsoft", "WindowsApps", "wsb.exe");

    public static bool IsSupportedOSForSandboxCli()
    {
        var ignoreOSVersion = string.Equals(
            bool.TrueString,
            Environment.GetEnvironmentVariable("WINSANDMCP_IGNORE_OS_VERSION_CHECKS"),
            StringComparison.OrdinalIgnoreCase);

        if (ignoreOSVersion)
            return true;

        return OperatingSystem.IsWindowsVersionAtLeast(10, 0, 26100);
    }

    public static string EnsureSandboxCliAvailability()
    {
        if (!IsSupportedOSForSandboxCli())
            throw new NotSupportedException("This utility requires Windows 11 24H2 or later version.");

        var wsbPath = InferSandboxCliPath();

        if (!File.Exists(wsbPath))
            throw new NotSupportedException("Please install the latest Windows Sandbox app from Microsoft Store.");

        return wsbPath;
    }

    public static JsonDocument ToJsonDocument(string? rawOutput)
    {
        if (string.IsNullOrWhiteSpace(rawOutput))
            rawOutput = "{}";

        return JsonDocument.Parse(rawOutput);
    }

    private static async Task<CliExecutionResult> ExecuteCliAsync(string wsbPath, IEnumerable<string> arguments, CancellationToken cancellationToken = default)
    {
        if (!OperatingSystem.IsWindows())
            throw new NotSupportedException("This MCP server is not supported on non-Windows operating systems.");

        var startInfo = new ProcessStartInfo(wsbPath, arguments)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            LoadUserProfile = true,
            UseShellExecute = false,
        };

        using var process = new Process() { StartInfo = startInfo, EnableRaisingEvents = true, };
        process.Start();

        var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

        var output = await outputTask.ConfigureAwait(false);
        var error = await errorTask.ConfigureAwait(false);

        return new CliExecutionResult
        {
            OutputDocument = ToJsonDocument(output),
            StandardError = string.IsNullOrWhiteSpace(error) ? null : error.Trim(),
            ExitCode = process.ExitCode
        };
    }

    public static async Task<CliExecutionResult> StartSandboxRawAsync(WindowsSandboxConfiguration? config = default, CancellationToken cancellationToken = default)
    {
        var wsbPath = EnsureSandboxCliAvailability();
        var argList = new List<string>() { "StartSandbox", "--raw", };
        var configXml = config?.ToString() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(configXml))
            argList.AddRange(["--config", configXml]);

        return await ExecuteCliAsync(wsbPath, argList, cancellationToken).ConfigureAwait(false);
    }

    public static async Task<CliExecutionResult> ConnectToSandboxRawAsync(string sandboxId, CancellationToken cancellationToken = default)
    {
        var wsbPath = EnsureSandboxCliAvailability();
        return await ExecuteCliAsync(wsbPath, ["ConnectToSandbox", "--id", sandboxId, "--raw"], cancellationToken).ConfigureAwait(false);
    }

    public static async Task<CliExecutionResult> ExecuteInSandboxRawAsync(string sandboxId, string commandToRunInSandbox, SandboxRunningContext runAs, string? workingDirectoryInSandbox = default, CancellationToken cancellationToken = default)
    {
        var wsbPath = EnsureSandboxCliAvailability();
        var runAsOption = runAs switch
        {
            SandboxRunningContext.ExistingLogin => "ExistingLogin",
            SandboxRunningContext.System => "System",
            _ => throw new ArgumentException($"Selected value {runAs} is not supported.", nameof(runAs)),
        };
        var argList = new List<string> { "Execute", "--id", sandboxId, "--raw", "--command", commandToRunInSandbox, "--run-as", runAsOption, };

        if (workingDirectoryInSandbox != null)
            argList.AddRange(["--working-directory", workingDirectoryInSandbox]);

        return await ExecuteCliAsync(wsbPath, argList, cancellationToken).ConfigureAwait(false);
    }

    public static async Task<CliExecutionResult> StopSandboxRawAsync(string sandboxId, CancellationToken cancellationToken = default)
    {
        var wsbPath = EnsureSandboxCliAvailability();
        return await ExecuteCliAsync(wsbPath, ["StopSandbox", "--id", sandboxId, "--raw"], cancellationToken).ConfigureAwait(false);
    }

    public static async Task<CliExecutionResult> AddSharedFolderRawAsync(string sandboxId, string hostDirectoryPath, string? sandboxAbsolutePath = default, bool? allowWrite = default, CancellationToken cancellationToken = default)
    {
        var wsbPath = EnsureSandboxCliAvailability();
        var argList = new List<string> { "ShareFolder", "--id", sandboxId, "--raw", "--host-path", hostDirectoryPath, };
        if (sandboxAbsolutePath != null)
            argList.AddRange(["--sandbox-path", sandboxAbsolutePath,]);
        if (allowWrite == true)
            argList.Add("--allow-write");

        return await ExecuteCliAsync(wsbPath, argList, cancellationToken).ConfigureAwait(false);
    }

    public static async Task<CliExecutionResult> GetSandboxNetworkRawAsync(string sandboxId, CancellationToken cancellationToken = default)
    {
        var wsbPath = EnsureSandboxCliAvailability();
        return await ExecuteCliAsync(wsbPath, ["GetIpAddress", "--id", sandboxId, "--raw"], cancellationToken).ConfigureAwait(false);
    }

    public static async Task<CliExecutionResult> GetRunningSandboxesRawAsync(CancellationToken cancellationToken = default)
    {
        var wsbPath = EnsureSandboxCliAvailability();
        return await ExecuteCliAsync(wsbPath, ["ListRunningSandboxes", "--raw"], cancellationToken).ConfigureAwait(false);
    }
}
