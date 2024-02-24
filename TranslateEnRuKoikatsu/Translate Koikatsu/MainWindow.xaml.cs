using MahApps.Metro.Controls.Dialogs;
using TranslateEnRuKoikatsu;

namespace Translate_Koikatsu
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow 
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel(DialogCoordinator.Instance);
        }
    }
}