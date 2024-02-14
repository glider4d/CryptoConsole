using CryptoConsoleGUI.Core;

namespace CryptoConsoleGUI.MVVM.ViewModel
{
    class MainViewModel : ObservableObject
    {
        public RelayCommand HomeViewCommand { get; set; }
        public RelayCommand EncryptViewCommand { get; set; }
        public RelayCommand DecryptViewCommand { get; set; }

        public HomeViewModel HomeVm { get; set; }
        public EncryptViewModel EncryptVm { get; set; }
        public DecryptViewModel DecryptVm { get; set; }





        private object currentView;
        public object CurrentView
        {
            get { return currentView; }
            set { currentView = value; OnPropertyChanged(); }
        }
        public MainViewModel()
        {
            HomeVm = new HomeViewModel();
            EncryptVm = new EncryptViewModel();
            DecryptVm = new DecryptViewModel();
            CurrentView = EncryptVm;


            HomeViewCommand = new RelayCommand(o =>
            {
                CurrentView = HomeVm;
            });


            EncryptViewCommand = new RelayCommand(o =>
            {
                CurrentView = EncryptVm;
            });

            DecryptViewCommand = new RelayCommand(o =>
            {
                CurrentView = DecryptVm;
            });
         }
    }
}
