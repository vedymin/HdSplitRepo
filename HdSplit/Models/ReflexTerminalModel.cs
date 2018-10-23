using AutConnListTypeLibrary;
using AutConnMgrTypeLibrary;
using AutOIATypeLibrary;
using AutPSTypeLibrary;
using System;
using System.Diagnostics;
using System.Windows;
using Caliburn.Micro;
using HdSplit.Framework;

namespace HdSplit.Models
{
	
	public class ReflexTerminalModel
	{

		private static readonly log4net.ILog log = LogHelper.GetLogger();


		public AutConnListClass connectionList = new AutConnListClass();
		public AutConnMgrClass connectionManager = new AutConnMgrClass();
		public AutOIAClass operatorInfoArea = new AutOIAClass();
		public AutPSClass presentationSpace = new AutPSClass();
		public string Login { get; set; }
		public string Password { get; set; }
		public string FolderPath { get; private set; }
		public string FilePath { get; private set; }
		public string Environment { get; set; }

		public const string View = "AA_SPLIT";

		public ReflexTerminalModel()
		{
			if (App.Environment == "production")
			{
				Environment = "1";
			}
			else
			{
				Environment = "20";
			}
			log.Info("Contructor ReflexTerminalModel");
			SetConnectionForOIAandPS();
		}

		

		#region Future functions for general library

		//public BindableCollection<string> GetAllConnections()
		//{
		//    List<object> openSessions = new List<object>();

		//    connectionList.Refresh();

		//    for (int i = 1; i <= connectionList.Count; i++)
		//    {
		//        openSessions.Add(connectionList[i]);
		//    }

		//    BindableCollection<string> openSessionNames = new BindableCollection<string>();

		//    foreach (var session in openSessions)
		//    {
		//        openSessionNames.Add(session.GetType().InvokeMember("Name", System.Reflection.BindingFlags.GetProperty, null, session, null).ToString());
		//    }

		//    if (openSessionNames.Count == 0)
		//    {
		//        return null;
		//    }
		//    else
		//    {
		//        return openSessionNames;
		//    }
		//}

		//public string GetValueOfObjectProperty(string _property, string _sessionName)
		//{
		//    if (_sessionName != null)
		//    {
		//        connectionList.Refresh();
		//        var session = connectionList.FindConnectionByName(_sessionName);
		//        if (session == null)
		//        {
		//            return "false";
		//        }
		//        return session.GetType().InvokeMember(_property, System.Reflection.BindingFlags.GetProperty, null, session, null).ToString();
		//    }
		//    else
		//    {
		//        return "false";
		//    }
		//}

		#endregion Future functions for general library

		#region Check and Wait for Connection

		public bool CheckIfConnected(string _sessionName)
		{
			connectionList.Refresh();
			var session = connectionList.FindConnectionByName(_sessionName);
			if (session == null)
			{
				return false;
			}
			return Boolean.Parse(session.GetType().InvokeMember("Ready", System.Reflection.BindingFlags.GetProperty, null, session, null).ToString());
		}

		public void WaitForConnectionIsReady(string _sessionName)
		{
			log.Info("Waiting for connection with Z - 30 seconds timeout");
			TimeSpan maxDuration = TimeSpan.FromSeconds(30);
			Stopwatch sw = Stopwatch.StartNew();

			while (sw.Elapsed < maxDuration)
			{
				if (CheckIfConnected(_sessionName))
				{
					log.Info("Connected with Z");
					return;
				}
			}
			log.Error("Timeout - Message box with error info pop up. Going to close app.");
			CloseAppBecauseOfCriticalError("There is problem with connection to Reflex. " +
										   "Please Check your connection and" +
										   "run the application again.");
		}

		#endregion Check and Wait for Connection

		#region Check and Wait for Input

		public void WaitForInput()
		{
			TimeSpan maxDuration = TimeSpan.FromSeconds(20);
			Stopwatch sw = Stopwatch.StartNew();

			while (sw.Elapsed < maxDuration)
			{
				if (operatorInfoArea.InputInhibited == InhibitReason.pcNotInhibited)
				{
					presentationSpace.SendKeys("[reset]");
					operatorInfoArea.WaitForInputReady(null);
					return;
				}
				presentationSpace.SendKeys("[reset]");
			}
			CloseAppBecauseOfCriticalError("Unexpected error occur while waiting for input. Please run application again.");
		}

		public void WaitForInput(int seconds)
		{
			TimeSpan maxDuration = TimeSpan.FromSeconds(seconds);
			Stopwatch sw = Stopwatch.StartNew();

			while (sw.Elapsed < maxDuration)
			{
				if (operatorInfoArea.InputInhibited == InhibitReason.pcNotInhibited)
				{
					presentationSpace.SendKeys("[reset]");
					operatorInfoArea.WaitForInputReady(null);
					return;
				}
				presentationSpace.SendKeys("[reset]");
			}
			CloseAppBecauseOfCriticalError("Unexpected error occur while waiting for input. Please run application again.");
		}

		#endregion Check and Wait for Input

		#region Search and Wait for text on screen

		public void WaitForText(string text)
		{
			log.Info("Waiting for text \"Sign On\" on the screen - 15 seconds timeout");
			TimeSpan maxDuration = TimeSpan.FromSeconds(15);
			Stopwatch sw = Stopwatch.StartNew();

			while (sw.Elapsed < maxDuration)
			{
				WaitForInput();
				if (presentationSpace.SearchText(text))
				{
					return;
				}
			}
			log.Info("Timeout - Messagebox pop up - going to close App");
			CloseAppBecauseOfCriticalError("Unexpected error occur while waiting for string. Please run application again.");
		}

		public bool IsTextOnScreen(string text)
		{
			WaitForInput();
			return presentationSpace.SearchText(text);
		}

		public bool IsTextOnScreen(string text, int waitingTime)
		{
			TimeSpan maxDuration = TimeSpan.FromSeconds(waitingTime);
			Stopwatch sw = Stopwatch.StartNew();

			while (sw.Elapsed < maxDuration)
			{
				WaitForInput();
				if (presentationSpace.SearchText(text))
				{
					return true;
				}
			}

			return false;
		}

		public bool IsTextOnScreen(string text, ref object row, ref object column)
		{
			WaitForInput();
			return presentationSpace.SearchText(text, PsDir.pcSrchForward, ref row, ref column);
		}

		#endregion Search and Wait for text on screen

		#region Open/Close Reflex Terminal

		public void OpenReflexTerminal()
		{
			log.Info("Opening Reflex Terminal Z");
			FolderPath = $"{System.Environment.GetEnvironmentVariable("LocalAppData")}\\HdSplit";
			FilePath = $"{FolderPath}\\Reflex.ws";
			log.Info("Starting connection");
			if (CheckIfConnected("z"))
			{
				operatorInfoArea.StopCommunication();
				operatorInfoArea.StartCommunication();
			}
			else
			{
				connectionManager.StartConnection($"profile={FilePath} connname=Z winstate=min");
			}
			WaitForConnectionIsReady("Z");
			SetConnectionForOIAandPS();
			//WaitForText("Sign On");
		}

		public void CloseReflexTerminal()
		{
			log.Info("Closing Reflex Terminal");
			//SendEnter();
			//SendFkey(9);
			//SendString(16, 12, 2);
			//SendEnter();
			//log.Info("Waiting 5 sec for input");
			//WaitForInput(5);
			log.Info("ConnectionManager.StopConnection");
			connectionManager.StopConnection("Z", "saveprofile=no");
		}

		public void CloseAppBecauseOfCriticalError(string message)
		{
			log.Info($"CloseAppBecauseOfCriticalError - {message}");
			MessageBox.Show(message);
			try
			{
				CloseReflexTerminal();
			}
			finally
			{
				log.Info("Application Shutdown");
				Application.Current.Shutdown();
				throw new InvalidOperationException();
			}
		}

		#endregion Open/Close Reflex Terminal

		#region SendKeys methods

		public void SendEreaseField(int row, int column)
		{
			presentationSpace.SetCursorPos(row, column);
			WaitForInput();
			presentationSpace.SendKeys("[eraseeof]");
		}

		public void SendString(string text, int row, int column)
		{
			WaitForInput();
			presentationSpace.SendKeys("[tab]");
			presentationSpace.SendKeys(text, row, column);
			WaitForInput();
		}

		public void SendString(int text, int row, int column)
		{
			WaitForInput();
			presentationSpace.SendKeys("[tab]");
			presentationSpace.SendKeys(text.ToString(), row, column);
			WaitForInput();
		}

		public void SendEnter()
		{
			WaitForInput();
			presentationSpace.SendKeys("[tab]");
			presentationSpace.SendKeys("[enter]");
			WaitForInput();
		}

		public void SendEnter(string textForWait)
		{
			WaitForInput();
			presentationSpace.SendKeys("[tab]");
			presentationSpace.SendKeys("[enter]");
			WaitForText(textForWait);
			WaitForInput();
		}

		public void SendPageDown()
		{
			WaitForInput();
			presentationSpace.SendKeys("[pagedn]");
			WaitForInput();
		}

		public void SendFkey(int number)
		{
			WaitForInput();
			presentationSpace.SendKeys($"[pf{number.ToString()}]");
			WaitForInput();
		}

		public void SendFkey(int number, string textForWait)
		{
			WaitForInput();
			presentationSpace.SendKeys($"[pf{number.ToString()}]");
			WaitForText(textForWait);
			WaitForInput();
		}

		internal void GoToSelectIpgByLocation()
		{
			while (!IsTextOnScreen("HLGE40")) 
			{
				SendFkey(12);
				WaitForInput();
			} 
		}

		#endregion SendKeys methods

		public void ConfirmHd(BindableCollection<HdModel> hds)
		{
			while (GetText(11, 8, 8) != " ")
			{
				MoveToPickBlo();
			}
			SendFkey(12);
			for (int i = 1; i < hds.Count; i++)
			{
				MergeIpgs(hds[i].HdNumber.ToString());
			}
		}

		private void MergeIpgs(string hd)
		{
			SendString(hd, 20, 28);
			SendEnter("HLGE41");
			SendString(14, 11, 2);
			SendEnter("HLST63");
			SendFkey(17);
			SendString(13, 12, 2);
			SendEnter();
			SendFkey(12);
			SendFkey(12);
			SendFkey(12);
			WaitForText("HLGE40");
		}

		private void MoveToPickBlo()
		{
			SendString(20, 11, 2);
			SendEnter();
			SendEreaseField(16, 28);
			SendString(GetText(15, 28, 34), 16, 28);
			SendString(70, 17, 50);
			SendString("n", 18, 34);
			SendString("n", 20, 34);
			SendEnter();
			SendString(1, 13, 6);
			SendEnter();
			SendFkey(20);
		}

		public string GetText(int row, int column, int lastLetterOfTextColumn)
		{
			WaitForInput();
			// + 1 is for including last letter
			int lenght = lastLetterOfTextColumn + 1 - column;
			return presentationSpace.GetText(row, column, lenght);
		}

		public void ClearScreen()
		{
			WaitForInput();
			presentationSpace.SendKeys("[clear]");
		}

		public bool TryToLogin()
		{
			try
			{
				WaitForText("Sign On");
				WaitForInput();
				ClearScreen();
				SendString(Login, 6, 53);
				SendString(Password, 7, 53);
				SendEnter();

				if (IsTextOnScreen("CPF1107"))
				{
					MessageBox.Show("Password not correct for user profile.");
					return false;
				}

				if (IsTextOnScreen("CPF1116"))
				{
					MessageBox.Show("Next not valid sign-on attempt will disconnect you. After that restart program.");
					return false;
				}

				if (IsTextOnScreen("CPF1120"))
				{
					MessageBox.Show($"User {Login} does not exist.");
					return false;
				}

				SendEnter();
				SendEnter();
				SendEnter();
				WaitForInput();
				if (IsTextOnScreen("Sign On"))
				{
					MessageBox.Show(GetText(24, 1, 75));
					return false;
				}
				else if (IsTextOnScreen("Procedures"))
				{
					MessageBox.Show("You need extended Reflex for this login.");
					SendFkey(3);
					return false;
				}
				else if (IsTextOnScreen("MENUINI"))
				{
					return true;
				}
				else
				{
					MessageBox.Show("Login Failed for unknown reason.");
					return false;
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("Login Failed" + e);
				throw;
			}
		}

		public void GoFromLoginToSelectIpgByLocationAsync()
		{
			SetConnectionForOIAandPS();

			WaitForText("MENUINI");
			// Production or Test
			SendString(Environment, 20, 7);
			SendEnter();
			SendEnter();
			SendString(4, 20, 7);
			SendEnter();
			SendString(2, 20, 7);
			SendEnter();
		}

		public void SetCorrectView()
		{
			if (IsTextOnScreen("HLGE40"))
			{
				SendEnter();
				WaitForText("HLGE41");
				if (IsTextOnScreen("SPLIT"))
				{
					SendFkey(12);
					return;
				}
				else
				{
					object searchResultRow = 1;
					object searchResultColumn = 1;
					SendFkey(9);
					WaitForText("HLVW11");
					SendFkey(22);
					if (IsTextOnScreen("A user or a group cannot be assigned more than"))
					{
						SendString(16, 20, 2);
						SendEnter();
						SendFkey(22);
					}
					WaitForText("HLVW12");
					if (IsTextOnScreen(View, ref searchResultRow, ref searchResultColumn))
					{
						SendString(15, (int)searchResultRow, 2);
						SendEnter();
						SendFkey(12);
					}
					WaitForText("HLVW11");
					SendFkey(18);
					WaitForText("HLVW18");
					searchResultColumn = 1;
					searchResultRow = 1;
					if (IsTextOnScreen(View, ref searchResultRow, ref searchResultColumn))
					{
						SendString("1  ", (int)searchResultRow, 4);
						SendEnter();
						SendFkey(12);
					}

					if (IsTextOnScreen("SPLIT"))
					{
						SendFkey(12);
						return;
					}
					else
					{
						SendFkey(12);
						SetCorrectView();
					}
				}
			}
		}

		public void HdScanned(string hd)
		{
			SetConnectionForOIAandPS();
			SendString(hd, 20, 28);
			SendEnter();
		}

		public void ItemScanned(string item)
		{
			object searchResultRow = 1;
			object searchResultColumn = 1;

			if (IsTextOnScreen(item, ref searchResultRow, ref searchResultColumn))
			{
				SendString(20, (int)searchResultRow, 2);
				SendEnter();
			}
			else
			{
				if (IsTextOnScreen("+"))
				{
					SendPageDown();
					ItemScanned(item);
				}
			}
		}

		public void SetConnectionForOIAandPS()
		{
			log.Info("Setting connection to OIA and PS classes");
			operatorInfoArea = null;
			presentationSpace = null;
			operatorInfoArea = new AutOIAClass();
			presentationSpace = new AutPSClass();
			operatorInfoArea.SetConnectionByName("Z");
			presentationSpace.SetConnectionByName("Z");
			log.Info("Setting connection to OIA and PS classes SUCCESFULL");

		}

		public string ReflexIpgBreakdownToNewHd(string hd, string location)
		{
			SendEreaseField(16, 28);
			SendString(1, 16, 28);
			SendString(hd, 17, 34);
			SendString("y", 18, 34);
			SendString("            N", 19, 34);
			SendString(location, 19, 34);
			SendEnter();
			if (presentationSpace.GetText(24, 2, 3) == "   ")
			{
				SendFkey(20);
				return null;
			}
			else
			{
				return presentationSpace.GetText(24, 2, 40);
			}
		}

		public void ReflexIpgBreakdownToOldHd(string hd)
		{
			SendString(1, 16, 28);
			SendString(hd, 17, 34);
			SendString("n", 18, 34);
			SendString("n", 20, 34);
			SendEnter();
			SendFkey(20);
		}
	}
}