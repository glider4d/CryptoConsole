using CryptoConsoleGUI.Core;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace CryptoConsoleGUI.MVVM.ViewModel
{
    class DecryptViewModel : ObservableObject
    {
        private BackgroundWorker bgWorker_ = new BackgroundWorker();
        private int percent_;
        public int Percent
        {
            get { return percent_; }
            set
            {
                percent_ = value;
                OnPropertyChanged();
            }
        }




        private int workerState_;
        public int WorkerState
        {
            get { return workerState_; }
            set
            {
                workerState_ = value;
                OnPropertyChanged();
            }
        }


        private string srcPathString_;
        private string dstPathString_;
        public string SrcPathString { get => srcPathString_; set { srcPathString_ = value; OnPropertyChanged(); } }
        public string DstPathString { get => dstPathString_; set { dstPathString_ = value; OnPropertyChanged(); } }

        public RelayCommand DecryptCommand { get; set; }
        public RelayCommand SrcPath { get; set; }
        public RelayCommand TrgPath { get; set; }

        Progress<int> progress;
        void ProgressDisplay(int update)
        {
            WorkerState = update;
            Percent = update * 3;
        }

        public DecryptViewModel()
        {
            progress = new Progress<int>(ProgressDisplay);
            DecryptCommand = new RelayCommand(o =>
            {
                bgWorker_.RunWorkerAsync();
            });


            SrcPath = new RelayCommand(o =>
            {
                var dialog = new Microsoft.Win32.OpenFileDialog();
                dialog.FileName = SrcPathString;
     
                dialog.DefaultExt = ".extra"; // Default file extension
                dialog.Filter = "extra (*.extra)|*.extra|All files (*.*)|*.*"; // Filter files by extension

                // Show open file dialog box
                bool? result = dialog.ShowDialog();

                // Process open file dialog box results
                if (result == true)
                {
                    SrcPathString = dialog.FileName;
                    string extension = Path.GetExtension(SrcPathString);
                    DstPathString = SrcPathString.Replace(extension, string.Empty);
                }

            });



            bgWorker_.DoWork += (s, e) =>
            {
                string programKey = CryptoLib.CryptoMethods.RandomString(8);
                if (!string.IsNullOrEmpty(programKey))
                    Task.Run(async () => await CryptoLib.CryptoMethods.DecryptFilePagedFromOneFilesWithCertAsync(progress, SrcPathString, DstPathString));
                return;
            };

        }
    }
}
