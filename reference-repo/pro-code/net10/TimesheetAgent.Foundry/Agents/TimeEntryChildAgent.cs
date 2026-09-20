using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using OpenAI.Chat;
using TimesheetAgent.Foundry.Mcp;

namespace TimesheetAgent.Foundry.Agents;

/// <summary>
/// The Time-Entry specialist agent. Turns calendar events into draft cre_timeentry rows, then
/// writes confirmed rows to Dataverse via the Dataverse MCP. System prompt is loaded from
/// shared-assets/prompts/time-entry-child.system.md, with {{SOURCE_VALUE}} substituted for the
/// numeric choice value for "Foundry agent" (100000002 in the reference environment — cre_source
/// is a Dataverse choice column, so this must be the integer, never the string label; confirmed
/// the hard way in testing on 2026-09-19 when the agent tried the label first and the write
/// failed). Re-verify this number against your own environment's option set before relying on it —
/// Dataverse auto-generates these values at build time and they are not guaranteed portable.
/// </summary>
public static class TimeEntryChildAgent
{
    public static async Task<ChatClientAgent> CreateAsync(
        ChatClient chatClient,
        string environmentUrl,
        string tenantId,
        string clientId,
        string promptsDirectory,
        CancellationToken cancellationToken = default)
    {
        var instructionsTemplate = await File.ReadAllTextAsync(
            Path.Combine(promptsDirectory, "time-entry-child.system.md"), cancellationToken);
        var instructions = instructionsTemplate.Replace("{{SOURCE_VALUE}}", "100000002");

        async Task<IList<McpClientTool>> ReconnectAsync(CancellationToken ct)
        {
            var freshClient = await DataverseMcpClient.ConnectAsync(environmentUrl, tenantId, clientId, ct);
            return await freshClient.ListToolsAsync(cancellationToken: ct);
        }

        var mcpTools = await ReconnectAsync(cancellationToken);

        // Scope to just the two tools this agent's prompt says it's allowed to use — same
        // "Allow all off, enable only what's needed" discipline as the Copilot Studio build.
        // Reconnect-and-retry-once is safe here even for create_record: the cre_calendareventid
        // alternate key already rejects a duplicate write, so retrying a create_record that
        // actually succeeded just before a connection failure surfaces as an ordinary (and correct)
        // duplicate-key error, never a double-write.
        var scopedTools = mcpTools
            .Where(t => t.Name is "read_query" or "create_record")
            .Select(t => new ReconnectingMcpTool(t, ReconnectAsync))
            .Cast<AITool>()
            .ToList();

        return chatClient.AsAIAgent(
            instructions: instructions,
            name: "Time Entry Agent",
            description: "Turns a list of calendar events into draft time entry rows, confirms them with the user, then writes the confirmed rows to Dataverse. Use for anything about logging, recording, drafting, confirming, or writing time entries or hours.",
            tools: scopedTools,
            clientFactory: null!,
            loggerFactory: null!,
            services: null!);
    }
}
