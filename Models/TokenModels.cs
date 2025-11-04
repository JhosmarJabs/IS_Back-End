namespace IS_Back_End.Models
{

  public class TokenResult
  {
    public int Id { get; set; }
    public string Tipo { get; set; }           // "correo", "telefono", "whatsapp", "verificacion"
    public string Correo { get; set; }
    public string Telefono { get; set; }
    public string TokenCorreo { get; set; }
    public string TokenTelefono { get; set; }
    public DateTime Fecha { get; set; }
  }

  public class ValidacionTokenResult
  {
    public bool EntradaValida { get; set; }
    public string Mensaje { get; set; }
    public string Jwt { get; set; }
  }
  public class TokenTemporal
  {
    public string TokenId { get; set; }
    public int UsuarioId { get; set; }
    public string Tipo { get; set; }
    public string Valor { get; set; }
    public string Valor2 { get; set; }
    public DateTime FechaGeneracion { get; set; }
    public DateTime FechaExpiracion { get; set; }
    public bool Usado { get; set; }
  }
  public class TokenVerificacionRequest
  {
    public int Id { get; set; }
    public string Correo { get; set; }
    public string Telefono { get; set; }
    public string Tipo { get; set; } // "verificacion", "correo", "telefono", "whatsapp"
  }

  public class JsonWebTokenResponse
  {
    public string Message { get; set; }
    public string TokenCorreo { get; set; }
    public string TokenTelefono { get; set; }
    public string Tipo { get; set; }
    public int Id { get; set; }
  }

  public class RecuperarPasswordRequest
  {
    public string Correo { get; set; }
    public string NuevaPassword { get; set; }
  }
  public class SolicitarRecuperacionRequest
  {
    public string Correo { get; set; }
    public string Tipo { get; set; } // "correo", "telefono", "whatsapp"
  }
  public class VerificarTokenRequest
  {
    public string Correo { get; set; }
    public string Token { get; set; }
  }
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
    public double[]? FaceID { get; set; }

    public string PasswordHash { get; set; }
  }
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
  public class LoginRequest
  {
    public string Correo { get; set; }             // siempre requerido
    public string TipoAuth { get; set; }            // password, token, huella, faceid
    public string? Password { get; set; }          // para password
    public string? TypeToken { get; set; }         // Para el tipo de token SMS, WhatsApp, Correo
    public string? Token { get; set; }             // para token
    public double[]? FaceID { get; set; }

  }

  public class Admin
  {
    public string Usuario { get; set; }
    public string Contraseña { get; set; }
  }
}

