using System.Windows;


namespace CryptoConsoleGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private FrameworkElement _title;

        public MainWindow()
        {
            InitializeComponent(); 
        }

        private void this_Loaded(object sender, RoutedEventArgs e)
        {
 

            var res = (FrameworkElement)this.Template.FindName("PART_Title", this);
            if (null != this._title)
            {
            }
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
