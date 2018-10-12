﻿using System;
using System.Windows;

namespace HdSplit {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {

	    [STAThread]
	    public static void Main()
	    {
			try
			{
				var application = new App();
				application.InitializeComponent();
				application.Run();
			}

			catch (Exception e)
			{
				MessageBox.Show(e.ToString());
			}
		}

		
	}
}
