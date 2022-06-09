using DurableTask.ServiceBus;
using DurableTask.ServiceBus.Settings;
using DurableTask.ServiceBus.Tracking;
using DurableTaskConsole;

var host = CreateHostBuilder(args).Build();
await host.StartAsync();

const string sbConnectionString = "";
const string storageConnectionString = "";
const string demoHubName = "DemoHubName";

IOrchestrationServiceInstanceStore instanceStore = new AzureTableInstanceStore(demoHubName, storageConnectionString);
var blobStore = new AzureStorageBlobStore(demoHubName, storageConnectionString);
ServiceBusOrchestrationServiceSettings orchestrationServiceSettings = null!;

/*
 * Cannot use basic tier service bus instance
 */
var serviceBusOrchestrationServiceAndClient = new ServiceBusOrchestrationService(sbConnectionString, demoHubName, instanceStore,
    blobStore, orchestrationServiceSettings);

var taskHubWorker = new TaskHubWorker(serviceBusOrchestrationServiceAndClient);
taskHubWorker.AddTaskOrchestrations(typeof(SimpleOrchestration));
taskHubWorker.AddTaskActivities(typeof(EchoTask));

await serviceBusOrchestrationServiceAndClient.CreateIfNotExistsAsync();
await taskHubWorker.StartAsync(); //starts worker. does not block while running

// Create the client
var taskHubClient = new TaskHubClient(serviceBusOrchestrationServiceAndClient);

var instanceId = Guid.NewGuid().ToString();
const string data = "Hello World";

var instance =
    await taskHubClient.CreateOrchestrationInstanceAsync(typeof(SimpleOrchestration), instanceId, data);

const int timeoutSeconds = 30;
Console.WriteLine($"Waiting up to {timeoutSeconds} seconds for orchestration to complete.");
OrchestrationState taskResult = await taskHubClient.WaitForOrchestrationAsync(instance, TimeSpan.FromSeconds(timeoutSeconds), CancellationToken.None);

Console.WriteLine($"Task done: {taskResult?.OrchestrationStatus}");
Console.WriteLine($"Task Result: {taskResult?.Output}");

Console.WriteLine("Press any key to quit.");
Console.ReadLine();

await taskHubWorker.StopAsync(true);
await host.StopAsync();

static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .UseConsoleLifetime();