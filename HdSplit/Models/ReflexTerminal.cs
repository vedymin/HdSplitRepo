using System;
using System.Collections.Generic;
using AutConnListTypeLibrary;
using AutConnMgrTypeLibrary;
using AutOIATypeLibrary;
using AutPSTypeLibrary;
using AutWinMetricsTypeLibrary;
using Caliburn.Micro;

namespace HdSplit.Models
{
    public class ReflexTerminal
    {
        public AutPSClass ps = new AutPSClass();
        public AutWinMetricsClass wm = new AutWinMetricsClass();
        public AutOIAClass oi = new AutOIAClass();
        //public AutECLFieldListClass fl = new AutECLFieldListClass ();
        public AutConnMgrClass connectionManager = new AutConnMgrClass();
        public AutConnListClass connectionList = new AutConnListClass();


        /// <summary>
        /// Returns collection of Session Names.
        /// </summary>
        /// <returns></returns>
        public BindableCollection<string> GetAllConnections()
        {

            List<object> openSessions = new List<object>();

            connectionList.Refresh();

            for (int i = 1; i <= connectionList.Count; i++)
            {
                openSessions.Add(connectionList[i]);
            }

            BindableCollection<string> openSessionNames = new BindableCollection<string>();

            foreach (var session in openSessions)
            {
                openSessionNames.Add(session.GetType().InvokeMember("Name", System.Reflection.BindingFlags.GetProperty, null, session, null).ToString());
            }

            if (openSessionNames.Count == 0)
            {
                return null;
            }
            else
            {
                return openSessionNames;
            }
        }

        public bool CheckIfConnectionIsReady(string _sessionName)
        {
            return Boolean.Parse(GetValueOfObjectProperty("Ready", _sessionName));
        }

        public string GetValueOfObjectProperty(string _property, string _sessionName)
        {
            if (_sessionName != null)
            {
                var session = connectionList.FindConnectionByName(_sessionName);
                return session.GetType().InvokeMember(_property, System.Reflection.BindingFlags.GetProperty, null, session, null).ToString();
            }
            else
            {
                return "false";
            }
        }

        public void OpenReflexTerminal()
        {
            connectionManager.StartConnection("profile=C:\\Users\\dmichalski\\Desktop\\Reflex.ws connname=z winstate=max");
        }
    }
}