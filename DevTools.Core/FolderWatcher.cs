namespace DevTools.Core;

public class FolderWatcher : IDisposable
{
    private readonly string _fileNamePattern;
    private readonly string _folder;
    private readonly Action _action;
    private FileSystemWatcher? _watcher;
    private bool _isStarted;
    private readonly object _startLock = new();

    public FolderWatcher(string folder, string fileNamePattern, Action action)
    {
        _folder = folder;
        _action = action;
        _fileNamePattern = fileNamePattern;
    }

    public void Run()
    {
        lock (_startLock)
        {
            if (_isStarted) throw new InvalidOperationException("The watcher is already started");
            _isStarted = true;
        }
        
        Console.WriteLine("watcher started");
        _watcher = new FileSystemWatcher(_folder, _fileNamePattern);
        _watcher.Created += (s, e) =>
        {
            _action();
        };

        _watcher.NotifyFilter = NotifyFilters.FileName;
        _watcher.EnableRaisingEvents = true;
    }

    public void Dispose()
    {
        Console.WriteLine("disposed");
        _watcher?.Dispose();
    }
}