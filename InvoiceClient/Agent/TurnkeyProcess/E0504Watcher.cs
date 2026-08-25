using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;

using CommonLib.Core.Utility;
using CommonLib.Utility;
using ModelCore.DataEntity;
using ModelCore.Locale;

namespace InvoiceClient.Agent.TurnkeyProcess
{
    /// <summary>
    /// 處理財政部下傳之中獎清冊(E0504)：解析清冊明細後，將中獎發票寫入 InvoiceWinningNumber。
    /// </summary>
    public class E0504Watcher : InvoiceWatcher
    {
        public E0504Watcher(String fullPath)
            : base(fullPath)
        {
        }

        /// <summary>
        /// 中獎清冊獎別代碼對應系統獎別；代碼與中獎金額不一致時以中獎金額為準(<see cref="resolvePrizeType"/>)。
        /// </summary>
        private static readonly Dictionary<String, Naming.WinningPrizeType> __PrizeType = new Dictionary<String, Naming.WinningPrizeType>
        {
            ["C"] = Naming.WinningPrizeType.頭獎,
            ["2"] = Naming.WinningPrizeType.二獎,
            ["3"] = Naming.WinningPrizeType.三獎,
            ["4"] = Naming.WinningPrizeType.四獎,
            ["5"] = Naming.WinningPrizeType.五獎,
            ["6"] = Naming.WinningPrizeType.六獎,
            ["7"] = Naming.WinningPrizeType.增開六獎,
            ["8"] = Naming.WinningPrizeType.特獎,
            ["9"] = Naming.WinningPrizeType.特別獎,
        };

        protected override void processFile(String invFile)
        {
            if (!File.Exists(invFile))
                return;

            String fileName = Path.GetFileName(invFile);
            String fullPath = Path.Combine(_inProgressPath, fileName);
            String backupPath = Path.Combine(Logger.LogDailyPath, fileName);

            try
            {
                File.Move(invFile, fullPath);
            }
            catch (Exception ex)
            {
                Logger.Error($"while processing move {invFile} => {fullPath}\r\n{ex}");
                return;
            }

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(fullPath);

                processAward(doc);
                storeFile(fullPath, backupPath);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                storeFile(fullPath, Path.Combine(_failedTxnPath, fileName));
            }
        }

        private void processAward(XmlDocument doc)
        {
            var main = selectNode(doc.DocumentElement, "Main");
            var details = doc.DocumentElement?.SelectNodes($".//*[local-name()='PrizeInvoice']");

            if (main == null || details == null || details.Count == 0)
            {
                Logger.Warn($"E0504 無中獎清冊明細:\r\n{doc.DocumentElement?.OuterXml}");
                return;
            }

            ///期別格式為民國年3碼 + 月份2碼(例：11506 表示115年5~6月)
            String? yearMonth = innerText(main, "YearMonth");
            int year = 0, period = 0;
            if (yearMonth?.Length == 5 && int.TryParse(yearMonth, out int yearMonthNo))
            {
                year = yearMonthNo / 100 + 1911;
                period = (yearMonthNo % 100 + 1) / 2;
            }
            else
            {
                Logger.Warn($"E0504 期別資料錯誤:{yearMonth}");
            }

            int totalRecordCnt = amount(main, "TotalRecordCnt") ?? 0;
            if (totalRecordCnt != details.Count)
            {
                Logger.Warn($"E0504({innerText(main, "HeadBan")}/{yearMonth}) 明細筆數({details.Count})與清冊總筆數({totalRecordCnt})不符!!");
            }

            using (ModelSource models = new ModelSource())
            {
                ///一期中獎號碼僅數十筆，一次載入後於各明細比對，避免逐筆查詢
                var winningNumbers = year > 0
                    ? models.GetTable<UniformInvoiceWinningNumber>()
                        .Where(u => u.Year == year && u.Period == period)
                        .ToList()
                    : new List<UniformInvoiceWinningNumber>();

                int matchedCount = 0;
                foreach (XmlElement detail in details.Cast<XmlNode>().OfType<XmlElement>())
                {
                    try
                    {
                        if (commitItem(models, detail, yearMonth, winningNumbers))
                        {
                            matchedCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"{detail.OuterXml}\r\n{ex}");
                    }
                }

                Logger.Info($"E0504({innerText(main, "HeadBan")}/{yearMonth}) 中獎清冊共{details.Count}筆，已建檔{matchedCount}筆。");
            }
        }

        /// <summary>
        /// 將單筆中獎明細寫入 InvoiceWinningNumber；發票不存在時回傳 false。
        /// </summary>
        private bool commitItem(ModelSource models, XmlElement detail, String? yearMonth, List<UniformInvoiceWinningNumber> winningNumbers)
        {
            String? invoiceNo = innerText(detail, "InvoiceNumber");
            if (invoiceNo == null || invoiceNo.Length <= 2)
            {
                Logger.Warn($"E0504 明細發票號碼錯誤:\r\n{detail.OuterXml}");
                return false;
            }

            String trackCode = innerText(detail, "InvTrack") ?? invoiceNo.Substring(0, 2);
            String no = invoiceNo.StartsWith(trackCode) ? invoiceNo.Substring(trackCode.Length) : invoiceNo;
            DateTime? invoiceDate = date(detail, "InvoiceDate");

            var candidates = models.GetTable<InvoiceItem>()
                    .Where(i => i.TrackCode == trackCode && i.No == no)
                    .Where(i => invoiceDate.HasValue && i.InvoiceDate >= invoiceDate);

            String? sellerId = innerText(selectNode(detail, "Seller"), "Identifier");
            candidates = candidates.Where(i => sellerId != null && i.Organization.ReceiptNo == sellerId);

            ///發票號碼於各期別會重複配發，僅比對同期別之發票，並優先取用開立日期相符者
            var invoice = candidates
                    .ToList()
                    .Where(i => yearMonth == null || periodOf(i.InvoiceDate!.Value) == yearMonth)
                    .FirstOrDefault();

            if (invoice == null)
            {
                Logger.Warn($"E0504 發票號碼({invoiceNo})不存在，期別:{yearMonth}");
                return false;
            }

            int prizeAmount = amount(detail, "PrizeAmount") ?? 0;
            String? prizeTypeCode = innerText(detail, "PrizeType");
            //var prizeType = resolvePrizeType(prizeTypeCode, prizeAmount);
            //if (!prizeType.HasValue)
            //{
            //    Logger.Warn($"E0504 發票({invoiceNo})中獎獎別({prizeTypeCode})/獎金({prizeAmount})無法對應系統獎別!!");
            //}

            var winningInvoice = invoice.InvoiceWinningNumber;
            if (winningInvoice == null)
            {
                winningInvoice = new InvoiceWinningNumber
                {
                    InvoiceID = invoice.InvoiceID,
                };

                models.GetTable<InvoiceWinningNumber>().InsertOnSubmit(winningInvoice);
            }

            //winningInvoice.PrizeType = prizeType?.ToString() ?? prizeTypeCode;
            winningInvoice.PrizeType = prizeTypeCode;
            winningInvoice.Bonus = prizeAmount;
            winningInvoice.DownloadDate = DateTime.Now;

            ///系統未維護該期中獎號碼時保留原對獎結果，不覆寫為空值
            var winningID = winningNumbers
                    //.Where(u => !prizeType.HasValue || u.Rank == (int)prizeType.Value)
                    .Where(u => u.WinningNO != null && no.EndsWith(u.WinningNO))
                    .OrderBy(u => u.Rank)
                    .FirstOrDefault()?.WinningID;
            if (winningID.HasValue)
            {
                winningInvoice.WinningID = winningID;
            }

            models.SubmitChanges();
            return true;
        }

        /// <summary>
        /// 以中獎獎別代碼與中獎金額推算系統獎別；中獎金額為清冊內權威資料，
        /// 代碼與金額不符時改以金額回推，獎金同為200元之六獎/增開六獎則以代碼區分。
        /// </summary>
        private static Naming.WinningPrizeType? resolvePrizeType(String? prizeTypeCode, int prizeAmount)
        {
            Naming.WinningPrizeType? byCode = prizeTypeCode != null && __PrizeType.TryGetValue(prizeTypeCode, out var code)
                    ? code : (Naming.WinningPrizeType?)null;

            if (byCode.HasValue && Naming.WinningBonus[(int)byCode.Value] == prizeAmount)
                return byCode;

            for (int rank = 1; rank < Naming.WinningBonus.Length; rank++)
            {
                if (Naming.WinningBonus[rank] == prizeAmount)
                    return (Naming.WinningPrizeType)rank;
            }

            return byCode;
        }

        /// <summary>
        /// 發票開立日期所屬期別(民國年3碼 + 雙月期別2碼)。
        /// </summary>
        private static String periodOf(DateTime invoiceDate)
        {
            return $"{invoiceDate.Year - 1911:000}{(invoiceDate.Month + 1) / 2 * 2:00}";
        }

        ///中獎清冊之命名空間隨版本異動(E0504:4.0/4.1)，一律以區域名稱取值
        private static XmlNode? selectNode(XmlNode? parent, String localName)
        {
            return parent?.SelectSingleNode($"*[local-name()='{localName}']");
        }

        private static String? innerText(XmlNode? parent, String localName)
        {
            return selectNode(parent, localName)?.InnerText.GetEfficientString();
        }

        private static int? amount(XmlNode? parent, String localName)
        {
            return decimal.TryParse(innerText(parent, localName), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal value)
                    ? (int)value : (int?)null;
        }

        private static DateTime? date(XmlNode? parent, String localName)
        {
            return DateTime.TryParseExact(innerText(parent, localName), "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime value)
                    ? value : (DateTime?)null;
        }

        protected override void processComplete()
        {

        }
    }
}
