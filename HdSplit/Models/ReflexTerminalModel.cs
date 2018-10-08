using AutConnListTypeLibrary;
using AutConnMgrTypeLibrary;
using AutOIATypeLibrary;
using AutPSTypeLibrary;
using System;
using System.Diagnostics;
using System.Windows;

namespace HdSplit.Models
{
	public class ReflexTerminalModel
	{
		public AutConnListClass connectionList = new AutConnListClass();
		public AutConnMgrClass connectionManager = new AutConnMgrClass();
		public AutOIAClass operatorInfoArea = new AutOIAClass();
		public AutPSClass presentationSpace = new AutPSClass();

		public ReflexTerminalModel()
		{
			SetConnectionForOIAandPS();
		}

		public string Login { get; set; }
		public string Password { get; set; }
		public const string View = "AA_SPLIT";
		public const string Prod_Test = "20";

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
			TimeSpan maxDuration = TimeSpan.FromSeconds(15);
			Stopwatch sw = Stopwatch.StartNew();

			while (sw.Elapsed < maxDuration)
			{
				if (CheckIfConnected(_sessionName))
				{
					return;
				}
			}
			CloseAppBecauseOfCriticalError("There is problem with connection to Reflex. " +
										   "Please Check your connection and" +
										   "run the application again.");
		}

		#endregion Check and Wait for Connection

		#region Check and Wait for Input

		public void WaitForInput()
		{
			TimeSpan maxDuration = TimeSpan.FromSeconds(15);
			Stopwatch sw = Stopwatch.StartNew();

			while (sw.Elapsed < maxDuration)
			{
				if (operatorInfoArea.InputInhibited == InhibitReason.pcNotInhibited)
				{
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
			connectionManager.StartConnection("profile=./Resources/Reflex.ws connname=Z winstate=min");

			WaitForConnectionIsReady("Z");
			SetConnectionForOIAandPS();
			WaitForText("Sign On");
		}

		public void CloseReflexTerminal()
		{
			SendEnter();
			SendFkey(9);
			SendString(16, 12, 2);
			SendEnter();
			connectionManager.StopConnection("Z", "saveprofile=no");
		}

		public void CloseAppBecauseOfCriticalError(string message)
		{
			MessageBox.Show(message);
			CloseReflexTerminal();
			Application.Current.Shutdown();
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

		#endregion SendKeys methods

		public void ConfirmHd()
		{
			while (GetText(11, 8, 8) != " ")
			{
				MoveToPickBlo();
			}
		}

		private void MoveToPickBlo()
		{
			SendString(20, 11, 2);
			SendEnter();
			SendEreaseField(16, 28);
			SendString(GetText(15, 28, 34), 16, 28);
			SendString(2, 17, 51);
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
			SendString(Prod_Test, 20, 7);
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
			operatorInfoArea = null;
			presentationSpace = null;
			operatorInfoArea = new AutOIAClass();
			presentationSpace = new AutPSClass();
			operatorInfoArea.SetConnectionByName("Z");
			presentationSpace.SetConnectionByName("Z");
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