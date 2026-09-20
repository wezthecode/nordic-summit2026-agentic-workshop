using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using OpenAI.Chat;
using TimesheetAgent.Foundry.Mcp;

namespace TimesheetAgent.Foundry.Agents;

/// <summary>
/// The Calendar specialist agent. Reads the signed-in user's calendar via Work IQ MCP and returns
/// a structured event list. System prompt is loaded from Labs/prompts/calendar-child.system.md
/// at startup — never copy-pasted into source — so this stays in sync with the Copilot Studio build.
/// </summary>
public static class CalendarChildAgent
{
    public static async Task<ChatClientAgent> CreateAsync(
        ChatClient chatClient,
        string tenantId,
        string clientId,
        string promptsDirectory,
        CancellationToken cancellationToken = default)
    {
        var instructions = await File.ReadAllTextAsync(
            Path.Combine(promptsDirectory, "calendar-child.system.md"), cancellationToken);

        async Task<IList<McpClientTool>> ReconnectAsync(CancellationToken ct)
        {
            var freshClient = await WorkIqCalendarMcpClient.ConnectAsync(tenantId, clientId, ct);
            return await freshClient.ListToolsAsync(cancellationToken: ct);
        }

        var mcpTools = await ReconnectAsync(cancellationToken);
        var reconnectingTools = mcpTools.Select(t => new ReconnectingMcpTool(t, ReconnectAsync)).ToList();

        return chatClient.AsAIAgent(
            instructions: instructions,
            name: "Calendar Agent",
            description: "Reads the signed-in user's Outlook calendar for a date range and returns a structured list of events with date, duration, subject, and attendees. Use for anything about what happened on the calendar or what meetings occurred.",
            tools: reconnectingTools.Cast<AITool>().ToList(),
            clientFactory: null!,
            loggerFactory: null!,
            services: null!);
    }
}
