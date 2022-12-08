using System;
using System.IO;
using System.Xml.Serialization;

namespace Morpheus.Core.Serialization
{
    public class DotNetXmlSerializer : ISerializer
    {
        private Type currentType;
        private XmlSerializer serializer;

        public byte[] Serialize<T>(T obj)
        {
            if (typeof(T) != currentType)
            {
                currentType = typeof(T);
                serializer = new XmlSerializer(typeof(T));
            }

            MemoryStream ms = new MemoryStream();
            StreamWriter sw = new StreamWriter(ms, System.Text.Encoding.UTF8);
            try
            {
                serializer.Serialize(sw, obj);
                return ms.ToArray();
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                sw.Close(); // NOTE: StreamWriter.Close() will call Stream.Close()
            }
        }

        public T Deserialize<T>(byte[] serializedData)
        {
            if (typeof(T) != currentType)
            {
                currentType = typeof(T);
                serializer = new XmlSerializer(typeof(T));
            }

            StreamReader sr = new StreamReader(new MemoryStream(serializedData), System.Text.Encoding.UTF8);
            try
            {
                return (T)serializer.Deserialize(sr);
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                sr.Close(); // NOTE: StreamReader.Close() will call Stream.Close()
            }
        }
    }
}