using System.Text.Json.Serialization;

namespace TaskCenter.Core.DTOs
{
    /// <summary>
    /// 店家 POS 機資料（對應舊版 InvoiceBusiness/POSDevice 檢視項目）。
    /// 全域序列化為 PascalCase，故逐欄位以 [JsonPropertyName] 指定 camelCase。
    /// </summary>
    public class POSDeviceDto
    {
        /// <summary>POS 機序號（POSDevice.DeviceID，新增時由後端配號）</summary>
        [JsonPropertyName("deviceId")]
        public int DeviceId { get; set; }

        /// <summary>POS 機編號（POSDevice.POSNo）</summary>
        [JsonPropertyName("posNo")]
        public string? PosNo { get; set; }
    }

    /// <summary>
    /// 新增 / 編輯店家 POS 機的請求（對應舊版 InvoiceBusiness.CommitPOS）。
    /// 沿用舊版以加密 KeyID 傳遞 CompanyID 的做法，避免外露原始 CompanyID。
    /// </summary>
    public class CommitPOSDeviceDto
    {
        /// <summary>加密後的 CompanyID（來自列表 keyId 欄位）</summary>
        [JsonPropertyName("keyId")]
        public string? KeyId { get; set; }

        /// <summary>要編輯的 POS 機序號；新增時為 null</summary>
        [JsonPropertyName("deviceId")]
        public int? DeviceId { get; set; }

        /// <summary>POS 機編號</summary>
        [JsonPropertyName("posNo")]
        public string? PosNo { get; set; }
    }
}
