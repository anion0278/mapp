namespace Shmap.UI.ViewModels;

public abstract class TabViewModelBase : ViewModelBase
{
    public string Title { get; protected set; }

    public TabViewModelBase(string title)
    {
        Title = title;
    }
}