namespace Eval.Runners;

public interface IRunner
{
    Task RunOneAsync(string scenarioId, bool dryRun);
    Task RunAllAsync();
}

/// <summary>
/// Drives scenarios against the published Copilot Studio agent via Direct Line / the
/// Power Platform bot connection.
/// </summary>
public sealed class CopilotStudioRunner : IRunner
{
    public Task RunOneAsync(string scenarioId, bool dryRun)
    {
        // TODO(rehearsal-week): connect to the published bot, drive the scripted
        // conversation from scenarios.json, capture transcript + Dataverse rows,
        // run both graders, emit a result record.
        Console.WriteLine($"CopilotStudioRunner: {scenarioId} (dry-run={dryRun}) — TODO");
        return Task.CompletedTask;
    }

    public Task RunAllAsync()
    {
        // TODO(rehearsal-week): iterate all scenarios with --reset-test-data between each.
        Console.WriteLine("CopilotStudioRunner: run all — TODO");
        return Task.CompletedTask;
    }
}
