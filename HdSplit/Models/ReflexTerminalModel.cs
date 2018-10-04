using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using AutConnListTypeLibrary;
using AutConnMgrTypeLibrary;
using AutOIATypeLibrary;
using AutPSTypeLibrary;
using AutWinMetricsTypeLibrary;
using Caliburn.Micro;

namespace HdSplit.Models
{
    public class ReflexTerminalModel
    {
        public AutPSClass ps = new AutPSClass();
        public AutWinMetricsClass wm = new AutWinMetricsClass();
        public AutOIAClass oi = new AutOIAClass();
        //public AutECLFieldListClass fl = new AutECLFieldListClass ();
        public AutConnMgrClass connectionManager = new AutConnMgrClass();
        public AutConnListClass connectionList = new AutConnListClass();

        public string Login { get; set; }
        public string Password { get; set; }

        /// <summary>
        /// Returns collection of Session Names.
        /// </summary>
        /// <returns></returns>
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

        #region Check and Wait for Connection

        public bool WaitForConnectionIsReady(string _sessionName)
        {
            while (CheckIfConnected(_sessionName))
            {

            }

            return true;
        }

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

        #endregion

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

        public void OpenReflexTerminal()
        {
            connectionManager.StartConnection("profile=./Resources/Reflex.ws connname=Z winstate=min");

            
            if (WaitForConnectionIsReady("Z"))
            {
                oi.SetConnectionByName("Z");
                ps.SetConnectionByName("Z");
            }

            

            //ps.SetConnectionByName("z");
        }

        public bool TryToLogin()
        {
            oi.WaitForInputReady(null);
            ps.SendKeys(Login);
            ps.SendKeys("[tab]");
            ps.SendKeys(Password);
            ps.SendKeys("[enter]");
            oi.WaitForInputReady(null);
            ps.SendKeys("[enter]");
            ps.SendKeys("[enter]");
            ps.SendKeys("[enter]");
            oi.WaitForInputReady(null);
            if (ps.SearchText("Sign On"))
            {
                return false;
            }
            else if (ps.SearchText("MENUINI"))
            {
                return true;
            }
            else return false;
        }

        public void CloseReflexTerminal()
        {
            connectionManager.StopConnection("Z","saveprofile=no");
        }


    }
}