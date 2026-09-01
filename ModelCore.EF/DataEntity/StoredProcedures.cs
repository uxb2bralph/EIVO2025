using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CommonLib.Core.DataWork;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ModelCore.DataEntity
{
    /// <summary>
    /// 對應原 LINQ to SQL DataContext 上以 FunctionAttribute 宣告的預存程序。
    /// EF Core 沒有等價的對應機制，改以原生 SQL 查詢傳回未對應型別 (unmapped type)。
    /// </summary>
    public class GetInvoiceReportResult
    {
        public int TrackID { get; set; }
        public int SellerID { get; set; }
        public int CurrencyID { get; set; }
        public string? CurrencyName { get; set; }
        public string? AbbrevName { get; set; }
        public string? TrackCode { get; set; }
        public int? RecordCount { get; set; }
        public int? StartNo { get; set; }
        public int? EndNo { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? TotalSalesAmount { get; set; }
        public decimal? TotalTaxAmount { get; set; }
        public short Year { get; set; }
        public short PeriodNo { get; set; }
        public int? PeriodID { get; set; }
    }

    public class GetAllowanceReportResult
    {
        public int CurrencyID { get; set; }
        public string? CurrencyName { get; set; }
        public string? AbbrevName { get; set; }
        public int? SellerID { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? TotalTaxAmount { get; set; }
        public int? RecordCount { get; set; }
        public string? TrackCode { get; set; }
        public int? StartNo { get; set; }
        public int? EndNo { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }
        public int? PeriodNo { get; set; }
        public int? PeriodID { get; set; }
    }

    public static class StoredProcedures
    {
        static SqlParameter[] BuildReportParameters(int? sellerID, DateTime? startDate, DateTime? endDate)
        {
            return
            [
                new SqlParameter("@SellerID", SqlDbType.Int) { Value = (object?)sellerID ?? DBNull.Value },
                new SqlParameter("@StartDate", SqlDbType.DateTime) { Value = (object?)startDate ?? DBNull.Value },
                new SqlParameter("@EndDate", SqlDbType.DateTime) { Value = (object?)endDate ?? DBNull.Value },
            ];
        }

        public static IEnumerable<GetInvoiceReportResult> GetInvoiceReport(this GenericDbContext<ApplicationDbContext> models,
            int? sellerID, DateTime? startDate, DateTime? endDate)
        {
            return models.DataContext.Database
                .SqlQueryRaw<GetInvoiceReportResult>("EXEC dbo.GetInvoiceReport @SellerID, @StartDate, @EndDate",
                    BuildReportParameters(sellerID, startDate, endDate))
                .ToList();
        }

        public static IEnumerable<GetAllowanceReportResult> GetAllowanceReport(this GenericDbContext<ApplicationDbContext> models,
            int? sellerID, DateTime? startDate, DateTime? endDate)
        {
            return models.DataContext.Database
                .SqlQueryRaw<GetAllowanceReportResult>("EXEC dbo.GetAllowanceReport @SellerID, @StartDate, @EndDate",
                    BuildReportParameters(sellerID, startDate, endDate))
                .ToList();
        }
    }
}
