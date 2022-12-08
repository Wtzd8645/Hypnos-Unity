using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Morpheus.Core.Serialization
{
    public class DotNetBinaryFormatter : ISerializer
    {
        private readonly BinaryFormatter serializer = new BinaryFormatter();

        public byte[] Serialize<T>(T obj)
        {
            MemoryStream ms = new MemoryStream();
            try
            {
                serializer.Serialize(ms, obj);
                return ms.ToArray();
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                ms.Close();
            }
        }

        public T Deserialize<T>(byte[] serializedData)
        {
            MemoryStream ms = new MemoryStream(serializedData);
            try
            {
                return (T)serializer.Deserialize(ms);
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                ms.Close();
            }
        }
    }
}