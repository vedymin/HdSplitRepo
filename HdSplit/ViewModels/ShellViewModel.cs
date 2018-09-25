using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using HdSplit.Models;
using OperationCanceledException = System.OperationCanceledException;

namespace HdSplit.ViewModels {
    internal class ShellViewModel : Screen
    {
        private Thread NotifyAboutIncorrectHdAsyncThread = null;
        private HdModel _originalHd = new HdModel (false);
        private HdModel _countedHd = new HdModel (false);
        private string _hdNumber;
        private States _scanningState;
        private IpgModel _selectedIpg;
        private string _scannedBarcode;
        private string _informationText;
        private string _previousInformationText;

        public string HdNumber
        {
            get { return _hdNumber; }
            set {
                _hdNumber = OriginalHd.HdNumber;
                NotifyOfPropertyChange (() => OriginalHd.HdNumber);
            }
        }

        public HdModel OriginalHd
        {
            get { return _originalHd; }
            set
            {
                _originalHd = value;
                NotifyOfPropertyChange(() => OriginalHd);
                NotifyOfPropertyChange (() => OriginalHd.HdNumber);
            }
        }

        public HdModel CountedHd {
            get { return _countedHd; }
            set {
                _countedHd = value;
                NotifyOfPropertyChange (() => CountedHd);
            }
        }

        public ReflexConnectionModel ReflexConnection = new ReflexConnectionModel();

        

        public ShellViewModel()
        {
            //var OriginalHd = new HdModel (true);
            //var CountedHd = new HdModel(false);
            //var ReflexConnection = new ReflexConnectionModel();
            //CopyOriginalHdToCountedHd ();
            InformationText = "Scan HD to start splitting";
            ErrorLabelShowRunning = false;
            
        }

        public States ScanningState
        {
            get { return _scanningState; }
            set { _scanningState = value; }
        }

        public IpgModel SelectedIpg
        {
            get { return _selectedIpg; }
            set
            {
                _selectedIpg = value;
                NotifyOfPropertyChange(() => SelectedIpg);
            }
        }

        public string ScannedBarcode
        {
            get { return _scannedBarcode; }
            set
            {
                _scannedBarcode = value;
                NotifyOfPropertyChange(() => ScannedBarcode);
            }
        }

        public string InformationText {
            get { return _informationText; }
            set {
                _informationText = value;
                NotifyOfPropertyChange (() => InformationText);
            }
        }

        public string PreviousInformationText {
            get { return _previousInformationText; }
            set {
                _previousInformationText = value;
                NotifyOfPropertyChange (() => PreviousInformationText);
            }
        }

        public bool ErrorLabelShowRunning { get; set; }

        public async Task ScanItemAsync(KeyEventArgs keyArgs) 
        {
            if (keyArgs == null || keyArgs.Key == Key.Enter) {
                //Download HD content and load it into OriginalHD
                //Copy OriginalHD to CountedHD:
                if (ScanningState == States.firstScanOfHd)
                {
                    if (!CountedHd.CheckIfHdNumberIsCorrect (ScannedBarcode)) {
                        if (!ErrorLabelShowRunning) 
                        {
                            PreviousInformationText = InformationText;
                        }
                        NotifyAboutIncorrectHdAsync ();
                        return;
                    }
                    ScanHd();
                    await ReflexConnection.downloadUpcForItemsAsync (CountedHd);
                    CountedHd.ListOfIpgs.Refresh ();

                }
                else if (ScanningState == States.itemScan)
                {


                }
                //Tutaj jest kod który usuwa IPG z hd jeśli ilość dotarła do 0.                    
                //SelectedIpg.Quantity--;
                //if (SelectedIpg.Quantity == 0) {
                //    OriginalHd.ListOfIpgs.RemoveAt (OriginalHd.ListOfIpgs.IndexOf (SelectedIpg));
                //}
                //OriginalHd.ListOfIpgs.Refresh ();

            }
        }

        private async Task NotifyAboutIncorrectHdAsync()
        {
            ErrorLabelShowRunning = true;
            if (NotifyAboutIncorrectHdAsyncThread != null)
            {
                NotifyAboutIncorrectHdAsyncThread.Abort ();
                NotifyAboutIncorrectHdAsyncThread = null;
            }
            
            InformationText = "This is not correct HD";
            await Task.Run(() =>
            {

                NotifyAboutIncorrectHdAsyncThread = Thread.CurrentThread;
                System.Threading.Thread.Sleep(2000);
            });
            if (InformationText == "This is not correct HD") {
                InformationText = PreviousInformationText;
                ErrorLabelShowRunning = false;
            }
        }

        public void CopyOriginalHdToCountedHd()
        {
            foreach (PropertyInfo property in typeof (HdModel).GetProperties ()) {
                property.SetValue (CountedHd, property.GetValue (OriginalHd, null), null);
            }
        }

        public void ScanHd()
        {
            var reflexConnection = new ReflexConnectionModel ();
            reflexConnection.downloadHdFromReflex (ScannedBarcode);
            CountedHd.ListOfIpgs.Clear();
            OriginalHd.ListOfIpgs.Clear();
            OriginalHd.HdNumber = reflexConnection.OriginalHdModel.HdNumber;
            foreach (var Ipg in reflexConnection.OriginalHdModel.ListOfIpgs)
            {
                CountedHd.ListOfIpgs.Add(new IpgModel(){Item = Ipg.Item, Line = Ipg.Line, Quantity = Ipg.Quantity, Grade = Ipg.Grade});
                OriginalHd.ListOfIpgs.Add (new IpgModel () { Item = Ipg.Item, Line = Ipg.Line, Quantity = Ipg.Quantity, Grade = Ipg.Grade });
            }
            //ReflexConnection.downloadUpcForItemsAsync (CountedHd);
            //CountedHd.ListOfIpgs.Refresh ();
        }

        //public void DownloadUpc() {
        //    foreach (var _ipg in CountedHd.ListOfIpgs)
        //    {
        //        Task<IpgModel> t = Task.Factory.StartNew(() => ReflexConnection.downloadUpcForItemsAsync(_ipg));
        //        _ipg.Item = ReflexConnection.downloadUpcForItemsAsync (_ipg.Item);
        //        CountedHd.ListOfIpgs.Refresh ();
        //    }


        }
    }
}