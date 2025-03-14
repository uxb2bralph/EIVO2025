using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;


using ModelCore.DataEntity;
using ModelCore.Locale;
using ModelCore.UploadManagement;
using CommonLib.Utility;
using WebHome.Properties;
using CommonLib.DataAccess;
using ModelCore.Models.ViewModel;
using ModelCore.Helper;

namespace WebHome.Helper
{
    public class BusinessCounterpartUploadManager : CsvUploadManager<EIVOEntityDataContext, Organization, ItemUpload<Organization>>
    {
        public BusinessCounterpartUploadManager() : base()
        {

        }

        public BusinessCounterpartUploadManager(GenericManager<EIVOEntityDataContext> manager)
            : base(manager)
        {
        }

        public Naming.InvoiceCenterBusinessType BusinessType { get; set; }

        private int? _masterID;
        private List<UserProfile> _userList;

        public int? MasterID
        {
            get
            {
                return _masterID;
            }
            set
            {
                _masterID = value;
            }
        }

        public override void ParseData(UserProfile userProfile, string fileName, System.Text.Encoding encoding)
        {
            _userProfile = userProfile;
            if (!_masterID.HasValue)
                _masterID = _userProfile.CurrentUserRole.OrganizationCategory.CompanyID;
            base.ParseData(userProfile, fileName, encoding);
        }

        public enum FieldIndex
        {
            Business = 0,
            ReceiptNo = 1,
            Email = 2,
            Address = 3,
            Phone = 4,
        }

        protected override void initialize()
        {
            __COLUMN_COUNT = 5;
            _userList = new List<UserProfile>();
        }

        protected override void doSave()
        {
            this.SubmitChanges();

            var enterprise = this.GetTable<Organization>().Where(o => o.CompanyID == _masterID)
                .FirstOrDefault().EnterpriseGroupMember.FirstOrDefault();

            String subject = (enterprise != null ? enterprise.EnterpriseGroup.EnterpriseName : "") + " 會員啟用認證信";

            ThreadPool.QueueUserWorkItem(p =>
            {
                foreach (var u in _userList)
                {
                    try
                    {
                        u.NotifyToActivate();
                    }
                    catch (Exception ex)
                    {
                        CommonLib.Core.Utility.FileLogger.Logger.Warn("［" + subject + "］傳送失敗,原因 => " + ex.Message);
                        CommonLib.Core.Utility.FileLogger.Logger.Error(ex);
                    }
                }
            });
        }

        protected override bool validate(ItemUpload<Organization> item)
        {
            String[] column = item.Columns;
            BusinessRelationshipViewModel viewModel = new BusinessRelationshipViewModel
            {
                MasterID = _masterID.Value,
                ReceiptNo = column[(int)FieldIndex.ReceiptNo].GetEfficientString(),
                CompanyName = column[(int)FieldIndex.Business].GetEfficientString(),
                ContactEmail = column[(int)FieldIndex.Email].GetEfficientString(),
                Addr = column[(int)FieldIndex.Address].GetEfficientString(),
                Phone = column[(int)FieldIndex.Phone].GetEfficientString(),
            };

            if (viewModel.CompanyName==null)
            {
                item.Status = String.Join("、", item.Status, "營業人名稱格式錯誤");
                _bResult = false;
            }

            if (viewModel.ReceiptNo==null || viewModel.ReceiptNo.Length != 8 || !ValidityAgent.ValidateString(viewModel.ReceiptNo, 20))
            {
                item.Status = String.Join("、", item.Status, "統編格式錯誤");
                _bResult = false;
            }

            if (viewModel.ContactEmail == null || !viewModel.ContactEmail.ValidateString(16))
            {
                item.Status = String.Join("、", item.Status, "聯絡人電子郵件格式錯誤");
                _bResult = false;
            }

            if (viewModel.Addr == null)
            {
                item.Status = String.Join("、", item.Status, "地址格式錯誤");
                _bResult = false;
            }

            if (viewModel.Phone == null)
            {
                item.Status = String.Join("、", item.Status, "電話格式錯誤");
                _bResult = false;
            }

            BusinessRelationship relationship = null;
            item.Entity = this.EntityList.Where(o => o.ReceiptNo == viewModel.ReceiptNo).FirstOrDefault();

            if (item.Entity == null)
            {
                item.Entity = _items.Where(t => t.Entity != null && t.Entity.ReceiptNo == viewModel.ReceiptNo).FirstOrDefault()?.Entity;

                if (item.Entity == null)
                {
                    item.Entity = new Organization
                    {
                        OrganizationStatus = new OrganizationStatus
                        {
                            CurrentLevel = (int)Naming.MemberStatusDefinition.Checked
                        },
                        ReceiptNo = viewModel.ReceiptNo,
                        OrganizationExtension = new OrganizationExtension
                        {
                            CustomerNo = viewModel.CustomerNo,
                        },
                        CompanyName = viewModel.CompanyName,
                        Addr = viewModel.Addr,
                        Phone = viewModel.Phone,
                        ContactEmail = viewModel.ContactEmail,
                    };

                    relationship = new BusinessRelationship
                    {
                        Counterpart = item.Entity,
                        BusinessID = (int)BusinessType,
                        MasterID = _masterID.Value,
                        CurrentLevel = (int)Naming.MemberStatusDefinition.Checked
                    };

                    var orgaCate = new OrganizationCategory
                    {
                        Organization = item.Entity,
                        CategoryID = (int)CategoryDefinition.CategoryEnum.相對營業人
                    };

                    this.EntityList.InsertOnSubmit(item.Entity);

                    var userProfile = new UserProfile
                    {
                        PID = viewModel.ReceiptNo,
                        Phone = viewModel.Phone,
                        EMail = viewModel.ContactEmail,
                        Address = viewModel.Addr,
                        UserProfileExtension = new UserProfileExtension
                        {
                            IDNo = viewModel.ReceiptNo
                        },
                        UserProfileStatus = new UserProfileStatus
                        {
                            CurrentLevel = (int)Naming.MemberStatusDefinition.Wait_For_Check
                        }
                    };
                    userProfile.MailID = userProfile.EMail.GetEfficientString()?
                        .Split(';', ',', ',')?[0];

                    _userList.Add(userProfile);

                    this.GetTable<UserRole>().InsertOnSubmit(new UserRole
                    {
                        RoleID = (int)Naming.RoleID.相對營業人,
                        UserProfile = userProfile,
                        OrganizationCategory = orgaCate
                    });
                }
            }
            else
            {
                if (!this.GetTable<BusinessRelationship>().Any(r => r.MasterID == _masterID && r.BusinessID == (int)BusinessType && r.RelativeID == item.Entity.CompanyID))
                {
                    relationship = new BusinessRelationship
                    {
                        Counterpart = item.Entity,
                        BusinessID = (int)BusinessType,
                        MasterID = _masterID.Value
                    };
                }

                //var currentUser = this.GetTable<UserProfile>().Where(u => u.PID == viewModel.ReceiptNo).FirstOrDefault();
                //if (currentUser != null)
                //{
                //    currentUser.Phone = viewModel.Phone;
                //    currentUser.EMail = viewModel.ContactEmail;
                //    currentUser.Address = viewModel.Addr;
                //}
            }

            if (relationship != null)
            {
                relationship.CompanyName = viewModel.CompanyName;
                relationship.ContactEmail = viewModel.ContactEmail;
                relationship.Addr = viewModel.Addr;
                relationship.Phone = viewModel.Phone;
            }

            return _bResult;
        }
    }
}