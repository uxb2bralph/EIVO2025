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
using DocumentFormat.OpenXml.Wordprocessing;
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

namespace TestConsole
{
    class Program
    {
        [STAThread()]
        static void Main(string[] args)
        {
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
            int docID = 0;
            if(args.Length>0 && int.TryParse(args[0], out docID))
            {
                using (ModelSource models = new ModelSource())
                {
                    var items = models.GetTable<InvoiceItem>()
                        .Where(i => i.InvoiceID > docID)
                        .Where(i => i.InvoiceBuyer.EMail != null)
                        .Where(i=>i.CDS_Document.IssuingNotice == null)
                        .Select(d => d.InvoiceID).ToArray();
                    items.NotifyIssuedInvoice(false, forceTodo: false);
                }
            }
            Console.ReadKey();
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

        private static void test41()
        {
            using (ModelSource models = new ModelSource())
            {
                var migC0501 = models.GetTable<C0501DispatchQueue>().Where(d => d.StepID == (int)Naming.InvoiceStepDefinition.回傳MIG)
                                    .ToList();

                foreach (var d in migC0501)
                {
                    var a = (d.CDS_Document.DerivedDocument?.ParentDocument?.InvoiceItem ?? d.CDS_Document.InvoiceItem)?.CreateF0501();
                    var s = a?.OuterXml;
                    var c = new MIGContent
                    {
                        DocID = d.DocID,
                        DocDate = d.CDS_Document.DocDate,
                        No = (d.CDS_Document.DerivedDocument?.ParentDocument?.InvoiceItem ?? d.CDS_Document.InvoiceItem)?.InvoiceNo(),
                        ReceiptNo = (d.CDS_Document.DerivedDocument?.ParentDocument?.InvoiceItem ?? d.CDS_Document.InvoiceItem)?.Organization?.ReceiptNo,
                        MIG = s
                    };
                    Console.WriteLine(c.JsonStringify());
                }
                var items = migC0501
                    .Select(d =>
                    new MIGContent
                    {
                        DocID = d.DocID,
                        DocDate = d.CDS_Document.DocDate,
                        No = (d.CDS_Document.DerivedDocument?.ParentDocument?.InvoiceItem ?? d.CDS_Document.InvoiceItem)?.InvoiceNo(),
                        ReceiptNo = (d.CDS_Document.DerivedDocument?.ParentDocument?.InvoiceItem ?? d.CDS_Document.InvoiceItem)?.Organization?.ReceiptNo,
                        MIG = (d.CDS_Document.DerivedDocument?.ParentDocument?.InvoiceItem ?? d.CDS_Document.InvoiceItem)?.CreateF0501()?.OuterXml
                    }).ToArray();

                //File.WriteAllText("G:\\temp\\data.json", items.JsonStringify());
                foreach (var item in items)
                {
                    Console.WriteLine(item.JsonStringify());
                }
            }
        }

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
            //String dataSignature = "MIIMEAYJKoZIhvcNAQcCoIIMATCCC/0CAQExCzAJBgUrDgMCGgUAMAsGCSqGSIb3DQEHAaCCClgwggUmMIIDDqADAgECAhEAhynNXPkL+rYS0mwvn2e23TANBgkqhkiG9w0BAQsFADA/MQswCQYDVQQGEwJUVzEwMC4GA1UECgwnR292ZXJubWVudCBSb290IENlcnRpZmljYXRpb24gQXV0aG9yaXR5MB4XDTEzMDEzMTAzMjkyMFoXDTMzMDEzMTAzMjkyMFowRDELMAkGA1UEBhMCVFcxEjAQBgNVBAoMCeihjOaUv+mZojEhMB8GA1UECwwY5bel5ZWG5oaR6K2J566h55CG5Lit5b+DMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAwbdYhBgAB22P4CF8qbDkOTLE8+A0OF1FZKLt4V214tS4PrJGfoRC02b059xKk+GJl0GxYVn/KAFYmz66vpnQxkmI8EPm03wZxKSp9VXKYnWlwlGjDlsJLmiUpA6kTKg4qPdRH6mJl6oV9ec4h1bSYASxr05zhFKA1BS6mwdGEGJTdOMVs4uDYD7upsd0JOrBfRqQg8oYi5l/XrPwJ1KPiQUdjamNkLeUMu88sQ5BY1QLPZ8z6+uGrJKYYOvwBIDUniUzx9goSDy8KO8s20KYlE1w30kiUtumnvdOyTSpIqwWHuhpwXcD84CVC0f4SsblXFSp+/lgbuoNg56wbTiaPwIDAQABo4IBFjCCARIwHwYDVR0jBBgwFoAU1Wcd4Jx6LJzLxZjnHQcmKobsdM0wHQYDVR0OBBYEFJlEegJy621lIrMCV4/Wod06Ag9sMA4GA1UdDwEB/wQEAwIBBjAUBgNVHSAEDTALMAkGB2CGdmUAAwMwEgYDVR0TAQH/BAgwBgEB/wIBADA+BgNVHR8ENzA1MDOgMaAvhi1odHRwOi8vZ3JjYS5uYXQuZ292LnR3L3JlcG9zaXRvcnkvQ1JMMi9DQ  S5jcmwwVgYIKwYBBQUHAQEESjBIMEYGCCsGAQUFBzAChjpodHRwOi8vZ3JjYS5uYXQuZ292LnR3L3JlcG9zaXRvcnkvQ2VydHMvSXNzdWVkVG9UaGlzQ0EucDdiMA0GCSqGSIb3DQEBCwUAA4ICAQAIQ0nAVoSBm67LIZXTFI2W5QbN5uT/2LN9dTAgHyGLf/tFGfUEbgWhv+FEQ07GY9qFzzwxaPj2amikOsHfQWamzsoSRnWx2IKZ3vPZviGd0Gpbtmaa3mJLwMhc+k4CXZvhk/GLgJh3Pg2jh2ifN3bZjJ6D5TEhl+vtU8pZ2mqOmYK9Nzfs7PLRle3zVTleq7ffn43cgGNXhhiidtoASINxQUGZsWcgZg9DiFvKqLMD+3/sfrRe0uUku/m2gR+LEViaHO19TuTHI57seN3h0NEieBf8JnTZQqzUSzn3RJkpGSvFAPOgYtagbwwRitygVCQ1JWoUYeteMIBJvfsf13sUZgIb3cVDOyqrfz2Woc3qusEAqTC6/kyhIUvM8KYu1DHYLwZTB9Ceyh5znKQaeEArLCaoktEqaT7fraXH3VWmArGIOyjh1xFOemR7turDaVwWEaQRtnRANxtz0yAfoZ99SfAMip3yWmhwiyV0Lwaxj63chlCXSZx27qkIOUFORtvk8HeV6W0+IgBlW4mW2GW3Mae7WoHzuz2Vy09XDgFzVHQXbLoDQXxtKBkpiNAevJ28dl0ihTMSHlmCVA1P+6AQ6rG7HAwjfCv2MNNDTqmrGh8rTtHrcxixEddKZky3v06CmxOucr8D+iqNU5kwMDMr0XOM+4cMTFy2HhX4HY/6yDCCBSowggQSoAMCAQICEQCrCyhV/Ul0ht2awRCzrVr+MA0GCSqGSIb3DQEBCwUAMEQxCzAJBgNVBAYTAlRXMRIwEAYDVQQKDAnooYzmlL/pmaIxITAfBgNVBAsMGOW3peWVhuaGkeitieeuoeeQhuS4reW/gzAeFw0yMTAzMDkwOTI1NDRaFw0yNjAzMDkwOT  I1NDRaMEkxCzAJBgNVBAYTAlRXMScwJQYDVQQKDB7ntrLpmpvlhKrli6LogqHku73mnInpmZDlhazlj7gxETAPBgNVBAUTCDcwNzYyNDE5MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA0sOGlxms8qTvCCoXDUFeU7tCKWXlffMt6QJVurEjk5D4PsC+N5a8TTuht4B8g3fP3zAOy3CT8O0SNyfoptGDS5s+7TrKSA4iW9d4c4+ibxhOHDp6e695HG6VxyJnAwDP78UFNGvWl2HCCTPRd4GPDdVm15jXHPVtahEq90pQOjRKrdlrNRjLvzJ7N2BwT2cJ8lazAtFjLbnOA+aNQaBygmbWi7ZQd975Ghjj1yrhsApoviwIbCjw51uXqRt6KKOfkEyHqkqS6cALXDsYvFR1mFISmSRd3GvfkJFgxuukymwXxZ2nlQ5uV6vvPyJXLjNZVJSoYIlZRptWOZPDtrgyKwIDAQABo4ICEDCCAgwwHwYDVR0jBBgwFoAUmUR6AnLrbWUiswJXj9ah3ToCD2wwHQYDVR0OBBYEFNUyK5MLPUxpkbYsgOq/V21xHkl9MIGeBggrBgEFBQcBAQSBkTCBjjBIBggrBgEFBQcwAoY8aHR0cDovL21vZWFjYS5uYXQuZ292LnR3L3JlcG9zaXRvcnkvQ2VydHMvSXNzdWVkVG9UaGlzQ0EucDdiMEIGCCsGAQUFBzABhjZodHRwOi8vbW9lYWNhLm5hdC5nb3YudHcvY2dpLWJpbi9PQ1NQMi9vY3NwX3NlcnZlci5leGUwDgYDVR0PAQH/BAQDAgeAMBQGA1UdIAQNMAswCQYHYIZ2ZQADAzAaBgNVHREEEzARgQ9vcC10cEB1eGIyYi5jb20wUQYDVR0JBEowSDAXBgdghnYBZAIBMQwGCmCGdgFkAwICAQEwFgYHYIZ2AWQCAjELEwlzZWNvbmRhcnkwFQYHYIZ2AWQCZTEKDAg3MDc2MjQxOTCBkwYDVR0fBIGLMIG  IMEKgQKA+hjxodHRwOi8vbW9lYWNhLm5hdC5nb3YudHcvcmVwb3NpdG9yeS9NT0VBQ0EvQ1JMMi9DUkxfMDA2NC5jcmwwQqBAoD6GPGh0dHA6Ly9tb2VhY2EubmF0Lmdvdi50dy9yZXBvc2l0b3J5L01PRUFDQS9DUkwyL2NvbXBsZXRlLmNybDANBgkqhkiG9w0BAQsFAAOCAQEAu5nmrDZyzGM3UQ5k0f7pbZP+/ZXiFORfgtZnUyqc6q5u8zlCovLXBi74+xmOhz0PSvVlL6z/Q7gLMKGEt+9Vfw0WcEKJxgNqs67tp9oqvhZ2fC3JEjO0MyQOYWC8Kgb2zsG3xFqsJzO24g9M3ym32zkNhYZX6ndia/ygwVYi5WYYjdaAkpzNNuFfbtcIzfnMqtaa4uoufqzMz0DzZt51mNZOHCgJWp2bAGSF5yaWqP7KHNB6JvsNncRbUjWFdPaEEUvssyoygdBodc45Km8nrZhrCBQOw8iMA6/ensg1LcPzODwwlrkVBMZReTiF3uuie268eVPz7MnU91JlMTgEizGCAYAwggF8AgEBMFkwRDELMAkGA1UEBhMCVFcxEjAQBgNVBAoMCeihjOaUv+mZojEhMB8GA1UECwwY5bel5ZWG5oaR6K2J566h55CG5Lit5b+DAhEAqwsoVf1JdIbdmsEQs61a/jAJBgUrDgMCGgUAMA0GCSqGSIb3DQEBAQUABIIBAKGbmXp9+lWFRK66+uYAMw4q0f+3x7QuJkSrd/lKfSRR7dOgUE5DN8TJIZLjd9n3H9OkXGdBQ23cManBPPss+sMWixQfRmcg6XdT2D9b7NGF9HvLlGwW3pyi97MAndwgF4dO0WZ75O2Oio4bCFA3Lo4k/BD9erIGRv4JjPZMuTed+CDWhlVcLFv/8PrkzcvTPkPJ1EuXvZwZFac+bxKYewn8pBsNPLiJP8W+PL6L9vkVS4YWbxWNncB2R5pj8RkWf9CjSQ25Yp+ZbYGsvny627bDE6pHm7yZQtq3eH6sY1Os  sUqbIBW441vBQscJRluuLydqQTzjkydnJK0VLN57Udw=";
            String dataToSign = "PID:7570530401";
            String dataSignature = "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\r\naySMkW8tyqwv/do0u1B8KascVlw==";
            CryptoUtility utility = new CryptoUtility();
            Console.WriteLine(Encoding.Default.CodePage);
            Console.WriteLine(utility.VerifyPKCS7(dataToSign, dataSignature));
        }

        static void SaveToExcel()
        {
            String[] seller = { "70762419", "30414175" };
            String[] sellerName = { "UXB2B", "CSC" };

            Random rand = new Random((int)(DateTime.Now.Ticks % 10000));

            using (DataSet ds = new DataSet())
            {
                for (int i = 0; i < seller.Length; i++)
                {
                    DataTable table = new DataTable();
                    table.TableName = $"{sellerName[i]}";
                    ds.Tables.Add(table);
                    table.Columns.Add("營業人名稱");
                    table.Columns.Add("統一編號");
                    table.Columns.Add("發票", typeof(int));
                    table.Columns.Add("作廢發票", typeof(int));
                    table.Columns.Add("折讓", typeof(int));
                    table.Columns.Add("作廢折讓", typeof(int));
                    table.Columns.Add("月份");

                    for (DateTime idx = new DateTime(2022, 1, 1); idx < DateTime.Today;)
                    {
                        var end = idx.AddMonths(1);
                        var r = table.NewRow();

                        r[0] = sellerName[i];
                        r[1] = seller[i];
                        r[2] = rand.Next(5000);
                        r[3] = rand.Next(5000);
                        r[4] = rand.Next(5000);
                        r[5] = rand.Next(5000);
                        r[6] = $"{idx:yyyyMM}";

                        table.Rows.Add(r);
                        idx = end;
                    }
                }

                using (XLWorkbook xls = new ClosedXML.Excel.XLWorkbook())
                {
                    xls.Worksheets.Add(ds);
                    xls.SaveAs("G:\\temp\\test.xlsx");
                }
            }

        }
        private static void test32()
        {
            String test = "CN=amylee, OU=70762419-RA-UXRA, OU=Universal Exchange Inc., OU=Public Certification Authority, O=\"Chunghwa Telecom Co., Ltd.\", C=TW";
            String input = "CN = amylee, OU=70762419-RA-UXRA, OU=Universal Exchange Inc., OU=Public Certification Authority, O=\"Chunghwa Telecom Co., Ltd.\", C=TW";
            String pattern = @"[^=,\s]*[\s]*?=((\""[^""]*\"")|[^,]*)";

            var all = Regex.Matches(input, pattern);
            var items = all.Cast<Match>().Select(m => m.Value.Split('='))
                  .Select(pair => new KeyValuePair<String, String>(pair[0].Trim().ToUpper(), pair[1].Trim()))
                  .ToList();
        }

        private static void test31()
        {
            Regex reg = new Regex("^/((?<!,)[A-Z0-9+-\\.](?!,)){7}$");
            String line;
            while ((line = Console.ReadLine()) != "")
            {
                Console.WriteLine($"Matched : {reg.IsMatch(line)}");
            }
        }

        private static void test30()
        {
            Organization[] items = JsonConvert.DeserializeObject<Organization[]>(File.ReadAllText("G:\\temp\\Organization.json"));
            using (ModelSource<InvoiceItem> models = new ModelSource<InvoiceItem>())
            {
                var table = models.GetTable<Organization>();
                foreach (var item in items)
                {
                    if (!table.Any(o => o.ReceiptNo == item.ReceiptNo))
                    {
                        table.InsertOnSubmit(item);
                        models.SubmitChanges();
                    }
                }
            }
        }

        private static void test29(string[] args)
        {
            if (args != null && args.Length > 0)
            {
                using (ModelSource<InvoiceItem> models = new ModelSource<InvoiceItem>())
                {
                    var token = models.GetTable<Organization>().Where(c => c.ReceiptNo == args[0])
                            .FirstOrDefault()?.OrganizationToken;

                    if (token != null)
                    {
                        AuthTokenViewModel viewModel = new AuthTokenViewModel
                        {
                            SellerID = args[0],
                            Seed = $"{DateTime.Now.Ticks % 100000000:00000000}",
                        };

                        using (SHA256 hash = SHA256.Create())
                        {
                            viewModel.Authorization = token.ComputeAuthorization(hash, viewModel.Seed);
                            Console.WriteLine(viewModel.JsonStringify());
                        }
                    }
                }
            }
        }

        private static void test28(string[] args)
        {
            if (args != null && args.Length > 0)
            {
                if (Directory.Exists(args[0]))
                {
                    var files = Directory.EnumerateFiles(args[0], "*.xml");
                    if (files.Any())
                    {
                        List<AutomationItem> container = new List<AutomationItem>();
                        XmlDocument docRes = new XmlDocument();
                        String storedPath = Logger.LogDailyPath.CheckStoredPath();
                        foreach (var f in files)
                        {
                            docRes.Load(f);
                            var responseItem = docRes.ConvertTo<Automation>();
                            if (responseItem.Item != null && responseItem.Item.Length > 0)
                            {
                                container.AddRange(responseItem.Item);
                            }

                            String storedFile = Path.Combine(storedPath, Path.GetFileName(f));
                            if (File.Exists(storedFile))
                            {
                                File.Delete(storedFile);
                            }
                            File.Move(f, storedFile);
                        }

                        if (container.Any())
                        {
                            using (DataSet ds = new DataSet())
                            {
                                DataTable table = null;
                                if (container[0].Invoice != null)
                                {
                                    table = container
                                    .Select(i => new
                                    {
                                        i.Invoice?.SellerId,
                                        i.Invoice?.InvoiceNumber,
                                        i.Status,
                                        i.Description,
                                    })
                                    .ToDataTable();
                                }
                                else if (container[0].CancelInvoice != null)
                                {
                                    table = container
                                    .Select(i => new
                                    {
                                        i.CancelInvoice?.SellerId,
                                        i.CancelInvoice?.CancelInvoiceNumber,
                                        i.Status,
                                        i.Description,
                                    })
                                    .ToDataTable();
                                }
                                else if (container[0].Allowance != null)
                                {
                                    table = container
                                    .Select(i => new
                                    {
                                        i.Allowance?.SellerId,
                                        i.Allowance?.AllowanceNumber,
                                        i.Status,
                                        i.Description,
                                    })
                                    .ToDataTable();
                                }
                                else if (container[0].CancelAllowance != null)
                                {
                                    table = container
                                    .Select(i => new
                                    {
                                        i.CancelAllowance?.SellerId,
                                        i.CancelAllowance?.CancelAllowanceNumber,
                                        i.Status,
                                        i.Description,
                                    })
                                    .ToDataTable();
                                }

                                if (table != null)
                                {
                                    table.TableName = "Response Data";
                                    ds.Tables.Add(table);

                                    using (var xls = ds.ConvertToExcel())
                                    {
                                        String outputName = Path.Combine(args[0].CheckStoredPath(), $"report_{DateTime.Today.AddDays(-1):yyyyMMdd}.xlsx");
                                        xls.SaveAs(outputName);
                                    }
                                }
                            }
                        }
                    }
                }

            }
        }

        private static void test27()
        {
            using (ModelSource<InvoiceItem> models = new ModelSource<InvoiceItem>())
            {
                var invoiceItems = models.GetTable<InvoiceItem>().OrderByDescending(i => i.InvoiceID)
                                .Take(20);

                var dataItems = invoiceItems.Select(i => i.CreateF0401(false)).ToArray();

                var jsonData = JsonConvert.SerializeObject(dataItems);
                File.WriteAllText("G:\\temp\\INV0401.json", jsonData);

                var cancelItems = models.GetTable<InvoiceCancellation>()
                    .Select(c => c.InvoiceItem)
                    .OrderByDescending(i => i.InvoiceID)
                                .Take(20);

                jsonData = JsonConvert.SerializeObject(cancelItems.Select(i => i.CreateCancelInvoiceMIG(false)).ToArray());
                File.WriteAllText("G:\\temp\\INV0501.json", jsonData);

                var allowanceItems = models.GetTable<InvoiceAllowance>()
                    .OrderByDescending(i => i.AllowanceID)
                                .Take(20);

                jsonData = JsonConvert.SerializeObject(allowanceItems.Select(i => i.CreateAllowanceMIG(models, false)).ToArray());
                File.WriteAllText("G:\\temp\\ALN0401.json", jsonData.Replace(".00000", ""));

                var cancelAllowance = models.GetTable<InvoiceAllowanceCancellation>()
                    .Select(c => c.InvoiceAllowance)
                    .OrderByDescending(i => i.AllowanceID)
                                .Take(20);

                jsonData = JsonConvert.SerializeObject(cancelAllowance.Select(i => i.CreateG0501(false)).ToArray());
                File.WriteAllText("G:\\temp\\ALN0501.json", jsonData);
            }
        }

        public class InvoiceData
        {
            public InvoiceItem DataItem { get; set; }
            public InvoiceAmountType Amount => DataItem.InvoiceAmountType;
            public InvoiceSeller Seller => DataItem.InvoiceSeller;
            public InvoiceBuyer Buyer => DataItem.InvoiceBuyer;
            public InvoiceCarrier Carrier => DataItem.InvoiceCarrier;
            public InvoiceDonation Donation => DataItem.InvoiceDonation;
            public ProductItem[] Products => DataItem.InvoiceDetails
                .SelectMany(d => d.InvoiceProduct.InvoiceProductItem)
                .Select(p => new ProductItem
                {
                    Product = p,
                })
                .ToArray();
        }

        public class ProductItem
        {
            public InvoiceProductItem Product { get; set; }
            public String Brief => Product.InvoiceProduct.Brief;


        }

        private static void test26(string[] args)
        {
            if (args != null && args.Length > 0)
            {
                CipherDecipherSrv cipher = new CipherDecipherSrv(10);
                Console.WriteLine(cipher.cipher(args[0]));
            }
        }

        private static void test25()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("G:\\temp\\123.xml");

            using (StreamWriter writer = new StreamWriter("G:\\temp\\data.xml", false, Encoding.GetEncoding("utf-8")))
            {
                doc.Save(writer);
            }
        }

        private static void test24()
        {
            String aesNo = AESEncrypt("FP10000803" + "6577", "374EC8AF1BF8F4C21C7428DDC938B107");
            Console.WriteLine(aesNo);
            aesNo = ("FP10000803").EncryptContent("6577");
        }

        public static string AESEncrypt(string plainText, string AESKey)
        {
            byte[] bytes = Encoding.Default.GetBytes(plainText);
            RijndaelManaged rijndaelManaged = new RijndaelManaged
            {
                KeySize = 128,
                Key = AESKey.HexToByteArray(),
                BlockSize = 128,
                IV = Convert.FromBase64String("Dt8lyToo17X/XkXaQvihuA==")
            };
            ICryptoTransform encryptor = rijndaelManaged.CreateEncryptor();
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write);
            cryptoStream.Write(bytes, 0, bytes.Length);
            cryptoStream.FlushFinalBlock();
            cryptoStream.Close();
            var buf = memoryStream.ToArray();
            return Convert.ToBase64String(buf);
        }

        private static void test23()
        {
            using (InvoiceManager models = new InvoiceManager())
            {
                InvoiceHandler a0501 = new InvoiceHandler(models);
                a0501.WriteA0201ToTurnkey();
            }
        }

        private static void test22(string[] args)
        {
            if (args != null && args.Length > 0)
            {
                if (File.Exists(args[0]))
                {
                    var lines = File.ReadAllLines(args[0]);
                    if (lines != null && lines.Length > 0)
                    {
                        List<AllowanceRootAllowance> items = new List<AllowanceRootAllowance>();
                        using (InvoiceManager models = new InvoiceManager())
                        {
                            var table = models.GetTable<InvoiceItem>();
                            var dataItems = lines.Select(l => l.GetEfficientString())
                                    .Where(l => l != null)
                                    .Where(l => l.Length == 10)
                                    .Select(l => new
                                    {
                                        TrackCode = l.Substring(0, 2),
                                        No = l.Substring(2)
                                    })
                                    .Select(n => table.Where(i => i.TrackCode == n.TrackCode && i.No == n.No).FirstOrDefault())
                                    .Where(i => i != null)
                                    .Select(i => ToAllowance(i))
                                    .ToArray();

                            if (dataItems != null && dataItems.Length > 0)
                            {
                                AllowanceRoot allowance = new AllowanceRoot
                                {
                                    Allowance = dataItems,
                                };
                                String fileName = Path.Combine(Logger.LogDailyPath, $"taiwan_uxb2b_print_allowance_request_C_{DateTime.Now:yyyyMMddHHmmss_ffffff}.xml");
                                allowance.ConvertToXml().Save(fileName);
                            }

                        }
                    }
                }
            }
        }

        private static AllowanceRootAllowance ToAllowance(InvoiceItem item)
        {
            var productItems = item.InvoiceDetails.SelectMany(d => d.InvoiceProduct.InvoiceProductItem);
            short idx = 1;
            return new AllowanceRootAllowance
            {
                AllowanceNumber = item.InvoicePurchaseOrder?.OrderNo,
                AllowanceDate = DateTime.Today.ToString("yyyy/MM/dd"),
                GoogleId = item.InvoiceBuyer.CustomerID,
                SellerId = item.InvoiceSeller.ReceiptNo,
                BuyerName = item.InvoiceBuyer.Name,
                BuyerId = item.InvoiceBuyer.ReceiptNo,
                AllowanceType = 1,
                AllowanceItem = productItems.Select(p => new AllowanceRootAllowanceAllowanceItem
                {
                    OriginalDescription = p.InvoiceProduct.Brief,
                    Quantity = p.Piece ?? 1,
                    UnitPrice = p.UnitCost ?? 0,
                    Amount = p.CostAmount ?? 0,
                    Tax = (p.CostAmount ?? 0) * 0.05m,
                    AllowanceSequenceNumber = idx++,
                    TaxType = 1
                }).ToArray(),
                TaxAmount = item.InvoiceAmountType.TaxAmount ?? 0,
                TotalAmount = item.InvoiceAmountType.TotalAmount ?? 0,
                Currency = item.InvoiceAmountType.CurrencyType?.AbbrevName ?? "TWD"
            };
        }

        private static void test20(string[] args)
        {
            DateTime calcPeriod = DateTime.Today.AddMonths(-2);
            int year = calcPeriod.Year;
            int period = (calcPeriod.Month + 1) / 2;
            int? sellerID = null;

            if (args.Length > 1)
            {
                if (int.TryParse(args[0], out int v))
                {
                    year = v;
                }
                if (int.TryParse(args[1], out v))
                {
                    period = v;
                }

                if (args.Length > 2)
                {
                    if (int.TryParse(args[2], out v))
                    {
                        sellerID = v;
                    }
                }
            }



            using (TrackNoIntervalManager models = new TrackNoIntervalManager())
            {
                //models.SettleVacantInvoiceNo(year, period);
                models.SettleUnassignedInvoiceNOPeriodically(year, period, sellerID);
                if (sellerID.HasValue)
                {
                    foreach (var item in models.GetTable<InvoiceIssuerAgent>().Where(r => r.AgentID == sellerID))
                    {
                        models.SettleUnassignedInvoiceNOPeriodically(year, period, item.IssuerID);
                    }
                }
            }
        }

        private static void test19()
        {
            using (InvoiceManager models = new InvoiceManager())
            {
                InvoiceHandler c0501 = new InvoiceHandler(models);
                //c0501.NotifyIssued();
                c0501.WriteF0501ToTurnkey();
            }
        }

        private static void test18()
        {
            String jsonData = @"
{
    ""result"":true,
    ""data"":[1,3,4,5,6,2]
}
";
            dynamic json = JsonConvert.DeserializeObject(jsonData);
            _Test result = JsonConvert.DeserializeObject<_Test>(jsonData);
            Console.WriteLine(json.result);
        }

        class _Test
        {
            public bool? result { get; set; }
            public object data { get; set; }
            public int?[] asArray => (data as JArray)?.Select(i => (int?)i).ToArray();
        }

        private static void test17()
        {
            using (ModelSource<InvoiceItem> models = new ModelSource<InvoiceItem>())
            {
                var orgItem = models.GetTable<Organization>().Where(o => o.ReceiptNo == "70762419").FirstOrDefault();
                Console.WriteLine(JsonConvert.SerializeObject(orgItem));
            }
        }

        private static void test16()
        {
            (new Class1()).Test();
            (new Class2()).Test();
        }

        private static void test14()
        {
            var t = Task.Run(() =>
            {
                throw new Exception("test...");
            });

            t.ContinueWith(ts =>
            {
                Console.WriteLine(ts.Exception);

            });
        }

        private static void test15()
        {
            using (ModelSource<InvoiceItem> models = new ModelSource<InvoiceItem>())
            {
                var items = models.GetTable<InvoiceAllowance>()
                                .Join(models.GetTable<InvoiceAllowanceSeller>().Where(i => i.ReceiptNo == "27934855"),
                                    a => a.AllowanceID, s => s.AllowanceID, (a, s) => a)
                                .Join(models.GetTable<InvoiceAllowanceCancellation>(),
                                    a => a.AllowanceID, c => c.AllowanceID, (a, c) => c)
                                .OrderByDescending(i => i.AllowanceID).Take(100);

                var test = (IQueryable<dynamic>)items;

                using (DataSet ds = items.GetVoidAllowanceData(models))
                {
                    using (XLWorkbook xls = new ClosedXML.Excel.XLWorkbook())
                    {
                        xls.Worksheets.Add(ds);
                        xls.SaveAs("G:\\temp\\test-3.xlsx");
                    }
                }

            }
        }

        private static void test13()
        {
            using (ModelSource<InvoiceItem> models = new ModelSource<InvoiceItem>())
            {
                var items = models.GetTable<InvoiceItem>()
                                .Where(i => i.InvoiceSeller.ReceiptNo == "42954865")
                    .OrderByDescending(i => i.InvoiceID).Take(100);
                var test = (IQueryable<dynamic>)items;

                using (DataSet ds = items.GetInvoiceData(models))
                {
                    using (XLWorkbook xls = new ClosedXML.Excel.XLWorkbook())
                    {
                        xls.Worksheets.Add(ds);
                        xls.SaveAs("G:\\temp\\test-2.xlsx");
                    }
                }

            }
        }

        //private static void test12(string[] args)
        //{
        //    eInvoiceServiceSoapClient client = new eInvoiceServiceSoapClient("eInvoiceServiceSoap", "http://eceivo.uxifs.com/Published/eInvoiceService.asmx");
        //    Console.WriteLine(client.Endpoint.Address);

        //    if (args.Length > 1)
        //    {
        //        foreach (var f in new DirectoryInfo(args[0]).GetFiles(args[1], SearchOption.AllDirectories))
        //        {
        //            string s = File.ReadAllText(f.FullName, Encoding.GetEncoding(950));
        //            string t = File.ReadAllText(f.FullName, Encoding.UTF8);
        //            if (s != t)
        //            {
        //                File.WriteAllText(f.FullName, s, Encoding.UTF8);
        //                Console.WriteLine($"{f.FullName} converted!!");
        //            }
        //            else
        //            {
        //                Console.WriteLine($"{f.FullName} is utf-8!!");
        //            }
        //        }
        //    }
        //}

        private static void test11()
        {
            FileLogger.Logger.OutputWriter = Console.Out;
            Logger.Info("Run....");
            Logger.Info("test....");
            Logger.Info($"{DateTime.Now.Ticks}");
            Logger.Info("Run....");
            Logger.Info("test....");
            Logger.Info($"{DateTime.Now.Ticks}");

            Console.ReadKey();
        }

        private static void test10()
        {
            String xml = @"<?xml version='1.0' encoding='UTF-8' ?>
<AllowanceRoot>
	<Allowance>
		<AllowanceNumber>GU3DKNJSL4YQ0000</AllowanceNumber>
		<AllowanceDate>2019/02/22</AllowanceDate>
		<GoogleId>6101132063883093</GoogleId>
		<SellerId>42523557</SellerId>
		<BuyerName>2498</BuyerName>
		<BuyerId>0000000000</BuyerId>
		<AllowanceType>1</AllowanceType>
		<AllowanceItem>
			<OriginalDescription>Google Play 電影</OriginalDescription>
			<Quantity>1</Quantity>
			<UnitPrice>705</UnitPrice>
			<Amount>705</Amount>
			<Tax>35</Tax>
			<AllowanceSequenceNumber>1</AllowanceSequenceNumber>
			<TaxType>1</TaxType>
		</AllowanceItem>
		<TaxAmount>35</TaxAmount>
		<TotalAmount>740</TotalAmount>
	</Allowance>
</AllowanceRoot>";

            using (WebClient client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/xml";
                client.Encoding = Encoding.UTF8;
                Console.WriteLine(client.UploadString("http://localhost:2598/DataView/ConvertDataToAllowance", xml));
            }
        }

        private static void test09()
        {
            TheB b = new TheB();
            Console.WriteLine(b.Say());
            Console.WriteLine(((TheA)b).Say());
            Console.WriteLine(((TheBase)b).Say());
        }

        class TheBase
        {
            public virtual String Say()
            {
                return "nothing";
            }
        }

        class TheA : TheBase
        {
            public override String Say()
            {
                return "AAA";
            }
        }

        class TheB : TheA
        {
            public override String Say()
            {
                return "BBB";
            }
        }

        private static void test08(string[] args)
        {
            if (args != null && args.Length > 0)
            {
                (new TestC0701() { MIG_Only = true }).StartUp(args[0]);
            }
        }

        class TestC0701
        {
            public bool MIG_Only { get; set; }

            public void StartUp(String fileName)
            {
                String storedPath = Path.Combine(Logger.LogDailyPath, "MIG");
                storedPath.CheckStoredPath();
                ModelExtension.Properties.AppSettings.Default.F0701Outbound.CheckStoredPath();

                String invoiceNo = File.ReadAllText(fileName);

                String[] items = invoiceNo.Split(new String[] { "\r\n", ",", ";", "、" }, StringSplitOptions.RemoveEmptyEntries);
                Parallel.ForEach(items, invNo =>
                {
                    if (invNo.Length == 10)
                    {
                        doProcess(invNo, storedPath);
                    }
                });
            }

            void doProcess(String invoiceNo, String storedPath)
            {
                using (ModelSource<InvoiceItem> mgr = new ModelSource<InvoiceItem>())
                {
                    var item = mgr.EntityList
                        .Where(i => i.TrackCode == invoiceNo.Substring(0, 2)
                            && i.No == invoiceNo.Substring(2))
                        .OrderByDescending(i => i.InvoiceID)
                        .FirstOrDefault();
                    if (item != null)
                    {
                        doC0701(mgr, item, storedPath);
                    }

                }
            }

            void doC0701(ModelSource<InvoiceItem> mgr, InvoiceItem item, String storedPath)
            {
                storedMIG(item, storedPath);
                if (!MIG_Only)
                {
                    mgr.ExecuteCommand(@"DELETE FROM CDS_Document
                                            FROM    DerivedDocument INNER JOIN
                                                    CDS_Document ON DerivedDocument.DocID = CDS_Document.DocID
                                            WHERE   (DerivedDocument.SourceID = {0})", item.InvoiceID);
                    mgr.ExecuteCommand("delete CDS_Document where DocID={0}", item.InvoiceID);
                }
                Console.WriteLine($"{item.TrackCode}{item.No} done!!");
            }

            void storedMIG(InvoiceItem item, String storedPath)
            {
                String invoiceNo = item.TrackCode + item.No;
                item.CreateF0401().Save(Path.Combine(storedPath, "INV0401_" + invoiceNo + ".xml"));
                item.CreateF0701().Save(Path.Combine(ModelExtension.Properties.AppSettings.Default.F0701Outbound, "C0701_" + item.TrackCode + item.No + ".xml"));

            }
        }


        private static void test08()
        {
            String[] files = Directory.GetFiles("D:\\Download");
            var mode = DateTime.Now.Ticks % 10 + 1;
            var list = files.OrderBy(f => Path.GetFileName(f).Sum(c => (int)c) % mode).Take(10).ToArray();
        }

        private static void test07()
        {
            using (ModelSource<InvoiceItem> models = new ModelSource<InvoiceItem>())
            {
                var items = models.GetTable<InvoiceBuyer>().GroupJoin(models.GetTable<Organization>(), b => b.BuyerID, o => o.CompanyID,
                    (b, o) => new { buyer = b, org = o });
                Console.WriteLine(items.Count());
            }
        }

        private static void test06()
        {
            using (ModelSource<InvoiceItem> models = new ModelSource<InvoiceItem>())
            {
                DataLoadOptions ops = new DataLoadOptions();
                ops.LoadWith<InvoiceItem>(i => i.InvoiceBuyer);
                ops.LoadWith<InvoiceItem>(i => i.InvoiceSeller);
                ops.LoadWith<InvoiceItem>(i => i.InvoiceCancellation);
                ops.LoadWith<InvoiceItem>(i => i.InvoiceAmountType);
                ops.LoadWith<InvoiceItem>(i => i.InvoiceWinningNumber);
                ops.LoadWith<InvoiceItem>(i => i.InvoiceCarrier);
                ops.LoadWith<InvoiceItem>(i => i.InvoiceDonation);
                ops.LoadWith<InvoiceItem>(i => i.InvoicePurchaseOrder);
                ops.LoadWith<InvoiceItem>(i => i.InvoiceDetails);
                ops.LoadWith<InvoiceDetail>(i => i.InvoiceProduct);
                ops.LoadWith<InvoiceProduct>(i => i.InvoiceProductItem);

                models.GetDataContext().LoadOptions = ops;

                var invoices = models.GetTable<InvoiceItem>()
                        .Where(i => i.SellerID == 14567);

                //foreach (var item in invoices)
                //{
                //    foreach (var d in item.InvoiceDetails)
                //    {
                //        d.InvoiceProduct.InvoiceProductItem.ToArray();
                //    }
                //}

                var items = invoices
                        .Select(i => new InvoiceEntity
                        {
                            MainItem = i,
                            ItemDetails = i.InvoiceDetails.Select(d => d.InvoiceProduct).ToList()
                        });


                var s = JsonConvert.SerializeObject(items);
                File.WriteAllText("D:\\data.json", s);
            }
        }

        private static void test05()
        {
            var t1 = Task.Run(() =>
            {
                Console.WriteLine("go to sleep...");
                Thread.Sleep(3000);
                throw new Exception("test...");
            });

            var t2 = t1.ContinueWith(ts =>
            {
                Console.WriteLine("wake up...");
            });

            //t2.Wait();
            Console.ReadKey();
        }

        private static void test04()
        {
            using (ModelSource<InvoiceItem> models = new ModelSource<InvoiceItem>())
            {
                var item = models.GetTable<InvoiceItem>().OrderByDescending(i => i.InvoiceID).FirstOrDefault();
                var a0401 = item.CreateF0401();
                var data = JsonConvert.SerializeObject(a0401);
                var a0101 = JsonConvert.DeserializeObject<ModelCore.Schema.TurnKey.Invoice.Invoice>(data);
            }
        }

        private static void test03()
        {
            byte? b;
            String data = "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";
            CryptoUtility crypto = new CryptoUtility();
            byte[] dataToSign;
            var result = crypto.VerifyEnvelopedPKCS7(Convert.FromBase64String(data), out dataToSign);
            Console.WriteLine(result);
        }

        public enum TEST01
        {
            A,
            B
        }

        private static void test02()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("test.xml");
            X509Certificate2 cert;
            cert = new X509Certificate2("30414175.p12", "111111", X509KeyStorageFlags.Exportable);
            //using (FileStream fs = new FileStream("DefaultSigner.bin", FileMode.Open, FileAccess.Read))
            //{
            //    using (MemoryStream ms = new MemoryStream())
            //    {
            //        fs.CopyTo(ms);
            //        CipherDecipherSrv cipher = new CipherDecipherSrv();
            //        var certBytes = cipher.decipherCode(ms.ToArray());
            //        cert = new X509Certificate2(certBytes);
            //        ms.Close();
            //    }
            //    fs.Close();
            //}
            if (CryptoUtility.SignXmlSHA256(doc, "Microsoft Strong Cryptographic Provider", null, cert))
            {
                doc.Save("SignContext.xml");
            }
        }

        private static void test01()
        {
            using (ModelSource<InvoiceItem> models = new ModelSource<InvoiceItem>())
            {
                var items = models.Items.Select((i, x) => new
                {
                    RowIndex = x,
                    InvoiceNo = i.TrackCode + i.No
                }).Take(100).ToList();

            }
        }
    }
}
