using ModelCore.DataEntity;

namespace ModelCore.DTOs
{
    public static class ProductCatalogMappingExtensions
    {
        public static ProductCatalogDto ToDto(this ProductCatalog src) => new()
        {
            ProductID = src.ProductID,
            Barcode = src.Barcode,
            ProductName = src.ProductName,
            SalePrice = src.SalePrice,
            PurchasePrice = src.PurchasePrice,
            Spec = src.Spec,
            Remark = src.Remark,
            PieceUnit = src.PieceUnit,
        };
    }

    /// <summary>
    /// 商品快速搜尋（ProductCatalog/QuickSearch）的回傳項目。
    /// </summary>
    /// <remarks>
    /// 直接序列化 ProductCatalog 實體會因 lazy loading proxy 的 Supplier 導覽
    /// 造成循環參考（Supplier→BusinessRelationship→Business→…），故改回傳純資料。
    /// </remarks>
    public class ProductCatalogDto
    {
        public int ProductID { get; set; }

        public string? Barcode { get; set; }

        public string ProductName { get; set; } = null!;

        public decimal SalePrice { get; set; }

        public decimal? PurchasePrice { get; set; }

        public string? Spec { get; set; }

        public string? Remark { get; set; }

        public string? PieceUnit { get; set; }
    }
}
