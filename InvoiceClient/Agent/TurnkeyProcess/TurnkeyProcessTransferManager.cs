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
        private InvoiceWatcher? _SummaryResultWatcher;

        public ITabWorkItem? WorkItem { get; set; }

        public TurnkeyProcessTransferManager()
        {
            InvoiceWatcher watcher;
            foreach (var msgType in TurnkeyProcessResultSettings.Default.ResultMessageType)
            {
                switch (msgType)
                {

                    case "E0501":
                        watcher = new E0501Watcher(TurnkeyProcessResultSettings.Default.MessageResponseGood[msgType])
                        {
                            TransferManager = this,
                        };
                        watcher.StartUp();

                        break;

                    case "A0101":
                        watcher = new ReceivedA0101Watcher(TurnkeyProcessResultSettings.Default.MessageResponseGood[msgType])
                        {
                            TransferManager = this,
                        };
                        watcher.StartUp();

                        break;

                    case "A0102":
                        watcher = new ReceivedA0102Watcher(TurnkeyProcessResultSettings.Default.MessageResponseGood[msgType])
                        {
                            TransferManager = this,
                        };
                        watcher.StartUp();

                        break;

                    case "A0301":
                        watcher = new ReceivedA0301Watcher(TurnkeyProcessResultSettings.Default.MessageResponseGood[msgType])
                        {
                            TransferManager = this,
                        };
                        watcher.StartUp();

                        break;

                    case "A0302":
                        watcher = new ReceivedA0302Watcher(TurnkeyProcessResultSettings.Default.MessageResponseGood[msgType])
                        {
                            TransferManager = this,
                        };
                        watcher.StartUp();

                        break;

                    case "A0201":
                        watcher = new ReceivedA0201Watcher(TurnkeyProcessResultSettings.Default.MessageResponseGood[msgType])
                        {
                            TransferManager = this,
                        };
                        watcher.StartUp();

                        break;

                    case "A0202":
                        watcher = new ReceivedA0202Watcher(TurnkeyProcessResultSettings.Default.MessageResponseGood[msgType])
                        {
                            TransferManager = this,
                        };
                        watcher.StartUp();

                        break;

                    case "B0101":
                        watcher = new ReceivedB0101Watcher(TurnkeyProcessResultSettings.Default.MessageResponseGood[msgType])
                        {
                            TransferManager = this,
                        };
                        watcher.StartUp();

                        break;

                    case "B0102":
                        watcher = new ReceivedB0102Watcher(TurnkeyProcessResultSettings.Default.MessageResponseGood[msgType])
                        {
                            TransferManager = this,
                        };
                        watcher.StartUp();

                        break;

                    case "B0201":
                        watcher = new ReceivedB0201Watcher(TurnkeyProcessResultSettings.Default.MessageResponseGood[msgType])
                        {
                            TransferManager = this,
                        };
                        watcher.StartUp();
                        break;

                    case "B0202":
                        watcher = new ReceivedB0202Watcher(TurnkeyProcessResultSettings.Default.MessageResponseGood[msgType])
                        {
                            TransferManager = this,
                        };
                        watcher.StartUp();
                        break;

                }
            }
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

        public Type? UIConfigType
        {
            get
            {
                //return typeof(InvoiceClient.MainContent.POSChannelConfig); 
                return null;
            }
        }

    }
}
