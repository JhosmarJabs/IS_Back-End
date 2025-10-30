namespace IS_Back_End.Models
{
  public class LoginRequest
  {
    public string Correo { get; set; }             // siempre requerido
    public string TipoAuth { get; set; }            // password, token, huella, faceid
    public string? Password { get; set; }          // para password
    public string? TypeToken { get; set; }         // Para el tipo de token SMS, WhatsApp, Correo
    public string? Token { get; set; }             // para token
    public double[]? FaceID { get; set; }

  }
}
