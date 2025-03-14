using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

using ModelCore.Models.ViewModel;
using WebHome.Models.Validation;

namespace WebHome.Models.ViewModel
{
    public class CbsLoginViewModel : UserProfileViewModel
    {
        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }

        public String? ReturnUrl { get; set; }
    }
    public class LoginViewModel : CbsLoginViewModel
    {

        [Display(Name = "ValidCode")]
        [CaptchaValidation("EncryptedCode", ErrorMessage = "驗證碼錯誤!!")]
        public string ValidCode { get; set; }

        [Display(Name = "EncryptedCode")]
        public string EncryptedCode { get; set; }

        public String Token { get; set; }
        public string CardNo1 { get; set; }
        public string CardNo2 { get; set; }
        public string CardType { get; set; }
        public string Signature { get; set; }
    }

    public class TwoFactorViewModel : QueryViewModel
    {
        public int? UID { get; set; }
        public String CodeDigit { get; set; }
    }
}