using System.Text.Json;

namespace Backend.Services
{
    public class JsonDataService
    {
        private readonly string _filePath = Path.Combine(AppContext.BaseDirectory, "data.json");

        public List<Usuario> LeerUsuarios()
        {
            if (!File.Exists(_filePath))
                return new List<Usuario>();

            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<Usuario>>(json) ?? new List<Usuario>();
        }

        public void GuardarUsuarios(List<Usuario> usuarios)
        {
            var json = JsonSerializer.Serialize(usuarios, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
    }

    public class Usuario
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string Rol { get; set; } = "Usuario";
    }
}
