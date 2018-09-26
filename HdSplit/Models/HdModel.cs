using Caliburn.Micro;
using HdSplit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HdSplit.Models {
    public class HdModel {

        public BindableCollection<IpgModel> ListOfIpgs { get; set; } = new BindableCollection<IpgModel> ();
        public string HdNumber { get; set; }

        public HdModel(Boolean original)
        {
            if (original)
            {
                ListOfIpgs.Add (new IpgModel () { Item = "7613392345612", Line = Lines.LINE_10, Quantity = 15 , Grade = "T30"});
                ListOfIpgs.Add (new IpgModel () { Item = "7613826458568", Line = Lines.LINE_15, Quantity = 6, Grade = "T30" });
                ListOfIpgs.Add (new IpgModel () { Item = "7613398562231", Line = Lines.LINE_30, Quantity = 9, Grade = "T30" });
            }
            
        }

        public bool CheckIfHdNumberIsCorrect(string hd) {
            if (hd != null) {
                return System.Text.RegularExpressions.Regex.IsMatch (hd, "^(00)?[0-9]{18}$");
            }
            else
            {
                return false;
            }
        }
    }
}
