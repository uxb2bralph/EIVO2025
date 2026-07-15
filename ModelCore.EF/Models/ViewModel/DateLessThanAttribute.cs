using System;
using System.ComponentModel.DataAnnotations;

namespace ModelCore.Models.ViewModel
{
    /// <summary>
    /// 驗證屬性：目前欄位（日期）不得大於指定的比較欄位（遷移自 WebHome.Models.ViewModel.DateLessThanAttribute）。
    /// 置於共用模型層（ModelCore.EF），供 InvoiceNumberApply 等模型使用。
    /// </summary>
    public class DateLessThanAttribute : ValidationAttribute
    {
        private readonly string _comparisonProperty;

        public DateLessThanAttribute(string comparisonProperty)
        {
            _comparisonProperty = comparisonProperty;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            ErrorMessage = ErrorMessageString;
            //wait to do...判斷空值
            if (value == null)
                return ValidationResult.Success;

            var currentValue = (DateTime)value;

            var property = validationContext.ObjectType.GetProperty(_comparisonProperty);

            if (property == null)
                throw new ArgumentException("Property with this name not found");

            var comparisonValue = (DateTime)property.GetValue(validationContext.ObjectInstance);

            return (currentValue > comparisonValue) ? new ValidationResult(ErrorMessage) : ValidationResult.Success;
        }
    }
}
