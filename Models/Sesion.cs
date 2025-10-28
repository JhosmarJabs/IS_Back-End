using System;

namespace IS_Back_End.Models
{
    public class Sesion
    {
        public int Id { get; set; }
        public int PersonaId { get; set; }
        public string TokenJwt { get; set; }
        public DateTime FechaExpiracion { get; set; }
        public bool Activo { get; set; }
    }
    public class SesionRequest
    {
        public int UsuarioId { get; set; }
        public string Metodo { get; set; }  // "sms", "correo", "whatsapp"
    }
}
