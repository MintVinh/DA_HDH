namespace MiniFileManager.Services;

public interface INavigationService
{
    string CurrentPath { get; }
    void NavigateTo(string path);
    void NavigateBack();
    void NavigateForward();
    void NavigateUp();
    event EventHandler<string> PathChanged;
}
