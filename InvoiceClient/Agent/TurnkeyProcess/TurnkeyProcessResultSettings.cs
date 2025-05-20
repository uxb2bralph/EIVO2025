using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonLib.Core.Utility;
using CommonLib.Utility;
using CommonLib.Utility.Properties;

namespace InvoiceClient.Agent.TurnkeyProcess
{
    public partial class TurnkeyProcessResultSettings : AppSettingsBase
    {
        static TurnkeyProcessResultSettings()
        {
            _default = Initialize<TurnkeyProcessResultSettings>(typeof(TurnkeyProcessResultSettings).Namespace);
        }

        public TurnkeyProcessResultSettings() : base()
        {
            MessageResponseGood = new Dictionary<string, string>();
            MessageResponseFailed = new Dictionary<string, string>();
            foreach (var item in ResultMessageType)
            {
                MessageResponseGood.Add(item, Path.Combine(Logger.LogPath, "TurnkeyProcessResult", item, "Good").CheckStoredPath());
                MessageResponseFailed.Add(item, Path.Combine(Logger.LogPath, "TurnkeyProcessResult", item, "Failed").CheckStoredPath());
            }
        }

        static TurnkeyProcessResultSettings _default;
        public static TurnkeyProcessResultSettings Default
        {
            get
            {
                return _default;
            }
        }

        public static void Reload()
        {
            Reload<TurnkeyProcessResultSettings>(ref _default, typeof(TurnkeyProcessResultSettings).Namespace);
        }

        public String[] ResultMessageType { get; set; } =
        {
            "C0401","C0501","D0401","D0501","A0401","A0501","B0401","B0501","C0701","E0501","A0101"
        };

        public Dictionary<String, String> MessageResponseGood { get; private set; }
        public Dictionary<String, String> MessageResponseFailed { get; private set; }
        public String SummaryResultPath { get; set; } = Path.Combine(Logger.LogPath, "SummaryResult");

    }

}
