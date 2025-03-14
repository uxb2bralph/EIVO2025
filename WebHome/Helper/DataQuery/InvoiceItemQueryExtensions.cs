using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Web;
using CommonLib.DataAccess;
using ModelCore.DataEntity;
using ModelCore.Locale;

using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using ModelCore.Models.ViewModel;
using CommonLib.Utility;
using ModelCore.Helper;

namespace WebHome.Helper.DataQuery
{
    public static class InvoiceItemQueryExtensions
    {
        public static IQueryable<InvoiceItem> Inquire(this InquireInvoiceViewModel viewModel, GenericManager<EIVOEntityDataContext> models)
        {
            IQueryable<InvoiceItem> items = models.GetTable<InvoiceItem>();

            if(viewModel.SellerID.HasValue)
            {
                items = items.Where(d => d.SellerID == viewModel.SellerID);
            }

            if(viewModel.DateFrom.HasValue)
            {
                items = items.Where(d => d.InvoiceDate >= viewModel.DateFrom);
            }

            if (viewModel.DateTo.HasValue)
            {
                items = items.Where(d => d.InvoiceDate < viewModel.DateTo.Value.AddDays(1));
            }

            return items;
        }

        public static IQueryable<InvoiceItem> InquireInvoiceCancellation(this InquireInvoiceViewModel viewModel, GenericManager<EIVOEntityDataContext> models)
        {
            IQueryable<InvoiceItem> items = models.GetTable<InvoiceItem>();

            if (viewModel.SellerID.HasValue)
            {
                items = items.Where(d => d.SellerID == viewModel.SellerID);
            }

            IQueryable<InvoiceCancellation> cancellation = models.GetTable<InvoiceCancellation>();

            if (viewModel.DateFrom.HasValue)
            {
                cancellation = cancellation.Where(d => d.CancelDate >= viewModel.DateFrom);
            }

            if (viewModel.DateTo.HasValue)
            {
                cancellation = cancellation.Where(d => d.CancelDate < viewModel.DateTo.Value.AddDays(1));
            }

            return items.Join(cancellation, i => i.InvoiceID, c => c.InvoiceID, (i, c) => i);
        }

        public static IQueryable<InvoiceAllowance> InquireAllowance(this InquireInvoiceViewModel viewModel, GenericManager<EIVOEntityDataContext> models)
        {
            IQueryable<InvoiceAllowance> items = models.GetTable<InvoiceAllowance>();

            if (viewModel.SellerID.HasValue)
            {
                items = items.Join(models.GetTable<InvoiceAllowanceSeller>()
                                    .Where(d => d.SellerID == viewModel.SellerID),
                                a => a.AllowanceID, s => s.AllowanceID, (a, s) => a);
            }

            if (viewModel.DateFrom.HasValue)
            {
                items = items.Where(d => d.AllowanceDate >= viewModel.DateFrom);
            }

            if (viewModel.DateTo.HasValue)
            {
                items = items.Where(d => d.AllowanceDate < viewModel.DateTo.Value.AddDays(1));
            }

            return items;
        }

        public static IQueryable<InvoiceAllowance> InquireAllowanceCancellation(this InquireInvoiceViewModel viewModel, GenericManager<EIVOEntityDataContext> models)
        {
            IQueryable<InvoiceAllowance> items = models.GetTable<InvoiceAllowance>();

            if (viewModel.SellerID.HasValue)
            {
                items = items.Join(models.GetTable<InvoiceAllowanceSeller>()
                                    .Where(d => d.SellerID == viewModel.SellerID),
                                a => a.AllowanceID, s => s.AllowanceID, (a, s) => a);
            }


            IQueryable<InvoiceAllowanceCancellation> cancellation = models.GetTable<InvoiceAllowanceCancellation>();

            if (viewModel.DateFrom.HasValue)
            {
                cancellation = cancellation.Where(d => d.CancelDate >= viewModel.DateFrom);
            }

            if (viewModel.DateTo.HasValue)
            {
                cancellation = cancellation.Where(d => d.CancelDate < viewModel.DateTo.Value.AddDays(1));
            }

            return items.Join(cancellation, i => i.AllowanceID, c => c.AllowanceID, (i, c) => i);
        }

    }
}