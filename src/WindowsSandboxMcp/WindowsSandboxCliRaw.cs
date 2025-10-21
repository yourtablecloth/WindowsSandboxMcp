using System.Diagnostics;
using System.Text.Json;

internal static class WindowsSandboxCliRaw
{
    public static string InferSandboxCliPath() => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Microsoft", "WindowsApps", "wsb.exe");

    public static string EnsureSandboxCliAvailability()
    {
        var ignoreOSVersion = string.Equals(
            bool.TrueString,
            Environment.GetEnvironmentVariable("WINSANDMCP_IGNORE_OS_VERSION_CHECKS"),
            StringComparison.OrdinalIgnoreCase);

        if (ignoreOSVersion == false)
        {
            if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 26100))
                throw new NotSupportedException("This utility requires Windows 11 24H2 or later version.");
        }

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

    public static async Task<JsonDocument> StartSandboxRawAsync(WindowsSandboxConfiguration? config = default, CancellationToken cancellationToken = default)
    {
        var wsbPath = EnsureSandboxCliAvailability();
        var argList = new List<string>() { "StartSandbox", "--raw", };
        var configXml = config?.ToString() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(configXml))
            argList.AddRange(["--config", configXml]);

        var startInfo = new ProcessStartInfo(wsbPath, argList)
        {
            RedirectStandardOutput = true,
            CreateNoWindow = true,
            LoadUserProfile = true,
            UseShellExecute = false,
        };
        using var process = new Process() { StartInfo = startInfo, EnableRaisingEvents = true, };
        process.Start();

        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
        var content = await process.StandardOutput.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        return ToJsonDocument(content);
    }

    public static async Task<JsonDocument> ConnectToSandboxRawAsync(string sandboxId, CancellationToken cancellationToken = default)
    {
        var wsbPath = EnsureSandboxCliAvailability();
        var startInfo = new ProcessStartInfo(wsbPath, ["ConnectToSandbox", "--id", sandboxId, "--raw"])
        {
            RedirectStandardOutput = true,
            CreateNoWindow = true,
            LoadUserProfile = true,
            UseShellExecute = false,
        };
        using var process = new Process() { StartInfo = startInfo, EnableRaisingEvents = true, };
        process.Start();

        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
        var content = await process.StandardOutput.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        return ToJsonDocument(content);
    }

    public static async Task<JsonDocument> ExecuteInSandboxRawAsync(string sandboxId, string commandToRunInSandbox, SandboxRunningContext runAs, string? workingDirectoryInSandbox = default, CancellationToken cancellationToken = default)
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

        var startInfo = new ProcessStartInfo(wsbPath, argList)
        {
            RedirectStandardOutput = true,
            CreateNoWindow = true,
            LoadUserProfile = true,
            UseShellExecute = false,
        };
        using var process = new Process() { StartInfo = startInfo, EnableRaisingEvents = true, };
        process.Start();

        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
        var content = await process.StandardOutput.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        return ToJsonDocument(content);
    }

    public static async Task<JsonDocument> StopSandboxRawAsync(string sandboxId, CancellationToken cancellationToken = default)
    {
        var wsbPath = EnsureSandboxCliAvailability();
        var startInfo = new ProcessStartInfo(wsbPath, ["StopSandbox", "--id", sandboxId, "--raw"])
        {
            RedirectStandardOutput = true,
            CreateNoWindow = true,
            LoadUserProfile = true,
            UseShellExecute = false,
        };
        using var process = new Process() { StartInfo = startInfo, EnableRaisingEvents = true, };
        process.Start();

        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
        var content = await process.StandardOutput.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        return ToJsonDocument(content);
    }

    public static async Task<JsonDocument> AddSharedFolderRawAsync(string sandboxId, string hostDirectoryPath, string? sandboxAbsolutePath = default, bool? allowWrite = default, CancellationToken cancellationToken = default)
    {
        var wsbPath = EnsureSandboxCliAvailability();
        var argList = new List<string> { "ShareFolder", "--id", sandboxId, "--raw", "--host-path", hostDirectoryPath, };
        if (sandboxAbsolutePath != null)
            argList.AddRange(["--sandbox-path", sandboxAbsolutePath,]);
        if (allowWrite == true)
            argList.Add("--allow-write");

        var startInfo = new ProcessStartInfo(wsbPath, argList)
        {
            RedirectStandardOutput = true,
            CreateNoWindow = true,
            LoadUserProfile = true,
            UseShellExecute = false,
        };
        using var process = new Process() { StartInfo = startInfo, EnableRaisingEvents = true, };
        process.Start();

        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
        var content = await process.StandardOutput.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        return ToJsonDocument(content);
    }

    public static async Task<JsonDocument> GetSandboxNetworkRawAsync(string sandboxId, CancellationToken cancellationToken = default)
    {
        var wsbPath = EnsureSandboxCliAvailability();
        var startInfo = new ProcessStartInfo(wsbPath, ["GetIpAddress", "--id", sandboxId, "--raw"])
        {
            RedirectStandardOutput = true,
            CreateNoWindow = true,
            LoadUserProfile = true,
            UseShellExecute = false,
        };
        using var process = new Process() { StartInfo = startInfo, EnableRaisingEvents = true, };
        process.Start();

        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
        var content = await process.StandardOutput.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        return ToJsonDocument(content);
    }

    public static async Task<JsonDocument> GetRunningSandboxesRawAsync(CancellationToken cancellationToken = default)
    {
        var wsbPath = EnsureSandboxCliAvailability();
        var startInfo = new ProcessStartInfo(wsbPath, ["ListRunningSandboxes", "--raw"])
        {
            RedirectStandardOutput = true,
            CreateNoWindow = true,
            LoadUserProfile = true,
            UseShellExecute = false,
        };
        using var process = new Process() { StartInfo = startInfo, EnableRaisingEvents = true, };
        process.Start();

        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
        var content = await process.StandardOutput.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        return ToJsonDocument(content);
    }
}
