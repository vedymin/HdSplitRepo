using HdSplit.Framework;
using System;

namespace HdSplit.Models
{
	public static class ZebraModel
	{
		private static readonly log4net.ILog log = LogHelper.GetLogger();


		public static string ipAddress;

		public static void Print(string line)
		{
			

			// Printer IP Address and communication port
			
			int port = 9100;

			var lineNumber = line.Substring(5, 2);
			// ZPL Command(s)
			string ZPLString =
				$"^XA^FO50,50^A0N60,60^FDLINE:^FS^FO80,110^A0N220,220^FD{lineNumber}^FS^XZ";

			try
			{
				// Open connection
				System.Net.Sockets.TcpClient client = new System.Net.Sockets.TcpClient();
				client.Connect(ipAddress, port);

				// Write ZPL String to connection
				System.IO.StreamWriter writer =
					new System.IO.StreamWriter(client.GetStream());
				writer.Write(ZPLString);
				writer.Flush();

				// Close Connection
				writer.Close();
				client.Close();
			}
			catch (Exception ex)
			{
				log.Error(ex);
			}
		}
	}
}