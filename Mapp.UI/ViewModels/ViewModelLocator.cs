using Shmap.Models;
using Unity;

namespace Shmap.ViewModels
{
    public class ViewModelLocator // View-first approach
    {
        private static readonly Bootstrapper _bootStrapper;

        static ViewModelLocator()
        {
            _bootStrapper = new Bootstrapper();
        }

        public IMainWindowViewModel MainWindowVm => _bootStrapper.Container.Resolve<IMainWindowViewModel>();
        public IManualChangeWindowViewModel ManualChangeWindowVm => _bootStrapper.Container.Resolve<IManualChangeWindowViewModel>();
    }
}