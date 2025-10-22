using System.Text.Json;

namespace WindowsSandboxMcp;

internal sealed class CliExecutionResult
{
    public required JsonDocument OutputDocument { get; init; }
    public string? StandardError { get; init; }
    public int ExitCode { get; init; }

    public bool HasError => !string.IsNullOrWhiteSpace(StandardError) || ExitCode != 0;

    public string GetErrorMessage()
    {
        if (!HasError)
            return string.Empty;

        var errorParts = new List<string>();

        if (!string.IsNullOrWhiteSpace(StandardError))
            errorParts.Add($"Error: {StandardError}");

        if (ExitCode != 0)
            errorParts.Add($"Exit code: {ExitCode}");

        return string.Join(" | ", errorParts);
    }
}
