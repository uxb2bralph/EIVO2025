using System.Text.Json.Serialization;

namespace TaskCenter.Core.DTOs
{
    /// <summary>
    /// 用戶端 G/W 設定（遷移自 WebHome Organization/GatewaySettings 對話框）。
    /// 目前僅涵蓋「傳送 Excel 發票開立方式」（OrganizationStatus.InvoiceClientDefaultProcessType）。
    /// 全域序列化為 PascalCase，故逐欄位以 [JsonPropertyName] 指定 camelCase。
    /// </summary>
    public class GatewaySettingsDto
    {
        /// <summary>加密後的 CompanyID（沿用舊版 KeyID 做法）；儲存時後端會解密還原 CompanyID。</summary>
        [JsonPropertyName("keyId")]
        public string? KeyId { get; set; }

        /// <summary>
        /// 傳送 Excel 發票開立方式（對應 Naming.InvoiceProcessType 之 Xlsx 系列）。
        /// null 表示尚未設定。
        /// </summary>
        [JsonPropertyName("defaultProcessType")]
        public int? DefaultProcessType { get; set; }

        /// <summary>
        /// 目前已設定的 PKCS12(PFX) 憑證金鑰（對應 OrganizationToken.KeyID）。
        /// null 表示尚未設定憑證。
        /// </summary>
        [JsonPropertyName("certificateKeyId")]
        public string? CertificateKeyId { get; set; }
    }
}
