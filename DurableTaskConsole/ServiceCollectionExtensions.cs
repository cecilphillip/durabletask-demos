using DurableTask.AzureStorage;
using DurableTask.Core.Common;
using DurableTask.Core.Settings;
using DurableTask.ServiceBus;
using DurableTask.ServiceBus.Settings;
using DurableTask.ServiceBus.Tracking;
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
    
    public static IServiceCollection AddDurableServiceBusTaskHubWorker(this IServiceCollection services, string hubName,
        string sbConnectionString, string storageConnectionString, Type[] taskOrchestrationTypes, Type[] taskActivityTypes)
    {
        var serviceBusOrchestrationService = new ServiceBusOrchestrationService(
            sbConnectionString,
            hubName,
            new AzureTableInstanceStore(hubName, storageConnectionString),
            new AzureStorageBlobStore(hubName, storageConnectionString),
            new ServiceBusOrchestrationServiceSettings
            {
                MessageCompressionSettings = new CompressionSettings
                {
                    Style = CompressionStyle.Threshold,
                    ThresholdInBytes = 1024
                }
            });
        services.TryAddSingleton(typeof(IOrchestrationServiceClient), sp => serviceBusOrchestrationService);
        services.TryAddSingleton(typeof(IOrchestrationService), sp => serviceBusOrchestrationService);

        services.AddSingleton<TaskHubWorker>(sp =>
            new TaskHubWorker(sp.GetRequiredService<IOrchestrationService>())
                .AddTaskOrchestrations(taskOrchestrationTypes)
                .AddTaskActivities(taskActivityTypes)
        );
        return services;
    }
    
    public static IServiceCollection AddTaskHubClientSb(this IServiceCollection services, string hubName,
        string sbConnectionString, string storageConnectionString)
    {
        var serviceBusOrchestrationService = new ServiceBusOrchestrationService(
            sbConnectionString,
            hubName,
            new AzureTableInstanceStore(hubName, storageConnectionString),
            new AzureStorageBlobStore(hubName, storageConnectionString),
            new ServiceBusOrchestrationServiceSettings
            {
                MessageCompressionSettings = new CompressionSettings
                {
                    Style = CompressionStyle.Threshold,
                    ThresholdInBytes = 1024
                }
            });
        services.TryAddSingleton(typeof(IOrchestrationServiceClient), sp => serviceBusOrchestrationService);
        services.AddSingleton<TaskHubClient>(sp => new TaskHubClient(sp.GetRequiredService<IOrchestrationServiceClient>()));
        return services;
    }
}