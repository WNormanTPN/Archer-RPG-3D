using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Generic
{
    public static class JSONLoader
    {
        public static T LoadJSON<T>(string jsonFilePath)
        {
            // Read JSON file
            string json = File.ReadAllText(jsonFilePath);

            // Parse JSON data
            return JsonConvert.DeserializeObject<T>(json);
        }
        
        public static T LoadJSON<T>(TextAsset jsonFile)
        {
            // Read JSON file
            string json = jsonFile.text;

            // Parse JSON data
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}