using System.ComponentModel.Composition;
using Caliburn.Micro;
using HdSplit.Models;

namespace HdSplit.ViewModels
{
    [Export(typeof(HdDataGridViewModel))]
    public class HdDataGridViewModel : PropertyChangedBase
    {
		
	    private BindableCollection<HdModel> _hds = new BindableCollection<HdModel>();
	    public BindableCollection<HdModel> Hds {
		    get { return _hds; }
		    set {
			    _hds = value;
			    NotifyOfPropertyChange(() => Hds);
		    }
	    }

		private HdModel _countedHd = new HdModel(false);
        public HdModel CountedHd {
            get { return _countedHd; }
            set {
                _countedHd = value;
                NotifyOfPropertyChange(() => CountedHd);
            }
        }

	    public void DownloadUpc()
	    {
		    // Creating instance of reflex connection so we can work with its HD instance.
		    // This is needed for passing back HD info from reflex connection to ShellViewModel instance of OriginalHD and COunted HD.
		    // TRY TO SET THIS AS PROPERTY OF SHELL VIEW MODELS
		    var reflexConnection = new ReflexConnectionModel();

		    // First we are going to download and save in a dictionary.
		    var Ean_Upc = reflexConnection.DownloadUpcForItemsAsync(CountedHd.ListOfIpgs);

		    //            This: v   is returning bindable collection of IPG's
		    foreach (var Ipg in CountedHd.ListOfIpgs)
		    {
			    // Take EAN items from IPG, look for a key in dictionary, and assign it to UPC.
			    Ipg.UpcCode = Ean_Upc[Ipg.Item];
		    }

		    // Unfortunately needed for updating DataGrid.
		    CountedHd.ListOfIpgs.Refresh();
	    }

		/// <summary>
		/// Download HD information
		/// </summary>
		public HdResult ScanHd()
		{
			// Clearing info about already working on HD's. Later should after Confirm function return all is done.
			// Can be moved also to Reset button. You need oto reset anyway when you are scanning the HD.
			CountedHd.ListOfIpgs.Clear();

			// Creating instance of reflex connection so we can work with its HD instance.
			// This is needed for passing back HD info from reflex connection to ShellViewModel instance of OriginalHD and COunted HD.
			// TRY TO SET THIS AS PROPERTY OF SHELL VIEW MODELS
			var reflexConnection = new ReflexConnectionModel();

			// IF checks here if we have some data inside reflexConnection.Hd.
			// If not then this function return HD unknown (false)
			var hdResult = reflexConnection.DownloadHdFromReflex(CountedHd.HdNumber);
			if (hdResult == HdResult.hdCorrect)
			{
				// Tricky way to copy one hd to another. Needs to go thru properties manually.
				// There is porobably better way with function CopyOriginalHdToCountedHd().
				foreach (var Ipg in reflexConnection.OriginalHdModel.ListOfIpgs)
				{
					// It is copied to original and counted at once. Question is it will change only counted or also original???
					//CountedHd.ListOfIpgs.Add (new IpgModel () {Item = Ipg.Item, Line = Ipg.Line, Quantity = Ipg.Quantity, Grade = Ipg.Grade });
					//OriginalHd.ListOfIpgs.Add (new IpgModel () { Item = Ipg.Item, Line = Ipg.Line, Quantity = Ipg.Quantity, Grade = Ipg.Grade });

					CountedHd.ListOfIpgs.Add(new IpgModel() { Item = Ipg.Item, Line = Ipg.Line, Quantity = Ipg.Quantity, Grade = Ipg.Grade });
				}
				// We still have some data so return true.
				return HdResult.hdCorrect;
			}
			else
			{
				return HdResult.hdUnknown;
			}
		}

	}
}