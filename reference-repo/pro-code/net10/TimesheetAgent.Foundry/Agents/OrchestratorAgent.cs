using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;

namespace TimesheetAgent.Foundry.Agents;

/// <summary>
/// The Timesheet Orchestrator. Delegates to the Calendar and Time-Entry specialist agents rather
/// than calling any MCP directly, mirroring the Copilot Studio orchestrator's connected-agent
/// pattern. Each specialist is exposed to the orchestrator as a callable AITool via
/// AIAgentExtensions.AsAIFunction — the SDK's equivalent of Copilot Studio's connected agents.
/// System prompt is loaded from shared-assets/prompts/orchestrator.system.md, which already
/// carries the explicit delegation triggers this project's own testing found necessary for
/// reliable routing — keep that behavioural parity, don't rely on tool descriptions alone.
/// </summary>
public static class OrchestratorAgent
{
    public static async Task<ChatClientAgent> CreateAsync(
        ChatClient chatClient,
        AIAgent calendarAgent,
        AIAgent timeEntryAgent,
        string promptsDirectory,
        CancellationToken cancellationToken = default)
    {
        var instructions = await File.ReadAllTextAsync(
            Path.Combine(promptsDirectory, "orchestrator.system.md"), cancellationToken);

        // One long-lived session per specialist, bound into its AsAIFunction wrapper, so the
        // orchestrator can call each specialist repeatedly across a conversation without
        // re-creating its session on every turn.
        var calendarSession = await calendarAgent.CreateSessionAsync(cancellationToken);
        var timeEntrySession = await timeEntryAgent.CreateSessionAsync(cancellationToken);

        var calendarTool = calendarAgent.AsAIFunction(
            new AIFunctionFactoryOptions { Name = "Calendar_Agent" }, calendarSession);
        var timeEntryTool = timeEntryAgent.AsAIFunction(
            new AIFunctionFactoryOptions { Name = "Time_Entry_Agent" }, timeEntrySession);

        return chatClient.AsAIAgent(
            instructions: instructions,
            name: "Timesheet Orchestrator",
            description: "Helps the signed-in user log their work hours for a recent period. Delegates to the Calendar Agent and Time Entry Agent; never reads the calendar or writes Dataverse itself.",
            tools: new List<AITool> { calendarTool, timeEntryTool },
            clientFactory: null!,
            loggerFactory: null!,
            services: null!);
    }
}
