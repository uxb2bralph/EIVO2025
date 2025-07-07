using ApplicationResource;

using DocumentFormat.OpenXml.Wordprocessing;
using WebHome.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using WebHome.Models.Validation;

namespace WebHome.Models.ViewModel
{
    public class InvoiceNumberApplyViewModel
    {

        public InvoiceNumberApplyViewModel(InvoiceNumberApply apply)
        {
            Apply = apply;
        }

        public InvoiceNumberApplyViewModel()
        {
            IsTransferToOrganization = false;
        }

        [Display(Name = "ValidCode")]
        [CaptchaValidation("EncryptedCode", ErrorMessage = "驗證碼錯誤!!")]
        public string? ValidCode { get; set; }

        [Display(Name = "EncryptedCode")]
        public string? EncryptedCode { get; set; }
        public bool? IsTransferToOrganization { get; set; }
        public bool IsApplyB2B { get; set; } = false;

        public InvoiceNumberApply Apply { get; set; }
        public IEnumerable<ValueObject> GetSysSupplierList()
        {
            return
                AppSettings.Default.InvoiceNumberApplySetting.SysSupplier
                .Select(x => new ValueObject
                {
                    ID = x.ID
                ,
                    Value = x.BusinessID
                });
        }

        public IEnumerable<ValueObject> GetPaperTestList()
        {
            return 
            AppSettings.Default.InvoiceNumberApplySetting.PaperTestSet
                .Select(x => new ValueObject
                {
                    ID = x.ID,
                    Value = x.TestBusinessName
                });
        }

    }

    public class ValueObject
    {
        public string ID { get; set; }
        public string Value { get; set; }
    }

}