namespace IS_Back_End.Models
{

  public class TokenResult
  {
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


}

