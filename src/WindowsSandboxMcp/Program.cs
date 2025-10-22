using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using WindowsSandboxMcp.Tools;

if (!OperatingSystem.IsWindows())
{
    Console.Error.WriteLine("This MCP server is not supported on non-Windows operating systems.");
    return 1;
}

// Start MCP stdio Server
using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

try
{
    var builder = Host.CreateEmptyApplicationBuilder(settings: null);
    builder.Configuration.AddCommandLine(args);
    builder.Configuration.AddEnvironmentVariables();

    builder.Services.AddMcpServer(o =>
        {
            if (o.ServerInfo != null)
            {
                o.ServerInfo.Name = "windows-sandbox-mcp";
                o.ServerInfo.Version = Assembly.GetExecutingAssembly()
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                    ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString()
                    ?? "0.0.1";
            }

            o.ServerInstructions = Prompts.McpServerPrompt;            
        })
        .WithStdioServerTransport()
        .WithTools(new WindowsSandboxTools());

    using var app = builder.Build();
    app.Run();
}
catch (OperationCanceledException) { }

return 0;
