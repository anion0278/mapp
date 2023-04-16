using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;

namespace Shmap.UI.ViewModels;

public class ViewModelBase : ObservableValidator
{
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        var value = GetType().GetProperty(e.PropertyName!)!.GetValue(this);
        ValidateProperty(value, e.PropertyName);
        base.OnPropertyChanged(e);
    }


    /// <summary>
    /// Returns actually new CV instead of default one - allows to separate filtering for different representations
    /// </summary>
    /// <typeparam name="T">Type of collection element</typeparam>
    /// <param name="collection">Target collection</param>
    /// <returns>New instance of Collection View of the target collection</returns>
    protected static ICollectionView GetNewCollectionViewInstance<T>(IEnumerable<T> collection)
    {
        return new CollectionViewSource { Source = collection }.View;
    }
}