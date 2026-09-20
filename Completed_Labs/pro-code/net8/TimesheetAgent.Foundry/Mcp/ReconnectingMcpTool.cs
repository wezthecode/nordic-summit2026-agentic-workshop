using System.Text.Json;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;

namespace TimesheetAgent.Foundry.Mcp;

/// <summary>
/// Wraps a single <see cref="McpClientTool"/> so a failed invocation reconnects the underlying MCP
/// client and retries once, instead of surfacing the failure straight to the agent.
///
/// Found live, 2026-09-20: a Work IQ calendar read succeeded once, then failed on every later call
/// in the same conversation — reproduced independently by scripting the exact same two calls a few
/// seconds apart (both succeeded), which ruled out a bad request shape and pointed at something that
/// only breaks after real elapsed time mid-conversation (an idle MCP session, a short-lived token
/// needing a refresh that didn't happen cleanly — the two are hard to tell apart from the client
/// side, and either way the existing single long-lived <see cref="McpClient"/> per agent has no way
/// to recover once it happens). Retrying in the same broken session didn't help either (confirmed
/// live — the user tried "try again" and got the identical failure). A fresh connection has worked
/// every single time tonight, so reconnect-and-retry-once is the fix, regardless of which of those
/// two root causes it actually is.
/// </summary>
public sealed class ReconnectingMcpTool : AIFunction
{
    private readonly string toolName;
    private readonly Func<CancellationToken, Task<IList<McpClientTool>>> reconnect;
    private McpClientTool current;

    public ReconnectingMcpTool(McpClientTool initial, Func<CancellationToken, Task<IList<McpClientTool>>> reconnect)
    {
        current = initial;
        toolName = initial.Name;
        this.reconnect = reconnect;
    }

    public override string Name => current.Name;

    public override string Description => current.Description;

    public override JsonElement JsonSchema => current.JsonSchema;

    public override JsonElement? ReturnJsonSchema => current.ReturnJsonSchema;

    public override IReadOnlyDictionary<string, object?> AdditionalProperties => current.AdditionalProperties;

    protected override async ValueTask<object?> InvokeCoreAsync(
        AIFunctionArguments arguments, CancellationToken cancellationToken)
    {
        try
        {
            return await current.InvokeAsync(arguments, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Console.WriteLine($"[MCP tool '{toolName}' failed ({ex.GetType().Name}: {ex.Message}) — reconnecting and retrying once]");
            var freshTools = await reconnect(cancellationToken);
            current = freshTools.FirstOrDefault(t => t.Name == toolName)
                ?? throw new InvalidOperationException($"Tool '{toolName}' not found after reconnect.");
            return await current.InvokeAsync(arguments, cancellationToken);
        }
    }
}
