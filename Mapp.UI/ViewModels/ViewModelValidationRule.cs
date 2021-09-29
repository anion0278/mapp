using System;

namespace Shmap.UI.ViewModels
{
    internal class ViewModelValidationRule
    {
        public string Error { get; private set; }
        public bool HasError { get; private set; }
        public bool IsDirty { get; set; }
        private readonly Func<bool> _ruleDelegate;
        private readonly string _errorMessage;

        public ViewModelValidationRule(Func<bool> ruleDelegate, string errorMessage)
        {
            _ruleDelegate = ruleDelegate;
            _errorMessage = errorMessage;
            IsDirty = true;
        }

        public void Revalidate()
        {
            if (!IsDirty)
                return;

            Error = null;
            HasError = false;
            try
            {
                if (!_ruleDelegate())
                {
                    Error = _errorMessage;
                    HasError = true;
                }
            }
            catch (Exception e)
            {
                Error = e.Message;
                HasError = true;
            }
        }
    }
}