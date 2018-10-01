using System;
using System.ComponentModel.Composition;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Caliburn.Micro;
using HdSplit.Models;
using OperationCanceledException = System.OperationCanceledException;

namespace HdSplit.ViewModels {

    [Export(typeof(ShellViewModel))]
    public class ShellViewModel : PropertyChangedBase
    {
        // For some reason needed for information label.
        private Thread NotifyAboutIncorrectHdAsyncThread = null;

        private string _hdNumber;
        public string HdNumber {
            get { return _hdNumber; }
            set {
                _hdNumber = value;
                NotifyOfPropertyChange (() => HdDataGridModel.OriginalHd.HdNumber);
            }
        }

        private int _selectedTab;
        public int SelectedTab {
            get { return _selectedTab; }
            set {
                _selectedTab = value;
                NotifyOfPropertyChange(() => SelectedTab);
            }
        }

        private BindableCollection<HdModel> _hds;
        public BindableCollection<HdModel> Hds {
            get { return _hds; }
            set {
                _hds = value;
                NotifyOfPropertyChange(() => Hds);
            }
        }
        
        private States _scanningState;
        public States ScanningState
        {
            get { return _scanningState; }
            set { _scanningState = value; }
        }

        private string _scannedBarcode;
        public string ScannedBarcode
        {
            get { return _scannedBarcode; }
            set
            {
                _scannedBarcode = value;
                NotifyOfPropertyChange(() => ScannedBarcode);
            }
        }

        private string _informationText;
        public string InformationText {
            get { return _informationText; }
            set {
                _informationText = value;
                NotifyOfPropertyChange (() => InformationText);
            }
        }

        private string _previousInformationText;
        public string PreviousInformationText {
            get { return _previousInformationText; }
            set {
                _previousInformationText = value;
                NotifyOfPropertyChange (() => PreviousInformationText);
            }
        }

        private bool _hdTaskIsRunning;
        public bool HdTaskIsRunning {
            get { return _hdTaskIsRunning; }
            set {
                _hdTaskIsRunning = value;
                NotifyOfPropertyChange(() => HdTaskIsRunning);
                NotifyOfPropertyChange(() => CanScanItemAsync);
            }
        }

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

        public bool ErrorLabelShowRunning { get; set; }
        public bool CanScanItemAsync
        {
            get { return !HdTaskIsRunning; }
        }
        public ReflexConnectionModel ReflexConnection = new ReflexConnectionModel();

        public HdDataGridViewModel HdDataGridModel { get; private set; }

        [ImportingConstructor]
        public ShellViewModel(HdDataGridViewModel hdDataGridModel)
        {
            InformationText = "Scan HD to start splitting";
            ErrorLabelShowRunning = false;
            HdTaskIsRunning = false;
            HdDataGridModel = hdDataGridModel;
            Hds = new BindableCollection<HdModel>();
            SelectedTab = 0;
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
                    if (!HdDataGridModel.CountedHd.CheckIfHdNumberIsCorrect (ScannedBarcode)) {
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
                        HdDataGridModel.OriginalHd.HdNumber = ScannedBarcode;
                        HdDataGridModel.CountedHd.HdNumber = ScannedBarcode;
                        HdTaskIsRunning = true;
                        // IF is checking if HD is unknown. IF yes then ScanHd return false, and Information label is updated.
                        if (!ScanHd ()) {


                            Notify ("Hd Unknown", Background = new SolidColorBrush (Colors.Red));
                            ScannedBarcode = string.Empty;
                            return;
                        }
                        
                        // HdNumber is binded to label on top of the program where you can see which HD user scanned.
                        HdNumber = HdDataGridModel.OriginalHd.HdNumber;

                        // Clearing scanning box
                        ScannedBarcode = string.Empty;

                        // As it says...
                        DownloadUpc ();
                        ScanningState = States.itemScan;
                        InformationText = "Scan item.";
                        HdTaskIsRunning = false;
                        Hds.Add(HdDataGridModel.CountedHd);
                        SelectedTab = 0;
                    });
                    Console.WriteLine("Jestem już poza DownloadUPC");
                    
                }
                else if (ScanningState == States.itemScan)
                {
                    
                    bool ItemNotFound = true;
                    try
                    {
                        foreach (var Ipg in HdDataGridModel.CountedHd.ListOfIpgs)
                        {
                            if (Ipg.Item == ScannedBarcode || Ipg.UpcCode == ScannedBarcode) {
                                ItemNotFound = false;
                                Ipg.Quantity--;
                                //Hds.Add(new HdModel(true){HdNumber = "test"});
                                HdDataGridModel.CountedHd.ListOfIpgs.Refresh();
                                if (Ipg.Quantity == 0) {
                                    HdDataGridModel.CountedHd.ListOfIpgs.RemoveAt (HdDataGridModel.CountedHd.ListOfIpgs.IndexOf (Ipg));
                                }

                                if (HdDataGridModel.CountedHd.ListOfIpgs.Count == 0)
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
            }
        }

        public void Restart()
        {
            HdDataGridModel.CountedHd = null;
            HdDataGridModel.CountedHd = new HdModel(false);
            HdDataGridModel.OriginalHd = null;
            HdDataGridModel.OriginalHd = new HdModel (false);
            Hds.Clear();
            ScanningState = States.firstScanOfHd;
            InformationText = "Scan HD to start splitting";
            ErrorLabelShowRunning = false;
            HdNumber = string.Empty;
            Background = new SolidColorBrush(Colors.Transparent);
            HdTaskIsRunning = false;
            SelectedTab = 0;
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
                property.SetValue (HdDataGridModel.CountedHd, property.GetValue (HdDataGridModel.OriginalHd, null), null);
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
            HdDataGridModel.CountedHd.ListOfIpgs.Clear ();
            HdDataGridModel.OriginalHd.ListOfIpgs.Clear ();

            // Creating instance of reflex connection so we can work with its HD instance.
            // This is needed for passing back HD info from reflex connection to ShellViewModel instance of OriginalHD and COunted HD.
            // TRY TO SET THIS AS PROPERTY OF SHELL VIEW MODELS
            var reflexConnection = new ReflexConnectionModel ();

            // IF checks here if we have some data inside reflexConnection.Hd.
            // If not then this function return HD unknown (false)
            if (reflexConnection.DownloadHdFromReflex (HdDataGridModel.OriginalHd.HdNumber))
            {
                // Tricky way to copy one hd to another. Needs to go thru properties manually.
                // There is porobably better way with function CopyOriginalHdToCountedHd().
                foreach (var Ipg in reflexConnection.OriginalHdModel.ListOfIpgs)
                {
                    // It is copied to original and counted at once. Question is it will change only counted or also original???
                    //CountedHd.ListOfIpgs.Add (new IpgModel () {Item = Ipg.Item, Line = Ipg.Line, Quantity = Ipg.Quantity, Grade = Ipg.Grade });
                    //OriginalHd.ListOfIpgs.Add (new IpgModel () { Item = Ipg.Item, Line = Ipg.Line, Quantity = Ipg.Quantity, Grade = Ipg.Grade });

                    HdDataGridModel.CountedHd.ListOfIpgs.Add(new IpgModel() { Item = Ipg.Item, Line = Ipg.Line, Quantity = Ipg.Quantity, Grade = Ipg.Grade });
                    HdDataGridModel.OriginalHd.ListOfIpgs.Add(new IpgModel() { Item = Ipg.Item, Line = Ipg.Line, Quantity = Ipg.Quantity, Grade = Ipg.Grade });
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
            var Ean_Upc = reflexConnection.DownloadUpcForItemsAsync(HdDataGridModel.CountedHd.ListOfIpgs);

            //            This: v   is returning bindable collection of IPG's
            foreach (var Ipg in HdDataGridModel.CountedHd.ListOfIpgs)
            {
                // Take EAN items from IPG, look for a key in dictionary, and assign it to UPC.
                Ipg.UpcCode = Ean_Upc[Ipg.Item];
            }

            foreach (var Ipg in HdDataGridModel.OriginalHd.ListOfIpgs)
            {
                // Take EAN items from IPG, look for a key in dictionary, and assign it to UPC.
                Ipg.UpcCode = Ean_Upc[Ipg.Item];
            }

            // Unfortunately needed for updating DataGrid.
            HdDataGridModel.CountedHd.ListOfIpgs.Refresh ();
        }
    }
}