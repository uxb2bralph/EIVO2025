using ModelCore.DataEntity;
using ModelCore.Locale;
using ModelCore.Helper;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using CommonLib.Utility;
using ModelCore.InvoiceManagement;
using ClosedXML.Excel;
using System.Net;

using ProcessorUnit.Properties;
using ModelCore.Models.ViewModel;
using Business.Helper.ProcessRequestProcessor;
using CommonLib.Core.Helper;

namespace ProcessorUnit.Execution
{
    public class ZipInvoicePdfProcessor : ExecutorForeverBase
    {
        public ZipInvoicePdfProcessor()
        {

        }

        protected override void DoSomething()
        {
            RenderStyleViewModel? viewModel = PersistenceExtensions.Popup<RenderStyleViewModel>(null!, AppSettings.Default.PersistenceModelPath!.StoreTargetPath());
            if (viewModel != null)
            {
                viewModel.ZipInvoicePDF();
            }

            base.DoSomething();
        }

    }
}
