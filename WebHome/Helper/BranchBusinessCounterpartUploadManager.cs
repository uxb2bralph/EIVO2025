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
using ModelCore.Helper;

namespace WebHome.Helper
{
    public class BranchBusinessCounterpartUploadManager : CsvUploadManager<EIVOEntityDataContext, OrganizationBranch, ItemUpload<OrganizationBranch>>
    {
        public BranchBusinessCounterpartUploadManager() : base() {

        }
        public BranchBusinessCounterpartUploadManager(GenericManager<EIVOEntityDataContext> manager)
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

        protected override void initialize()
        {
            __COLUMN_COUNT = 6;
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

        protected override bool validate(ItemUpload<OrganizationBranch> item)
        {
            String[] column = item.Columns;

            if (string.IsNullOrEmpty(column[0]))
            {
                item.Status = String.Join("、", item.Status, "營業人名稱格式錯誤");
                _bResult = false;
            }

            if (column[1].Length > 8 || !column[1].ValidateString(20))
            {
                item.Status = String.Join("、", item.Status, "統編格式錯誤");
                _bResult = false;
            }
            else if (column[1].Length == 0)
            {
                column[1] = "0000000000";
            }
            else if (column[1].Length < 8)
            {
                column[1] = ("0000000" + column[1]).Right(8);
            }

            //if (string.IsNullOrEmpty(column[2]) || !ValidityAgent.ValidateString(column[2], 16))
            //{
            //    item.Status = String.Join("、", item.Status, "聯絡人電子郵件格式錯誤");
            //    _bResult = false;
            //}

            //if (string.IsNullOrEmpty(column[3]))
            //{
            //    item.Status = String.Join("、", item.Status, "地址格式錯誤");
            //    _bResult = false;
            //}

            //if (string.IsNullOrEmpty(column[4]))
            //{
            //    item.Status = String.Join("、", item.Status, "電話格式錯誤");
            //    _bResult = false;
            //}

            if (string.IsNullOrEmpty(column[5]))
            {
                item.Status = String.Join("、", item.Status, "店號錯誤");
                _bResult = false;
            }

            item.Entity = this.EntityList.Where(o => o.BranchNo == column[5]).FirstOrDefault();
            if (item.Entity != null)
            {
                if (item.Entity.Organization.ReceiptNo != column[1])
                {
                    if (item.Entity.Organization.OrganizationBranch.Count == 1)
                    {
                        int relativeID = item.Entity.CompanyID;
                        this.DeleteAllOnSubmit<BusinessRelationship>(r => r.MasterID == _masterID && r.RelativeID == relativeID);
                    }
                    int branchID = item.Entity.BranchID;
                    this.DeleteAllOnSubmit<OrganizationBranch>(b => b.BranchID == branchID);
                    item.Entity = null;
                }
            }

            if(item.Entity == null)     //新的分店
            {
                item.Entity = new OrganizationBranch
                {
                    BranchNo = column[5]
                };
                this.GetTable<OrganizationBranch>().InsertOnSubmit(item.Entity);

                var orgItem = _items.Where(i => i.Entity.Organization.ReceiptNo == column[1])
                    .Select(i => i.Entity.Organization).FirstOrDefault();

                if (orgItem == null)
                {
                    orgItem = this.GetTable<Organization>().Where(o => o.ReceiptNo == column[1]).FirstOrDefault();
                }

                if(orgItem==null)       //新的相對營業人
                {

                    orgItem = new Organization
                    {
                        OrganizationStatus = new OrganizationStatus
                        {
                            CurrentLevel = (int)Naming.MemberStatusDefinition.Checked
                        },
                        OrganizationExtension = new OrganizationExtension { }
                    };

                    this.GetTable<Organization>().InsertOnSubmit(orgItem);

                    var relationship = new BusinessRelationship
                    {
                        Counterpart = orgItem,
                        BusinessID = (int)BusinessType,
                        MasterID = _masterID.Value,
                        CurrentLevel = (int)Naming.MemberStatusDefinition.Checked
                    };
                    orgItem.RelativeRelation.Add(relationship);

                    var orgaCate = new OrganizationCategory
                    {
                        Organization = orgItem,
                        CategoryID = (int)Naming.CategoryID.COMP_E_INVOICE_B2C_BUYER
                    };
                    orgItem.OrganizationCategory.Add(orgaCate);

                    orgItem.OrganizationBranch.Add(item.Entity);
                    item.Entity.Organization = orgItem;

                    checkUser(column, orgaCate, column[5]);

                }
                else
                {
                    if(!orgItem.RelativeRelation.ToList().Any(r=>r.MasterID==_masterID))
                    {
                        var relation = new BusinessRelationship
                        {
                            Counterpart = orgItem,
                            BusinessID = (int)BusinessType,
                            MasterID = _masterID.Value,
                            CurrentLevel = (int)Naming.MemberStatusDefinition.Checked
                        };
                        this.GetTable<BusinessRelationship>().InsertOnSubmit(relation);
                        orgItem.RelativeRelation.Add(relation);
                    }

                    var orgaCate = orgItem.OrganizationCategory.ToList()
                        .Where(c => c.CategoryID == (int)Naming.CategoryID.COMP_E_INVOICE_B2C_BUYER).FirstOrDefault();

                    if (orgaCate == null)
                    {
                        orgaCate = new OrganizationCategory
                        {
                            CategoryID = (int)Naming.CategoryID.COMP_E_INVOICE_B2C_BUYER,
                            Organization = orgItem
                        };

                        this.GetTable<OrganizationCategory>().InsertOnSubmit(orgaCate);
                        orgItem.OrganizationCategory.Add(orgaCate);
                    }

                    //var currentUser = checkUser(column, orgaCate, column[5].GetEfficientString() ?? column[1]);
                    //currentUser.Phone = column[4];
                    //currentUser.EMail = column[2];
                    //currentUser.Address = column[3];

                    orgItem.OrganizationBranch.Add(item.Entity);
                    item.Entity.Organization = orgItem;

                    checkUser(column, orgaCate, column[5]);
                }

            }


            item.Entity.BranchName = column[0];
            item.Entity.ContactEmail = column[2];
            item.Entity.Addr = column[3];
            item.Entity.Phone = column[4];

            item.Entity.Organization.CompanyName = column[0];
            item.Entity.Organization.ReceiptNo = column[1];
            item.Entity.Organization.ContactEmail = column[2];
            item.Entity.Organization.Addr = column[3];
            item.Entity.Organization.Phone = column[4];
            if (item.Entity.Organization.OrganizationExtension == null)
            {
                item.Entity.Organization.OrganizationExtension = new OrganizationExtension { };
            }
            item.Entity.Organization.OrganizationExtension.CustomerNo = column[5];

            return _bResult;
        }

        private UserProfile checkUser(string[] column, OrganizationCategory orgaCate,String pid)
        {
            var userProfile = this.GetTable<UserProfile>().Where(u => u.PID == pid).FirstOrDefault();
            if (userProfile == null)
            {
                userProfile = new UserProfile
                {
                    PID = pid,
                    Phone = column[4],
                    EMail = column[2],
                    Address = column[3],
                    Password2 = ValidityAgent.HashPassword(pid),
                    UserProfileExtension = new UserProfileExtension
                    {
                        IDNo = column[1]
                    },
                    UserProfileStatus = new UserProfileStatus
                    {
                        CurrentLevel = (int)Naming.MemberStatusDefinition.Wait_For_Check
                    }
                };

                userProfile.MailID = userProfile.EMail.GetEfficientString()?
                    .Split(';', ',', ',')?[0];

                _userList.Add(userProfile);

            }
            else
            {
                this.DeleteAllOnSubmit<UserRole>(r => r.UID == userProfile.UID);
            }

            this.GetTable<UserRole>().InsertOnSubmit(new UserRole
            {
                RoleID = (int)Naming.RoleID.分店相對營業人,
                UserProfile = userProfile,
                OrganizationCategory = orgaCate
            });

            return userProfile;
        }
    }
}