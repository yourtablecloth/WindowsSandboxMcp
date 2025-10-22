using System.Diagnostics;
using System.Management;
using System.Xml.Linq;

public static class WindowsSandbox
{
    public static bool CanUseSandboxCli()
        => WindowsSandboxCliRaw.IsSupportedOSForSandboxCli();

    public static async Task<(string? SandboxId, string? Error)> StartSandboxAsync(WindowsSandboxConfiguration? config = default, CancellationToken cancellationToken = default)
    {
        var result = await WindowsSandboxCliRaw.StartSandboxRawAsync(config, cancellationToken).ConfigureAwait(false);

        if (result.HasError)
            return (null, result.GetErrorMessage());

        if (result.OutputDocument.RootElement.TryGetProperty("Id", out var idElem))
            return (idElem.GetString(), null);

        return (null, null);
    }

    public static async Task<string?> ConnectToSandboxAsync(string sandboxId, CancellationToken cancellationToken = default)
    {
        var result = await WindowsSandboxCliRaw.ConnectToSandboxRawAsync(sandboxId, cancellationToken).ConfigureAwait(false);
        return result.HasError ? result.GetErrorMessage() : null;
    }

    public static async Task WaitUntilSandboxReadyAsync(string sandboxId, TimeSpan extraTimeSpan = default, CancellationToken cancellationToken = default)
    {
        while (!await IsSandboxLoggedInAsync(sandboxId, cancellationToken).ConfigureAwait(false))
            await Task.Delay(TimeSpan.FromSeconds(1d), cancellationToken).ConfigureAwait(false);

        if (extraTimeSpan > TimeSpan.Zero)
            await Task.Delay(extraTimeSpan, cancellationToken).ConfigureAwait(false);
    }

    public static async Task<bool> IsSandboxLoggedInAsync(string sandboxId, CancellationToken cancellationToken = default)
    {
        var (exitCode, error) = await ExecuteInSandboxAsync(sandboxId, "C:\\Windows\\System32\\cmd.exe /c ver", SandboxRunningContext.ExistingLogin, cancellationToken: cancellationToken).ConfigureAwait(false);
        return exitCode.HasValue;
    }

    public static async Task<(int? ExitCode, string? Error)> ExecuteInSandboxAsync(string sandboxId, string commandToRunInSandbox, SandboxRunningContext runAs, string? workingDirectoryInSandbox = default, CancellationToken cancellationToken = default)
    {
        var result = await WindowsSandboxCliRaw.ExecuteInSandboxRawAsync(sandboxId, commandToRunInSandbox, runAs, workingDirectoryInSandbox, cancellationToken).ConfigureAwait(false);

        if (result.HasError)
            return (null, result.GetErrorMessage());

        if (result.OutputDocument.RootElement.TryGetProperty("ExitCode", out var exitCodeElem))
        {
            var exitCode = exitCodeElem.TryGetInt32(out var exitCodeValue) ? exitCodeValue : default(int?);
            return (exitCode, null);
        }

        return (null, null);
    }

    public static async Task<string?> StopSandboxAsync(string sandboxId, CancellationToken cancellationToken = default)
    {
        var result = await WindowsSandboxCliRaw.StopSandboxRawAsync(sandboxId, cancellationToken).ConfigureAwait(false);
        return result.HasError ? result.GetErrorMessage() : null;
    }

    public static async Task<string?> AddSharedFolderAsync(string sandboxId, string hostDirectoryPath, string? sandboxAbsolutePath = default, bool? allowWrite = default, CancellationToken cancellationToken = default)
    {
        var result = await WindowsSandboxCliRaw.AddSharedFolderRawAsync(sandboxId, hostDirectoryPath, sandboxAbsolutePath, allowWrite, cancellationToken).ConfigureAwait(false);
        return result.HasError ? result.GetErrorMessage() : null;
    }

    public static async Task<(IEnumerable<WindowsSandboxNetwork> Networks, string? Error)> GetSandboxNetworkAsync(string sandboxId, CancellationToken cancellationToken = default)
    {
        var list = new List<WindowsSandboxNetwork>();
        var result = await WindowsSandboxCliRaw.GetSandboxNetworkRawAsync(sandboxId, cancellationToken).ConfigureAwait(false);

        if (result.HasError)
            return (list.AsReadOnly(), result.GetErrorMessage());

        foreach (var eachEnv in result.OutputDocument.RootElement.GetProperty("Networks").EnumerateArray())
        {
            var ipv4Address = eachEnv.GetProperty("IpV4Address").GetString();
            if (ipv4Address == null)
                continue;
            list.Add(new WindowsSandboxNetwork { SandboxId = sandboxId, IPv4Address = ipv4Address, });
        }

        return (list.AsReadOnly(), null);
    }

    public static async Task<(IEnumerable<string> SandboxIds, string? Error)> GetRunningSandboxIdsAsync(CancellationToken cancellationToken = default)
    {
        var list = new List<string>();
        var result = await WindowsSandboxCliRaw.GetRunningSandboxesRawAsync(cancellationToken).ConfigureAwait(false);

        if (result.HasError)
            return (list.AsReadOnly(), result.GetErrorMessage());

        foreach (var eachEnv in result.OutputDocument.RootElement.GetProperty("WindowsSandboxEnvironments").EnumerateArray())
        {
            var id = eachEnv.GetProperty("Id").GetString();
            if (id == null)
                continue;
            list.Add(id);
        }

        return (list.AsReadOnly(), null);
    }

    public static async Task<string?> GetSingleSandboxIdAsync(CancellationToken cancellationToken = default)
    {
        var (sandboxIds, error) = await WindowsSandbox.GetRunningSandboxIdsAsync(cancellationToken).ConfigureAwait(false);

        if (sandboxIds.Count() == 1)
            return sandboxIds.First();

        return default;
    }

    public static bool GetActiveSandboxRemoteSession(string sandboxId)
    {
        try
        {
            string q = $"SELECT CommandLine FROM Win32_Process WHERE Name = 'WindowsSandboxRemoteSession.exe' AND CommandLine LIKE '%{sandboxId}%'";
            using var searcher = new ManagementObjectSearcher(q);
            return searcher.Get().Count > 0;
        }
        catch (ManagementException) { /* 접근권한/예외 처리 */ }
        return false;
    }
}
