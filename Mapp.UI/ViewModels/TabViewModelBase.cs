namespace Shmap.UI.ViewModels;

public abstract class TabViewModelBase : ViewModelBase
{
    public virtual string Title { get; protected set; }

    protected TabViewModelBase(string title)
    {
        Title = title;
    }
}