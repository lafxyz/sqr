using Serilog;

namespace SQR.Workers;

public class BackgroundTask
{
    private readonly PeriodicTimer _timer;

    private CancellationTokenSource _cts = new();
    private Task? _task;

    public BackgroundTask(TimeSpan interval)
    {
        _timer = new PeriodicTimer(interval);
    }

    public void AssignAndStartTask(Func<Task> action)
    {
        if (_task is not null)
        {
            Log.Logger.Warning("Attempt to assign and start new task when task is already assigned.");
            return;
        }
        
        _cts = new CancellationTokenSource();
        _task = RepeatTask(action);
    }

    private async Task RepeatTask(Func<Task> action)
    {
        try
        {
            while (await _timer.WaitForNextTickAsync(_cts.Token))
            {
                await action.Invoke();
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Cancelled");
        }
    }
    
    public async Task StopAsync()
    {
        if (_task is null)
        {
            Log.Logger.Warning("Attempt to stop a task that does not exist.");
            return;
        }

        try
        {
            _cts.Cancel();
        }
        catch (ObjectDisposedException)
        {
            Log.Logger.Warning("Attempt to stop a task that already stopped.");
        }
        await _task;
        _task = null;
        _cts.Dispose();
    }
}