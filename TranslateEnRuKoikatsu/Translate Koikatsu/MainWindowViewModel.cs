using MahApps.Metro.Controls.Dialogs;

namespace TranslateEnRuKoikatsu
{
    public class MainWindowViewModel : ViewModelBase
    {
        private IDialogCoordinator DialogCoordinator;

        private ViewModelBase _currentViewModel;
        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set => SetField(ref _currentViewModel, value);
        }
       
        public MainWindowViewModel(IDialogCoordinator coordinator)
        {
            DialogCoordinator = coordinator;
            CurrentViewModel = new DictionaryViewModel(DialogCoordinator);
        }
    }
}
