using System.ComponentModel.DataAnnotations;

namespace SQR.Workers;

public class BackgroundTask
{
    private PeriodicTimer _timer;

    private readonly CancellationTokenSource _cts = new();
    private Task? _task;

    public BackgroundTask(TimeSpan interval)
    {
        _timer = new PeriodicTimer(interval);
    }

    public void Start(Action action)
    {
        _task = Do(action);
    }

    private async Task Do(Action action)
    {
        try
        {
            while (await _timer.WaitForNextTickAsync(_cts.Token))
            {
                action.Invoke();
            }
        }
        catch (OperationCanceledException) { }
    }

    public async Task StopAsync()
    {
        if (_task is null)
        {
            return;
        }
        
        _cts.Cancel();
        await _task;
        _cts.Dispose();
    }
}