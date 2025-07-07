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
        [
            "E0501","A0101", "A0102","A0301", "A0302", "A0201", "A0202", "B0101", "B0102", "B0201", "B0202"
        ];

        public Dictionary<String, String> MessageResponseGood { get; private set; }
        public Dictionary<String, String> MessageResponseFailed { get; private set; }
        public String SummaryResultPath { get; set; } = Path.Combine(Logger.LogPath, "SummaryResult");

    }

}
