﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;

//using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml.XPath;
using CommonLib.Helper;
using Newtonsoft.Json;

namespace CommonLib.Utility
{
    public static partial class ExtensionMethods
    {
        public static String Concatenate(this IEnumerable<String> strArray, string separator)
        {
            //StringBuilder sb = new StringBuilder();
            //foreach (var str in strArray)
            //{
            //    sb.Append(str).Append(separator);
            //}

            //if (sb.Length > 0)
            //{
            //    sb.Remove(sb.Length - separator.Length, separator.Length);
            //}
            //return sb.ToString();
            return String.Join(separator, strArray);
        }

        public static void WriteTo(this Stream srcStream, Stream toStream)
        {
            byte[] buf = new byte[4096];
            int nRead;
            while ((nRead = srcStream.Read(buf, 0, 4096)) > 0)
            {
                toStream.Write(buf, 0, nRead);
            }
        }

        public static String Right(this String src, int length)
        {
            int startIndex = src.Length - length;
            return startIndex >= 0 ? src.Substring(startIndex, length) : src;
        }

        public static String GetXml<T>(this T entData)
        {
            XmlSerializer serializer = new XmlSerializer(entData.GetType());
            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter(sb))
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings { CheckCharacters = false }))
                {
                    serializer.Serialize(xmlWriter, entData);
                }
            }
            return sb.ToString();
        }

        public static XmlDocument ConvertToXml<T>(this T entData)
        {
            XmlSerializer serializer = new XmlSerializer(entData.GetType());
            XmlDocument docMsg = new XmlDocument();
            using (MemoryStream ms = new MemoryStream())
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(ms, new XmlWriterSettings { CheckCharacters = false }))
                {
                    serializer.Serialize(xmlWriter, entData);
                }
                ms.Flush();
                ms.Seek(0, SeekOrigin.Begin);
                docMsg.Load(ms);
            }
            return docMsg;
        }

        public static Stream ConvertToXmlStream<T>(this T entData)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            MemoryStream ms = new MemoryStream();
            using (XmlWriter xmlWriter = XmlWriter.Create(ms, new XmlWriterSettings { CheckCharacters = false }))
            {
                serializer.Serialize(xmlWriter, entData);
            }
            ms.Flush();
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        public static void ValidateXmlAgainstSchema(XmlDocument doc, string xsdPath)
        {
            try
            {
                XmlSchemaSet schemaSet = new XmlSchemaSet();
                schemaSet.Add(null, xsdPath);

                doc.Schemas = schemaSet;

                doc.Validate((sender, args) =>
                {
                    Console.WriteLine($"Validation error: {args.Message}");
                    Console.WriteLine($"Line: {args.Exception?.LineNumber}, Position: {args.Exception?.LinePosition}");
                });

                Console.WriteLine("XML validation completed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Schema validation error: {ex.Message}");
            }
        }

        public static T ConvertTo<T>(this XmlDocument docMsg)
        {
            docMsg.RemoveCommentNodes();
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            XmlNodeReader xnr = new XmlNodeReader(docMsg);
            T entData = (T)serializer.Deserialize(xnr);
            xnr.Close();
            return entData;
        }

        public static T ConvertTo<T>(this XmlNode docMsg)
        {
            docMsg.RemoveCommentNodes();
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            XmlNodeReader xnr = new XmlNodeReader(docMsg);
            T entData = (T)serializer.Deserialize(xnr);
            xnr.Close();
            return entData;
        }

        public static T DebugConvertTo<T>(this XmlNode doc)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                using (var reader = new StringReader(doc.OuterXml))
                {
                    return (T)serializer.Deserialize(reader);
                }
            }
            catch (InvalidOperationException ex)
            {
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");

                    // Check if it's a format exception
                    if (ex.InnerException is FormatException formatEx)
                    {
                        Console.WriteLine($"Format error: {formatEx.Message}");
                    }
                }
                throw;
            }
        }



        public static T ConvertTo<T>(this Stream dataStream)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            T entData = (T)serializer.Deserialize(dataStream);
            return entData;
        }

        public static XmlDocument SerializeDataContractToXml<T>(this T target)
        {
            XmlDocument doc = new XmlDocument();
            if (target != null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(T));
                    XmlTextWriter xtw = new XmlTextWriter(ms, null);
                    serializer.WriteObject(xtw, target);
                    xtw.Flush();
                    ms.Flush();
                    ms.Seek(0, SeekOrigin.Begin);
                    doc.Load(ms);
                    ms.Close();
                }
            }
            return doc;
        }

        public static XmlDocument SerializeDataContractToXml(this Object target,Type type)
        {
            XmlDocument doc = new XmlDocument();
            if (target != null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    DataContractSerializer serializer = new DataContractSerializer(type);
                    XmlTextWriter xtw = new XmlTextWriter(ms, null);
                    serializer.WriteObject(xtw, target);
                    xtw.Flush();
                    ms.Flush();
                    ms.Seek(0, SeekOrigin.Begin);
                    doc.Load(ms);
                    ms.Close();
                }
            }
            return doc;
        }

        public static String SerializeDataContract<T>(this T target)
        {
            if (target != null)
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(T));
                StringBuilder sb = new StringBuilder();
                StringWriter sw = new StringWriter(sb);
                XmlTextWriter xtw = new XmlTextWriter(sw);
                serializer.WriteObject(xtw, target);
                xtw.Flush();
                xtw.Close();
                sw.Flush();
                sw.Close();
                return sb.ToString();
            }
            return null;
        }

        public static String SerializeDataContract(this Object target,Type type)
        {
            if (target != null)
            {
                DataContractSerializer serializer = new DataContractSerializer(type);
                StringBuilder sb = new StringBuilder();
                StringWriter sw = new StringWriter(sb);
                XmlTextWriter xtw = new XmlTextWriter(sw);
                serializer.WriteObject(xtw, target);
                xtw.Flush();
                xtw.Close();
                sw.Flush();
                sw.Close();
                return sb.ToString();
            }
            return null;
        }



        public static T ConvertToObjectByDataContract<T>(this IDictionary values, String attrPrefix) where T : class, new()
        {
            T item = new T();
            XmlDocument doc = item.SerializeDataContractToXml();
            XmlElement root = doc.DocumentElement;
            foreach (DictionaryEntry de in values)
            {
                if (de.Value != null)
                {
                    XmlElement elmt = root[attrPrefix + de.Key.ToString()];
                    if (elmt != null)
                    {
                        elmt.RemoveAll();
                        elmt.AppendChild(doc.CreateTextNode(de.Value.ToString()));
                    }
                }
            }

            return doc.DeserializeDataContract<T>();

        }

        public static T ConvertToObjectByDataContract<T>(this IDictionary values, T defaultValue, String attrPrefix) where T : class
        {
            XmlDocument doc = defaultValue.SerializeDataContractToXml();
            XmlElement root = doc.DocumentElement;
            foreach (DictionaryEntry de in values)
            {
                if (de.Value != null)
                {
                    XmlElement elmt = root[attrPrefix + de.Key.ToString()];
                    if (elmt != null)
                    {
                        elmt.RemoveAll();
                        elmt.AppendChild(doc.CreateTextNode(de.Value.ToString()));
                    }
                }
            }

            return doc.DeserializeDataContract<T>();

        }

        public static void AssignProperty<T>(this T srcItem, T targetItem)
        {
            foreach (var p in typeof(T).GetProperties())
            {
                p.SetValue(targetItem, p.GetValue(srcItem, null), null);
            }
        }

        public static void AssignProperty<T>(this T srcItem, T targetItem, Func<PropertyInfo, bool> expr)
        {
            foreach (var p in typeof(T).GetProperties().Where(expr))
            {
                p.SetValue(targetItem, p.GetValue(srcItem, null), null);
            }
        }


        public static T DeserializeDataContract<T>(this XmlNode serialized) where T : class
        {
            if (serialized != null)
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(T));
                using (XmlNodeReader xnr = new XmlNodeReader(serialized))
                {
                    object result = serializer.ReadObject(xnr);
                    xnr.Close();
                    return result as T;
                }
            }
            return (T)null;
        }

        public static Object DeserializeDataContract(this XmlNode serialized,Type type)
        {
            if (serialized != null)
            {
                DataContractSerializer serializer = new DataContractSerializer(type);
                using (XmlNodeReader xnr = new XmlNodeReader(serialized))
                {
                    object result = serializer.ReadObject(xnr);
                    xnr.Close();
                    return result;
                }
            }
            return null;
        }



        public static T DeserializeDataContract<T>(this String serialized) where T : class
        {
            if (serialized != null)
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(T));
                using (StringReader sr = new StringReader(serialized))
                {
                    XmlTextReader xtr = new XmlTextReader(sr);
                    object result = serializer.ReadObject(xtr);
                    xtr.Close();
                    sr.Close();
                    return result as T;
                }
            }
            return (T)null;
        }

        public static Object DeserializeDataContract(this String serialized,Type type) 
        {
            if (serialized != null)
            {
                DataContractSerializer serializer = new DataContractSerializer(type);
                using (StringReader sr = new StringReader(serialized))
                {
                    XmlTextReader xtr = new XmlTextReader(sr);
                    object result = serializer.ReadObject(xtr);
                    xtr.Close();
                    sr.Close();
                    return result ;
                }
            }
            return null;
        }



        public static byte[] StructureToByteArray<T>(this T obj) where T : struct
        {

            int len = Marshal.SizeOf(obj);

            byte[] arr = new byte[len];

            IntPtr ptr = Marshal.AllocHGlobal(len);

            Marshal.StructureToPtr(obj, ptr, true);

            Marshal.Copy(ptr, arr, 0, len);

            Marshal.FreeHGlobal(ptr);

            return arr;

        }

        public static T ByteArrayToStructure<T>(this byte[] bytearray) where T : struct
        {
            T obj;
            int len = Marshal.SizeOf(typeof(T));

            IntPtr i = Marshal.AllocHGlobal(len);

            Marshal.Copy(bytearray, 0, i, len);

            obj = (T)Marshal.PtrToStructure(i, typeof(T));

            Marshal.FreeHGlobal(i);
            return obj;

        }

        public static T ByteArrayToStructure<T>(this byte[] byteArray,out int objSize) where T : struct
        {
            T obj;
            objSize = Marshal.SizeOf(typeof(T));

            IntPtr i = Marshal.AllocHGlobal(objSize);

            Marshal.Copy(byteArray, 0, i, objSize);

            obj = (T)Marshal.PtrToStructure(i, typeof(T));

            Marshal.FreeHGlobal(i);
            return obj;

        }


        public static String AsString(this byte[] bytes, Encoding enc)
        {
            return enc.GetString(bytes);
        }

        public static String AsString(this byte[] bytes)
        {
            return Encoding.Default.GetString(bytes);
        }

        public static object GetPropertyValue<T>(this T obj, String propertyName)
        {
            PropertyInfo pi = typeof(T).GetProperty(propertyName);
            if (pi != null)
            {
                return pi.GetValue(obj, null);
            }
            return null;
        }

        public static XElement GetElement(this XElement srcElement, String expression, Func<XElement> builder)
        {
            XElement target = srcElement.XPathSelectElement(expression);
            if (target == null)
            {
                srcElement.MergeElement(builder());
                target = srcElement.XPathSelectElement(expression);
            }
            return target;
        }

        public static void MergeElement(this XElement srcElement, XElement newElement)
        {
            if (srcElement.Name == newElement.Name)
            {
                if (newElement.HasElements)
                {
                    if (!srcElement.HasElements)
                    {
                        srcElement.Add(newElement.Elements());
                    }
                    else
                    {
                        foreach (var item in newElement.Elements())
                        {
                            XElement src = srcElement.Element(item.Name);
                            if (src != null)
                            {
                                src.MergeElement(item);
                            }
                            else
                            {
                                srcElement.Add(item);
                            }
                        }
                    }
                }
                else
                {
                    srcElement.Parent.Add(newElement);
                }
            }
            else
            {
                srcElement.Parent.Add(newElement);
            }
        }

        public static Boolean CheckIDNo(this string id)
        {
            int value = 0;
            string sId;
            if (String.IsNullOrEmpty(id) || id.Length != 10)
            {
                return false;
            }
            else
            {
                sId = id.ToUpper();
                switch (sId[0])
                {
                    case 'A': value = 10; break;
                    case 'B': value = 11; break;
                    case 'C': value = 12; break;
                    case 'D': value = 13; break;
                    case 'E': value = 14; break;
                    case 'F': value = 15; break;
                    case 'G': value = 16; break;
                    case 'H': value = 17; break;
                    case 'I': value = 34; break;
                    case 'J': value = 18; break;
                    case 'K': value = 19; break;
                    case 'L': value = 20; break;
                    case 'M': value = 21; break;
                    case 'N': value = 22; break;
                    case 'O': value = 35; break;
                    case 'P': value = 23; break;
                    case 'Q': value = 24; break;
                    case 'R': value = 25; break;
                    case 'S': value = 26; break;
                    case 'T': value = 27; break;
                    case 'U': value = 28; break;
                    case 'V': value = 29; break;
                    case 'W': value = 32; break;
                    case 'X': value = 30; break;
                    case 'Y': value = 31; break;
                    case 'Z': value = 33; break;
                }

                if (value < 10 || value > 35)
                    return false;

            }

            long suffix;
            if (!long.TryParse(sId.Substring(1), out suffix))
                return false;

            value = value / 10 + (value % 10) * 9 +
            (sId[1] - '0') * 8 +
            (sId[2] - '0') * 7 +
            (sId[3] - '0') * 6 +
            (sId[4] - '0') * 5 +
            (sId[5] - '0') * 4 +
            (sId[6] - '0') * 3 +
            (sId[7] - '0') * 2 +
            (sId[8] - '0') +
            (sId[9] - '0');
            value = value % 10;
            if (value != 0)
            {
                return false;
            }

            return true;


        }

        public static byte[] ReadLine(this Stream stream)
        {
            int byteRead = stream.ReadByte();

            //end of file 回傳null
            if (byteRead == -1)
            {
                return null;
            }

            using (MemoryStream ms = new MemoryStream())
            {
                while (byteRead != -1)
                {
                    if (byteRead == 0x0A)
                    {
                        break;
                    }
                    else if (byteRead == 0x0D)
                    {
                        //skip
                    }
                    else if (byteRead == 0x00)
                    {
                        ms.WriteByte(0x20);
                    }
                    else
                    {
                        ms.WriteByte((byte)byteRead);
                    }
                    byteRead = stream.ReadByte();
                }
                return ms.ToArray();
            }
        }

        public static List<T> Parse<T>(this Stream stream,Encoding encoding = null) where T : struct
        {
            int objSize = Marshal.SizeOf(typeof(T));
            T obj;
            IntPtr ptrOfT = Marshal.AllocHGlobal(objSize);
            List<T> items = new List<T>();

            try
            {
                if (encoding == null)
                    encoding = Encoding.GetEncoding(950);
                String line;
                byte[] buf;
                using (StreamReader sr = new StreamReader(stream, encoding))
                {
                    line = sr.ReadLine();

                    while (!String.IsNullOrEmpty(line))
                    {
                        buf = encoding.GetBytes(line);
                        if (buf.Length >= objSize)
                        {
                            Marshal.Copy(buf, 0, ptrOfT, objSize);

                            obj = (T)Marshal.PtrToStructure(ptrOfT, typeof(T));
                            items.Add(obj);

                        }
                        line = sr.ReadLine();
                    }
                    sr.Close();
                }
            }
            finally
            {
                Marshal.FreeHGlobal(ptrOfT);
            }

            return items;
        }

        public static List<T> Parse<T>(this Stream stream, ref List<String> dataContent,Encoding encoding = null) where T : struct
        {
            if (dataContent == null)
            {
                dataContent = new List<string>();
            }

            int objSize = Marshal.SizeOf(typeof(T));
            T obj;
            IntPtr ptrOfT = Marshal.AllocHGlobal(objSize);
            List<T> items = new List<T>();

            try
            {
                if (encoding == null)
                    encoding = Encoding.GetEncoding(950);
                String line;
                byte[] buf;
                using (StreamReader sr = new StreamReader(stream, encoding))
                {
                    line = sr.ReadLine();

                    while (!String.IsNullOrEmpty(line))
                    {
                        dataContent.Add(line);

                        buf = encoding.GetBytes(line);
                        if (buf.Length >= objSize)
                        {
                            Marshal.Copy(buf, 0, ptrOfT, objSize);

                            obj = (T)Marshal.PtrToStructure(ptrOfT, typeof(T));
                            items.Add(obj);

                        }
                        line = sr.ReadLine();
                    }
                    sr.Close();
                }
            }
            finally
            {
                Marshal.FreeHGlobal(ptrOfT);
            }

            return items;

        }

        public static String CheckStoredPath(this String fullPath)
        {
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
            return fullPath;
        }

        public static String CheckFullPathExisted(this String fileName, String path)
        {
            string fullPath = Path.Combine(path.GetEfficientString() ?? "", fileName.GetEfficientString() ?? "");
            return File.Exists(fullPath) ? fullPath : null;
        }


        public static void Save(this XmlNode doc, String path)
        {
            using (StreamWriter sw = new StreamWriter(path))
            {
                using (XmlTextWriter xtw = new XmlTextWriter(sw))
                {
                    xtw.WriteStartDocument();
                    doc.WriteTo(xtw);
                    xtw.Flush();
                    sw.Flush();
                    xtw.Close();
                    sw.Close();
                }
            }
        }

        public static void Save(this XmlNode doc, String path, Encoding encoding)
        {
            using (XmlTextWriter xtw = new XmlTextWriter(path, encoding))
            {
                xtw.WriteStartDocument();
                doc.WriteTo(xtw);
                xtw.Flush();
                xtw.Close();
            }
        }

        public static void Save(this XmlNode doc, String path, XmlWriterSettings settings)
        {
            using (XmlWriter xtw = XmlWriter.Create(path,settings))
            {
                xtw.WriteStartDocument();
                doc.WriteTo(xtw);
                xtw.Flush();
                xtw.Close();
            }
        }


        public static IEnumerable<int> GenerateArray(this int start, int size)
        {
            for (int i = 0; i < size; i++)
            {
                yield return start + i;
            }
        }

        public static String WriteStringBytesUseLog(this Stream stream, Encoding enc, String target, char paddingChar, int totalLength)
        {
            String item = WriteStringBytes(stream, enc, target, paddingChar, totalLength);
            return item;
        }

        public static String WriteStringBytes(this Stream stream, Encoding enc, String target, char paddingChar, int totalLength)
        {
            String item = String.IsNullOrEmpty(target) ? new String(paddingChar, totalLength) : target.PadRight(totalLength, paddingChar);
            stream.Write(enc.GetBytes(item), 0, totalLength);
            return item;
        }


        public delegate XElement BuildElement();


        public static String CreateRandomPassword(this int passwordLength)
        {
            string allowedChars = "abcdefghjklmnopqrstuvwxyzABCDEFGHIJKMNOPQRSTUVWXYZ123456789";
            string num = "0123456789";
            char[] chars = new char[passwordLength];

            for (int i = 0; i < passwordLength; i++)
            {
                if (i == 4)
                    chars[i] = num[ValidityAgent.RANDOM.Next(0, num.Length)];
                else
                    chars[i] = allowedChars[ValidityAgent.RANDOM.Next(0, allowedChars.Length)];
            }

            return new string(chars);
        }

        internal const int LOCALE_SYSTEM_DEFAULT = 0x0800;
        internal const int LCMAP_SIMPLIFIED_CHINESE = 0x02000000;
        internal const int LCMAP_TRADITIONAL_CHINESE = 0x04000000;

        /// <summary> 
        /// 使用OS的kernel.dll做為簡繁轉換工具，只要有裝OS就可以使用，不用額外引用dll，但只能做逐字轉換，無法進行詞意的轉換 
        /// <para>所以無法將電腦轉成計算機</para> 
        /// </summary> 
        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern int LCMapString(int Locale, int dwMapFlags, string lpSrcStr, int cchSrc, [Out] string lpDestStr, int cchDest);

        /// <summary> 
        /// 繁體轉簡體 
        /// </summary> 
        /// <param name="pSource">要轉換的繁體字：體</param> 
        /// <returns>轉換後的簡體字：体</returns> 
        public static string ToSimplified(this string pSource)
        {
            String tTarget = new String(' ', pSource.Length);
            int tReturn = LCMapString(LOCALE_SYSTEM_DEFAULT, LCMAP_SIMPLIFIED_CHINESE, pSource, pSource.Length, tTarget, pSource.Length);
            return tTarget;
        }

        /// <summary> 
        /// 簡體轉繁體 
        /// </summary> 
        /// <param name="pSource">要轉換的繁體字：体</param> 
        /// <returns>轉換後的簡體字：體</returns> 
        public static string ToTraditional(this string pSource)
        {
            String tTarget = new String(' ', pSource.Length);
            int tReturn = LCMapString(LOCALE_SYSTEM_DEFAULT, LCMAP_TRADITIONAL_CHINESE, pSource, pSource.Length, tTarget, pSource.Length);
            return tTarget;
        }

        public static String InsteadOfNullOrEmpty(this String source, String replacement)
        {
            if (String.IsNullOrEmpty(source))
            {
                return replacement;
            }
            return source;
        }

        public static String GetEfficientString(this String source)
        {
            String val = source != null ? source.Trim() : null;
            return String.IsNullOrEmpty(val) ? null : val;
        }

        public static String GetEfficientString(this String source, String suffix)
        {
            String val = source != null ? source.Trim() : null;
            return String.IsNullOrEmpty(val) ? null : $"{val}{suffix}";
        }

        public static String GetEfficientString(this String source,int startIndex)
        {
            String val = source != null ? source.Trim() : null;
            return String.IsNullOrEmpty(val) ? null : val.Substring(startIndex);
        }

        public static String GetEfficientString(this String source, int startIndex,int length)
        {
            String val = source != null ? source.Trim() : null;
            return String.IsNullOrEmpty(val) ? null : val.Substring(startIndex,length);
        }

        public static String GetEfficientStringMaxSize(this String source, int startIndex, int maxLength)
        {
            String val = source != null ? source.Trim() : null;
            return String.IsNullOrEmpty(val) ? null
                : val.Length > (startIndex + maxLength)
                    ? val.Substring(startIndex, maxLength)
                    : val.Substring(startIndex);
        }


        public static XmlDocument FilterEmptyTag(this XmlDocument doc)
        {
            doc.LoadXml(Regex.Replace(doc.OuterXml, "(<\\s*[\\w_]*\\s*/\\s*>)|(<\\s*(?<tag>[\\w_]*)\\s*><\\s*/\\s*\\k<tag>\\s*>)", ""));
            return doc;
        }

        public static XmlElement GetXmlSignature(this XmlDocument doc)
        {
            var items = doc.GetElementsByTagName("Signature", "http://www.w3.org/2000/09/xmldsig#");
            if (items.Count > 0)
                return (XmlElement)items[0];
            return null;
        }

        public static DateTime FromChineseDate(this String chDateStr)
        {
            return DateTime.ParseExact((int.Parse(chDateStr) + 19110000).ToString(), "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
        }

        public static void Reset<T>(this T[] items, T newValue)
        {
            for (int i=0;i<items.Length;i++)
            {
                items[i] = newValue;
            }
        }

        public static XmlNode TrimAll(this XmlNode docMsg,bool removeEmpty = true)
        {
            foreach (XmlNode node in docMsg.SelectNodes("//*/text()"))
            {
                node.Value = node.Value.Trim();
            }
            return removeEmpty ? docMsg.RemoveAllEmpty() : docMsg;
        }

        public static XmlNode RemoveAllEmpty(this XmlNode docMsg)
        {
            var nodes = docMsg.SelectNodes("//*").Cast<XmlNode>()
                .Where(n => String.IsNullOrEmpty(n.InnerText) && !n.HasChildNodes);
            foreach (XmlNode node in nodes)
            {
                node.ParentNode.RemoveChild(node);
            }
            return docMsg;
        }

        public static string[] FromCsvLine(this string csvLine, char delimiter = ',', char quotation = '"', List<String> container = null)
        {
            List<string> fields = container ?? new List<string>();
            bool inQuotes = false;
            StringBuilder currentField = new StringBuilder();

            for (int i = 0; i < csvLine.Length; i++)
            {
                char c = csvLine[i];

                // 遇到雙引號
                if (c == quotation)
                {
                    // 檢查是否為字段開頭的引號（即字段被引號包裹）
                    if (!inQuotes && currentField.Length == 0)
                    {
                        inQuotes = true;
                    }
                    // 檢查是否為字段結尾的引號
                    else if (inQuotes && (i + 1 >= csvLine.Length || csvLine[i + 1] == delimiter))
                    {
                        inQuotes = false;
                    }
                    // 檢查是否為轉義的雙引號（僅在引號包裹的字段內有效）
                    else if (inQuotes && i + 1 < csvLine.Length && csvLine[i + 1] == quotation)
                    {
                        currentField.Append(quotation);
                        i++; // 跳過下一個引號
                    }
                    // 其他情況：直接當作普通字符（允許未包裹的字段包含雙引號）
                    else
                    {
                        currentField.Append(c);
                    }
                }
                // 遇到逗號，且不在引號包裹的字段內
                else if (c == delimiter && !inQuotes)
                {
                    fields.Add(currentField.ToString());
                    currentField.Clear();
                }
                else
                {
                    currentField.Append(c);
                }
            }

            // 添加最後一個欄位
            fields.Add(currentField.ToString());

            return fields.ToArray();
        }

        public static String[] ParseCsvLine(this String line, char delimiter = ',', char quotation = '"')
        {
            if (String.IsNullOrEmpty(line))
            {
                return null;
            }

            List<String> result = new List<string>();

            int start = 0, length = 0;
            bool quote = false;
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == quotation)
                {
                    quote = !quote;
                    length++;
                    continue;
                }

                if (line[i] == delimiter)
                {
                    if (quote)
                    {
                        length++;
                        continue;
                    }
                    else
                    {
                        if (length > 0)
                        {
                            if (line[start] == quotation && line[start + length - 1] == quotation)
                            {
                                result.Add(line.Substring(start + 1, length - 2));
                            }
                            else
                            {
                                result.Add(line.Substring(start, length));
                            }
                        }
                        else
                        {
                            result.Add(String.Empty);
                        }
                        start = i + 1;
                        length = 0;
                        continue;
                    }
                }

                length++;
            }

            if (length > 0)
            {
                if (length > 1 && line[start] == quotation && line[start + length - 1] == quotation)
                {
                    result.Add(line.Substring(start + 1, length - 2));
                }
                else
                {
                    result.Add(line.Substring(start, length));
                }
            }
            else
            {
                result.Add(String.Empty);
            }

            return result.ToArray();
        }


        public static String[] ParseCsv(this String line,char delimiter = ',',char quotation='"')
        {
            if (String.IsNullOrEmpty(line))
            {
                return null;
            }

            //紀錄','的索引值
            List<int> items = new List<int>();
            items.Add(-1);
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == delimiter)
                    items.Add(i);
            }
            items.Add(line.Length);

            //重新檢查、定義','的索引值，將雙引號內的','索引剔除。
            bool quoteStart = false;
            foreach (var idx in items.ToList())
            {
                if (idx >= line.Length - 1)
                    break;
                if (quoteStart)
                {
                    if (line[idx - 1] == quotation)
                    {
                        quoteStart = false;
                    }
                    else
                    {
                        items.Remove(idx);
                    }
                }
                else
                {
                    quoteStart = line[idx + 1] == quotation;
                }
            }

            //根據真正用來分隔資料的','索引值，重新切分CSV資料
            List<String> result = new List<string>();
            for (int j = 0; j < items.Count - 1; j++)
            {
                if (items[j + 1] - items[j] > 1 && line[items[j] + 1] == quotation && line[items[j + 1] - 1] == quotation)
                {
                    result.Add(line.Substring(items[j] + 2, items[j + 1] - items[j] - 3));
                }
                else
                {
                    result.Add(line.Substring(items[j] + 1, items[j + 1] - items[j] - 1));
                }
            }

            return result.ToArray();
        }

        public static Dictionary<TKey, TValue> Append<TKey, TValue>(this Dictionary<TKey, TValue> values, TKey key, TValue value)
        {
            values.Add(key, value);
            return values;
        }

        public static String TrimOrNull(this String source)
        {
            if (String.IsNullOrEmpty(source))
            {
                return null;
            }
            return source.Trim();
        }

        public static String RegularizeXmlString(this String data)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                if (XmlConvert.IsXmlChar(data[i]))
                    sb.Append(data[i]);
                else
                    sb.Append("&#x").Append(String.Format("{0:x4}", (uint)data[i])).Append(";");
            }
            return sb.ToString();
        }

        public static XmlNode RemoveCommentNodes(this XmlNode node)
        {
            foreach(var n in node.SelectNodes("//comment()").Cast<XmlNode>().ToList())
            {
                n.ParentNode.RemoveChild(n);
            }
            return node;
        }

        public static void RecycleJob(this Action runJob, int waitInMilliSeconds)
        {
            ThreadPool.QueueUserWorkItem(stateInfo =>
            {
                Thread.Sleep(waitInMilliSeconds);
                runJob();
                runJob.RecycleJob(waitInMilliSeconds);
            });
        }

        public static void RecycleJobImmediately(this Action runJob, int waitInMilliSeconds)
        {
            ThreadPool.QueueUserWorkItem(stateInfo =>
            {
                runJob();
                Thread.Sleep(waitInMilliSeconds);
                runJob.RecycleJob(waitInMilliSeconds);
            });
        }

        public static bool IsUtf8(this byte[] data, int length)
        {
            if (data == null)
                return true;

            bool asc = true;
            int utf8_n = 0, gbk_n = 0;
            for (int i = 0; i < length; i++)
            {
                if (data[i] > 0 && data[i] < 0x7F)
                    continue;
                asc = false;

                if ((data[i + 0] & 0xE0) == 0xC0 && i + 1 <= data.Length)
                { //双字节格式
                    if ((data[i + 1] & 0xC0) == 0x80)
                    {
                        int n = data[i + 0] & 0xFF;
                        int n2 = data[i + 1] & 0xFF;
                        if ((0x81 <= n && n <= 0xFE) && (0x40 <= n2 && n2 <= 0xFE) && (n2 != 0x7F))
                        {
                        }
                        else
                        {
                            utf8_n++;
                            i++;
                            continue;
                        }
                    }
                }
                else if ((data[i + 0] & 0xF0) == 0xE0 && i + 2 <= data.Length)
                { //三字节格式
                    if (((data[i + 1] & 0xC0) == 0x80) && ((data[i + 2] & 0xC0) == 0x80))
                    {
                        utf8_n++;
                        i += 2;
                        continue;
                    }
                }
                else if ((data[i + 0] & 0xF8) == 0xF0 && i + 3 <= data.Length)
                { //四字节格式
                    if (((data[i + 1] & 0xC0) == 0x80) && ((data[i + 2] & 0xC0) == 0x80) && ((data[i + 3] & 0xC0) == 0x80))
                    {
                        utf8_n++;
                        i += 3;
                        continue;
                    }
                }
                i++;
                gbk_n++;
            }
            if (asc == false)
            {
                if (gbk_n > 0 && 10 * utf8_n < gbk_n)
                    return false;
                return true;
            }
            return true;
        }

        public static String ReadString(this Stream stream)
        {
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public static byte[] FromHexStringToBytes(this String data, int delimitSpace = 0)
        {
            int length = (data.Length + delimitSpace + 1) / (2 + delimitSpace);
            byte[] buf = new byte[length];
            for (int i = 0; i < length; i++)
            {
                buf[i] = Convert.ToByte(data.Substring(i * (2 + delimitSpace), 2), 16);
            }
            return buf;
        }

        public static byte[] HexToByteArray(this string hexString)
        {
            byte[] bytes = new byte[hexString.Length / 2];

            for (int i = 0; i < hexString.Length; i += 2)
            {
                string s = hexString.Substring(i, 2);
                bytes[i / 2] = byte.Parse(s, NumberStyles.HexNumber, null);
            }

            return bytes;
        }

        public static TEnum? ToEnumValue<TEnum>(this String source)
            where TEnum : struct
        {
            TEnum val;
            return Enum.TryParse<TEnum>(source, out val) ? val : (TEnum?)null;
        }

        public static string StringMask(this string source, int startIndex, int length, char replaceCharacter)
        {
            StringBuilder sb = new StringBuilder(source);
            for (int i = 0, idx = startIndex; i < length && idx < sb.Length; i++, idx++)
            {
                sb[idx] = replaceCharacter;
            }
            return sb.ToString();
        }

        public static String GetString(this DataRow row, int index)
        {
            return (row[index] as String)?.GetEfficientString() ?? $"{row[index]}";
        }

        public static String GetString(this DataRow row, String columnName)
        {
            return (row[columnName] as String)?.GetEfficientString() ?? $"{row[columnName]}";
        }

        public static Newtonsoft.Json.JsonSerializerSettings CommonJsonSettings = new Newtonsoft.Json.JsonSerializerSettings
        {
            NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
        };

        public static String JsonStringify(this Object model)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(model, CommonJsonSettings);
        }

        public static String Mask(this String source,int start, int length, char mask)
        {
            return (new StringBuilder(source)).Mask(start, length, mask).ToString();
        }

        public static StringBuilder Mask(this StringBuilder source,int start,int length,char mask)
        {
            for (int i = start; i < start + length && i < source.Length; i++)
            {
                source[i] = mask;
            }
            return source;
        }

        public static String UrlEncodeBase64String(this String base64)
        {
            return base64?.Replace('+', '-').Replace('/', '_').Replace('=', '.');
        }

        public static String UrlDecodeBase64String(this String encodedBase64)
        {
            return encodedBase64?.Replace('-', '+').Replace('_', '/').Replace('.', '=');
        }

        public static T DeserializeObjectFromFile<T>(this string jsonPath)
        {
            if (File.Exists(jsonPath))
            {
                return JsonConvert.DeserializeObject<T>(File.ReadAllText(jsonPath));
            }
            return default;
        }

        public static void SerializeObjectToJsonFile(this Object model, String jsonPath)
        {
            File.WriteAllText(jsonPath, model.JsonStringify());
        }

        public static String ToAmountString(this decimal? value)
        {
            return $"{(value ?? 0):##,###,###,###,##0}";
        }

        public static String ToAmountString(this decimal value)
        {
            return $"{value:##,###,###,###,##0}";
        }

        public static String EscapeFileNameCharacter(this String forFileName, char replacement)
        {
            StringBuilder sb = new StringBuilder(forFileName);
            foreach (var ch in Path.GetInvalidFileNameChars())
            {
                for (int i = 0; i < sb.Length; i++)
                {
                    if (sb[i] == ch)
                    {
                        sb[i] = replacement;
                    }
                }
            }
            return sb.ToString();
        }
    }

    public class EventArgs<T> : EventArgs
    {
        public T Argument { get; set; }
    }
}
