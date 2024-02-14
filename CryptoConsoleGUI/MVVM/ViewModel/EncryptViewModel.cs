using CryptoConsoleGUI.Core;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace CryptoConsoleGUI.MVVM.ViewModel
{
    class EncryptViewModel : ObservableObject
    {
        private BackgroundWorker bgWorker_ = new BackgroundWorker();
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

        private string srcPathString_;
        private string dstPathString_;
        public string SrcPathString { get => srcPathString_; set { srcPathString_ = value; OnPropertyChanged(); } }
        public string DstPathString { get => dstPathString_; set { dstPathString_ = value; OnPropertyChanged(); } }

        public RelayCommand EncryptCommand { get; set; }
        public RelayCommand SrcPath { get; set; }
        public RelayCommand TrgPath { get; set; }

        Progress<int> progress;
        void ProgressDisplay(int update)
        {
            //            Console.WriteLine($"update = {update}");
            WorkerState = update;
            Percent = update * 3;
        }

        public EncryptViewModel()
        {
            progress = new Progress<int>(ProgressDisplay);
            EncryptCommand = new RelayCommand(o =>
            {
                bgWorker_.RunWorkerAsync();
            });


            SrcPath = new RelayCommand(o =>
            {
                var dialog = new Microsoft.Win32.OpenFileDialog();
                dialog.FileName = SrcPathString;
                dialog.DefaultExt = ".avi"; // Default file extension
                dialog.Filter = "avi (*.avi)|*.avi|mov (*.mov)|*.mov|mov (*.wmv)|*.wmv|mp4 (*.mp4)|*.mov|mkv (*.mkv)|*.mkv|All files (*.*)|*.*"; // Filter files by extension


                // Show open file dialog box
                bool? result = dialog.ShowDialog();

                // Process open file dialog box results
                if (result == true)
                {
                    // Open document
                    SrcPathString = dialog.FileName;
                    DstPathString = SrcPathString + ".extra";
                }

            });

            TrgPath = new RelayCommand(o =>
            {

                /*
                var dialog = new Microsoft.Win32.SaveFileDialog();
                bool? result = dialog.ShowDialog();
                dialog.Filter = "extra (*.extra)|";
                if (result == true)
                {
                    DstPathString = dialog.FileName;
                }*/
            });

            
            bgWorker_.DoWork += (s, e) =>
            {
                
                string programKey = CryptoLib.CryptoMethods.RandomString(8);
                if(!string.IsNullOrEmpty(programKey)) {
                    Task.Run(async () => await CryptoLib.CryptoMethods.EncryptFilePagedFromOneFilesWithCertAsync(progress, SrcPathString, DstPathString, programKey));
                }
                return;
             };
            
        } 
    }
}
