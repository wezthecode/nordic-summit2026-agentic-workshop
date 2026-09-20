namespace Eval.Runners;

/// <summary>
/// Drives scenarios against the Foundry-deployed agent via its hosted REST endpoint.
/// Uses an Azure access token for the test user.
/// </summary>
public sealed class FoundryRunner : IRunner
{
    public Task RunOneAsync(string scenarioId, bool dryRun)
    {
        // TODO(rehearsal-week): resolve endpoint via `az foundry agent show`,
        // acquire test-user token, drive the scripted conversation,
        // capture transcript + Dataverse rows, run both graders, emit result.
        Console.WriteLine($"FoundryRunner: {scenarioId} (dry-run={dryRun}) — TODO");
        return Task.CompletedTask;
    }

    public Task RunAllAsync()
    {
        // TODO(rehearsal-week): iterate all scenarios with --reset-test-data between each.
        Console.WriteLine("FoundryRunner: run all — TODO");
        return Task.CompletedTask;
    }
}
