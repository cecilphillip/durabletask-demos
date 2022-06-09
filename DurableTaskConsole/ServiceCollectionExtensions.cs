using DurableTask.AzureStorage;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DurableTaskConsole;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDurableTaskHubWorker(this IServiceCollection services, string hubName,
        string connectionString, Type[] taskOrchestrationTypes, Type[] taskActivityTypes)
    {
        var azureStorageOrchestrationService = new AzureStorageOrchestrationService(new()
        {
            StorageConnectionString = connectionString,
            TaskHubName = hubName
        });
        services.TryAddSingleton(typeof(IOrchestrationServiceClient), sp => azureStorageOrchestrationService);
        services.TryAddSingleton(typeof(IOrchestrationService), sp => azureStorageOrchestrationService);

        services.AddSingleton<TaskHubWorker>(sp =>
            new TaskHubWorker(sp.GetRequiredService<IOrchestrationService>())
                .AddTaskOrchestrations(taskOrchestrationTypes)
                .AddTaskActivities(taskActivityTypes)
        );
        return services;
    }

    public static IServiceCollection AddTaskHubClient(this IServiceCollection services, string hubName,
        string connectionString)
    {
        var azureStorageOrchestrationService = new AzureStorageOrchestrationService(new()
        {
            StorageConnectionString = connectionString,
            TaskHubName = hubName
        });
        services.TryAddSingleton(typeof(IOrchestrationServiceClient), sp => azureStorageOrchestrationService);
        services.AddSingleton<TaskHubClient>(sp => new TaskHubClient(sp.GetRequiredService<IOrchestrationServiceClient>()));
        return services;
    }
}