using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TaskCenter.Core.DTOs
{
    /// <summary>
    /// 側邊選單群組（對應前端 DefaultLayout 的 { label, icon, items }）。
    /// </summary>
    /// <remarks>
    /// 同時作為 App.settings.json 的設定物件（Newtonsoft 以 PascalCase 讀寫）與
    /// 登入 API 的回傳 DTO（System.Text.Json 依 <see cref="JsonPropertyNameAttribute"/> 輸出 camelCase）。
    /// </remarks>
    public class MenuGroupDto
    {
        [JsonPropertyName("label")]
        public string Label { get; set; } = string.Empty;

        [JsonPropertyName("icon")]
        public string Icon { get; set; } = string.Empty;

        [JsonPropertyName("items")]
        public List<MenuItemDto> Items { get; set; } = new();
    }

    /// <summary>
    /// 選單項目（對應前端 { label, href }）。
    /// </summary>
    public class MenuItemDto
    {
        [JsonPropertyName("label")]
        public string Label { get; set; } = string.Empty;

        [JsonPropertyName("href")]
        public string Href { get; set; } = string.Empty;
    }
}
