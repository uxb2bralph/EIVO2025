using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using CommonLib.Utility;
using Microsoft.Extensions.Logging;
using ModelCore.Helper;
using ModelCore.Models;
using ModelCore.Models.ViewModel;
using ModelCore.Properties;
using TaskCenter.Core.DTOs;
using TaskCenter.Core.Interfaces;
using SharedApplyService = ModelCore.Models.InvoiceNumberApplyService;

namespace TaskCenter.Core.Services
{
    /// <summary>
    /// 電子發票字軌號碼申請查詢 / 維護服務實作。
    /// 檔案列舉、Word 範本產出、營業人轉換皆委派共用引擎 <see cref="SharedApplyService"/>（ModelExtension.EF），
    /// 設定取自 <see cref="TaskCenter.Properties.AppSettings"/>。
    /// </summary>
    public class InvoiceNumberApplyService : IInvoiceNumberApplyService
    {
        private readonly ILogger<InvoiceNumberApplyService> _logger;

        private static InvoiceNumberApplySetting Setting
            => TaskCenter.Properties.AppSettings.Default.InvoiceNumberApplySetting;

        private static IEnumerable<InvoiceNumberApplyWordSetting> WordSettings
            => TaskCenter.Properties.AppSettings.Default.InvoiceNumberApplyWordSetting;

        public InvoiceNumberApplyService(ILogger<InvoiceNumberApplyService> logger)
        {
            _logger = logger;
        }

        public List<InvoiceNumberApplyItemDto> GetApplyFiles(string? businessId)
        {
            var setting = Setting;

            // 確保基底資料夾存在（避免 EnumerateFiles 於空環境丟例外；沿用舊版 GetApplyJsonFilesInfo 之建立行為）。
            Directory.CreateDirectory(setting.ApplyFileBaseFolder);

            var service = new SharedApplyService(setting, businessId.GetEfficientString());
            var files = service.GetApplyJsonFiles();

            return files.Select(path =>
            {
                var fileName = Path.GetFileNameWithoutExtension(path);
                // 沿用舊版 QueryItemList2024.cshtml：以 '_' 之後的字串作為統一編號。
                var idx = fileName.IndexOf('_');
                var parsedBusinessId = idx >= 0 ? fileName.Substring(idx + 1) : fileName;
                return new InvoiceNumberApplyItemDto
                {
                    BusinessId = parsedBusinessId,
                    ApplyUpdateTime = new FileInfo(path).LastWriteTime,
                    KeyId = path.EncryptData(),
                };
            })
            .OrderByDescending(x => x.ApplyUpdateTime)
            .ToList();
        }

        public string? ResolveApplyFilePath(string? keyId)
        {
            var key = keyId.GetEfficientString();
            if (key == null)
            {
                return null;
            }

            string? filePath;
            try
            {
                filePath = key.DecryptData();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to decrypt InvoiceNumberApply KeyId");
                return null;
            }

            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                return null;
            }

            return filePath;
        }

        public InvoiceNumberApply? LoadApply(string filePath)
        {
            try
            {
                return filePath.DeserializeObjectFromFile<InvoiceNumberApply>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize InvoiceNumberApply file {FilePath}", filePath);
                return null;
            }
        }

        public OrganizationViewModel ConvertToOrganization(InvoiceNumberApply apply)
        {
            return new SharedApplyService(Setting).ApplyConvertedOrganization(apply);
        }

        public void MoveJsonFile(string filePath)
        {
            new SharedApplyService(Setting).MoveJsonFile(filePath);
        }

        public (byte[] Content, string FileName)? BuildWordZip(string businessId)
        {
            var setting = Setting;

            var apply = new SharedApplyService(setting, businessId).GetApplyViewModelFromJson();
            if (apply == null)
            {
                return null;
            }

            var encoding = new UTF8Encoding();
            using var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                foreach (var wordSet in WordSettings)
                {
                    var entry = archive.CreateEntry(wordSet.OutputName);
                    var builder = new SharedApplyService(setting, apply, wordSet);
                    var contentAsBytes = encoding.GetBytes(builder.GetReplacedWordXmlString());
                    using var stream = entry.Open();
                    stream.Write(contentAsBytes, 0, contentAsBytes.Length);
                }
            }

            return (memoryStream.ToArray(), setting.GetZipFileName(businessId));
        }
    }
}
