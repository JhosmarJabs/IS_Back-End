namespace IS_Back_End.Models
{
    public class Persona
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string ApellidoPaterno { get; set; }
        public string ApellidoMaterno { get; set; }
        public string CorreoElectronico { get; set; }
        public string NumeroTelefono { get; set; }
        public string Sexo { get; set; }
        public string FechaNacimiento { get; set; }
        public string? HuellaDactilar { get; set; }
        public string? FaceID { get; set; }
        public string PasswordHash { get; set; }
    }
}
