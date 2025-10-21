public sealed class WindowsSandboxNetwork
{
    public required string SandboxId { get; set; }
    public string? IPv4Address { get; set; }
    public string? IPv6Address { get; set; }

    public override string ToString()
        => $"Sandbox ID: {SandboxId}, IPv4: {IPv4Address ?? "(Unassigned)"}, IPv6: {IPv6Address ?? "(Unassigned)"}";
}
