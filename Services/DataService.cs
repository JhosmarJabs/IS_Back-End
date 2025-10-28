using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;
using IS_Back_End.Models;

namespace IS_Back_End.Services
{
    public class DataService
    {
        private readonly string basePath = Path.Combine(Directory.GetCurrentDirectory(), "Data");

        private string GetPath(string file) => Path.Combine(basePath, file);

        public List<T> Load<T>(string file)
        {
            var path = GetPath(file);
            if (!File.Exists(path)) return new List<T>();
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
        }

        public void Save<T>(string file, List<T> data)
        {
            var path = GetPath(file);
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }
    }
}
