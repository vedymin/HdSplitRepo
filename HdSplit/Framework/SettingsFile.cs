using System;
using System.IO;
using System.Windows;

namespace HdSplit.Framework
{
	public static class SettingsFile
	{
		public static string FolderPath { get; set; }
		public static string FilePath { get; set; }

		private static string _location;
		public static string Location
		{
			get { return _location; }
			set
			{
				_location = value;
				SaveToFile();
			}
		}

		private static string _printerIp;
		public static string PrinterIp
		{
			get { return _printerIp; }
			set
			{
				_printerIp = value;
				SaveToFile();
			}
		}

		static SettingsFile()
		{
			//Location = "l";
			//PrinterIp = " ";
			FolderPath = $"{Environment.GetEnvironmentVariable ("LocalAppData")}\\HdSplit";
			FilePath = $"{FolderPath}\\HdSplitSettings.txt";
			CheckForFolderAndFile();
			ReadFromFile();
		}

		/// <summary>,
		/// Checks if folder and path exist.
		/// </summary>
		internal static void CheckForFolderAndFile()
		{
			if (File.Exists(FilePath) is false)
			{
				if (Directory.Exists(FolderPath) is false)
				{
					Directory.CreateDirectory(FolderPath);
				}
				SaveToFile();
			}

		}

		/// <summary>
		/// Use this method to read settings from the file.
		/// </summary>
		internal static void ReadFromFile()
		{

			string[] entries = File.ReadAllText(FilePath).Split(',');

			if (entries.Length == 2)
			{
				Location = entries[0];
				PrinterIp = entries[1];
			}
			else
			{
				MessageBox.Show("Setting file have errors");
				Application.Current.Shutdown();
				return;
			}
		}


		/// <summary>
		/// Use this method to save configuration into file. This is needed to use the saved pack station in combo box.
		/// </summary>
		internal static void SaveToFile()
		{

			string conSave = $"{Location},{PrinterIp}";
			File.WriteAllText(FilePath, conSave);
		}

		/// <summary>
		/// Use this method to update chosen pack station in combobox into ConnectionSettings object.
		/// </summary>
		/// <param name="comboValue">Value from combobox (pack station)</param>
		//internal static void UpdatePackStation(string comboValue)
		//{
		//	PackStation = comboValue;
		//}
	}
}