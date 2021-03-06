﻿using System;
using System.Windows;
using HdSplit.Framework;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace HdSplit {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

	    private static readonly log4net.ILog log = LogHelper.GetLogger();
	    public static string Environment;

	    [STAThread]
	    public static void Main(string[] args)
		{
			if (args.Length > 0)
			{
				Environment = args[0];
			}
			else
			{
				Environment = "production";
			}
			try
			{
				log.Info("------------------------ NEW OPENING OF APPLICATION --------------------------");
				var application = new App();
				application.InitializeComponent();
				application.Run();
				log.Info("App started");
			}

			catch (Exception e)
			{
				MessageBox.Show(e.ToString());
				log.Error(e);
			}
		}

		
	}
}
