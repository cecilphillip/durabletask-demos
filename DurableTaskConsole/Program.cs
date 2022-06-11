using DurableTaskConsole;
using DurableTaskConsole.Orchestrations;

var host = CreateHostBuilder(args).Build();
await host.StartAsync();

// Create the client
var taskHubClient = host.Services.GetRequiredService<TaskHubClient>();
var instanceId = Guid.NewGuid().ToString();
const string data = "Hello World";

var instance =
    await taskHubClient.CreateOrchestrationInstanceAsync(typeof(SimpleOrchestration), instanceId, data);

const int timeoutSeconds = 30;
Console.WriteLine($"Waiting up to {timeoutSeconds} seconds for orchestration to complete.");
var taskResult =
    await taskHubClient.WaitForOrchestrationAsync(instance, TimeSpan.FromSeconds(timeoutSeconds),
        CancellationToken.None);

Console.WriteLine($"Task done: {taskResult?.OrchestrationStatus}");
Console.WriteLine($"Task Result: {taskResult?.Output}");

await host.StopAsync();

static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args).ConfigureServices((hostCtx, services) =>
    {
        const string demoHubName = "DemoHubName";

        var storageConnectionString = hostCtx.Configuration["DurableTask:AzureStorageConnection"];
        var sbConnectionString = hostCtx.Configuration["DurableTask:ServiceBusConnection"];
           
        /*services.AddDurableTaskHubWorker(demoHubName, storageConnectionString, new[] {typeof(SimpleOrchestration)},
            new[] {typeof(EchoTask)});*/
        
        services.AddDurableServiceBusTaskHubWorker(demoHubName, sbConnectionString, storageConnectionString, new[] {typeof(SimpleOrchestration)},
            new[] {typeof(EchoTask)});

        // services.AddTaskHubClient(demoHubName, storageConnectionString);
        services.AddTaskHubClientSb(demoHubName, sbConnectionString, storageConnectionString);
        services.AddHostedService<TaskHubBackgroundWorker>();
    });