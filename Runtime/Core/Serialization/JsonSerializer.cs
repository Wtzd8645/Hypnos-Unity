using System;
using UnityEngine;

namespace Hypnos.Core.Serialization
{
    public class JsonSerializer : ISerializer
    {
        public byte[] Serialize<T>(T obj)
        {
            try
            {
                string json = JsonUtility.ToJson(obj);
                return System.Text.Encoding.UTF8.GetBytes(json);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public T Deserialize<T>(byte[] data)
        {
            try
            {
                string json = System.Text.Encoding.UTF8.GetString(data);
                return JsonUtility.FromJson<T>(json);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}