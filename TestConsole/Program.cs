using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using ModelCore.DataEntity;
using ModelCore.Helper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Data.Linq;
using Business.Helper;
using System.Net;
using TestConsole.ServiceReference1;
using ClosedXML.Excel;
using System.Data;
using ModelCore.InvoiceManagement;
using ModelCore.InvoiceManagement.InvoiceProcess;
using ModelCore.Schema.EIVO;
using System.Security.Cryptography;
using ModelCore.Schema.TXN;
using ModelCore.Locale;
using ModelCore.Models.ViewModel;
using System.Collections.Specialized;
using DocumentFormat.OpenXml.EMMA;
using System.Reflection;
using System.Linq.Dynamic.Core;
using ProcessorUnit.Execution;
using System.Windows.Forms;
using ModelCore.InvoiceManagement.ErrorHandle;
using System.ComponentModel;
using MimeKit;
using MailKit.Net.Smtp;
using System.Net.Security;
using System.Diagnostics;
using System.Xml.Schema;
using CommonLib.Core.Utility;
using CommonLib.Utility;
using CommonLib.Security.UseCrypto;
using CommonLib.DataAccess;
using System.Xml.Serialization;
using ModelCore.Schema.TurnKey.Invoice;
using System.Data.SqlClient;

namespace TestConsole
{
    class Program
    {
        [STAThread()]
        static void Main(string[] args)
        {
            // Register code page provider for non-Unicode encodings (e.g., 950/Big5)
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            FileLogger.Logger.OutputWriter = Console.Out;
            Logger.Info($"Process start at {DateTime.Now}");

            //ExternalPdfWrapper.AppSettings.Default.UseSelenium = true;
            //ExternalPdfWrapper.AppSettings.Default.Save();
            //var pdf = new ExternalPdfWrapper.PdfUtility();
            //pdf.ConvertHtmlToPDF("http://192.168.6.45/GAP/DataView/GetCustomerAllowancePDF?keyID=L5AHnA7a1TOygcaH0FmK6Q%3d%3d&html=True", "G:\\temp\\doc1.pdf", 5);
            //pdf.ConvertHtmlToPDF("https://www.google.com", "G:\\temp\\doc.pdf", 5);
            //SaveToExcel();
            //var data = ERPInvoiceParser.ConvertToXml(@"G:\temp\SelfDelivery_Sample\SAMPLE_存證開立發票_UTF8格式.csv");

            //test32();

            //test01();
            //test02();
            //test03();
            //XElement doc = XElement.Parse("<root><test>hello...</test></root>");

            //using (HttpClient client = new HttpClient())
            //{

            //}
            //XmlDocument doc = new XmlDocument();
            //doc.LoadXml("<root><book>test</book></root>");

            //test04();

            //test05();
            //Logger.OutputWritter = Console.Out;
            //AppResource.Instance.InitializeKey(true);
            //test08();
            //Logger.Info("test...");
            //test06();
            //test07();

            //Dictionary<int, String> d = new Dictionary<int, string>();
            //d.Add(0, "aaa");
            //d.Add(1, "bbb");
            //d.Add(2, "ccc");

            //test09();

            //test10();
            //test11();

            //if (args != null && args.Length > 0)
            //{
            //    XmlDocument doc = new XmlDocument();
            //    doc.Load(args[0]);
            //    String outFile = Path.Combine(Path.GetDirectoryName(args[0]), $"{Path.GetFileNameWithoutExtension(args[0])}(formatted).xml");
            //    Console.WriteLine($"Save to:{outFile}");
            //    doc.Save(outFile);
            //}

            //test12(args);

            //using (XLWorkbook xlwb = new XLWorkbook("G:\\temp\\發票資料明細.xlsx"))
            //{

            //}

            //DataSet ds = @"G:\temp\test.xlsx".ImportExcelXLS();
            //test14();
            //test15();
            //test16();

            //test17();

            //test18();
            //test19();

            //test20(args);
            //test24();
            //test21(args);
            //test22(args);
            //var result = "aaa,bbb,\"c,d\"e\"\",f,,,kkk".ParseCsvLine();
            //test23();

            //test25();

            //test26(args);

            //test27();
            //test28(args);
            //test29(args);

            //test30();

            //test31();

            //String json = File.ReadAllText("G:\\temp\\test.json");
            //InvoiceRoot invoice = JsonConvert.DeserializeObject<InvoiceRoot>(json);

            //test33();


            //test34();
            //test35(args);
            //test36();
            //test37();
            //test38();

            //string postData = "card_ban=97162640&card_no1=1234&card_no2=987654321&card_type=BG0001&token=eyJhbGciOiJkaXIiLCJlbmMiOiJBMjU2R0NNIiwiY3R5IjoiSldUIiwia2lkIjoiMDEifQ..wNSlKWvyuPo20rYz.9AH2ig6DWLTn1CEGDiSx72SNQVpsYKeNQ4SmI4xqcAcHuriic_2XokWfDBqJNi1uN1pO1iJk3WlANXVRG6W4MYFp_bKiuYRo1wITDo_xCy26WrkjSOQhtZfbltrzdFHPnKvMzBoiqu6njirr9uBPJFSlI7Qu8Er56NWnzWJNRtOUrkEcB4JQYdWZ2pfFvqKMDtmI_4iFWwNgCjHX3P1WWTxcnDKq5R_oX8xx_u9a3NhsKgEwOwCl3hyPawJmq9vWswbmCM5BSTYSVk5PHeWRKik9LTBd1KZuw3005RXlVRAYqJKrkvdOUMpmdmFFbfopZK4t4UjtfhXpvSXhAJbipgDL_EJSlbB09xrly3DCK5B2WMmUZ6TJ07ryOuhQ9ZhtxfFIzin6VeKu_YNT0D8nugegMVqWjXHJrw4BMBxchy5MCUZB_CMLyBGZvemCHDZrPOjckFEiORLt6D7TXJOOBQN5kCXE43zYxcv__bAqHtcOjR3q6Yy7i51caI2zmlgkS_G.oIjs4WBcUsTJkXmuw4_Z9g";

            //using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes("XQcpGwtz5esvvdqTTsQ0bA==")))
            //{
            //    var computeHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(postData));
            //    var signature = Convert.ToBase64String(computeHash);
            //    Console.WriteLine(signature);
            //    //icBGpAidXkbKUaO2GjMtJA5jRyJlYnnOy9/M98mnd4o=
            //    //icBGpAidXkbKUaO2GjMtJA5jRyJlYnnOy9/M98mnd4o=
            //    //w264bIhLQbFQ7Pz3Mwz2g+Tz5wnZQPlDoNOHXXuj+xA=
            //}

            // Step 1: Get byte array
            //string inputString = "card_ban=97162640&card_no1=1234&card_no2=987654321&card_type=BG0001&token=eyJhbGciOiJkaXIiLCJlbmMiOiJBMjU2R0NNIiwiY3R5IjoiSldUIiwia2lkIjoiMDEifQ..wNSlKWvyuPo20rYz.9AH2ig6DWLTn1CEGDiSx72SNQVpsYKeNQ4SmI4xqcAcHuriic_2XokWfDBqJNi1uN1pO1iJk3WlANXVRG6W4MYFp_bKiuYRo1wITDo_xCy26WrkjSOQhtZfbltrzdFHPnKvMzBoiqu6njirr9uBPJFSlI7Qu8Er56NWnzWJNRtOUrkEcB4JQYdWZ2pfFvqKMDtmI_4iFWwNgCjHX3P1WWTxcnDKq5R_oX8xx_u9a3NhsKgEwOwCl3hyPawJmq9vWswbmCM5BSTYSVk5PHeWRKik9LTBd1KZuw3005RXlVRAYqJKrkvdOUMpmdmFFbfopZK4t4UjtfhXpvSXhAJbipgDL_EJSlbB09xrly3DCK5B2WMmUZ6TJ07ryOuhQ9ZhtxfFIzin6VeKu_YNT0D8nugegMVqWjXHJrw4BMBxchy5MCUZB_CMLyBGZvemCHDZrPOjckFEiORLt6D7TXJOOBQN5kCXE43zYxcv__bAqHtcOjR3q6Yy7i51caI2zmlgkS_G.oIjs4WBcUsTJkXmuw4_Z9g";
            //byte[] byteArray = Encoding.UTF8.GetBytes(inputString);

            //// Step 2: Calculate signature using HMACSHA256 algorithm
            //byte[] key = Convert.FromBase64String("XQcpGwtz5esvvdqTTsQ0bA==");
            //byte[] result;

            //using (HMACSHA256 hmac = new HMACSHA256(key))
            //{
            //    result = hmac.ComputeHash(byteArray);
            //}

            //// Step 3: Encode the result in base64
            //string output = Convert.ToBase64String(result);

            //Console.WriteLine(output);
            //Task.Run(() => {
            //    while (true)
            //    {
            //        Console.WriteLine(DateTime.Now);
            //        Task.Run(() => 
            //        {
            //            MessageBox.Show("test...");
            //        });
            //        Thread.Sleep(1000);
            //    }
            //});
            //MessageBox.Show("test...");
            //using (ModelSource models = new ModelSource())
            //{
            //    DataLoadOptions options = new DataLoadOptions();
            //    options.LoadWith<Organization>(o => o.OrganizationStatus);
            //    options.LoadWith<Organization>(o => o.OrganizationCategory);
            //    options.LoadWith<Organization>(o => o.OrganizationCustomSetting);
            //    options.LoadWith<Organization>(o => o.OrganizationExtension);
            //    options.LoadWith<Organization>(o => o.OrganizationSettings);
            //    options.LoadWith<OrganizationCategory>(o => o.UserRole);
            //    options.LoadWith<UserRole>(o => o.UserProfile);
            //    options.LoadWith<UserProfile>(o => o.UserProfileExtension);
            //    options.LoadWith<UserProfile>(o => o.UserProfileStatus);

            //    models.DataContext.LoadOptions = options;

            //    var items = models.GetTable<Organization>().Where(o => o.ReceiptNo == "29057255");
            //    //foreach(var item in items)
            //    //{
            //    //    var s = item.OrganizationStatus;
            //    //}
            //    File.WriteAllText(@"G:\\temp\\data.json", items.JsonStringify());
            //}

            //if (args?.Length > 0 && File.Exists(args[0]))
            //{
            //    using (ModelSource models = new ModelSource())
            //    {
            //        var items = JsonConvert.DeserializeObject<Organization[]>(File.ReadAllText(args[0]));
            //        models.GetTable<Organization>()
            //            .InsertAllOnSubmit(items);
            //        models.SubmitChanges();
            //    }
            //}

            //using (ModelSource models = new ModelSource())
            //{
            //    DataLoadOptions options = new DataLoadOptions();
            //    options.LoadWith<UserProfile>(o => o.UserProfileExtension);
            //    options.LoadWith<UserProfile>(o => o.UserProfileStatus);

            //    models.DataContext.LoadOptions = options;

            //    var items = models.GetTable<UserProfile>().Where(o => o.PID == "43460094");
            //    File.WriteAllText(@"G:\\temp\\data.json", items.JsonStringify());
            //}

            //if (args?.Length > 0 && File.Exists(args[0]))
            //{
            //    using (ModelSource models = new ModelSource())
            //    {
            //        var items = JsonConvert.DeserializeObject<UserProfile[]>(File.ReadAllText(args[0]));
            //        models.GetTable<UserProfile>()
            //            .InsertAllOnSubmit(items);
            //        models.SubmitChanges();
            //    }
            //}

            //test39(args);

            //test40();

            //test08(args);

            //if (args?.Length > 1)
            //{
            //    System.Diagnostics.Debugger.Launch();
            //    Console.WriteLine(UploadAllowanceV2(args));
            //}

            //dynamic _message = JsonConvert.DeserializeObject("{ 'aa' : 1}");
            //int? val = (int?)_message.aa;
            //val = (int?)_message.bb;

            //if (args?.Length > 0)
            //{
            //    //System.Diagnostics.Debugger.Launch();
            //    SendEmailFile(args);
            //    //XmlDocument doc = new XmlDocument();
            //    //doc.Load(args[0]);
            //    //CancelAllowanceRoot root = doc.TrimAll().ConvertTo<CancelAllowanceRoot>();
            //    //Console.WriteLine((new { CancelAllowanceRoot = root }).JsonStringify());
            //}
            //JobHelper.Tasks.CheckTurnkeyLog.Notify();
            //ConsoleKeyInfo key;
            //do
            //{
            //    key = Console.ReadKey();
            //} while (key.Key != ConsoleKey.Q);
            //Logger.Info("Process terminated..");

            //Application.Run(new MyApplicationContext());

            //test41();

            //test42();

            //for (int i = 0; i < 100; i++)
            //{
            //    Thread.Sleep(1);
            //    Console.WriteLine(((String)null).CheckB2CMIGName());
            //}

            //if (args?.Length > 1)
            //{
            //    test43(args);
            //}

            //test43(args);

            //test45();
            //ShowMethodName();
            //test46();
            //test47();
            //test48(args);
            //test49(args);

            //test50();

            //InvoiceHandler.SendMailNotification();
            //XmlDocument doc = new XmlDocument();
            //doc.PreserveWhitespace = true;
            //doc.Load("C:\\Project\\AppDev\\CDS(Wcf)\\web\\logs\\2025\\12\\19\\ca_log\\000000000001(1341452361)-XmlSig.nfo.xml");
            //CryptoUtility crypto = new CryptoUtility();
            //var result = crypto.VerifyXmlSignature(doc);
            //Console.WriteLine(result);

            new Class1().Test5();

            Console.ReadKey();
        }

        private static void test50()
        {
            using (ModelSource models = new ModelSource())
            {
                var items = models.GetTable<InvoiceItem>()
                    .Where(i => i.InvoiceDate >= new DateTime(2025, 1, 1))
                    .Where(i => i.InvoiceSeller.ReceiptNo == "70762419");

                SqlCommand sqlCmd = (SqlCommand)models.DataContext.GetCommand(items);
                Console.WriteLine(sqlCmd.CommandText);
                var sqlText = sqlCmd.ToExecutableSql();
                Console.WriteLine(sqlText);

                var invoices = models.ExecuteQuery<InvoiceItem>(sqlText);

                foreach (var item in invoices)
                {
                    Console.WriteLine(item.InvoiceNo());
                }

                Console.WriteLine("//----------------------------------------");
                var orderItems = items.OrderBy(i => i.InvoiceID);
                invoices = models.ExecutePagingQuery(orderItems, 5, 5);

                foreach (var item in invoices)
                {
                    Console.WriteLine(item.InvoiceNo());
                }
            }
        }

        private static void test49(string[] args)
        {
            if (args?.Length > 0 && File.Exists(args[0]))
            {
                var lines = File.ReadAllLines(args[0]);
                if (lines.Any())
                {
                    List<AutomationItem> automation = new List<AutomationItem>();
                    using (ModelSource models = new ModelSource())
                    {
                        foreach (var line in lines.Select(l => l.Trim()))
                        {
                            if (line.Length == 10)
                            {
                                var trackCode = line[0..2];
                                var no = line[2..];
                                var item = models.GetTable<InvoiceItem>()
                                            .Where(i => i.TrackCode == trackCode && i.No == no)
                                            .OrderByDescending(i => i.InvoiceID)
                                            .FirstOrDefault();

                                if (item != null)
                                {
                                    Console.WriteLine($"TrackCode:{item.TrackCode}, No:{item.No}, InvoiceID:{item.InvoiceID}");
                                    automation.Add(new AutomationItem
                                    {
                                        Description = "",
                                        Status = 1,
                                        Invoice = new AutomationItemInvoice
                                        {
                                            SellerId = item.InvoiceSeller.ReceiptNo,
                                            InvoiceNumber = $"{item.TrackCode}{item.No}",
                                            EncData = item.BuildEncryptedData()
                                        },
                                    });
                                }
                            }
                        }

                        Root result = new Root
                        {
                            UXB2B = "電子發票系統",
                            Result = new RootResult
                            {
                                timeStamp = DateTime.Now,
                                value = 1
                            }
                        };

                        result.Automation = automation.ToArray();
                        result.ConvertToXml().Save("G:\\temp\\response.xml");
                    }

                }
            }
        }

        private static void test48(string[] args)
        {
            int docID = 0;
            if (args.Length > 0 && int.TryParse(args[0], out docID))
            {
                using (ModelSource models = new ModelSource())
                {
                    var items = models.GetTable<InvoiceItem>()
                        .Where(i => i.InvoiceID > docID)
                        .Where(i => i.InvoiceBuyer.EMail != null)
                        .Where(i => i.CDS_Document.IssuingNotice == null)
                        .Select(d => d.InvoiceID).ToArray();
                    items.NotifyIssuedInvoice(false, forceTodo: false);
                }
            }
        }

        private static void test47()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("G:\\temp\\invoice.xml");
            doc.DocumentElement.SetAttribute("xmlns", "");
            doc.LoadXml(doc.OuterXml);
            Invoice invoice = doc.TrimAll().DebugConvertTo<Invoice>();
            Console.WriteLine(doc.OuterXml);
        }

        private static void test46()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, eventArgs) =>
            {
                if (eventArgs.Name.StartsWith("System.Security.Cryptography"))
                {
                    return Assembly.Load("System.Security.Cryptography");
                }
                return null;
            };
            var assembly = Assembly.Load("System.Security.Cryptography");
            RSA rsa = System.Security.Cryptography.RSACryptoServiceProvider.Create();
            var type = typeof(System.Security.Cryptography.RSACryptoServiceProvider);
            var qName = type.AssemblyQualifiedName;
            type = Type.GetType("System.Security.Cryptography.RSACryptoServiceProvider");
            type = Type.GetType(qName);
        }

        private static void test45()
        {
            try
            {
                ValidateXml("G:\\temp\\invoice.xml", "C:\\Project\\Github\\IFS-EIVO03\\Model\\Schema\\EIVO\\B2CInvoice.xsd");
                XmlDocument doc = new XmlDocument();
                doc.Load("G:\\temp\\invoice.xml");
                InvoiceRoot invoice = doc.TrimAll().ConvertTo<InvoiceRoot>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public static void ValidateXml(string xmlFilePath, string xsdFilePath)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlFilePath);

            XmlSchemaSet schemaSet = new XmlSchemaSet();
            schemaSet.Add(null, xsdFilePath);

            xmlDoc.Schemas.Add(schemaSet);

            xmlDoc.Validate((sender, args) =>
            {
                Console.WriteLine($"Validation error: {args.Message}");
            });

            Console.WriteLine("XML validation completed.");
        }

        private static void test44()
        {
            String[] receiptNo = {
                "98837806",
                 };

            Console.WriteLine(ModelExtension.Properties.AppSettings.Default.F0401Outbound);
            Console.WriteLine(ModelExtension.Properties.AppSettings.Default.F0701Outbound);

            using (ModelSource models = new ModelSource())
            {
                var orgItems = models.GetTable<Organization>().Where(o => receiptNo.Contains(o.ReceiptNo)).ToArray();
                foreach (var org in orgItems)
                {
                    String storedPath = Path.Combine(Logger.LogDailyPath, "MIG", org.ReceiptNo);
                    storedPath.CheckStoredPath();

                    var items = models.GetTable<InvoiceItem>()
                            .Where(i => i.InvoiceDate >= new DateTime(2025, 1, 1))
                            .Where(i => i.SellerID == org.CompanyID)
                            .ToList();
                    Console.WriteLine($"{org.ReceiptNo}: {items.Count}");

                    int idx = 0;
                    foreach (var item in items)
                    {
                        var seller = item.InvoiceSeller;
                        Console.WriteLine($"{idx++}/{items.Count}:{item.InvoiceNo()}");
                        seller.Name = seller.CustomerName = org.CompanyName;
                        if (!(seller.Address?.Length > 0))
                        {
                            seller.Address = org.Addr;
                        }
                        models.SubmitChanges();
                        storedMIG(item, storedPath);
                    }
                }
            }
        }

        static void ShowMethodName()
        {
            var method = new StackTrace().GetFrame(0).GetMethod();
            Console.WriteLine($"Method: {method.Name}");
        }

        static void storedMIG(InvoiceItem item, String storedPath)
        {
            String invoiceNo = item.TrackCode + item.No;
            item.CreateF0401().Save(Path.Combine(storedPath, "F0401_" + invoiceNo + ".xml"));
            item.CreateF0701().Save(Path.Combine(ModelExtension.Properties.AppSettings.Default.F0701Outbound, "F0701_" + item.TrackCode + item.No + ".xml"));

        }

        static void test43(string[] args)
        {
            // Path to the source file
            string sourceFilePath = args[0];
            string outputDirectory = args[1].CheckStoredPath();
            String sampleFileName = Path.GetFileNameWithoutExtension(sourceFilePath);

            // Read the entire file into a string
            string fileContent = File.ReadAllText(sourceFilePath);

            // Regular expression to match <Invoice>...</Invoice>
            Regex invoiceRegex = new Regex(@"<Invoice>(.*?)</Invoice>", RegexOptions.Singleline);

            // Find all matches
            MatchCollection matches = invoiceRegex.Matches(fileContent);

            if (matches.Count > 0)
            {
                int count = 1;
                foreach (Match match in matches)
                {
                    // Wrap the matched Invoice in InvoiceRoot
                    string invoiceContent = $"<InvoiceRoot>{match.Value}</InvoiceRoot>";

                    // Save the new document
                    string fileName = $"{sampleFileName}_{count}.xml";
                    string fullPath = Path.Combine(outputDirectory, fileName);
                    File.WriteAllText(fullPath, invoiceContent);

                    Console.WriteLine($"Invoice saved as {fileName}");
                    count++;
                }
            }
            else
            {
                Console.WriteLine("No Invoice elements found in the file.");
            }
        }

        private static void test42()
        {
            using (ModelSource models = new ModelSource())
            {

                var item = models.GetTable<InvoiceItem>().Where(i => i.InvoiceID == 1240165).FirstOrDefault();
                if (item != null)
                {
                    string json = item.GetJsonString();
                    Console.WriteLine(json);
                }
            }
        }

        //private static void test41()
        //{
        //    using (ModelSource models = new ModelSource())
        //    {
        //        var migC0501 = models.GetTable<C0501DispatchQueue>().Where(d => d.StepID == (int)Naming.InvoiceStepDefinition.回傳MIG)
        //                            .ToList();

        //        foreach (var d in migC0501)
        //        {
        //            var a = (d.CDS_Document.DerivedDocument?.ParentDocument?.InvoiceItem ?? d.CDS_Document.InvoiceItem)?.CreateF0501();
        //            var s = a?.OuterXml;
        //            var c = new MIGContent
        //            {
        //                DocID = d.DocID,
        //                DocDate = d.CDS_Document.DocDate,
        //                No = (d.CDS_Document.DerivedDocument?.ParentDocument?.InvoiceItem ?? d.CDS_Document.InvoiceItem)?.InvoiceNo(),
        //                ReceiptNo = (d.CDS_Document.DerivedDocument?.ParentDocument?.InvoiceItem ?? d.CDS_Document.InvoiceItem)?.Organization?.ReceiptNo,
        //                MIG = s
        //            };
        //            Console.WriteLine(c.JsonStringify());
        //        }
        //        var items = migC0501
        //            .Select(d =>
        //            new MIGContent
        //            {
        //                DocID = d.DocID,
        //                DocDate = d.CDS_Document.DocDate,
        //                No = (d.CDS_Document.DerivedDocument?.ParentDocument?.InvoiceItem ?? d.CDS_Document.InvoiceItem)?.InvoiceNo(),
        //                ReceiptNo = (d.CDS_Document.DerivedDocument?.ParentDocument?.InvoiceItem ?? d.CDS_Document.InvoiceItem)?.Organization?.ReceiptNo,
        //                MIG = (d.CDS_Document.DerivedDocument?.ParentDocument?.InvoiceItem ?? d.CDS_Document.InvoiceItem)?.CreateF0501()?.OuterXml
        //            }).ToArray();

        //        //File.WriteAllText("G:\\temp\\data.json", items.JsonStringify());
        //        foreach (var item in items)
        //        {
        //            Console.WriteLine(item.JsonStringify());
        //        }
        //    }
        //}

        class MyApplicationContext : ApplicationContext
        {
            public MyApplicationContext() 
            {
                Console.WriteLine("Hello,World!!");
            }
        }

        private static void SendEmailFile(string[] args)
        {
            if (args != null && args.Length > 0)
            {
                if (Directory.Exists(args[0]))
                {
                    var files = Directory.EnumerateFiles(args[0], "*.eml");
                    if (files.Any())
                    {
                        using (var client = new SmtpClient())
                        {
                            var hostUrl = ModelExtension.Properties.AppSettings.Default.MailServer;
                            var port = 25;
                            var useSsl = false;

                            client.ServerCertificateValidationCallback = (s, certificate, chain, sslPolicyErrors) => true;

                            // 連接 Mail Server (郵件伺服器網址, 連接埠, 是否使用 SSL)
                            client.Connect(hostUrl, port, useSsl);

                            // 如果需要的話，驗證一下
                            // client.Authenticate("account", "password");

                            // 寄出郵件
                            foreach (var f in files)
                            {
                                using (MimeMessage mimeMessage = MimeMessage.Load(f))
                                {
                                    try
                                    {
                                        client.Send(mimeMessage);
                                    }
                                    catch(Exception ex)
                                    {
                                        Console.WriteLine(ex.ToString());
                                        continue;
                                    }
                                    //using (MailMessage message = new MailMessage())
                                    //{
                                    //    message.From = new MailAddress(mimeMessage.From.ToString());
                                    //    message.To.Add(mimeMessage.To.ToString());
                                    //    message.Subject = mimeMessage.Subject;
                                    //    message.IsBodyHtml = true;
                                    //    message.Body = mimeMessage.Body.ToString();

                                    //    using (SmtpClient smtpclient = new SmtpClient(Uxnet.Web.Properties.Settings.Default.MailServer)
                                    //    {
                                    //        Credentials = CredentialCache.DefaultNetworkCredentials
                                    //    })
                                    //    {
                                    //        smtpclient.Send(message);
                                    //    }
                                    //}

                                }
                                Console.WriteLine($"send => {f}");
                                File.Delete(f);

                            }

                            // 中斷連線
                            client.Disconnect(true);

                        }
                    }
                }

            }

        }

        private static XmlDocument UploadAllowanceV2(string[] args)
        {
            XmlDocument uploadData = new XmlDocument();
            uploadData.Load(args[1]);

            Root result = new Root
            {
                UXB2B = "電子發票系統",
                Result = new RootResult
                {
                    timeStamp = DateTime.Now,
                    value = 0
                }
            };

            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");

            try
            {
                AllowanceRoot allowance = uploadData.TrimAll().ConvertTo<AllowanceRoot>();

                using (GoogleInvoiceManagerV3 mgr = new GoogleInvoiceManagerV3 { InvoiceClientID = null, ChannelID = null, IgnoreDuplicateDataNumberException = true })
                {
                    ///憑證資料檢查
                    ///
                    var token = mgr.GetTable<Organization>()
                        .Where(c => c.ReceiptNo == args[0])
                        .FirstOrDefault()?.OrganizationToken;

                    if (token != null)
                    {
                        List<AutomationItem> automation = new List<AutomationItem>();
                        var items = mgr.SaveUploadAllowance(allowance, token);
                        if (items.Count > 0)
                        {
                            result.Response = new RootResponse
                            {
                                InvoiceNo =
                                items.Select(d => new RootResponseInvoiceNo
                                {
                                    Value = allowance.Allowance[d.Key].AllowanceNumber,
                                    Description = d.Value.Message,
                                    ItemIndexSpecified = true,
                                    ItemIndex = d.Key,
                                    StatusCode = (d.Value is InvoiceNotFoundException) ? "I01" : null,
                                }).ToArray()
                            };

                            automation.AddRange(items.Select(d => new AutomationItem
                            {
                                Description = d.Value.Message,
                                Status = 0,
                                Allowance = new AutomationItemAllowance
                                {
                                    AllowanceNumber = allowance.Allowance[d.Key].AllowanceNumber,
                                },
                            }));

                            ThreadPool.QueueUserWorkItem(ExceptionNotification.SendNotification,
                                new ExceptionInfo
                                {
                                    Token = token,
                                    ExceptionItems = items,
                                    AllowanceData = allowance
                                });
                        }
                        else
                        {
                            result.Result.value = 1;
                        }

                        if (mgr.EventItems_Allowance != null && mgr.EventItems_Allowance.Count() > 0)
                        {
                            //上傳後折讓
                            automation.AddRange(mgr.EventItems_Allowance.Select(d => new AutomationItem
                            {
                                Description = "",
                                Status = 1,
                                Allowance = new AutomationItemAllowance
                                {
                                    AllowanceNumber = d.AllowanceNumber,
                                    InvoiceNumber = d.InvoiceAllowanceDetails.Select(a => a.InvoiceAllowanceItem.InvoiceNo).ToArray()
                                },
                            }));
                        }

                        result.Automation = automation.ToArray();
                    }
                    else
                    {
                        result.Result.message = "營業人憑證資料驗證不符!!";
                    }
                }


            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                result.Result.message = ex.Message;
            }
            return result.ConvertToXml();
        }

        private static void test40()
        {
            string xmlString = @"<root>
                                <element1>Some text with < and > characters</element1>
                                <element2>Another text with & character</element2>
                            </root>";

            // Create a new XmlDocument instance
            XmlDocument xmlDoc = new XmlDocument();

            //// Load the XML string into the XmlDocument
            //xmlDoc.LoadXml(xmlString);

            //// Get the XML content as a string
            //string xmlContent = xmlDoc.InnerXml;

            //// Escape the reserved characters in the XML content
            //string escapedXmlContent = System.Security.SecurityElement.Escape(xmlContent);

            //// Print the escaped XML content
            //Console.WriteLine(escapedXmlContent);

            // Create XmlReaderSettings to configure the XmlReader
            XmlReaderSettings settings = new XmlReaderSettings();

            // Set the CheckCharacters property to true to enable checking for illegal characters
            settings.CheckCharacters = true;

            try
            {
                using (MemoryStream stream = new MemoryStream(Encoding.Unicode.GetBytes(xmlString)))
                {
                    // Create an XmlReader to read the XML file
                    using (XmlReader reader = XmlReader.Create(stream, settings))
                    {
                        // Read the XML content using the XmlReader
                        while (reader.Read())
                        {
                            // Check if the current node is an element
                            if (reader.NodeType == XmlNodeType.Element)
                            {
                                // Do something with the element
                                Console.WriteLine("Element: " + reader.Name);
                            }
                            // Check if the current node is text
                            else if (reader.NodeType == XmlNodeType.Text)
                            {
                                // Do something with the text content
                                Console.WriteLine("Text: " + reader.Value);
                            }
                            // Handle other node types as needed
                        }
                    }
                }
            }
            catch (XmlException ex)
            {
                // Handle the XmlException
                Console.WriteLine("Error parsing XML file: " + ex.Message);
            }
        }

        private static void test39(string[] args)
        {
            if (args?.Length > 0 && File.Exists(args[0]))
            {
                ProcessResult(args[0]);
            }
        }

        private static void ProcessResult(String resultFile)
        {
            using (ModelSource models = new ModelSource())
            {
                XmlDocument data = new XmlDocument();
                data.Load(resultFile);
                if (data.DocumentElement?.FirstChild != null)
                {
                    foreach (var item in data.DocumentElement.ChildNodes)
                    {
                        var msgType = (item as XmlElement)?["MessageType"]?.InnerText;
                        var dataNo = (item as XmlElement)?["InvoiceNumber"]?.InnerText;
                        var code = (item as XmlElement)?["ReturnCode"]?.InnerText;
                        String no = dataNo;

                        models.TurnkeyLogFeedback(msgType, code, no);
                    }
                }



            }

        }

        private static void test38()
        {
            Console.WriteLine("Input TaskID:");
            int taskID;
            if (!int.TryParse(Console.ReadLine(), out taskID))
            {
                return;
            }

            var processor = new InvoiceExcelRequestForIssuerProcessor();
            processor.ProcessRequestItem(taskID);

        }

        private static void test37()
        {
            using (ModelSource<EIVOEntityDataContext> models = new ModelSource<EIVOEntityDataContext>())
            {
                String expr = "ReceiptNo.StartsWith(@0)";
                IQueryable items = models.GetTable<Organization>()
                    .Where(expr, "7076");
                String sqlCmd = items.ToString();
                items = items.OrderBy("CompanyName desc");
                sqlCmd = items.ToString();
                items = items.OrderBy("ReceiptNo");
                sqlCmd = items.ToString();
                items = items.Skip(1000).Take(500);
                sqlCmd = items.ToString();
            }
        }

        private static void test36()
        {
            // 根據傳入的類別名稱動態載入類別
            var type = typeof(ModelCore.DataEntity.Organization);
            if (type == null)
            {
                // 若類別不存在，回傳錯誤頁面
                return;
            }

            foreach (var propertyInfo in type.GetProperties())
            {
                if (propertyInfo.PropertyType.GetInterface("System.Data.Linq.ITable") != null)
                {
                    Console.WriteLine($"{propertyInfo.Name}:{propertyInfo.PropertyType}");
                }
            }
        }

        private static void test35(string[] args)
        {
            Uxnet.Com.Helper.DefaultTools.Program.Main(args);
        }

        private static void test34()
        {
            NameValueCollection data = new NameValueCollection();
            data.Add("SignDate", "中 華 民 國 一 一 一 年 十 二 月 十 七 日");
            data.Add("BuyerIdNo", "70762419");
            data.Add("BuyerAddress", "台北市中正區南海路20號6樓");
            data.Add("BuyerName", "網際優勢股份有限公司");
            data.Add("PayWeekDate", "三");
            data.Add("EndDate", "113 年 10 月 31 日");
            data.Add("CreditDate", "150");
            data.Add("Amount", "150,000,000");
            data.Add("No", "12-F1O-1234");
            ///印鑑圖檔送格式如下擇一：
            ///1、將圖檔讀出以base64 inline格式傳送
            ///或
            ///2、URL型式 => http(s)://...../someone.jpg
            ///範例採方法 1
            String buyerSeal = "buyer.jpg";
            data.Add("BuyerSeal", $"data:image/jpeg;base64,{Convert.ToBase64String(File.ReadAllBytes(buyerSeal))}");
            String sellerSeal = "seller.jpg";
            data.Add("SellerSeal", $"data:image/jpeg;base64,{Convert.ToBase64String(File.ReadAllBytes(sellerSeal))}");
            var pdfData = GetContractPdf("https://ff.uxcds.com/ContractHome/Home/GetContract", data);
        }

        public static byte[] GetContractPdf(String contractUrl, NameValueCollection values)
        {
            using (WebClient client = new WebClient())
            {
                return client.UploadValues(contractUrl, values);
            }
        }

        private static void test33()
        {
            //String dataToSign = "登入帳號:bdseller";
            //String dataSignature = "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
        }

        private static void test12(string[] args)
        {
            if (args.Length > 1)
            {
                foreach (var f in new DirectoryInfo(args[0]).GetFiles(args[1], SearchOption.AllDirectories))
                {
                    string s = File.ReadAllText(f.FullName, Encoding.GetEncoding(950));
                    string t = File.ReadAllText(f.FullName, Encoding.UTF8);
                    if (s != t)
                    {
                        File.WriteAllText(f.FullName, s, Encoding.UTF8);
                        Console.WriteLine($"{f.FullName} converted to utf-8!!");
                    }
                    else
                    {
                        Console.WriteLine($"{f.FullName} is utf-8!!");
                    }
                }
            }
        }
    }
}
