﻿using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Dynamic;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using HdSplit.Framework;
using HdSplit.ViewModels;

namespace HdSplit
{
    public class Bootstrapper : BootstrapperBase {

	    private static readonly log4net.ILog log = LogHelper.GetLogger();

		public Bootstrapper() {
            Initialize ();

        }

        protected override void OnStartup(object sender, StartupEventArgs e) {
			log.Info("OnStartup method -> DisplayRootViewFor<ShellViewModel>");
            DisplayRootViewFor<ShellViewModel> ();
        }

        private CompositionContainer container;

        protected override void Configure()
        {
            container = new CompositionContainer(new AggregateCatalog(AssemblySource.Instance.Select(x => new AssemblyCatalog(x)).OfType<ComposablePartCatalog>()));

            CompositionBatch batch = new CompositionBatch();

            batch.AddExportedValue<IWindowManager>(new WindowManager());
            batch.AddExportedValue<IEventAggregator>(new EventAggregator());
            batch.AddExportedValue(container);

            container.Compose(batch);
        }

        protected override object GetInstance(Type serviceType, string key)
        {
            string contract = string.IsNullOrEmpty(key) ? AttributedModelServices.GetContractName(serviceType) : key;
            var exports = container.GetExportedValues<object>(contract);

            if (exports.Count() > 0)
            {
                return exports.First();
            }

            throw new Exception(string.Format("Could not locate any instances of contract {0}.", contract));
        }

    }
}
