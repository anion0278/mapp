using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using WPFLocalizeExtension.Engine;

namespace Mapp.UI.Views.Extensions;


public class LocBindingExtension : MarkupExtension 
{
    private readonly Binding _inner;// composition, since ProvideValue of Binding is sealed
    public LocBindingExtension()
    {
        _inner = new Binding();
    }

    public LocBindingExtension(PropertyPath path)
    {
        _inner = new Binding();
        this.Path = path;
    }

    public IValueConverter Converter
    {
        get => _inner.Converter;
        set => _inner.Converter = value;
    }

    public PropertyPath Path
    {
        get => _inner.Path;
        set => _inner.Path = value;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var expression = _inner.ProvideValue(serviceProvider) as BindingExpressionBase;
        if (expression == null) return this;

        var wr = new WeakReference<BindingExpressionBase>(expression); // avoid leaking
        PropertyChangedEventHandler handler = null;
        handler = (_, e) =>
        {
            if (e.PropertyName != nameof(LocalizeDictionary.Instance.Culture)) return;

            // when culture changed and our binding expression is still alive - update target
            if (wr.TryGetTarget(out var target))
            {
                target.UpdateTarget();
            }
            else
            {
                // if dead - unsubsribe
                LocalizeDictionary.Instance.PropertyChanged -= handler;
            }
        };
        LocalizeDictionary.Instance.PropertyChanged += handler;
        return expression; // no binding target
    }
}
