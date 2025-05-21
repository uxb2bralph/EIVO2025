using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;



using ModelCore.DataEntity;
using ModelCore.Helper;
using ModelCore.InvoiceManagement.ErrorHandle;
using ModelCore.InvoiceManagement.InvoiceProcess;
using ModelCore.InvoiceManagement.Validator;
using ModelCore.InvoiceManagement.zhTW;
using ModelCore.Locale;
using ModelCore.Schema.EIVO;
using CommonLib.Utility;
using CommonLib.DataAccess;
using CommonLib.Core.Utility;

namespace ModelCore.InvoiceManagement
{
    public class InvoiceManagerV3 : InvoiceManagerV2
    {
        public InvoiceManagerV3() : base() { }
        public InvoiceManagerV3(GenericManager<EIVOEntityDataContext> mgr) : base(mgr) { }
        public bool HasError { get; set; }

        protected void PushProcessExceptionNotification(ProcessRequest requestItem, Organization notified)
        {
            if (requestItem != null && notified != null)
            {
                if (!this.GetTable<ProcessExceptionNotification>().Any(n => n.TaskID == requestItem.TaskID && n.CompanyID == notified.CompanyID))
                {
                    this.GetTable<ProcessExceptionNotification>().InsertOnSubmit(
                        new ProcessExceptionNotification
                        {
                            TaskID = requestItem.TaskID,
                            CompanyID = notified.CompanyID,
                        }
                        );
                    this.SubmitChanges();
                }
            }
        }
    }
}