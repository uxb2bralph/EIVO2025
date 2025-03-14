using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

using ModelCore.DataEntity;
using ModelCore.Locale;
using ModelCore.Security.MembershipManagement;
using CommonLib.Utility;
using ModelCore.UploadManagement;
using CommonLib.DataAccess;


namespace ModelCore.InvoiceManagement
{
    public class CsvAllowanceCancellationUploadManager : CsvUploadManager<EIVOEntityDataContext, InvoiceAllowance, ItemUpload<InvoiceAllowance>>
    {
        private int _sellerID;

        public const String __Fields = "日期,折讓單號碼,買受人統編,作廢原因";

        public enum FieldIndex
        {
            日期 = 0,
            折讓單號碼 = 1,
            買受人統編 = 2,
            作廢原因 = 3
        }

        private DateTime _uploadCancellationDate;

        public CsvAllowanceCancellationUploadManager(GenericManager<EIVOEntityDataContext> manager,int sellerID)
            : base(manager)
        {
            _sellerID = sellerID;
        }

        public CsvAllowanceCancellationUploadManager(int sellerID)
            : this(null,sellerID)
        {

        }


        protected override void initialize()
        {
            __COLUMN_COUNT = 4; 
            _uploadCancellationDate = DateTime.Now;
        }

        public override void ParseData(UserProfile userProfile, string fileName, Encoding encoding)
        {
            base.ParseData(userProfile, fileName, encoding);
        }

        protected override void doSave()
        {
            this.SubmitChanges();
        }

        protected override bool validate(ItemUpload<InvoiceAllowance> item)
        {
            string[] column = item.Columns;

            String allowanceNo = column[(int)FieldIndex.折讓單號碼];
            item.Entity = this.EntityList.Where(i => i.AllowanceNumber == allowanceNo).FirstOrDefault();

            if (item.Entity == null)
            {
                item.Status = String.Join("、", item.Status, "折讓證明單不存在");
                _bResult = false;
            }
            else if (_items.Any(a => a.Columns[(int)FieldIndex.折讓單號碼] == column[(int)FieldIndex.折讓單號碼]))
            {
                item.Status = String.Join("、", item.Status, "作廢折讓證明單號碼重複匯入");
                _bResult = false;
            }
            else if (item.Entity.InvoiceAllowanceCancellation != null)
            {
                item.Status = String.Join("、", item.Status, "折讓證明單已作廢");
                _bResult = false;
            }

            if (String.IsNullOrEmpty(column[(int)FieldIndex.作廢原因]))
            {
                item.Status = String.Join("、", item.Status, "未指定作廢原因");
                _bResult = false;
            }

            DateTime dateValue;
            if (!DateTime.TryParseExact(column[(int)FieldIndex.日期], "yyyy/M/d", CultureInfo.CurrentCulture, DateTimeStyles.None, out dateValue))
            {
                item.Status = String.Join("、", item.Status, "日期錯誤");
                _bResult = false;
            }

            if (item.Entity != null)
            {
                if (String.IsNullOrEmpty(column[(int)FieldIndex.買受人統編]) || column[(int)FieldIndex.買受人統編]=="0000000000")
                {
                    if (!item.Entity.InvoiceAllowanceBuyer.IsB2C())
                    {
                        item.Status = String.Join("、", item.Status, "對方統編錯誤");
                        _bResult = false;
                    }
                }
                else
                {
                    if (column[(int)FieldIndex.買受人統編] != item.Entity.InvoiceAllowanceBuyer.ReceiptNo)
                    {
                        item.Status = String.Join("、", item.Status, "對方統編錯誤");
                        _bResult = false;
                    }
                }
            }

            if (item.Entity != null && item.Entity.InvoiceAllowanceCancellation == null)
            {
                item.Entity.InvoiceAllowanceCancellation = new InvoiceAllowanceCancellation
                {
                    CancelDate = dateValue,
                    //Remark = column[3]
                    CancelReason = column[(int)FieldIndex.作廢原因],
                };

                var doc = new DerivedDocument
                {
                    CDS_Document = new CDS_Document
                    {
                        DocType = (int)Naming.DocumentTypeDefinition.E_AllowanceCancellation,
                        DocDate = DateTime.Now,
                        DocumentOwner = new DocumentOwner
                        {
                            OwnerID = _sellerID
                        }
                    },
                    SourceID = item.Entity.AllowanceID
                };
                this.GetTable<DerivedDocument>().InsertOnSubmit(doc);
            }


            return _bResult;
        }

    }
}
