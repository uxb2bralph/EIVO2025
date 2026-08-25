using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Newtonsoft.Json;
using ModelCore.DataEntity;
using TaskCenter.Properties;

namespace TaskCenter.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class DocumentController : SampleController
    {
        /// <summary>
        /// 載入 navigation 的層數：1 = navigation，2 = 巢狀 navigation。
        /// </summary>
        private const int IncludeDepth = 2;

        public DocumentController(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : base(serviceProvider, loggerFactory)
        {
            DumpRequest = AppSettings.Default.EnableRequestDump;
        }

        /// <summary>
        /// 依 DocID 查詢 <see cref="CDS_Document"/>，並將實體（含 navigation 及巢狀 navigation）以 JSON 輸出。
        /// </summary>
        [HttpGet("GetDocument")]
        public IActionResult GetDocument(int id)
        {
            // Lazy Loading Proxy 已開啟，序列化時若任由其延遲載入會遞迴拉出整個資料庫，
            // 因此關閉 Lazy Loading，改以明確 Include 控制載入的 navigation 範圍。
            models!.DataContext.ChangeTracker.LazyLoadingEnabled = false;

            IQueryable<CDS_Document> query = models.GetTable<CDS_Document>().AsNoTracking();

            foreach (var path in BuildIncludePaths(models.DataContext, typeof(CDS_Document), IncludeDepth))
            {
                query = query.Include(path);
            }

            // 多個 navigation 一次 Include 會造成 join 笛卡兒積膨脹，改用 split query。
            CDS_Document? document = query.AsSplitQuery().FirstOrDefault(c => c.DocID == id);

            if (document == null)
            {
                return NotFound(new { result = false, message = $"Document not found. DocID={id}" });
            }

            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
            };

            return Content(JsonConvert.SerializeObject(document, settings), "application/json");
        }

        /// <summary>
        /// 由 EF Core 模型 metadata 推導出指定深度內所有 navigation 的 Include 路徑。
        /// </summary>
        private static List<string> BuildIncludePaths(DbContext context, Type rootType, int maxDepth)
        {
            var paths = new List<string>();
            var rootEntity = context.Model.FindEntityType(rootType);
            if (rootEntity == null)
            {
                return paths;
            }

            void Walk(IReadOnlyEntityType entityType, string prefix, int depth, HashSet<IReadOnlyEntityType> ancestors)
            {
                IEnumerable<IReadOnlyNavigationBase> navigations = entityType.GetNavigations()
                    .Concat<IReadOnlyNavigationBase>(entityType.GetSkipNavigations());

                foreach (var nav in navigations)
                {
                    var target = nav.TargetEntityType;

                    // 跳過會回到上層 entity 的 navigation（例如 Attachment->CDS_Document）。
                    // no-tracking 查詢不允許 Include 路徑形成循環。
                    if (ancestors.Contains(target))
                    {
                        continue;
                    }

                    string path = string.IsNullOrEmpty(prefix) ? nav.Name : $"{prefix}.{nav.Name}";
                    paths.Add(path);

                    if (depth < maxDepth)
                    {
                        Walk(target, path, depth + 1, new HashSet<IReadOnlyEntityType>(ancestors) { target });
                    }
                }
            }

            Walk(rootEntity, string.Empty, 1, new HashSet<IReadOnlyEntityType> { rootEntity });
            return paths;
        }
    }
}
