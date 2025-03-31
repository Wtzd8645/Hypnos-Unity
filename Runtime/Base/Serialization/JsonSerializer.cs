using System;

namespace Blanketmen.Hypnos.Serialization
{
    public class JsonSerializer : ISerializer
    {
        public byte[] Serialize<T>(T obj)
        {
            try
            {
                string json = UnityEngine.JsonUtility.ToJson(obj);
                return System.Text.Encoding.UTF8.GetBytes(json);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public T Deserialize<T>(byte[] data)
        {
            try
            {
                string json = System.Text.Encoding.UTF8.GetString(data);
                return UnityEngine.JsonUtility.FromJson<T>(json);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}