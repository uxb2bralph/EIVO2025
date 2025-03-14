using JobHelper.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonLib.Utility;
using CommonLib.Core.Utility;

namespace JobHelper
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args != null && args.Length > 0)
            {
                switch(args[0])
                {
                    case "1":
                        DecryptPGP();
                        break;
                    case "2":
                        PrepareRequestFiles();
                        break;
                    case "3":
                        RetrieveResponse();
                        break;
                }
            }
        }

        private static void RetrieveResponse()
        {
            using (StreamWriter writer = new StreamWriter("retrieve.bat"))
            {
                foreach (var type in AppSettings.Default.RequestType)
                {
                    foreach (var filePath in Directory.EnumerateFiles($"{AppSettings.Default.InvoiceTransactionRoot}\\{type}(Response)"))
                    {
                        String fileName = Path.GetFileName(filePath);
                        writer.WriteLine($"move {filePath} {AppSettings.Default.ResponseReady}");
                        String dest = $"{fileName.Substring(0, fileName.Length - 4)}_Response.xml";
                        writer.WriteLine($"ren {AppSettings.Default.ResponseReady}\\{fileName} {dest}");
                        writer.WriteLine(String.Format(AppSettings.Default.CommandEncryptGPG, $"{AppSettings.Default.ResponseReady}\\{dest}", $"{AppSettings.Default.ResponseReady}\\{dest}.pgp"));
                    }
                    foreach (var filePath in Directory.EnumerateFiles($"{AppSettings.Default.InvoiceTransactionRoot}\\{type}(Failure)"))
                    {
                        String fileName = Path.GetFileName(filePath);
                        writer.WriteLine($"move {filePath} {AppSettings.Default.ResponseReady}");
                        String dest = $"{fileName.Substring(0, fileName.Length - 4)}_Failure.xml";
                        writer.WriteLine($"ren {AppSettings.Default.ResponseReady}\\{fileName} {dest}");
                        writer.WriteLine(String.Format(AppSettings.Default.CommandEncryptGPG, $"{AppSettings.Default.ResponseReady}\\{dest}", $"{AppSettings.Default.ResponseReady}\\{dest}.pgp"));
                    }

                }
            }

            using (StreamWriter writer = new StreamWriter("clear.bat"))
            {
                String pgpStore = Path.Combine(Logger.LogDailyPath, "UPPGP").CheckStoredPath();
                writer.WriteLine($"move {AppSettings.Default.ResponseReady}\\*.* {pgpStore}");
            }
        }

        private static void DecryptPGP()
        {
            using(StreamWriter writer = new StreamWriter("decrypt.bat"))
            {
                String pgpStore = Path.Combine(Logger.LogDailyPath, "PGP").CheckStoredPath();
                foreach (var filePath in Directory.EnumerateFiles(AppSettings.Default.Workspace))
                {
                    if (filePath.EndsWith(".gpg", StringComparison.CurrentCultureIgnoreCase)
                        || filePath.EndsWith(".pgp", StringComparison.CurrentCultureIgnoreCase))
                    {
                        writer.WriteLine(String.Format(AppSettings.Default.CommandDecryptGPG, filePath, filePath.Substring(0, filePath.Length - 4)));
                        writer.WriteLine($"move {filePath} {pgpStore}");
                    }
                }
            }
        }

        private static void PrepareRequestFiles()
        {
            using (StreamWriter writer = new StreamWriter("prepare.bat"))
            {
                foreach (var filePath in Directory.EnumerateFiles(AppSettings.Default.Workspace))
                {
                    if (filePath.EndsWith(".xml", StringComparison.CurrentCultureIgnoreCase))
                    {
                        FileInfo info = new FileInfo(filePath);
                        String fileName = Path.GetFileName(filePath);
                        if (info.Length == 0)
                        {
                            writer.WriteLine($"move {filePath} {AppSettings.Default.ResponseReady}");
                            String dest = $"{fileName.Substring(0, fileName.Length - 4)}_Response.xml";
                            writer.WriteLine($"ren {AppSettings.Default.ResponseReady}\\{fileName} {dest}");
                            writer.WriteLine(String.Format(AppSettings.Default.CommandEncryptGPG, $"{AppSettings.Default.ResponseReady}\\{dest}", $"{AppSettings.Default.ResponseReady}\\{dest}.pgp"));
                        }
                        else
                        {
                            foreach(var type in AppSettings.Default.RequestType)
                            {
                                if(fileName.Contains(type))
                                {
                                    writer.WriteLine($"move {filePath} {AppSettings.Default.InvoiceTransactionRoot}\\{type}");
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        public static Process RunBatch(String batchFileName, String args)
        {
            Logger.Info($"{batchFileName} {args}");
            ProcessStartInfo info = new ProcessStartInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, batchFileName), args)
            {
                CreateNoWindow = true,
                UseShellExecute = false
            };
            return Process.Start(info);
        }
    }
}
