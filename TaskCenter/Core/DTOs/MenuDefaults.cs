using System.Collections.Generic;

namespace TaskCenter.Core.DTOs
{
    /// <summary>
    /// 角色選單的內建預設值（未在 App.settings.json 覆寫時使用）。
    /// roleId 以字串為鍵，對應 JSON 物件鍵值。
    /// </summary>
    public static class MenuDefaults
    {
        // 角色代碼（與前端 DefaultLayout.vue 一致）
        private const string RoleSys = "1";
        private const string RoleSeller = "51";
        private const string RoleBuyer = "52";
        private const string RoleNetworkSeller = "54";
        private const string RoleGoogleTw = "55";
        private const string RoleGroupMember = "61";
        private const string RoleRelativeEntity = "62";
        private const string RoleBranchEntity = "63";
        private const string RoleDataAuditor = "64";

        private static MenuItemDto Item(string label, string href) => new() { Label = label, Href = href };

        private static MenuGroupDto Group(string label, string icon, params MenuItemDto[] items) =>
            new() { Label = label, Icon = icon, Items = new List<MenuItemDto>(items) };

        /// <summary>ROLE_NETWORKSELLER、ROLE_GOOGLETW 及各群組角色共用的會員選單。</summary>
        private static List<MenuGroupDto> MemberMenu() => new()
        {
            Group("帳號設定", "fa-user",
                Item("帳號管理", "/UserProfile/EditMySelf"),
                Item("營業人資料管理", "/UserProfile/EditMyBusiness")),
            Group("查詢與報表", "fa-search",
                Item("資料查詢／列印／匯出", "/InvoiceProcess/Index"),
                Item("上期發票空白號碼查詢", "/InvoiceNo/VacantNoIndex"),
                Item("發票媒體申報檔查詢", "/InvoiceQuery/InvoiceMediaReport"),
                Item("下載MIG檔案", "/InvoiceProcess/InquireToMIG"),
                Item("發票統計表", "/InvoiceQuery/InvoiceSummary")),
        };

        public static Dictionary<string, List<MenuGroupDto>> Build()
        {
            var menus = new Dictionary<string, List<MenuGroupDto>>
            {
                [RoleSys] = new()
                {
                    Group("系統管理維護", "fa-cogs",
                        Item("電子發票字軌維護", "/TrackCode/Index"),
                        Item("期別匯率維護", "/PeriodicalExchangeRate/Index"),
                        Item("電子發票中獎號碼維護", "/WinningNumber/Index")),
                    Group("會員管理維護", "fa-users",
                        Item("使用者帳號管理", "/Account/AccountIndex"),
                        Item("營業人資料管理", "/OrganizationQuery"),
                        Item("相對營業人資料管理", "/BusinessRelationship/MaintainRelationship"),
                        Item("新登錄營業人資料受理", "/InvoiceNumberApply/QueryIndex")),
                    Group("發票作業", "fa-file-text-o",
                        Item("電子發票號碼維護", "/InvoiceNo/MaintainInvoiceNoInterval"),
                        Item("資料查詢／列印／匯出", "/InvoiceProcess/Index"),
                        Item("線上開立發票", "/InvoiceBusiness/CreateInvoice"),
                        Item("線上作廢發票", "/InvoiceProcess/InquireToCancel"),
                        Item("線上開立折讓證明", "/InvoiceProcess/InquireToIssueAllowance"),
                        Item("上期發票空白號碼查詢", "/InvoiceNo/VacantNoIndex"),
                        Item("下載MIG檔案", "/InvoiceProcess/InquireToMIG"),
                        Item("核准重印發票", "/InvoiceProcess/InquireToAuthorize"),
                        Item("註銷發票", "/InvoiceProcess/InquireToVoid"),
                        Item("核准註銷發票", "/InvoiceProcess/AllowToVoid")),
                    Group("發票通知", "fa-bell-o",
                        Item("重送開立發票通知", "/InvoiceProcess/IssuingNotice"),
                        Item("重送發票中獎通知", "/InvoiceProcess/InquireToNotifyWinning")),
                    Group("統計報表", "fa-bar-chart",
                        Item("發票明細查詢", "/InvoiceQuery/InvoiceReport"),
                        Item("發票統計表", "/InvoiceQuery/InvoiceSummary"),
                        Item("中獎統計表", "/WinningInvoice/ReportIndex"),
                        Item("捐贈統計表", "/DonatedInvoice/ReportIndex"),
                        Item("媒體申報檔匯出", "/InvoiceQuery/InvoiceMediaReport"),
                        Item("下載發票月報表", "/InvoiceQuery/MonthlyReport")),
                },

                [RoleSeller] = new()
                {
                    Group("系統使用設定", "fa-wrench",
                        Item("帳號管理", "/UserProfile/EditMySelf"),
                        Item("營業人資料管理", "/UserProfile/EditMyBusiness"),
                        Item("使用者管理", "/Account/AccountIndex"),
                        Item("相對營業人資料維護", "/BusinessRelationship/MaintainRelationship"),
                        Item("常用品項維護", "/ProductCatalog/QueryIndex")),
                    Group("發票開立", "fa-pencil-square-o",
                        Item("電子發票號碼維護", "/InvoiceNo/MaintainInvoiceNoInterval"),
                        Item("線上開立發票", "/InvoiceBusiness/CreateInvoice"),
                        Item("線上作廢發票", "/InvoiceProcess/InquireToCancel"),
                        Item("線上開立折讓證明", "/InvoiceProcess/InquireToIssueAllowance"),
                        Item("線上註銷發票", "/InvoiceProcess/InquireToVoid"),
                        Item("A0101接收待確認", "/InvoiceProcess/DealReceivedA0101"),
                        Item("A0301退回待確認", "/InvoiceProcess/DealReceivedA0301"),
                        Item("A0201接收待確認", "/InvoiceProcess/DealReceivedA0201"),
                        Item("B0101接收待確認", "/AllowanceProcess/DealReceivedB0101"),
                        Item("B0201接收待確認", "/AllowanceProcess/DealReceivedB0201")),
                    Group("查詢與報表", "fa-search",
                        Item("資料查詢／列印／匯出", "/InvoiceProcess/Index"),
                        Item("上期發票空白號碼查詢", "/InvoiceNo/VacantNoIndex"),
                        Item("發票媒體申報檔查詢", "/InvoiceQuery/InvoiceMediaReport"),
                        Item("下載MIG檔案", "/InvoiceProcess/InquireToMIG"),
                        Item("發票統計表", "/InvoiceQuery/InvoiceSummary")),
                    Group("訊息通知", "fa-envelope-o",
                        Item("重送開立發票通知", "/InvoiceProcess/IssuingNotice"),
                        Item("核准重印發票", "/InvoiceProcess/InquireToAuthorize"),
                        Item("工作清單", "/ProcessRequest/QueryIndex")),
                },

                [RoleBuyer] = new()
                {
                    Group("帳號管理", "fa-user",
                        Item("帳號管理", "/UserProfile/EditMySelf")),
                    Group("查詢", "fa-search",
                        Item("資料查詢／列印／匯出", "/InvoiceProcess/InquireForIncoming")),
                },

                [RoleDataAuditor] = new()
                {
                    Group("稽核查詢", "fa-eye",
                        Item("資料查詢", "/InvoiceAudit/QueryIndex")),
                },
            };

            foreach (var role in new[] { RoleNetworkSeller, RoleGoogleTw, RoleGroupMember, RoleRelativeEntity, RoleBranchEntity })
            {
                menus[role] = MemberMenu();
            }

            return menus;
        }
    }
}
