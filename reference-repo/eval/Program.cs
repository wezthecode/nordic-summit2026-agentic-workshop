using Eval.Runners;

// Eval harness — drives scenarios.json against either runtime, produces results.json.
// See workshop/shared-assets/eval/README.md for usage.

if (args.Length == 0)
{
    Console.WriteLine("Usage:");
    Console.WriteLine("  eval --runtime <copilot-studio|foundry> --scenario <id> [--dry-run]");
    Console.WriteLine("  eval --runtime <copilot-studio|foundry> --all > results.json");
    Console.WriteLine("  eval --reset-test-data");
    Console.WriteLine("  eval --compare results-cs.json results-foundry.json");
    return 1;
}

// TODO(rehearsal-week): argument parsing + dispatch to the relevant runner.
// Implementation is deliberately deferred — the surface above is the contract
// the lab-05 README depends on. Implementations live in Runners/.

var op = args[0];
return op switch
{
    "--reset-test-data" => await ResetTestDataAsync(),
    "--compare"         => await CompareAsync(args[1], args[2]),
    _                   => await RunAsync(args),
};

static async Task<int> RunAsync(string[] args)
{
    var runtimeIdx = Array.IndexOf(args, "--runtime");
    var runtime = args[runtimeIdx + 1];

    IRunner runner = runtime switch
    {
        "copilot-studio" => new CopilotStudioRunner(),
        "foundry"        => new FoundryRunner(),
        _ => throw new ArgumentException($"Unknown runtime: {runtime}")
    };

    if (args.Contains("--all"))
    {
        await runner.RunAllAsync();
    }
    else
    {
        var scenarioIdx = Array.IndexOf(args, "--scenario");
        var scenarioId = args[scenarioIdx + 1];
        await runner.RunOneAsync(scenarioId, dryRun: args.Contains("--dry-run"));
    }

    return 0;
}

static Task<int> ResetTestDataAsync()
{
    // TODO(rehearsal-week): delete all cre_timeentry rows owned by the test user
    // via the Dataverse Web API. Projects are untouched.
    Console.WriteLine("reset-test-data: TODO");
    return Task.FromResult(0);
}

static Task<int> CompareAsync(string path1, string path2)
{
    // TODO(rehearsal-week): load both results files, render the side-by-side table
    // described in workshop/labs/lab-05-evaluation/README.md step 5.
    Console.WriteLine($"compare {path1} vs {path2}: TODO");
    return Task.FromResult(0);
}
