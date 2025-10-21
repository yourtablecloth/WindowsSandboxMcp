using System.Diagnostics;
using System.Management;
using System.Xml.Linq;

public static class WindowsSandbox
{
    public static bool CanUseSandboxCli()
        => WindowsSandboxCliRaw.IsSupportedOSForSandboxCli();

    public static async Task<string> StartSandboxViaWsbFileAsync(WindowsSandboxConfiguration config)
    {
        try
        {
            // Generate XML document with proper declaration
            var xDocument = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                config.ToXmlElement()
            );

            // Create temporary WSB file
            var tempWsbPath = Path.Combine(Path.GetTempPath(), $"sandbox_{Guid.NewGuid():N}.wsb");

            // Save XML to WSB file
            await using (var fileStream = new FileStream(tempWsbPath, FileMode.Create, FileAccess.Write, FileShare.None))
            await using (var streamWriter = new StreamWriter(fileStream, System.Text.Encoding.UTF8))
            {
                await streamWriter.WriteAsync(xDocument.ToString());
            }

            // Launch WSB file using shell execution
            var processStartInfo = new ProcessStartInfo
            {
                FileName = tempWsbPath,
                UseShellExecute = true,
                Verb = "open"
            };

            var process = Process.Start(processStartInfo);

            if (process == null)
            {
                // Clean up temp file if process failed to start
                try { File.Delete(tempWsbPath); } catch { }
                return "Failed to start sandbox via WSB file.";
            }

            // Note: The WSB file will be automatically cleaned up by the temp folder cleanup mechanism
            // We don't delete it immediately as Windows Sandbox might still be reading it
            return $"Sandbox started successfully via WSB file. Note: Advanced features (execute commands, network info, etc.) are not available without Windows Sandbox CLI.";
        }
        catch (Exception ex)
        {
            return $"Error starting sandbox via WSB file: {ex.Message}";
        }
    }

    public static async Task<string?> StartSandboxAsync(WindowsSandboxConfiguration? config = default, CancellationToken cancellationToken = default)
    {
        var doc = await WindowsSandboxCliRaw.StartSandboxRawAsync(config, cancellationToken).ConfigureAwait(false);

        if (doc.RootElement.TryGetProperty("Id", out var idElem))
            return idElem.GetString();

        return default;
    }

    public static Task ConnectToSandboxAsync(string sandboxId, CancellationToken cancellationToken = default)
        => WindowsSandboxCliRaw.ConnectToSandboxRawAsync(sandboxId, cancellationToken);

    public static async Task WaitUntilSandboxReadyAsync(string sandboxId, TimeSpan extraTimeSpan = default, CancellationToken cancellationToken = default)
    {
        while (!await IsSandboxLoggedInAsync(sandboxId, cancellationToken).ConfigureAwait(false))
            await Task.Delay(TimeSpan.FromSeconds(1d), cancellationToken).ConfigureAwait(false);

        if (extraTimeSpan > TimeSpan.Zero)
            await Task.Delay(extraTimeSpan, cancellationToken).ConfigureAwait(false);
    }

    public static async Task<bool> IsSandboxLoggedInAsync(string sandboxId, CancellationToken cancellationToken = default)
        => (await ExecuteInSandboxAsync(sandboxId, "C:\\Windows\\System32\\cmd.exe /c ver", SandboxRunningContext.ExistingLogin, cancellationToken: cancellationToken).ConfigureAwait(false)).HasValue;

    public static async Task<int?> ExecuteInSandboxAsync(string sandboxId, string commandToRunInSandbox, SandboxRunningContext runAs, string? workingDirectoryInSandbox = default, CancellationToken cancellationToken = default)
    {
        var doc = await WindowsSandboxCliRaw.ExecuteInSandboxRawAsync(sandboxId, commandToRunInSandbox, runAs, workingDirectoryInSandbox, cancellationToken).ConfigureAwait(false);

        if (doc.RootElement.TryGetProperty("ExitCode", out var exitCodeElem))
            return exitCodeElem.TryGetInt32(out var exitCodeValue) ? exitCodeValue : default;

        return default;
    }

    public static Task StopSandboxAsync(string sandboxId, CancellationToken cancellationToken = default)
        => WindowsSandboxCliRaw.StopSandboxRawAsync(sandboxId, cancellationToken);

    public static Task AddSharedFolderAsync(string sandboxId, string hostDirectoryPath, string? sandboxAbsolutePath = default, bool? allowWrite = default, CancellationToken cancellationToken = default)
        => WindowsSandboxCliRaw.AddSharedFolderRawAsync(sandboxId, hostDirectoryPath, sandboxAbsolutePath, allowWrite, cancellationToken);

    public static async Task<IEnumerable<WindowsSandboxNetwork>> GetSandboxNetworkAsync(string sandboxId, CancellationToken cancellationToken = default)
    {
        var list = new List<WindowsSandboxNetwork>();
        var doc = await WindowsSandboxCliRaw.GetSandboxNetworkRawAsync(sandboxId, cancellationToken).ConfigureAwait(false);

        foreach (var eachEnv in doc.RootElement.GetProperty("Networks").EnumerateArray())
        {
            var ipv4Address = eachEnv.GetProperty("IpV4Address").GetString();
            if (ipv4Address == null)
                continue;
            list.Add(new WindowsSandboxNetwork { SandboxId = sandboxId, IPv4Address = ipv4Address, });
        }

        return list.AsReadOnly();
    }

    public static async Task<IEnumerable<string>> GetRunningSandboxIdsAsync(CancellationToken cancellationToken = default)
    {
        var list = new List<string>();
        var doc = await WindowsSandboxCliRaw.GetRunningSandboxesRawAsync();

        foreach (var eachEnv in doc.RootElement.GetProperty("WindowsSandboxEnvironments").EnumerateArray())
        {
            var id = eachEnv.GetProperty("Id").GetString();
            if (id == null)
                continue;
            list.Add(id);
        }

        return list.AsReadOnly();
    }

    public static async Task<string?> GetSingleSandboxIdAsync(CancellationToken cancellationToken = default)
    {
        var sandboxIds = await WindowsSandbox.GetRunningSandboxIdsAsync(cancellationToken).ConfigureAwait(false);

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
