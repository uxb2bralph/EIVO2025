using System;
using System.Reflection;

namespace ModelCore.DataEntity
{
    public static class EntitySerializationExtensions
    {
        /// <summary>
        /// 建立僅複製純量欄位的實體複本。
        /// 啟用 Lazy Loading Proxy 之後，EF Core 取回的實體實際型別是 Castle.Proxies.XxxProxy，
        /// DataContractSerializer 無法序列化(基底型別未標記 DataContract/Serializable)，
        /// 因此對外傳輸前必須先轉回未被代理的實體，順便避免序列化時觸發關聯資料的延遲載入。
        /// </summary>
        public static TEntity? ToPlainEntity<TEntity>(this TEntity? entity)
            where TEntity : class, new()
        {
            if (entity == null)
            {
                return null;
            }

            TEntity plain = new TEntity();
            foreach (PropertyInfo p in typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (p.CanRead && p.CanWrite
                    && p.GetIndexParameters().Length == 0
                    && IsScalarType(p.PropertyType))
                {
                    p.SetValue(plain, p.GetValue(entity));
                }
            }
            return plain;
        }

        private static bool IsScalarType(Type type)
        {
            Type t = Nullable.GetUnderlyingType(type) ?? type;
            return t.IsPrimitive || t.IsEnum
                || t == typeof(String) || t == typeof(Decimal)
                || t == typeof(DateTime) || t == typeof(DateTimeOffset)
                || t == typeof(TimeSpan) || t == typeof(Guid)
                || t == typeof(Byte[]);
        }
    }
}
