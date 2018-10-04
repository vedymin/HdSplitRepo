using System.Windows.Media;

namespace HdSplit.Models {
    public class IpgModel {

        public IpgModel()
        {
            Highlight = "White";
        }

        public string Item { get; set; }
        public Lines Line { get; set; }
        public int Quantity { get; set; }
        public string Grade { get; set; }
        public string UpcCode { get; set; }
        public string Highlight { get; set; }
    }
}
