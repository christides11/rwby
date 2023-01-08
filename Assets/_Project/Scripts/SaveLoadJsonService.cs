using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace rwby
{
    public static class SaveLoadJsonService
    {

        public static bool Save(string fileName, string text)
        {
            try
            {
                using (StreamWriter streamWriter = File.CreateText(Path.Combine(Application.persistentDataPath, fileName)))
                {
                    streamWriter.Write(text);
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static bool Save<T>(string fileName, T obj, bool prettyPrint = false)
        {
            string jsonObject = JsonConvert.SerializeObject(obj, prettyPrint ? Formatting.Indented : Formatting.None);
            try
            {
                using (StreamWriter streamWriter = File.CreateText(Path.Combine(Application.persistentDataPath, fileName)))
                {
                    streamWriter.Write(jsonObject);
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static T Load<T>(string path)
        {
            try
            {
                string p = Path.Combine(Application.persistentDataPath, path);
                if (!File.Exists(p)) return default(T);
                string jsonString = String.Empty;
                using (StreamReader streamReader = File.OpenText(p))
                {
                    jsonString = streamReader.ReadToEnd();
                }

                return JsonConvert.DeserializeObject<T>(jsonString);
            }
            catch
            {
                return default(T);
            }
        }
        
        public static bool TryLoad<T>(string path, out T result)
        {
            try
            {
                string p = Path.Combine(Application.persistentDataPath, path);
                if (!File.Exists(p)) throw new Exception($"File {p} does not exist.");
                string jsonString = String.Empty;
                using (StreamReader streamReader = File.OpenText(p))
                {
                    jsonString = streamReader.ReadToEnd();
                }

                if (jsonString == String.Empty) throw new Exception("File is empty.");
                result = JsonConvert.DeserializeObject<T>(jsonString);
                return true;
            }
            catch
            {
                result = default(T);
                return false;
            }
        }

        public static bool TryLoadFile(string path, out string result)
        {
            try
            {
                string p = Path.Combine(Application.persistentDataPath, path);
                if (!File.Exists(p)) throw new Exception($"File {p} does not exist.");
                string str = String.Empty;
                using (StreamReader streamReader = File.OpenText(p))
                {
                    str = streamReader.ReadToEnd();
                }
                result = str;
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }
    }
}
