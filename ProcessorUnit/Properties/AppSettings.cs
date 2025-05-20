using CommonLib.Utility;
using CommonLib.Utility.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessorUnit.Properties
{
    public partial class AppSettings : AppSettingsBase
    {
        static AppSettings()
        {
            _default = Initialize<AppSettings>(typeof(AppSettings).Namespace);
        }

        public AppSettings() : base()
        {

        }

        static AppSettings _default;
        public static AppSettings Default => _default;

        public static void Reload()
        {
            Reload<AppSettings>(ref _default, typeof(AppSettings).Namespace);
        }

        public Guid InstanceID { get; set; } = Guid.NewGuid();
        public int ProcessorID { get; set; }
        public String? ResponsePath { get; set; }
        public String? PersistenceModelPath { get; set; } = "Persistence";
        public String? EIVOPortal { get; set; } = "http://localhost:2598";
    }
}
