using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;
using ModelCore.DataEntity;
using ModelCore.InvoiceManagement;
using CommonLib.Utility;
using ModelCore.Helper;
using ModelCore.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using CommonLib.DataAccess;

namespace ModelCore.DataExchange
{
    public class CounterpartBusinessExchange
    {
        public enum ColumnIndex
        {
            相對營業人名稱 = 1,
            統一編號,
            聯絡人電子郵件,
            地址,
            電話,
            自動開立,
            主動列印,
            處理狀態
        }

        public bool HasError { get; private set; } = false;

        public XLWorkbook GetSample()
        {
            using (ModelSource<EIVOEntityDataContext> models = new ModelSource<EIVOEntityDataContext>())
            {
                var items = models.GetTable<Organization>()
                    .Where(o => false)
                    .Select(o =>
                        new
                        {
                            相對營業人名稱 = o.CompanyName,
                            統一編號 = o.ReceiptNo,
                            聯絡人電子郵件 = o.ContactEmail,
                            地址 = o.Addr,
                            電話 = o.Phone,
                        });

                using (DataSet ds = models.GetDataSetResult(items))
                {
                    var table = ds.Tables[0];
                    ds.DataSetName = table.TableName = "匯入相對營業人";
                    table.Columns.Add("自動開立＼接收(是: true，否: false或留空白)", typeof(bool));
                    table.Columns.Add("啟動＼停止 主動列印(是：true，否：false或留空白)", typeof(bool));

                    return ds.ConvertToExcel();
                }
            }
        }

        public void ExchangeData(BusinessRelationshipViewModel viewModel, String xlsFile)
        {
            using (XLWorkbook xlwb = new XLWorkbook(xlsFile))
            {
                ExchangeData(viewModel, xlwb);
            }
        }

        public void ExchangeData(BusinessRelationshipViewModel viewModel, XLWorkbook xlwb)
        {
            if (xlwb.Worksheets.Count < 1)
                return;
            var ws = xlwb.Worksheet(1);
            var firstRow = ws.FirstRowUsed();
            if (firstRow == null)
                return;
            var row = firstRow.RowUsed();
            if (row == null)
                return;

            ///title row
            ///
            row.Cell((int)ColumnIndex.處理狀態).Value = ColumnIndex.處理狀態.ToString();

            ///data row
            row = row.RowBelow();
            StringBuilder status = new StringBuilder();
            while (!row.Cell((int)ColumnIndex.相對營業人名稱).IsEmpty())
            {
                status.Clear();
                var dataRow = row;
                row = row.RowBelow();
                try
                {
                    if (validate(dataRow, viewModel, status))
                    {
                        dataRow.Cell((int)ColumnIndex.處理狀態).Value = "OK";
                    }
                    else
                    {
                        dataRow.Cell((int)ColumnIndex.處理狀態).Value = status.Remove(0, 1).ToString();
                        HasError = true;
                    }
                }
                catch (Exception ex)
                {
                    CommonLib.Core.Utility.FileLogger.Logger.Error(ex);
                    HasError = true;
                    dataRow.Cell((int)ColumnIndex.處理狀態).Value = ex.Message;
                }
            }

            xlwb.Save();
        }

        protected bool validate(IXLRangeRow dataRow, BusinessRelationshipViewModel viewModel, StringBuilder status)
        {
            bool bResult = true;

            viewModel.CompanyName = dataRow.Cell((int)ColumnIndex.相對營業人名稱).GetString().GetEfficientString();
            viewModel.ReceiptNo = dataRow.Cell((int)ColumnIndex.統一編號).GetString().GetEfficientString();
            viewModel.ContactEmail = dataRow.Cell((int)ColumnIndex.聯絡人電子郵件).GetString().GetEfficientString();
            viewModel.Addr = dataRow.Cell((int)ColumnIndex.地址).GetString().GetEfficientString();
            viewModel.Phone = dataRow.Cell((int)ColumnIndex.電話).GetString().GetEfficientString();
            viewModel.Entrusting = dataRow.Cell((int)ColumnIndex.自動開立).GetValue<bool?>();
            viewModel.EntrustToPrint = dataRow.Cell((int)ColumnIndex.主動列印).GetValue<bool?>();

            if (viewModel.CompanyName == null)
            {
                status.Append('、').Append("營業人名稱格式錯誤");
                bResult = false;
            }

            if (String.IsNullOrEmpty(viewModel.ReceiptNo) || viewModel.ReceiptNo.Length != 8 || !viewModel.ReceiptNo.ValidateString(20))
            {
                status.Append('、').Append("統編格式錯誤");
                bResult = false;
            }

            //if (string.IsNullOrEmpty(item.ContactEmail) || !ValueValidity.ValidateString(item.ContactEmail, 16))
            //{
            //    status.Append("、").Append("聯絡人電子郵件格式錯誤");
            //    bResult = false;
            //}

            //if (string.IsNullOrEmpty(item.Addr))
            //{
            //    status.Append("、").Append("地址格式錯誤");
            //    bResult = false;
            //}

            //if (string.IsNullOrEmpty(item.Phone))
            //{
            //    status.Append("、").Append("電話格式錯誤");
            //    bResult = false;
            //}

            using (ModelSource<EIVOEntityDataContext> models = new ModelSource<EIVOEntityDataContext>())
            {
                if (bResult)
                {
                    ModelStateDictionary modelState = new ModelStateDictionary { };
                    BusinessRelationship model = viewModel.CommitBusinessRelationshipViewModel(models, modelState);
                    if (model == null)
                    {
                        status.Append(modelState.ErrorMessage());
                        bResult = false;
                    }
                }
            }

            return bResult;
        }

    }
}
