using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonLib.Utility;
using ModelCore.Helper;
using CommonLib.Core.Utility;

namespace ModelCore.Models.Persistence
{
    public static class PersistenceHelper
    {
        public static T LoadInstance<T>(this String instanceKey)
            where T : new()
        {
            T item;
            lock (typeof(T))
            {
                String fileName = $"{instanceKey}.json";
                String filePath = Path.Combine(Path.Combine(Logger.LogPath, "PersistenceModel").CheckStoredPath(), fileName);
                if (File.Exists(filePath))
                {
                    item = JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath));
                }
                else
                {
                    item = new T();
                }
            }

            return item;
        }

        public static void SaveInstance<T>(this T item,String instanceKey)
            where T : new()
        {
            lock (typeof(T))
            {
                String fileName = $"{instanceKey}.json";
                String filePath = Path.Combine(Path.Combine(Logger.LogPath, "PersistenceModel").CheckStoredPath(), fileName);
                File.WriteAllText(filePath, item.JsonStringify());
            }
        }

    }
}
