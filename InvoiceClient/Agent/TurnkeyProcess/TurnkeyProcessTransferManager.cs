using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Globalization;

using InvoiceClient.Properties;
using ModelCore.Schema.EIVO;
using InvoiceClient.Agent;
using InvoiceClient.Agent.POSHelper;
using InvoiceClient.Agent.MIGHelper;
using InvoiceClient.Agent.CsvRequestHelper;
using InvoiceClient.Helper;
using InvoiceClient.TransferManagement;
using ModelCore.InvoiceManagement;
using CommonLib.Core.Utility;
using CommonLib.Utility;

namespace InvoiceClient.Agent.TurnkeyProcess
{
    public class TurnkeyProcessTransferManager : ITransferManager
    {
        private InvoiceWatcher _SummaryResultWatcher;

        public ITabWorkItem WorkItem { get; set; }

        public TurnkeyProcessTransferManager()
        {
            InvoiceWatcher watcher;
            foreach (var msgType in TurnkeyProcessResultSettings.Default.ResultMessageType)
            {
                switch (msgType)
                {
                    //case "C0401":
                    //case "A0401":
                    //    watcher = new C0401ResultWatcher(TurnkeyProcessResultSettings.Default.MessageResponseGood[msgType])
                    //    {
                    //        TransferManager = this,
                    //    };
                    //    watcher.StartUp();

                    //    watcher = new C0401FailedResultWatcher(TurnkeyProcessResultSettings.Default.MessageResponseFailed[msgType])
                    //    {
                    //        TransferManager = this,
                    //    };
                    //    watcher.StartUp();

                    //    break;

                    //case "C0501":
                    //case "A0501":
                    //    watcher = new C0501ResultWatcher(TurnkeyProcessResultSettings.Default.MessageResponseGood[msgType])
                    //    {
                    //        TransferManager = this,
                    //    };
                    //    watcher.StartUp();

                    //    watcher = new C0501FailedResultWatcher(TurnkeyProcessResultSettings.Default.MessageResponseFailed[msgType])
                    //    {
                    //        TransferManager = this,
                    //    };
                    //    watcher.StartUp();

                    //    break;
                    //case "D0401":
                    //case "B0401":
                    //    watcher = new D0401ResultWatcher(TurnkeyProcessResultSettings.Default.MessageResponseGood[msgType])
                    //    {
                    //        TransferManager = this,
                    //    };
                    //    watcher.StartUp();

                    //    watcher = new D0401FailedResultWatcher(TurnkeyProcessResultSettings.Default.MessageResponseFailed[msgType])
                    //    {
                    //        TransferManager = this,
                    //    };
                    //    watcher.StartUp();

                    //    break;
                    //case "D0501":
                    //case "B0501":
                    //    watcher = new D0501ResultWatcher(TurnkeyProcessResultSettings.Default.MessageResponseGood[msgType])
                    //    {
                    //        TransferManager = this,
                    //    };
                    //    watcher.StartUp();

                    //    watcher = new D0501FailedResultWatcher(TurnkeyProcessResultSettings.Default.MessageResponseFailed[msgType])
                    //    {
                    //        TransferManager = this,
                    //    };
                    //    watcher.StartUp();

                    //    break;
                    case "E0501":
                        watcher = new E0501Watcher(TurnkeyProcessResultSettings.Default.MessageResponseGood[msgType])
                        {
                            TransferManager = this,
                        };
                        watcher.StartUp();

                        break;

                    case "A0101":
                        watcher = new A0101Watcher(TurnkeyProcessResultSettings.Default.MessageResponseGood[msgType])
                        {
                            TransferManager = this,
                        };
                        watcher.StartUp();

                        break;
                }
            }

            //JobHelper.Tasks.CheckTurnkeyLog.Notify();
            //EIVOPlatformFactory.CheckTurnkey.Notify();
        }

        public void EnableAll(String fullPath)
        {
            //_SummaryResultWatcher = new SummaryResultWatcher(TurnkeyProcessResultSettings.Default.SummaryResultPath)
            //{
            //    TransferManager = this,
            //};
            //_SummaryResultWatcher.StartUp();
        }

        public void PauseAll()
        {
            //_SummaryResultWatcher?.Dispose();
        }

        public String ReportError()
        {
            StringBuilder sb = new StringBuilder();

            if (_SummaryResultWatcher != null)
                sb.Append(_SummaryResultWatcher.ReportError());

            return sb.ToString();

        }

        public void SetRetry()
        {
            //_SummaryResultWatcher.Retry();
        }

        public Type UIConfigType
        {
            get
            {
                //return typeof(InvoiceClient.MainContent.POSChannelConfig); 
                return null;
            }
        }

    }
}
