using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using GalaSoft.MvvmLight;
using Shmap.BusinessLogic.Invoices.Annotations;
using Shmap.UI.ViewModels;

namespace Shmap.ViewModels
{
    public class ViewModelWithErrorValidationBase : ViewModelBase, IDataErrorInfo
    {
        private readonly Dictionary<string, ViewModelValidationRule> _ruleMap = new();

        public string Error
        {
            get
            {
                var errors = _ruleMap.Values.Where(r => r.HasError).Select(r => r.Error);
                return string.Join("\n", errors);
            }
        }

        public string this[string columnName]
        {
            get
            {
                if (_ruleMap.ContainsKey(columnName)) 
                {
                    _ruleMap[columnName].Revalidate();
                    return _ruleMap[columnName].Error;
                }

                return null;
            }
        }

        public bool HasErrors
        {
            get
            {
                var values = _ruleMap.Values.ToList();
                values.ForEach(b => b.Revalidate());
                return values.Any(b => b.HasError);
            }
        }

        protected void AddValidationRule<T>(Expression<Func<T>> expression, Func<bool> ruleDelegate,
            string errorMessage)
        {
            var name = GetPropertyName(expression);

            _ruleMap.Add(name, new ViewModelValidationRule(ruleDelegate, errorMessage));
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

        public override void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.RaisePropertyChanged(propertyName);
            if (propertyName != null && _ruleMap.ContainsKey(propertyName)) 
            {
                _ruleMap[propertyName].IsDirty = true;
            }
        }

    }
}