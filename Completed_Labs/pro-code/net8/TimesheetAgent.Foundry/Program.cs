using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using TimesheetAgent.Foundry.Agents;

// Config: user-secrets locally, environment variables when deployed to Foundry. Never appsettings —
// no secrets committed, matching the rest of this project's demo hygiene.
var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>(optional: true)
    .AddEnvironmentVariables()
    .Build();

string Require(string key) => config[key]
    ?? throw new InvalidOperationException($"Missing required config value: {key}. Set it via `dotnet user-secrets set \"{key}\" \"...\"`.");

var foundryEndpoint = Require("Foundry:Endpoint");
var foundryDeployment = Require("Foundry:DeploymentName");
var dataverseEnvironmentUrl = Require("Dataverse:EnvironmentUrl");
var dataverseTenantId = Require("Dataverse:TenantId");
var dataverseClientId = Require("Dataverse:ClientId");
var workIqTenantId = config["WorkIq:TenantId"] ?? dataverseTenantId;
var workIqClientId = Require("WorkIq:ClientId");
var promptsDirectory = config["Prompts:Directory"]
    ?? Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "..", "..", "Labs", "prompts");

// Tracing — console exporter for the demo, per architecture.md's "Application Insights for
// production is a slide, not demoed" decision.
using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("TimesheetAgent.Foundry"))
    .AddSource("TimesheetAgent.Foundry")
    .AddConsoleExporter()
    .Build();

// The Foundry-hosted model, reached through the OpenAI-compatible endpoint. Prefer an API key
// (Foundry:ApiKey) when set — confirmed 2026-09-20 that a signed-in user can have Contributor on
// the Foundry resource without the "Cognitive Services OpenAI User" data-plane role, and an ABAC
// condition on a Contributor grant can block self-assigning it via CLI, so DefaultAzureCredential
// (AAD token auth) may not work without an admin's help. API key auth sidesteps that entirely.
// Falls back to DefaultAzureCredential for environments where the RBAC role IS granted, since
// that's still what Microsoft's Agent Framework hosting docs recommend by default (with the
// standard caveat that production hosting should pin a specific credential type).
var foundryApiKey = config["Foundry:ApiKey"];
var azureOpenAIClient = string.IsNullOrEmpty(foundryApiKey)
    ? new AzureOpenAIClient(new Uri(foundryEndpoint), new DefaultAzureCredential())
    : new AzureOpenAIClient(new Uri(foundryEndpoint), new Azure.AzureKeyCredential(foundryApiKey));
var chatClient = azureOpenAIClient.GetChatClient(foundryDeployment);

Console.WriteLine("Building agents...");

var calendarAgent = await CalendarChildAgent.CreateAsync(chatClient, workIqTenantId, workIqClientId, promptsDirectory);
var timeEntryAgent = await TimeEntryChildAgent.CreateAsync(
    chatClient, dataverseEnvironmentUrl, dataverseTenantId, dataverseClientId, promptsDirectory);
var orchestrator = await OrchestratorAgent.CreateAsync(chatClient, calendarAgent, timeEntryAgent, promptsDirectory);

Console.WriteLine("Agents ready.");

// --scenario <name> loads a single scenario from Completed_Labs/eval/scenarios.json and runs it
// end to end against live MCPs and live Dataverse — same smoke-test shape as lab-03's step 4.
var scenarioArgIndex = Array.IndexOf(args, "--scenario");
var scenarioName = scenarioArgIndex >= 0 && scenarioArgIndex + 1 < args.Length ? args[scenarioArgIndex + 1] : null;
var message = scenarioName is not null
    ? ScenarioLoader.LoadPrompt(scenarioName, promptsDirectory)
    : "Log my time for last week.";

var session = await orchestrator.CreateSessionAsync();

// Interactive loop: the orchestrator's own instructions require explicit user confirmation before
// drafting AND before writing (see orchestrator.system.md rules 1 and 4), so a single-shot
// send-one-message-and-exit run can never reach an actual Dataverse write — it always stops at the
// first clarifying question. --scenario seeds the first turn from scenarios.json; every turn after
// that (including the seeded one) is a normal back-and-forth so the conversation can actually
// complete live. Type 'exit' or 'quit' to end.
//
// Two things fixed here after live testing on 2026-09-20 surfaced real confusion: (1) the seeded
// --scenario message was sent silently, so it looked like the orchestrator's first reply had
// "preempted" whatever the user was about to type themselves — now it's echoed explicitly, labelled
// as coming from the scenario file, before it's sent. (2) each orchestrator turn can involve several
// chained model calls (orchestrator reasoning + delegating to one or both specialist agents, each of
// which may call its own MCP tool), which can take a real number of seconds — with nothing printed
// in between, that silence was mistaken for a hang. A "Thinking..." line now brackets every call.
Console.WriteLine();
Console.WriteLine("--- Orchestrator ---");

if (scenarioName is not null)
{
    Console.WriteLine();
    Console.WriteLine($"[Scenario '{scenarioName}'] {message}");
}

var nextMessage = message;
while (!string.IsNullOrWhiteSpace(nextMessage) &&
    !string.Equals(nextMessage, "exit", StringComparison.OrdinalIgnoreCase) &&
    !string.Equals(nextMessage, "quit", StringComparison.OrdinalIgnoreCase))
{
    Console.WriteLine();
    Console.Write("Thinking...");
    var response = await orchestrator.RunAsync(nextMessage, session);
    Console.Write("\r            \r"); // clear the "Thinking..." line before printing the reply
    Console.WriteLine(response.ToString());
    Console.WriteLine();
    Console.Write("You > ");
    nextMessage = Console.ReadLine();
}

static class ScenarioLoader
{
    public static string LoadPrompt(string scenarioName, string promptsDirectory)
    {
        var scenariosPath = Path.Combine(promptsDirectory, "..", "..", "Completed_Labs", "eval", "scenarios.json");
        if (!File.Exists(scenariosPath))
            throw new FileNotFoundException($"scenarios.json not found at {scenariosPath}. Check --scenario or Prompts:Directory config.");

        // Root is { "scenarios": [...] }, not a bare array (confirmed 2026-09-20 against the real
        // file — the eval harness's fixture-driven format, keyed by "id" with the prompt text under
        // "user_turn_1", not the flatter {name, prompt} shape this loader originally assumed). Each
        // scenario also carries "fixture" (mock calendar_events / pre_existing_entries) and
        // "expected_writes" for lab 05's graders — this smoke-test loader intentionally ignores
        // those and just extracts the prompt text to run against whatever is live in Work IQ and
        // Dataverse right now, not the fixture's mock data.
        using var doc = System.Text.Json.JsonDocument.Parse(File.ReadAllText(scenariosPath));
        foreach (var scenario in doc.RootElement.GetProperty("scenarios").EnumerateArray())
        {
            if (scenario.TryGetProperty("id", out var id) && id.GetString() == scenarioName)
                return scenario.GetProperty("user_turn_1").GetString()
                    ?? throw new InvalidOperationException($"Scenario '{scenarioName}' has no 'user_turn_1' field.");
        }
        throw new ArgumentException($"No scenario with id '{scenarioName}' found in {scenariosPath}.");
    }
}
