using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using GalaSoft.MvvmLight;

namespace Shmap.ViewModels
{
    public class ViewModelWithErrorValidationBase : ViewModelBase, IDataErrorInfo
    {
        private readonly Dictionary<string, ValidationRule> _ruleMap = new();

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

            _ruleMap.Add(name, new ValidationRule(ruleDelegate, errorMessage));
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