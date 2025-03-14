using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommonLib.DataAccess;
using ModelCore.DataEntity;
using ModelCore.Helper;
using ModelCore.InvoiceManagement.Validator;
using ModelCore.InvoiceManagement.zhTW;
using ModelCore.Locale;
using ModelCore.Schema.EIVO;
using CommonLib.Utility;
using CommonLib.Core.Utility;

namespace ModelCore.InvoiceManagement
{
    public partial class InvoiceEnterpriseManager : EIVOEntityManager<Organization>
    {
        public InvoiceEnterpriseManager() : base() { }
        public InvoiceEnterpriseManager(GenericManager<EIVOEntityDataContext> mgr) : base(mgr) { }

        public static Action<Organization> CreateMatchedDefaultUser;

        public Dictionary<int, Exception> SaveInvoiceEnterprise(InvoiceEnterpriseRoot item, OrganizationToken token)
        {
            Dictionary<int, Exception> result = new Dictionary<int, Exception>();

            if (item != null && item.InvoiceEnterprise != null && item.InvoiceEnterprise.Length > 0)
            {
                EventItems = new List<Organization>();
                var agentTable = this.GetTable<InvoiceIssuerAgent>();

                for (int idx = 0; idx < item.InvoiceEnterprise.Length; idx++)
                {
                    try
                    {
                        var enterprise = item.InvoiceEnterprise[idx];
                        bool isNewItem = false;

                        _entity = this.EntityList.Where(o => o.ReceiptNo == enterprise.SellerId).FirstOrDefault();
                        if (_entity == null)
                        {
                            _entity = new Organization
                            {
                                OrganizationStatus = new OrganizationStatus
                                {
                                    CurrentLevel = (int)Naming.MemberStatusDefinition.Checked
                                }

                            };

                            _entity.OrganizationCategory.Add(new OrganizationCategory
                            {
                                CategoryID = (int)CategoryDefinition.CategoryEnum.發票開立營業人
                            });
                            isNewItem = true;
                        }

                        _entity.ReceiptNo = enterprise.SellerId;
                        _entity.CompanyName = enterprise.SellerName.GetEfficientString();
                        _entity.Addr = enterprise.Address.GetEfficientString();
                        _entity.Phone = enterprise.TEL.GetEfficientString();
                        _entity.UndertakerName = enterprise.UndertakerName.GetEfficientString();
                        _entity.ContactName = enterprise.ContactName.GetEfficientString();
                        _entity.ContactPhone = enterprise.ContactPhone.GetEfficientString();
                        _entity.ContactMobilePhone = enterprise.ContactMobilePhone.GetEfficientString();
                        _entity.ContactEmail = enterprise.Email.GetEfficientString();
                        _entity.OrganizationStatus.DownloadDataNumber = true;
                        _entity.OrganizationStatus.PrintAll = false;
                        _entity.OrganizationStatus.SettingInvoiceType = enterprise.InvoiceType;

                        Exception ex = _entity.OrganizationValueCheck();
                        if (ex != null)
                        {
                            result.Add(idx, ex);
                            continue;
                        }

                        if (isNewItem)
                        {
                            this.EntityList.InsertOnSubmit(_entity);
                            this.SubmitChanges();

                            if (CreateMatchedDefaultUser != null)
                            {
                                CreateMatchedDefaultUser(_entity);
                            }
                        }
                        else
                        {
                            this.SubmitChanges();
                        }


                        if (!agentTable.Any(a => a.AgentID == token.CompanyID
                            && a.IssuerID == _entity.CompanyID))
                        {
                            agentTable.InsertOnSubmit(new InvoiceIssuerAgent 
                            {
                                AgentID = token.CompanyID,
                                IssuerID = _entity.CompanyID
                            });
                            this.SubmitChanges();
                        }

                        EventItems.Add(_entity);

                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                        result.Add(idx, ex);

                        //var changes = this._db.GetChangeSet();
                        //changes.Inserts.Clear();
                        //changes.Updates.Clear();
                        //changes.Deletes.Clear();

                    }
                }

            }

            return result;
        }


    }
}
