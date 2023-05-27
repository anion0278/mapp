namespace Shmap.UI.ViewModels;

public abstract class TabViewModelBase : ViewModelBase
{
    /// <summary>
    /// Refreshed each time the Culture is changed
    /// </summary>
    public virtual string Title { get; } 

}