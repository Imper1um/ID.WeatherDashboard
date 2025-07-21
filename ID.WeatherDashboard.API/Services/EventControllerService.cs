using ID.WeatherDashboard.API.Events;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace ID.WeatherDashboard.API.Services
{
    public class EventControllerService(ILogger<EventControllerService> logger) : IEventControllerService, IDisposable
    {
        private readonly ILogger<EventControllerService> Log = logger;

        private readonly ConcurrentDictionary<Guid, EventAction> Actions = [];
        private CancellationTokenSource? _cts;
        private Task? _eventLoopTask;
        private readonly object _startLock = new();

        public void AddEvent(EventAction action)
        {
            Actions.TryAdd(Guid.NewGuid(), action);
        }

        public void End()
        {
            lock (_startLock)
            {
                _cts?.Cancel();
                _cts = null;
                _eventLoopTask = null;
            }
        }

        public Task Start()
        {
            lock (_startLock)
            {
                if (_cts != null) return Task.CompletedTask;

                _cts = new CancellationTokenSource();
                _eventLoopTask = Task.Run(() => EventLoopAsync(_cts.Token));
            }

            return Task.CompletedTask;
        }

        private void RemoveAction(Guid id)
        {
            Actions.TryRemove(id, out _);
        }

        private async Task EventLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                foreach (var pair in Actions.ToArray())
                {
                    var id = pair.Key;
                    var action = pair.Value;

                    if (action.IsTriggered)
                        continue;
                    try
                    {
                        var trigger = action.Triggers.FirstOrDefault(t => t.IsTimeToTrigger);
                        if (trigger != null)
                        {
                            action.Execution(action, trigger);
                            action.IsTriggered = true;
                            RemoveAction(id);
                        }
                    } 
                    catch (Exception ex)
                    {
                        Log.LogError(ex, "Error while trying to trigger an event: {Exception}", ex.GetFullMessage());
                        action.IsTriggered = true;
                        RemoveAction(id);
                    }
                }
                await Task.Delay(1000, token);
            }
        }

        public void Dispose()
        {
            End();
        }
    }
}
