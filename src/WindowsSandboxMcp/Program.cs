using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol;
using System.Text.Json;
using System.Text.Json.Nodes;
using WindowsSandboxMcp.Tools;

// MCP stdio 서버 시작
var cts = new CancellationTokenSource();
Console.CancelKeyPress += (s, e) => { e.Cancel = true; cts.Cancel(); };

try
{
    await RunMcpServerAsync(cts.Token);
}
catch (OperationCanceledException)
{
    // 정상 종료
}

static async Task RunMcpServerAsync(CancellationToken cancellationToken)
{
    var builder = Host.CreateEmptyApplicationBuilder(settings: null);

    builder.Services.AddMcpServer()
        .WithStdioServerTransport()
        .WithTools(new WindowsSandboxTools());

    var app = builder.Build();

    await app.RunAsync();
}
