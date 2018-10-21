using System.Configuration;

namespace HdSplit.Framework
{
	public static class ConfigurationHelper
	{
		public static void SaveValue(string key, string value)
		{
			var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
			config.AppSettings.Settings[key].Value = value;
			config.Save(ConfigurationSaveMode.Modified);

			ConfigurationManager.RefreshSection("appSettings");
		}
	}
}