using Caliburn.Micro;
using HdSplit.Framework;
using HdSplit.Models;
using System;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace HdSplit.ViewModels
{
	[Export(typeof(ShellViewModel))]
	public class ShellViewModel : PropertyChangedBase, IHandle<LoginEvent>, IHandle<CloseOnLoginEvent>
	{
		public int IndexOfIpgToMinusOne;

		public ReflexConnectionModel ReflexConnection = new ReflexConnectionModel();

		private readonly IEventAggregator _events;

		private Brush _background;

		private string _hdNumber;

		private BindableCollection<HdModel> _hds;

		private bool _hdTaskIsRunning;

		private string _informationText;

		private string _login;

		private string _location;

		private string _previousInformationText;

		private string _scannedBarcode;

		private States _scanningState;

		private string _selectedSession;

		private int _selectedTab;

		private BindableCollection<string> _sessionNames;

		// For some reason needed for information label.
		private Thread NotifyAboutIncorrectHdAsyncThread = null;

		[ImportingConstructor]
		public ShellViewModel(HdDataGridViewModel hdDataGridModel, LoginViewModel loginViewModel, IWindowManager windowManager, IEventAggregator events)
		{
			LoginViewModel = loginViewModel;
			WindowManager = windowManager;
			HdDataGridModel = hdDataGridModel;
			_events = events;
			events.Subscribe(this);
			InformationText = "Scan HD to start splitting";
			ErrorLabelShowRunning = false;
			HdTaskIsRunning = false;
			Hds = new BindableCollection<HdModel>();
			SelectedTab = 0;
			Reflex = new ReflexTerminalModel();
			Reflex.OpenReflexTerminal();
		}

		public string ScannedItem { get; set; }

		public Brush Background {
			get {
				return _background;
			}

			set {
				_background = value;
				NotifyOfPropertyChange(() => Background);
			}
		}

		public bool CanScanItemAsync {
			get { return !HdTaskIsRunning; }
		}

		public bool ErrorLabelShowRunning { get; set; }

		public HdDataGridViewModel HdDataGridModel { get; private set; }

		//Cut zeros
		public string HdNumber {
			get { return _hdNumber; }
			set {
				_hdNumber = value;
				NotifyOfPropertyChange(() => HdDataGridModel.OriginalHd.HdNumber);
				NotifyOfPropertyChange(() => CanConfirm);
			}
		}

		public string HdForBreakdown { get; set; }

		public string Location {
			get { return _location; }
			set {
				_location = value;
				NotifyOfPropertyChange(() => Location);
			}
		}

		public BindableCollection<HdModel> Hds {
			get { return _hds; }
			set {
				_hds = value;
				NotifyOfPropertyChange(() => Hds);
			}
		}

		public bool HdTaskIsRunning {
			get { return _hdTaskIsRunning; }
			set {
				_hdTaskIsRunning = value;
				NotifyOfPropertyChange(() => HdTaskIsRunning);
				NotifyOfPropertyChange(() => CanScanItemAsync);
			}
		}

		public string InformationText {
			get { return _informationText; }
			set {
				_informationText = value;
				NotifyOfPropertyChange(() => InformationText);
			}
		}

		public IpgModel IpgToCreate { get; set; }

		public string Login {
			get {
				return _login;
			}

			set {
				_login = value;
				NotifyOfPropertyChange(() => Login);
			}
		}

		public LoginViewModel LoginViewModel { get; private set; }

		public string PreviousInformationText {
			get { return _previousInformationText; }
			set {
				_previousInformationText = value;
				NotifyOfPropertyChange(() => PreviousInformationText);
			}
		}

		public ReflexTerminalModel Reflex { get; set; }

		public string ScannedBarcode {
			get { return _scannedBarcode; }
			set {
				_scannedBarcode = value;
				NotifyOfPropertyChange(() => ScannedBarcode);
			}
		}

		public States ScanningState {
			get { return _scanningState; }
			set { _scanningState = value; }
		}

		public string SelecetedSession {
			get { return _selectedSession; }
			set {
				_selectedSession = value;
				NotifyOfPropertyChange(() => SelecetedSession);
			}
		}

		public int SelectedTab {
			get { return _selectedTab; }
			set {
				_selectedTab = value;
				NotifyOfPropertyChange(() => SelectedTab);
			}
		}

		public BindableCollection<string> SessionNames {
			get { return _sessionNames; }
			set {
				_sessionNames = value;
				NotifyOfPropertyChange(() => SessionNames);
			}
		}

		public IWindowManager WindowManager { get; private set; }

		public void AddIpgToExistingHd(int _index)
		{
			Hds[_index].ListOfIpgs.Add(new IpgModel()
			{
				Grade = IpgToCreate.Grade,
				Item = IpgToCreate.Item,
				Line = IpgToCreate.Line,
				UpcCode = IpgToCreate.UpcCode,
				Quantity = 1
			});
		}

		// For now not used anywhere. Can be used maybe for changing ScanHD function (instead of foreach about all property one by one).
		public void CopyOriginalHdToCountedHd()
		{
			foreach (PropertyInfo property in typeof(HdModel).GetProperties())
			{
				property.SetValue(HdDataGridModel.CountedHd, property.GetValue(HdDataGridModel.OriginalHd, null), null);
			}
		}

		public bool CanConfirm {
			get { return !String.IsNullOrEmpty(HdNumber); }
		}

		public void Confirm()
		{
			MessageBoxResult result = MessageBox.Show("Are You sure that HD is empty?", "Confirm HD", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
			switch (result)
			{
				case MessageBoxResult.Yes:
					Reflex.ConfirmHd();
					Restart();
					break;

				case MessageBoxResult.No:
					break;
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
			var reflexConnection = new ReflexConnectionModel();

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
			HdDataGridModel.CountedHd.ListOfIpgs.Refresh();
		}

		public void Handle(LoginEvent message)
		{
			Login = message.Login?.ToUpper();
			Reflex.Login = message.Login.ToUpper();
			Reflex.Password = message.Password;
			if (Reflex.TryToLogin())
			{
				//MessageBox.Show("Logged in!");
				_events.PublishOnUIThread(new LoginConfirmedEvent(true));
				//StartSTATask(ReflexLoginAndSetView);
				Reflex.GoFromLoginToSelectIpgByLocationAsync();
				Reflex.SetCorrectView();
			}
			//Reflex.CloseReflexTerminal();
		}

		public void Handle(CloseOnLoginEvent message)
		{
			OnClose(null);
		}

		public void Loaded(KeyEventArgs keyArgs)
		{
			WindowManager.ShowDialog(LoginViewModel);
		}

		public void OnClose(KeyEventArgs keyArgs)
		{
			if (Reflex.CheckIfConnected("Z"))
			{
				Reflex.CloseReflexTerminal();
			}
			Application.Current.Shutdown();
		}

		public void Restart()
		{
			HdDataGridModel.CountedHd = null;
			HdDataGridModel.CountedHd = new HdModel(false);
			HdDataGridModel.OriginalHd = null;
			HdDataGridModel.OriginalHd = new HdModel(false);
			Hds.Clear();
			ScanningState = States.firstScanOfHd;
			InformationText = "Scan HD to start splitting";
			ErrorLabelShowRunning = false;
			HdNumber = string.Empty;
			Background = new SolidColorBrush(Colors.Transparent);
			HdTaskIsRunning = false;
			SelectedTab = 0;

			//RefreshTerminalSessionNames();
			//Loaded();
		}

		/// <summary>
		/// Download HD information
		/// </summary>
		/// <returns></returns>
		public bool ScanHd()
		{
			// Clearing info about already working on HD's. Later should after Confirm function return all is done.
			// Can be moved also to Reset button. You need oto reset anyway when you are scanning the HD.
			HdDataGridModel.CountedHd.ListOfIpgs.Clear();
			HdDataGridModel.OriginalHd.ListOfIpgs.Clear();

			// Creating instance of reflex connection so we can work with its HD instance.
			// This is needed for passing back HD info from reflex connection to ShellViewModel instance of OriginalHD and COunted HD.
			// TRY TO SET THIS AS PROPERTY OF SHELL VIEW MODELS
			var reflexConnection = new ReflexConnectionModel();

			// IF checks here if we have some data inside reflexConnection.Hd.
			// If not then this function return HD unknown (false)
			if (reflexConnection.DownloadHdFromReflex(HdDataGridModel.OriginalHd.HdNumber))
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

		public HdResult ScanHdToCheckLines(string _hd)
		{
			// Creating instance of reflex connection so we can work with its HD instance.
			// This is needed for passing back HD info from reflex connection to ShellViewModel instance of OriginalHD and COunted HD.
			// TRY TO SET THIS AS PROPERTY OF SHELL VIEW MODELS
			var reflexConnection = new ReflexConnectionModel();

			// IF checks here if we have some data inside reflexConnection.Hd.
			// If not then this function return HD unknown (false)
			if (reflexConnection.DownloadHdFromReflex(_hd))
			{
				// Tricky way to copy one hd to another. Needs to go thru properties manually.
				// There is porobably better way with function CopyOriginalHdToCountedHd().
				foreach (var Ipg in reflexConnection.OriginalHdModel.ListOfIpgs)
				{
					if (Ipg.Line == IpgToCreate.Line)
					{
						continue;
					}
					else
					{
						return HdResult.differentLine;
					}
				}
				// We still have some data so return true.
				return HdResult.hdCorrect;
			}
			else
			{
				// Hd is unknown
				return HdResult.hdUnknown;
			}
		}

		/// <summary>
		/// Reaction to scan button click or to press enter
		/// inside scanning box.
		/// </summary>
		/// <param name="keyArgs"></param>
		/// <returns></returns>
		public async Task ScanItemAsync(KeyEventArgs keyArgs)
		{
			// keyArgs will be null if user clicked button scan.
			// IF also checking if there is enter pressed.
			if (keyArgs == null || keyArgs.Key == Key.Enter)
			{
				Sounds.PlayScanSound();
				// IF checking about the state. Always start from firstScanOfHd. State needs to be set also in Reset().
				if (ScanningState == States.firstScanOfHd)
				{
					// Validation of hd, and also updating info label. Needs to be refactored.
					if (ValidateHd()) return;

					// Hd is validated, so here we are going to download all information asynchronized.
					await Task.Factory.StartNew(() =>
					{
						// Save scanned barcoded so we can use it later.
						HdDataGridModel.OriginalHd.HdNumber = ScannedBarcode;
						HdDataGridModel.CountedHd.HdNumber = ScannedBarcode;
						HdDataGridModel.CountedHd.TabHeader = ScannedBarcode;
						HdTaskIsRunning = true;
						// IF is checking if HD is unknown. IF yes then ScanHd return false, and Information label is updated.
						if (!ScanHd())
						{
							Notify("Hd Unknown", Background = new SolidColorBrush(Colors.Red));
							ScannedBarcode = string.Empty;
							return;
						}

						// HdNumber is binded to label on top of the program where you can see which HD user scanned.
						HdNumber = HdDataGridModel.OriginalHd.HdNumber;

						// Clearing scanning box
						ScannedBarcode = string.Empty;

						// As it says...
						StartSTATask(ReflexScanHd);
						//Reflex.HdScanned(HdNumber);
						DownloadUpc();
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
							if (Ipg.Item == ScannedBarcode || Ipg.UpcCode == ScannedBarcode)
							{
								ScannedItem = ScannedBarcode;
								//StartSTATask(ReflexScanItem);
								Reflex.ItemScanned(ScannedItem);
								ItemNotFound = false;
								IpgToCreate = Ipg;
								IndexOfIpgToMinusOne = HdDataGridModel.CountedHd.ListOfIpgs.IndexOf(Ipg);
								Ipg.Highlight = "LightGreen";
								HdDataGridModel.CountedHd.ListOfIpgs.Refresh();
								ScanningState = States.newHdScan;
								InformationText = $"Scan HD with Line {IpgToCreate.Line}.";
								return;
							}
						}

						if (ItemNotFound)
						{
							Notify("This item do not belong to this HD.", new SolidColorBrush(Colors.Red));

							return;
						}
					}
					finally
					{
						ScannedBarcode = string.Empty;
					}
				}
				else if (ScanningState == States.newHdScan)
				{
					if (string.IsNullOrEmpty(Location))
					{
						MessageBox.Show("Location cannot be empty.");
						return;
					}
					if (ValidateHd()) return;

					if (SearchForHd(ScannedBarcode))
					{
						QuantityMinusOne();

						return;
					}
				}
			}
		}

		public bool ValidateHd()
		{
			if (!HdDataGridModel.CountedHd.CheckIfHdNumberIsCorrect(ScannedBarcode))
			{
				if (!ErrorLabelShowRunning)
				{
					PreviousInformationText = InformationText;
				}

				Notify("Incorrect HD", Background = new SolidColorBrush(Colors.Red));

				return true;
			}

			return false;
		}

		private async Task Notify(string _message, Brush color)
		{
			if (!ErrorLabelShowRunning)
			{
				PreviousInformationText = InformationText;
			}

			//string _message = "Hd Unknown";
			ErrorLabelShowRunning = true;
			if (NotifyAboutIncorrectHdAsyncThread != null)
			{
				NotifyAboutIncorrectHdAsyncThread.Abort();
				NotifyAboutIncorrectHdAsyncThread = null;
				Background = new SolidColorBrush(Colors.Transparent);
			}

			InformationText = _message;
			await Task.Run(() =>
			{
				Background = color;
				NotifyAboutIncorrectHdAsyncThread = Thread.CurrentThread;
				System.Threading.Thread.Sleep(3000);
			});

			if (InformationText == _message)
			{
				InformationText = PreviousInformationText;
				ErrorLabelShowRunning = false;
				Background = new SolidColorBrush(Colors.Transparent);
			}
		}

		private void QuantityMinusOne()
		{
			var TempIpg = HdDataGridModel.CountedHd.ListOfIpgs[IndexOfIpgToMinusOne];
			TempIpg.Quantity--;

			if (TempIpg.Quantity == 0)
			{
				HdDataGridModel.CountedHd.ListOfIpgs.RemoveAt(IndexOfIpgToMinusOne);
			}
			TempIpg.Highlight = "White";
			HdDataGridModel.CountedHd.ListOfIpgs.Refresh();

			ScanningState = States.itemScan;
			InformationText = "Scan item.";
			ScannedBarcode = string.Empty;
		}

		private bool SearchForHd(string _hd)
		{
			bool ItemFounded = false;
			bool HdFounded = false;

			for (int i = 1; i < Hds.Count; i++)
			{
				if (Hds[i].HdNumber == _hd)
				{
					if (Hds[i].Line == IpgToCreate.Line)
					{
						foreach (var Ipg in Hds[i].ListOfIpgs)
						{
							if (Ipg.Item == IpgToCreate.Item || Ipg.UpcCode == IpgToCreate.UpcCode)
							{
								Ipg.Quantity++;
								ItemFounded = true;
								HdForBreakdown = _hd;
								//StartSTATask(ReflexIpgBreakdownToOldHd);
								Reflex.ReflexIpgBreakdownToOldHd(HdForBreakdown);
								return true;
							}
						}

						if (!ItemFounded)
						{
							HdForBreakdown = _hd;
							AddIpgToExistingHd(i);
							//StartSTATask(ReflexIpgBreakdownToOldHd);
							Reflex.ReflexIpgBreakdownToOldHd(HdForBreakdown);
							return true;
						}
					}
					else
					{
						Notify("This HD have wrong LINE!", Brushes.Red);
						ScannedBarcode = String.Empty;
						return false;
					}
				}
			}

			if (!HdFounded)
			{
				var result = ScanHdToCheckLines(_hd);
				if (result == HdResult.hdCorrect || result == HdResult.hdUnknown)
				{
					string IpgBreakdownResult = null;
					if (result == HdResult.hdCorrect)
					{
						Reflex.ReflexIpgBreakdownToOldHd(_hd);
					}
					else
					{
						IpgBreakdownResult = Reflex.ReflexIpgBreakdownToNewHd(_hd, Location);
					}
					if (IpgBreakdownResult != null)
					{
						MessageBox.Show(IpgBreakdownResult);
						ScannedBarcode = String.Empty;
						return false;
					}
					Hds.Add(new HdModel(false)
					{
						Grade = IpgToCreate.Grade,
						Line = IpgToCreate.Line,
						HdNumber = ScannedBarcode,
						ListOfIpgs = new BindableCollection<IpgModel>(),
						TabHeader = $"{ScannedBarcode} - {IpgToCreate.Line.ToString()}"
					});

					//HdForBreakdown = _hd;
					AddIpgToExistingHd(Hds.Count - 1);

					//StartSTATask(ReflexIpgBreakdownToNewHd);
					QuantityMinusOne();
					return false;
				}
				else if (result == HdResult.differentLine)
				{
					Notify("This HD have wrong LINE!", Brushes.Red);
					ScannedBarcode = String.Empty;
					return false;
				}

				// Sprawdź ten HD w reflexie.
			}

			return false;
		}

		private static Task StartSTATask(System.Action func)
		{
			TaskCompletionSource<object> source = new TaskCompletionSource<object>();
			Thread thread = new Thread(() =>
			{
				try
				{
					func();
					source.SetResult(null);
				}
				catch (Exception ex)
				{
					source.SetException(ex);
				}
			});
			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();
			return source.Task;
		}

		private void ReflexLoginAndSetView()
		{
			HdTaskIsRunning = true;
			ReflexTerminalModel ReflexAsync = new ReflexTerminalModel();
			ReflexAsync.GoFromLoginToSelectIpgByLocationAsync();
			ReflexAsync.SetCorrectView();
			ReflexAsync = null;
			HdTaskIsRunning = false;
		}

		public void ReflexScanHd()
		{
			HdTaskIsRunning = true;
			ReflexTerminalModel ReflexTest = new ReflexTerminalModel();
			ReflexTest.HdScanned(HdNumber);
			ReflexTest = null;
			HdTaskIsRunning = false;
		}

		public void ReflexScanItem()
		{
			HdTaskIsRunning = true;
			ReflexTerminalModel ReflexTest = new ReflexTerminalModel();
			ReflexTest.ItemScanned(ScannedItem);
			ReflexTest = null;
			HdTaskIsRunning = false;
		}

		public void ReflexIpgBreakdownToNewHd()
		{
			HdTaskIsRunning = true;
			ReflexTerminalModel ReflexTest = new ReflexTerminalModel();
			ReflexTest.ReflexIpgBreakdownToNewHd(HdForBreakdown, Location);
			ReflexTest = null;
			HdTaskIsRunning = false;
		}

		public void ReflexIpgBreakdownToOldHd()
		{
			HdTaskIsRunning = true;
			ReflexTerminalModel ReflexTest = new ReflexTerminalModel();
			ReflexTest.ReflexIpgBreakdownToOldHd(HdForBreakdown);
			ReflexTest = null;
			HdTaskIsRunning = false;
		}
	}
}