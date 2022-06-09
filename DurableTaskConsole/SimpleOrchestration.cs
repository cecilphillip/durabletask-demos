namespace DurableTaskConsole;

public class SimpleOrchestration : TaskOrchestration<string, string>
{
    public override async Task<string> RunTask(OrchestrationContext context, string input)
    {
        var echoResult = await context.ScheduleTask<string>(typeof(EchoTask), input);
        return $"Orchestration {nameof(SimpleOrchestration)}-{context.OrchestrationInstance.InstanceId} {echoResult}";
    }
}

public class EchoTask : AsyncTaskActivity<string, string>
{
    protected override async Task<string> ExecuteAsync(TaskContext context, string input)
    {
        // do some work
        await Task.Delay(TimeSpan.FromSeconds(2));
        return input;
    }
}