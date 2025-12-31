using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommonLib.Core.Utility;
using CommonLib.DataAccess;
using CommonLib.Utility;
using ModelCore.DataEntity;
using ModelCore.Helper;
using AutoMapper;
using Newtonsoft.Json;

namespace ArchiveData
{
    internal class Program
    {
        // Add a static mapper instance so it can be used across methods in this simple console app
        static IMapper? Mapper;

        static void Main(string[] args)
        {
            Console.WriteLine("Archive Data Process Started");

            // 1. get commands from args as -o <output path> -n <count of tasks>
            string? outputPath = null;
            int taskCount = 1;
            int? maxID = null;
            int? minID = null;
            int sellerID = 8175;
            bool deleteDoc = false;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-o" && i + 1 < args.Length)
                {
                    outputPath = args[i + 1];
                    i++;
                }
                else if (args[i] == "-n" && i + 1 < args.Length)
                {
                    int.TryParse(args[i + 1], out taskCount);
                    i++;
                }
                else if (args[i] == "-max" && i + 1 < args.Length)
                {
                    int.TryParse(args[i + 1], out int maxId);
                    maxID = maxId;
                    i++;
                }
                else if (args[i] == "-min" && i + 1 < args.Length)
                {
                    int.TryParse(args[i + 1], out int minId);
                    minID = minId;
                    i++;
                }
                else if (args[i] == "-seller" && i + 1 < args.Length)
                {
                    int.TryParse(args[i + 1], out int sellerId);
                    sellerID = sellerId;
                    i++;
                }
                else if (args[i] == "-d")
                {
                    deleteDoc = true;
                }
            }

            if (string.IsNullOrEmpty(outputPath))
            {
                Console.WriteLine("Error: Output path not specified. Use -o <output path>");
                return;
            }

            if (taskCount < 1)
            {
                Console.WriteLine("Error: Invalid task count. Use -n <count of tasks>");
                return;
            }

            Console.WriteLine($"Output Path: {outputPath}");
            Console.WriteLine($"Task Count: {taskCount}");
            Console.WriteLine($"Seller ID: {sellerID}");
            if(minID.HasValue)
            {
                Console.WriteLine($"Min ID: {minID.Value}");
            }
            if(maxID.HasValue)
            {
                Console.WriteLine($"Max ID: {maxID.Value}");
            }

            // Initialize AutoMapper for this console app
            try
            {
                var config = new MapperConfiguration(cfg =>
                {
                    // scan this assembly for profiles
                    cfg.AddMaps(typeof(Program).Assembly);
                }, new FileLoggerFactory());

                // optional: validate configuration
                config.AssertConfigurationIsValid();

                Mapper = config.CreateMapper();
                Console.WriteLine("AutoMapper initialized.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AutoMapper initialization failed: {ex}");
            }

            // 2. 創建 n 個執行任務並傳入 output path, 及各自的任務編號 0~n-1, 每個任務負責處理部分資料
            var tasks = new List<Task>();
            for (int taskId = 0; taskId < taskCount; taskId++)
            {
                int currentTaskId = taskId;
                tasks.Add(Task.Run(() => ProcessArchiveData(outputPath, currentTaskId, taskCount, sellerID, minID, maxID, deleteDoc)));
            }

            Task.WaitAll(tasks.ToArray());

            Console.WriteLine("Archive Data Process Completed");
        }

        static void ProcessArchiveData(string outputPath, int taskId, int totalTasks, int sellerID, int? minID = null, int? maxID = null, bool deleteDoc = false)
        {
            Console.WriteLine($"Task {taskId} started");

            while (BatchProcesArchiveData(outputPath, taskId, totalTasks, sellerID, minID, maxID, deleteDoc));

            Console.WriteLine($"Task {taskId} completed");
        }

        private static bool BatchProcesArchiveData(string outputPath, int taskId, int totalTasks, int sellerID, int? minID, int? maxID, bool deleteDoc)
        {
            bool hasItem = false;
            try
            {
                using (GenericManager<EIVOEntityDataContext> models = new GenericManager<EIVOEntityDataContext>())
                {
                    // 3.1. 掃瞄 InvoiceItem 資料表, 由舊到新取得所有資料, 如果 InvoiceItem.InvoiceId % n == taskId, 則進行處理
                    IQueryable<InvoiceItem> invoiceItems = models.GetTable<InvoiceItem>()
                        .Where(i => i.SellerID == sellerID)
                        .OrderBy(i => i.InvoiceID);

                    if (maxID.HasValue)
                    {
                        invoiceItems = invoiceItems.Where(i => i.InvoiceID < maxID.Value);
                    }

                    if (minID.HasValue)
                    {
                        invoiceItems = invoiceItems.Where(i => i.InvoiceID >= minID.Value);
                    }

                    if (totalTasks > 1)
                    {
                        invoiceItems = invoiceItems.Where(i => i.InvoiceID % totalTasks == taskId);
                    }
                    var itemsList = invoiceItems.Take(4096).ToList();
                    hasItem = itemsList.Any();

                    foreach (var item in itemsList)
                    {
                        //if (item.InvoiceID % totalTasks != taskId)
                        //{
                        //    continue;
                        //}
                        try
                        {
                            Console.WriteLine($"Task {taskId}: Processing InvoiceItem {item.InvoiceID}");
                            ProcessInvoiceItem(models, item, outputPath, deleteDoc);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Task {taskId}: Error processing InvoiceItem {item.InvoiceID}: {ex}");
                        }
                    }

                    // 3.2. 掃瞄 InvoiceAllowance 資料表, 由舊到新取得所有資料, 如果 InvoiceAllowance.AllowanceID % n == taskId, 則進行處理
                    IQueryable<InvoiceAllowance> allowanceItems = models.GetTable<InvoiceAllowance>()
                        .Where(a => a.InvoiceAllowanceSeller.SellerID == sellerID)
                        .OrderBy(a => a.AllowanceID);

                    if (maxID.HasValue)
                    {
                        allowanceItems = allowanceItems.Where(a => a.AllowanceID < maxID.Value);
                    }

                    if (minID.HasValue)
                    {
                        allowanceItems = allowanceItems.Where(a => a.AllowanceID >= minID.Value);
                    }

                    if (totalTasks > 1)
                    {
                        allowanceItems = allowanceItems.Where(a => a.AllowanceID % totalTasks == taskId);
                    }

                    var allowanceList = allowanceItems.Take(4096).ToList();
                    hasItem = hasItem || allowanceList.Any();
                    foreach (var allowance in allowanceList)
                    {
                        //if (allowance.AllowanceID % totalTasks != taskId)
                        //{
                        //    continue;
                        //}
                        try
                        {
                            Console.WriteLine($"Task {taskId}: Processing InvoiceAllowance {allowance.AllowanceID}");
                            ProcessAllowanceItem(models, allowance, outputPath, deleteDoc);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Task {taskId}: Error processing InvoiceAllowance {allowance.AllowanceID}: {ex}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Task {taskId}: Fatal error: {ex}");
            }
            return hasItem;
        }

        static void ProcessInvoiceItem(GenericManager<EIVOEntityDataContext> models, InvoiceItem item, string outputPath, bool deleteDoc)
        {
            // 3.1.1. 處理邏輯: 根據 InvoiceItem 資料, 產生對應的 F0401 資料, 轉換成 json 格式, 依 InvoiceDate 並寫入指定的檔案
            var invoiceDate = item.InvoiceDate!.Value;
            var dateFolder = Path.Combine(outputPath, invoiceDate.Year.ToString(), invoiceDate.Month.ToString("00"), invoiceDate.Day.ToString("00"), invoiceDate.ToString("HH"));
            Directory.CreateDirectory(dateFolder);

            var invoiceNo = $"{item.TrackCode}{item.No}";
            var jsonFile = Path.Combine(dateFolder, $"{invoiceNo}-{item.InvoicePurchaseOrder?.OrderNo}.json");

            try
            {
                var json = item.GetJsonString();
                File.WriteAllText(jsonFile, json);
                Console.WriteLine($"Created InvoiceItem: {jsonFile}");

                // Example: use AutoMapper to map InvoiceItem to a lightweight DTO for logging or further processing
                //try
                //{
                //    if (Mapper != null)
                //    {
                //        var invoiceDto = Mapper.Map<Mapping.InvoiceDto>(item);
                //        Console.WriteLine($"Mapped InvoiceDto: InvoiceID={invoiceDto.InvoiceID}, InvoiceNo={invoiceDto.InvoiceNo}, InvoiceDate={invoiceDto.InvoiceDate}");
                //    }
                //    else
                //    {
                //        Console.WriteLine("Mapper not initialized; skipping mapping example.");
                //    }
                //}
                //catch (Exception mex)
                //{
                //    Console.WriteLine($"Error mapping InvoiceItem {invoiceNo}: {mex}");
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating F0401 for {invoiceNo}: {ex}");
            }

            // 3.1.2. 檢查 InvoiceItem 是否有 InvoiceCancellation 資料, 如果有, 則產生對應的 F0501 資料
            //if (item.InvoiceCancellation != null)
            //{
            //    var f0501File = Path.Combine(dateFolder, $"{invoiceNo}_F0501.json");

            //    try
            //    {
            //        var f0501Data = item.CreateCancelInvoiceMIG();
            //        if (f0501Data != null)
            //        {
            //            File.WriteAllText(f0501File, f0501Data.JsonStringify());
            //            Console.WriteLine($"Created F0501: {f0501File}");
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine($"Error creating F0501 for {invoiceNo}: {ex}");
            //    }
            //}

            // 3.1.3. 檢查 InvoiceItem 是否有 InvoiceAllowance 資料, 如果有, 則產生對應的 G0401 資料
            //var allowances = item.InvoiceAllowances;

            //foreach (var allowance in allowances)
            //{
            //    var allowanceNo = allowance.AllowanceNumber;
            //    var jsonFile = Path.Combine(dateFolder, $"{invoiceNo}_{allowanceNo}_G0401.json");

            //    try
            //    {
            //        var json = allowance.CreateAllowanceMIG(models);
            //        File.WriteAllText(jsonFile, json.JsonStringify());
            //        Console.WriteLine($"Created G0401: {jsonFile}");
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine($"Error creating G0401 for {allowanceNo}: {ex}");
            //    }

            //    // 3.1.4. 如果 InvoiceAllowance 是否有 InvoiceAllowanceCancellation 資料, 如果有, 則產生對應的 G0501 資料
            //    if (allowance.InvoiceAllowanceCancellation != null)
            //    {
            //        var g0501File = Path.Combine(dateFolder, $"{invoiceNo}_{allowanceNo}_G0501.json");

            //        try
            //        {
            //            var g0501Data = allowance.CreateCancelAllowanceMIG();
            //            if (g0501Data != null)
            //            {
            //                File.WriteAllText(g0501File, g0501Data.JsonStringify());
            //                Console.WriteLine($"Created G0501: {g0501File}");
            //            }
            //        }
            //        catch (Exception ex)
            //        {
            //            Console.WriteLine($"Error creating G0501 for {allowanceNo}: {ex}");
            //        }
            //    }
            //}

            // 3.1.5 刪除已處理的 InvoiceItem 對應的 CDS_Document 資料, 以節省空間
            if(deleteDoc)
            {
                try
                {
                    var cdsDoc = item.CDS_Document;
                    if (cdsDoc != null)
                    {
                        //models.DeleteAny<CDS_Document>(d => d.DocID == cdsDoc.DocID);
                        //models.SubmitChanges();
                        models.ExecuteCommand("DELETE FROM CDS_Document WHERE DocID = {0}", cdsDoc.DocID);
                        Console.WriteLine($"Deleted CDS_Document for InvoiceID: {item.InvoiceID}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting CDS_Document for InvoiceID {item.InvoiceID}: {ex}");
                }
            }

        }

        static void ProcessAllowanceItem(GenericManager<EIVOEntityDataContext> models, InvoiceAllowance item, string outputPath, bool deleteDoc)
        {
            // 3.2.1. 處理邏輯: 根據 InvoiceAllowance 資料, 產生對應的 G0401 資料, 轉換成 json 格式, 依 AllowanceDate 並寫入指定的檔案
            var allowanceDate = item.AllowanceDate!.Value;
            var dateFolder = Path.Combine(outputPath, allowanceDate.Year.ToString(), allowanceDate.Month.ToString("00"), allowanceDate.Day.ToString("00"), allowanceDate.ToString("HH"));
            Directory.CreateDirectory(dateFolder);

            var allowanceNo = item.AllowanceNumber;
            var jsonFile = Path.Combine(dateFolder, $"{item.InvoiceAllowanceDetails.FirstOrDefault()?.InvoiceAllowanceItem.InvoiceNo}_{allowanceNo}.json");

            try
            {
                var json = item.GetJsonString();
                File.WriteAllText(jsonFile, json);
                Console.WriteLine($"Created InvoiceAllowance: {jsonFile}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating G0401 for {allowanceNo}: {ex}");
            }

            // 3.2.2. 檢查 InvoiceAllowance 是否有 InvoiceAllowanceCancellation 資料, 如果有, 則產生對應的 G0501 資料
            //if (item.InvoiceAllowanceCancellation != null)
            //{
            //    var g0501File = Path.Combine(dateFolder, $"{allowanceNo}_G0501.json");

            //    try
            //    {
            //        var g0501Data = item.CreateCancelAllowanceMIG();
            //        if (g0501Data != null)
            //        {
            //            File.WriteAllText(g0501File, g0501Data.JsonStringify());
            //            Console.WriteLine($"Created G0501: {g0501File}");
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine($"Error creating G0501 for {allowanceNo}: {ex}");
            //    }
            //}

            // 3.2.3. 刪除已處理的 InvoiceAllowance 對應的 CDS_Document 資料, 以節省空間
            if(deleteDoc)
            {
                try
                {
                    var cdsDoc = item.CDS_Document;
                    if (cdsDoc != null)
                    {
                        //models.DeleteAny<CDS_Document>(d => d.DocID == cdsDoc.DocID);
                        //models.SubmitChanges();
                        models.ExecuteCommand("DELETE FROM CDS_Document WHERE DocID = {0}", cdsDoc.DocID);
                        Console.WriteLine($"Deleted CDS_Document for AllowanceID: {item.AllowanceID}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting CDS_Document for AllowanceID {item.AllowanceID}: {ex}");
                }
            }
        }
    }
}
