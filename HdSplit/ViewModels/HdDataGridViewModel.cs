using System.ComponentModel.Composition;
using Caliburn.Micro;
using HdSplit.Models;

namespace HdSplit.ViewModels
{
    [Export(typeof(HdDataGridViewModel))]
    public class HdDataGridViewModel : PropertyChangedBase
    {
        private HdModel _originalHd = new HdModel(false);
        public HdModel OriginalHd {
            get { return _originalHd; }
            set {
                _originalHd = value;
                NotifyOfPropertyChange(() => OriginalHd);
                NotifyOfPropertyChange(() => OriginalHd.HdNumber);
            }
        }

        private HdModel _countedHd = new HdModel(false);
        public HdModel CountedHd {
            get { return _countedHd; }
            set {
                _countedHd = value;
                NotifyOfPropertyChange(() => CountedHd);
            }
        }
    }
}