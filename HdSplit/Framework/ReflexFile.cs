using System;
using System.IO;

namespace HdSplit.Framework
{
	public static class ReflexFile
	{
		private static readonly log4net.ILog log = LogHelper.GetLogger();

		// constructor
		static ReflexFile()
		{
			FolderPath = $"{Environment.GetEnvironmentVariable("LocalAppData")}\\HdSplit";
			FilePath = $"{FolderPath}\\Reflex.ws";
			CheckForFolderAndFile();
		}

		public static string FolderPath { get; }
		public static string FilePath { get; }

		internal static void CheckForFolderAndFile()
		{
			log.Info("Checking for Reflex ws file and directory");
			if (File.Exists(FilePath) is false)
			{
				log.Info("File not exists. Going to create.");
				if (Directory.Exists(FolderPath) is false)
				{
					log.Info("Folder also not exists. Going to create.");
					Directory.CreateDirectory(FolderPath);
				}
				File.Delete(FilePath);
				SaveToFile();
			}


		}

		private static void SaveToFile()
		{
			string conSave = @"
[Profile]
ID=WS
Version=9
[CT]
trace=Y
[Telnet5250]
HostName=10.52.1.100
AssociatedPrinterStartMinimized=N
AssociatedPrinterClose=N
AssociatedPrinterTimeout=0
Security=N
SSLClientAuthentication=Y
CertSelection=AUTOSELECT
AutoReconnect=N
[KeepAlive]
KeepAliveTimeOut=0
[Communication]
Link=telnet5250
Session=5250
[5250]
ScreenSize=27x132
HostCodePage=037-U
PrinterType=IBM3812
BypassSignon=N
[Keyboard]
CuaKeyboard=2
Language=United-States
IBMDefaultKeyboard=N
[LastExitView]
A=4 366 94 940 813 3 13 29 400 0 IBM3270— 37
[Window]
ViewFlags=CF00
RuleLinePos=6 19
SessFlags=38C6A
[Poppad]
PoppadLeft=695
PoppadTop=510
[Edit]
WordBreak=N
TrimRectSizingHandles=N
TrimRectRe\AfterEdit=N
TrimRectAsSolid=Y
Autocopy=N
[Colors]
ExtendedColorGreen=24D830 000000
OtherRuleLine=17801F
[printers]
printer=Laser Teamleader Island,winspool,10.55.138.43
CPI=10
LPI=6
FaceName=[BatangChe]
Raster=N
Drawer1Orient=Portrait
Drawer2Orient=Portrait
VTPrintArea=Scroll
VTPrintChar=ASCII
VTTerminator=None

";
			File.WriteAllText(FilePath, conSave);
		}
	}
}