using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using Caliburn.Micro;
using HdSplit.Models;
using OperationCanceledException = System.OperationCanceledException;

namespace HdSplit.ViewModels {
    internal class ShellViewModel : PropertyChangedBase
    {
        // For some reason needed for infotmation label.
        private Thread NotifyAboutIncorrectHdAsyncThread = null;


        private HdModel _originalHd = new HdModel (false);
        private HdModel _countedHd = new HdModel (false);
        private string _hdNumber;
        private States _scanningState;
        private string _scannedBarcode;
        private string _informationText;
        private string _previousInformationText;
        private bool _hdTaskIsRunning;

        private Brush _background;
        public Brush Background {
            get {
                return _background;
            }

            set {
                _background = value;
                NotifyOfPropertyChange (() => Background );
            }
        }

        public ShellViewModel()
        {
            InformationText = "Scan HD to start splitting";
            ErrorLabelShowRunning = false;
            HdTaskIsRunning = false;

        }

        public bool HdTaskIsRunning {
            get { return _hdTaskIsRunning; }
            set {
                _hdTaskIsRunning = value;
                NotifyOfPropertyChange(() => HdTaskIsRunning);
                NotifyOfPropertyChange(() => CanScanItemAsync);
            }
        }

        public bool ErrorLabelShowRunning { get; set; }


        public string HdNumber
        {
            get { return _hdNumber; }
            set {
                _hdNumber = value;
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

        public States ScanningState
        {
            get { return _scanningState; }
            set { _scanningState = value; }
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

        public bool CanScanItemAsync
        {
            get { return !HdTaskIsRunning; }
        }

        /// <summary>
        /// Reaction to scan button click or to press enter inside scanning box.
        /// </summary>
        /// <param name="keyArgs"></param>
        /// <returns></returns>
        public async Task ScanItemAsync(KeyEventArgs keyArgs) 
        {
            
            // keyArgs will be null if user clicked button scan.
            // IF also checking if there is enter pressed.
            if (keyArgs == null || keyArgs.Key == Key.Enter) {
                
                // IF checking about the state. Always start from firstScanOfHd. State needs to be set also in Reset().
                if (ScanningState == States.firstScanOfHd)
                {
                    // Validation of hd, and also updating info label. Needs to be refactored.
                    if (!CountedHd.CheckIfHdNumberIsCorrect (ScannedBarcode)) {
                        if (!ErrorLabelShowRunning) 
                        {
                            PreviousInformationText = InformationText;
                        }
                        Notify ("Incorrect HD", Background = new SolidColorBrush (Colors.Red));

                        return;
                    }
                    
                    // Hd is validated, so here we are going to download all information asynchronized.
                    await Task.Factory.StartNew (() =>
                    {
                        

                        // Save scanned barcoded so we can use it later.
                        OriginalHd.HdNumber = ScannedBarcode;
                        HdTaskIsRunning = true;
                        // IF is checking if HD is unknown. IF yes then ScanHd return false, and Information label is updated.
                        if (!ScanHd ()) {


                            Notify ("Hd Unknown", Background = new SolidColorBrush (Colors.Red));
                            ScannedBarcode = string.Empty;
                            return;
                        }
                        
                        // HdNumber is binded to label on top of the program where you can see which HD user scanned.
                        HdNumber = OriginalHd.HdNumber;

                        // Clearing scanning box
                        ScannedBarcode = string.Empty;

                        // As it says...
                        DownloadUpc ();
                        ScanningState = States.itemScan;
                        InformationText = "Scan item.";
                        HdTaskIsRunning = false;
                    });
                    Console.WriteLine("Jestem już poza DownloadUPC");
                    
                }
                else if (ScanningState == States.itemScan)
                {
                    
                    bool ItemNotFound = true;
                    try
                    {
                        foreach (var Ipg in CountedHd.ListOfIpgs)
                        {
                            if (Ipg.Item == ScannedBarcode || Ipg.UpcCode == ScannedBarcode) {
                                ItemNotFound = false;
                                Ipg.Quantity--;
                                CountedHd.ListOfIpgs.Refresh();
                                if (Ipg.Quantity == 0) {
                                    CountedHd.ListOfIpgs.RemoveAt (CountedHd.ListOfIpgs.IndexOf (Ipg));
                                }

                                if (CountedHd.ListOfIpgs.Count == 0)
                                {
                                    Restart();
                                }
                                return;
                            }

                        }

                        if (ItemNotFound) {
                            Notify ("This item do not belong to this HD.", new SolidColorBrush (Colors.Red));
                            return;
                        }
                    }
                    finally
                    {
                        ScannedBarcode = string.Empty;
                    }
                    

                    
                }

                //Tutaj jest kod który usuwa IPG z hd jeśli ilość dotarła do 0.                    
                //SelectedIpg.Quantity--;
                //if (SelectedIpg.Quantity == 0) {
                //    OriginalHd.ListOfIpgs.RemoveAt (OriginalHd.ListOfIpgs.IndexOf (SelectedIpg));
                //}
                //OriginalHd.ListOfIpgs.Refresh ();

            }
        }

        public void Restart()
        {
            CountedHd = null;
            CountedHd = new HdModel(false);
            OriginalHd = null;
            OriginalHd = new HdModel (false);
            ScanningState = States.firstScanOfHd;
            InformationText = "Scan HD to start splitting";
            ErrorLabelShowRunning = false;
            HdNumber = string.Empty;
            Background = new SolidColorBrush(Colors.Transparent);
            HdTaskIsRunning = false;
        }


        private async Task Notify(string _message, Brush color) {
            

            if (!ErrorLabelShowRunning) {
                PreviousInformationText = InformationText;
            }

            //string _message = "Hd Unknown";
            ErrorLabelShowRunning = true;
            if (NotifyAboutIncorrectHdAsyncThread != null) {
                NotifyAboutIncorrectHdAsyncThread.Abort ();
                NotifyAboutIncorrectHdAsyncThread = null;
                Background = new SolidColorBrush (Colors.Transparent);
            }

            InformationText = _message;
            await Task.Run (() => {
                Background = color;
                NotifyAboutIncorrectHdAsyncThread = Thread.CurrentThread;
                System.Threading.Thread.Sleep (3000);
            });

            if (InformationText == _message) {
                InformationText = PreviousInformationText;
                ErrorLabelShowRunning = false;
                Background = new SolidColorBrush (Colors.Transparent);
            }
        }
        // End of information label functions.


        // For now not used anywhere. Can be used maybe for changing ScanHD function (instead of foreach about all property one by one).
        public void CopyOriginalHdToCountedHd()
        {
            foreach (PropertyInfo property in typeof (HdModel).GetProperties ()) {
                property.SetValue (CountedHd, property.GetValue (OriginalHd, null), null);
            }
        }

        /// <summary>
        /// Download HD information
        /// </summary>
        /// <returns></returns>
        public bool ScanHd()
        {
            // Clearing info about already working on HD's. Later should after Confirm function return all is done.
            // Can be moved also to Reset button. You need oto reset anyway when you are scanning the HD. 
            CountedHd.ListOfIpgs.Clear ();
            OriginalHd.ListOfIpgs.Clear ();

            // Creating instance of reflex connection so we can work with its HD instance.
            // This is needed for passing back HD info from reflex connection to ShellViewModel instance of OriginalHD and COunted HD.
            // TRY TO SET THIS AS PROPERTY OF SHELL VIEW MODELS
            var reflexConnection = new ReflexConnectionModel ();

            // IF checks here if we have some data inside reflexConnection.Hd.
            // If not then this function return HD unknown (false)
            if (reflexConnection.DownloadHdFromReflex (OriginalHd.HdNumber))
            {
                // Tricky way to copy one hd to another. Needs to go thru properties manually.
                // There is porobably better way with function CopyOriginalHdToCountedHd().
                foreach (var Ipg in reflexConnection.OriginalHdModel.ListOfIpgs)
                {
                    // It is copied to original and counted at once. Question is it will change only counted or also original???
                    CountedHd.ListOfIpgs.Add (new IpgModel () {Item = Ipg.Item, Line = Ipg.Line, Quantity = Ipg.Quantity, Grade = Ipg.Grade });
                    OriginalHd.ListOfIpgs.Add (new IpgModel () { Item = Ipg.Item, Line = Ipg.Line, Quantity = Ipg.Quantity, Grade = Ipg.Grade });
                }
                // We still have some data so return true.
                return true;
            } 
            else
            {
                // Hd is unknown
                return false;
            }
        }


        /// <summary>
        /// Download UPC barcodes for items
        /// </summary>
        public void DownloadUpc()
        {
            // Creating instance of reflex connection so we can work with its HD instance.
            // This is needed for passing back HD info from reflex connection to ShellViewModel instance of OriginalHD and COunted HD.
            // TRY TO SET THIS AS PROPERTY OF SHELL VIEW MODELS
            var reflexConnection = new ReflexConnectionModel ();

            // First we are going to download and save in a dictionary.
            var Ean_Upc = reflexConnection.DownloadUpcForItemsAsync(CountedHd.ListOfIpgs);

            //            This: v   is returning bindable collection of IPG's
            foreach (var Ipg in CountedHd.ListOfIpgs)
            {
                // Take EAN items from IPG, look for a key in dictionary, and assign it to UPC.
                Ipg.UpcCode = Ean_Upc[Ipg.Item];
            }

            // Unfortunately needed for updating DataGrid.
            CountedHd.ListOfIpgs.Refresh ();
        }
    }
}