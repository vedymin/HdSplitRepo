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

		public string Login { get; set; }
		public string Password { get; set; }
		public object rowtest = 1;
		public object coltest = 1;

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

		public bool WaitForConnectionIsReady(string _sessionName)
		{
			// we'll stop after 10 minutes
			TimeSpan maxDuration = TimeSpan.FromSeconds(15);
			Stopwatch sw = Stopwatch.StartNew();
			bool DoneWithWork = false;

			while (sw.Elapsed < maxDuration && !DoneWithWork)
			{
				DoneWithWork = (CheckIfConnected(_sessionName)) ? true : false;
			}

			return true;
		}

		#endregion Check and Wait for Connection

		#region Check and Wait for Input

		public void WaitForInput()
		{
			while (operatorInfoArea.InputInhibited != 0)
			{
				presentationSpace.SendKeys("[reset]");
			}

			operatorInfoArea.WaitForInputReady(null);
		}

		#endregion Check and Wait for Input

		#region Search and Wait for text on screen

		public void WaitForText(string text)
		{
			while (presentationSpace.SearchText(text) == false)
			{
			}
		}

		public bool IsTextOnScreen(string text)
		{
			return presentationSpace.SearchText(text, PsDir.pcSrchForward, ref rowtest, ref coltest);
		}

		#endregion Search and Wait for text on screen

		#region Open/Close Reflex Terminal

		public void CloseReflexTerminal()
		{
			connectionManager.StopConnection("Z", "saveprofile=no");
		}

		public void OpenReflexTerminal()
		{
			connectionManager.StartConnection("profile=./Resources/Reflex.ws connname=Z winstate=min");

			if (WaitForConnectionIsReady("Z"))
			{
				SetConnectionForOIAandPS();
				WaitForText("Sign On");
			}
		}

		#endregion Open/Close Reflex Terminal

		#region SendKeys methods

		public void SendString(string text, int row, int column)
		{
			WaitForInput();
			presentationSpace.SendKeys(text, row, column);
		}

		public void SendString(int text, int row, int column)
		{
			WaitForInput();
			presentationSpace.SendKeys(text.ToString(), row, column);
		}

		public void SendEnter()
		{
			WaitForInput();
			presentationSpace.SendKeys("[enter]");
		}

		public void SendFkey(int number)
		{
			WaitForInput();
			presentationSpace.SendKeys($"[pf{number.ToString()}]");
		}

		#endregion SendKeys methods

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
				SendString(Login, 6, 53);
				SendString(Password, 7, 53);
				SendEnter();
				WaitForInput();

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
				return false;
			}
		}

		public void GoFromLoginToSelectIpgByLocationAsync()
		{
			SetConnectionForOIAandPS();

			WaitForText("MENUINI");
			SendString(1, 20, 7);
			SendEnter();
			SendEnter();
			SendString(4, 20, 7);
			SendEnter();
			SendString(2, 20, 7);
			SendEnter();
		}

		public void SetCorrectView()
		{
			WaitForInput();
			if (IsTextOnScreen("HLGE40"))
			{
				SendEnter();
				if (IsTextOnScreen("SPLIT"))
				{
					return;
				}
				else
				{
					SendFkey(9);
					WaitForText("HLVW11");
					SendFkey(22);
					if (IsTextOnScreen("A user or a group cannot be assigned more than"))
					{
						SendString(16, 20, 2);
						SendEnter();
						SendFkey(22);
					}
					// search
				}
			}
		}

		public void CheckIfViewIsCorrect()
		{
			SetConnectionForOIAandPS();
		}

		private void SetConnectionForOIAandPS()
		{
			operatorInfoArea.SetConnectionByName("Z");
			presentationSpace.SetConnectionByName("Z");
		}
	}
}