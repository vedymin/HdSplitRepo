using Caliburn.Micro;

namespace HdSplit.Models
{
	public class HdModel
	{
		public BindableCollection<IpgModel> ListOfIpgs { get; set; } = new BindableCollection<IpgModel>();
		public string HdNumber { get; set; }
		public string TabHeader { get; set; }
		public Lines Line { get; set; }
		public string Grade { get; set; }

		public HdModel(bool original)
		{
			if (original)
			{
				ListOfIpgs.Add(new IpgModel() { Item = "7613392345612", Line = Lines.LINE_10, Quantity = 15, Grade = "T30" });
				ListOfIpgs.Add(new IpgModel() { Item = "7613826458568", Line = Lines.LINE_15, Quantity = 6, Grade = "T30" });
				ListOfIpgs.Add(new IpgModel() { Item = "7613398562231", Line = Lines.LINE_30, Quantity = 9, Grade = "T30" });
			}
		}

		public bool CheckIfHdNumberIsCorrect(string hd)
		{
			if (hd != null)
			{
				return System.Text.RegularExpressions.Regex.IsMatch(hd, "^(00)?[0-9]{18}$");
			}
			else
			{
				return false;
			}
		}
	}
}