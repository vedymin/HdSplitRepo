using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using HdSplit.ViewModels;

namespace HdSplit {
    public class Bootstrapper : BootstrapperBase {

        public Bootstrapper() {
            Initialize ();
        }

        protected override void OnStartup(object sender, StartupEventArgs e) {
            DisplayRootViewFor<ShellViewModel> ();
        }
    }
}
