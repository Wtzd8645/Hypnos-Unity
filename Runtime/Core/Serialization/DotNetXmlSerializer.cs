using System;
using System.IO;
using System.Xml.Serialization;

namespace Blanketmen.Hypnos.Serialization
{
    public class DotNetXmlSerializer : ISerializer
    {
        private Type targetType;
        private XmlSerializer serializer;

        public byte[] Serialize<T>(T obj)
        {
            if (typeof(T) != targetType)
            {
                targetType = typeof(T);
                serializer = new XmlSerializer(typeof(T));
            }

            MemoryStream ms = new MemoryStream();
            StreamWriter sw = new StreamWriter(ms, System.Text.Encoding.UTF8);
            try
            {
                serializer.Serialize(sw, obj);
                return ms.ToArray();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                sw.Close(); // NOTE: StreamWriter.Close() will call Stream.Close()
            }
        }

        public T Deserialize<T>(byte[] data)
        {
            if (typeof(T) != targetType)
            {
                targetType = typeof(T);
                serializer = new XmlSerializer(typeof(T));
            }

            StreamReader sr = new StreamReader(new MemoryStream(data), System.Text.Encoding.UTF8);
            try
            {
                return (T)serializer.Deserialize(sr);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                sr.Close(); // NOTE: StreamReader.Close() will call Stream.Close()
            }
        }
    }
}