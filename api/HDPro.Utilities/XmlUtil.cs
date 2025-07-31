using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;

namespace HDPro.Utilities
{
    public class XmlUtil
    {
        public static object Deserialize(Type type, string xml)
        {
            object result;
            try
            {
                using (StringReader sr = new StringReader(xml))
                {
                    XmlSerializer xmldes = new XmlSerializer(type);
                    result = xmldes.Deserialize(sr);
                }
            }
            catch (Exception e)
            {
                result = null;
            }
            return result;
        }

        // Token: 0x060048EB RID: 18667 RVA: 0x00116018 File Offset: 0x00114218
        public static string Serializer(Type type, object obj)
        {
            MemoryStream Stream = new MemoryStream();
            XmlSerializer xml = new XmlSerializer(type);
            try
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(Stream, new XmlWriterSettings
                {
                    Encoding = Encoding.UTF8
                }))
                {
                    XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
                    namespaces.Add(string.Empty, string.Empty);
                    xml.Serialize(xmlWriter, obj, namespaces);
                    xmlWriter.Close();
                }
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            Stream.Position = 0L;
            string str = GetStreamString(Stream, "utf-8", 1024);
            Stream.Dispose();
            return str;
        }
        // Token: 0x060001C1 RID: 449 RVA: 0x0000E424 File Offset: 0x0000C624
        public static string GetStreamString(Stream stream, string pEncode = "utf-8", int bufferSize = 1024)
        {
            string result;
            using (StreamReader streamReader = new StreamReader(stream, Encoding.GetEncoding(pEncode), false, bufferSize, false))
            {
                result = streamReader.ReadToEnd();
            }
            return result;
        }
    }
}
