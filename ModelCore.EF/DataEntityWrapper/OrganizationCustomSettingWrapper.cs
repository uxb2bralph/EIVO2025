using CommonLib.Utility;
using ModelCore.DataEntity;
using ModelCore.Models.ViewModel;
using Newtonsoft.Json;

namespace ModelCore.DataEntityWrapper
{
    public class OrganizationCustomSettingWrapper : EntityWrapper<OrganizationCustomSetting>
    {
        private OrganizationCustomSettingsModel? _settings;

        public OrganizationCustomSettingWrapper(OrganizationCustomSetting entity) : base(entity)
        {
        }

        public virtual OrganizationCustomSettingsModel Settings
        {
            get
            {
                if (_settings == null)
                {
                    if (Entity.SettingData != null)
                    {
                        _settings = JsonConvert.DeserializeObject<OrganizationCustomSettingsModel>(Entity.SettingData);
                    }
                }

                if (_settings == null)
                {
                    _settings = new OrganizationCustomSettingsModel { };
                    Accept();
                }

                return _settings;
            }
        }

        public void Accept()
        {
            Entity.SettingData = _settings?.JsonStringify();
        }
    }
}
