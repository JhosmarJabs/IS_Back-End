namespace MiWebService.Models
{
    public class Persona // Lo recibe del front
    {
        public int Id { get; set; }
        public string? Nombre { get; set; }
        public string? APaterno { get; set; }
        public string? AMaterno { get; set; }
        public long Telefono { get; set; }
        public string? Correo { get; set; }
        public string? FechaNacimiento { get; set; }
        public string? sexo { get; set; }
    }
}