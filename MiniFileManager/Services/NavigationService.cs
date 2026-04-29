namespace MiniFileManager.Services;

public class NavigationService : INavigationService
{
    private readonly Stack<string> _backStack = new();
    private readonly Stack<string> _forwardStack = new();
    private string _currentPath = string.Empty;

    public string CurrentPath => _currentPath;
    public event EventHandler<string> PathChanged = delegate { };

    public void NavigateTo(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        string normalizedPath;
        try
        {
            normalizedPath = Path.GetFullPath(path.Trim());
        }
        catch
        {
            return;
        }

        if (!Directory.Exists(normalizedPath) ||
            string.Equals(_currentPath, normalizedPath, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (!string.IsNullOrEmpty(_currentPath))
        {
            _backStack.Push(_currentPath);
        }

        _forwardStack.Clear();
        _currentPath = normalizedPath;
        PathChanged(this, _currentPath);
    }

    public void NavigateBack()
    {
        if (_backStack.Count == 0)
        {
            return;
        }

        try
        {
            var targetPath = _backStack.Pop();
            if (!Directory.Exists(targetPath))
            {
                return;
            }

            if (!string.IsNullOrEmpty(_currentPath))
            {
                _forwardStack.Push(_currentPath);
            }

            _currentPath = targetPath;
            PathChanged(this, _currentPath);
        }
        catch
        {
        }
    }

    public void NavigateForward()
    {
        if (_forwardStack.Count == 0)
        {
            return;
        }

        try
        {
            var targetPath = _forwardStack.Pop();
            if (!Directory.Exists(targetPath))
            {
                return;
            }

            if (!string.IsNullOrEmpty(_currentPath))
            {
                _backStack.Push(_currentPath);
            }

            _currentPath = targetPath;
            PathChanged(this, _currentPath);
        }
        catch
        {
        }
    }

    public void NavigateUp()
    {
        if (string.IsNullOrWhiteSpace(_currentPath))
        {
            return;
        }

        try
        {
            var parent = Directory.GetParent(_currentPath);
            if (parent != null)
            {
                NavigateTo(parent.FullName);
            }
        }
        catch
        {
        }
    }
}
