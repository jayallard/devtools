using System.Runtime.InteropServices;

namespace DevTools.Core;

public class TimeBuffer
{
    // TODO: see about an rx solution to replace this

    private readonly TimeSpan _idleTime;
    private readonly Action _action;

    private readonly object _sync = new();
    private bool _started;

    /// <summary>
    /// Returns the number of notifications that has occurred since the last time the
    /// action was invoked.
    /// </summary>
    public long NotificationCountSinceLastExecute { get; private set; }


    /// <summary>
    /// Gets the last time the action method was invoked.
    /// </summary>
    public DateTime LastInvoke { get; private set; }

    /// <summary>
    /// Gets the last time the NotifyActivity method was executed.
    /// </summary>
    public DateTime LastNotification { get; private set; }


    /// <summary>
    /// Initializes a new instance of the TimeBuffer class.
    /// </summary>
    /// <param name="idleTime">The amount of time that must elapse without activity in order to invoke the action.</param>
    /// <param name="action">The action to invoke once idleTime elapses with no new activity.</param>
    public TimeBuffer(TimeSpan idleTime, Action action)
    {
        _idleTime = idleTime;
        _action = action;
    }

    /// <summary>
    /// Executes the action method when some time has elapsed since the last activity.
    /// IE: it has been 5 seconds since any activity occurred, so do something.
    /// The timer resets each time NotifyActivity is called.
    /// The action is only invoked when there has been activity since the last time
    /// it was invoked.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public void Run(CancellationToken cancellationToken)
    {
        lock (_sync)
        {
            if (_started) throw new InvalidOperationException("The buffer is already running");
            _started = true;
        }

        Task.Run(ReallyRun, cancellationToken);

        async Task ReallyRun()
        {
            // every 500 seconds, check the state of things.
            using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(500));
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                lock (_sync)
                {
                    if (NotificationCountSinceLastExecute == 0)
                    {
                        // nothing to do
                        continue;
                    }

                    // if x time hasn't passed since the last activity,
                    // then wait longer.
                    var diff = DateTime.Now.Subtract(LastNotification);
                    if (diff <= _idleTime) continue;

                    Console.WriteLine("No activity for at least " + _idleTime.TotalMilliseconds +
                                      "ms, so firing. ActivityCount=" + NotificationCountSinceLastExecute);
                    _action();
                    LastInvoke = DateTime.Now;
                    Console.WriteLine("TimeBuffer: Complete");
                    NotificationCountSinceLastExecute = 0;
                }
            }
        }
    }

    /// <summary>
    /// Indicate that activity has occurred.
    /// The action won't invoke until idleTime has elapsed since the last time
    /// this method was called.
    /// </summary>
    public void NotifyActivity()
    {
        lock (_sync)
        {
            if (NotificationCountSinceLastExecute == 0)
            {
                Console.WriteLine("TimeBuffer: Changes started. The action will be invoked " +
                                  _idleTime.TotalMilliseconds + "ms after the changes complete.");
            }

            NotificationCountSinceLastExecute++;
            LastNotification = DateTime.Now;
        }
    }
}