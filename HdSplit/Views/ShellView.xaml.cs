using HdSplit.Framework;
using HdSplit.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace HdSplit.Views {
    /// <summary>
    /// Interaction logic for ShellView.xaml
    /// </summary>
    public partial class ShellView : Window {
        public ShellView() {
            InitializeComponent ();
        }

	    private void ShellView_OnLoaded(object sender, RoutedEventArgs e)
	    {
		    IRequestFocus focus = (IRequestFocus)DataContext;

		    focus.FocusRequsted += OnFocusRequest;
	    }

	    private void OnFocusRequest(object sender, FocusRequestedEventArgs e)
	    {
		    switch (e.PropertyName)
		    {
			    case "ScannedBarcode":
				    FocusScannedBarcodeAndSelectAll();
				    break;
		    }
	    }

	    private void TabControl_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
	    {
		    FocusScannedBarcodeAndSelectAll();
		}

	    private void FocusScannedBarcodeAndSelectAll()
	    {
		    ScannedBarcode.Focus();
		    ScannedBarcode.SelectAll();
	    }

	    private void TabControl_OnGotFocus(object sender, RoutedEventArgs e)
	    {
		    FocusScannedBarcodeAndSelectAll();
	    }
	}
}
