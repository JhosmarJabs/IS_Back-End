using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using IS_Back_End;
using IS_Back_End.Models;

namespace IS_Back_End.Services
{
  public class TokenService
  {
    private readonly HttpClient _httpClient;
    private readonly DataService data;

    public TokenService()
    {
      _httpClient = new HttpClient();
      data = new DataService(); // Así nunca será null
    }

    // Función central para generacion de uno o dos tokens según el tipo
    public async Task<TokenResult> GenerarToken(int usuarioId, string correo, string telefono, string tipo)
    {
      var random = new Random();

      string tokenCorreo = null;
      string tokenTelefono = null;

      // Generar tokens según tipo
      switch (tipo.ToLower())
      {
        case "verificacion":
          tokenCorreo = random.Next(100000, 999999).ToString();
          tokenTelefono = random.Next(100000, 999999).ToString();
          break;

        case "correo":
          tokenCorreo = random.Next(100000, 999999).ToString();
          break;

        case "telefono":
        case "whatsapp":
          tokenTelefono = random.Next(100000, 999999).ToString();
          break;

        default:
          throw new ArgumentException("Tipo de token no válido");
      }

      // Crear nuevo token temporal asignando valor y valor2
      var nuevoToken = new TokenTemporal
      {
        TokenId = Guid.NewGuid().ToString(),
        UsuarioId = usuarioId,
        Tipo = tipo,
        Valor = tokenCorreo ?? tokenTelefono,
        Valor2 = tipo.ToLower() == "verificacion" ? tokenTelefono : null,
        FechaGeneracion = DateTime.UtcNow,
        FechaExpiracion = DateTime.UtcNow.AddMinutes(15),
        Usado = false
      };

      // Guardar el token en archivo JSON
      var tokens = data.Load<TokenTemporal>("tokens_temporales.json").ToList();
      tokens.Add(nuevoToken);
      data.Save("tokens_temporales.json", tokens);

      // Construir objeto resultado
      var resultado = new TokenResult
      {
        Tipo = tipo,
        Correo = correo,
        Telefono = telefono,
        TokenCorreo = tokenCorreo,
        TokenTelefono = tokenTelefono,
        Fecha = DateTime.UtcNow
      };

      // Enviar token al sistema externo (n8n)
      await EnviarTokenN8n(resultado);

      return resultado;
    }

    // Función única para enviar token(s) por el canal que corresponda
    public async Task EnviarTokenN8n(TokenResult tokenResult)
    {
      try
      {
        var json = JsonSerializer.Serialize(tokenResult);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        string n8nUrl = Env.urlN8n;

        var response = await _httpClient.PostAsync(n8nUrl, content);
        response.EnsureSuccessStatusCode();
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error al enviar tokens a n8n: {ex.Message}");
      }
    }
    public bool VerificarTokens(string tokenCorreoFront, string tokenTelefonoFront)
    {
      var tokens = data.Load<TokenTemporal>("tokens_temporales.json");

      // debug para ver qué tokens carga
      Console.WriteLine("Tokens cargados:");
      foreach (var t in tokens)
      {
        Console.WriteLine($"Tipo: {t.Tipo}, Valor: {t.Valor}, Valor2: {t.Valor2}");
      }

      // Busca un token que coincida con ambos valores al mismo tiempo
      bool esValido = tokens.Any(t =>
          (string.IsNullOrEmpty(tokenCorreoFront) || (t.Valor?.Trim() == tokenCorreoFront.Trim() || t.Valor2?.Trim() == tokenCorreoFront.Trim())) &&
          (string.IsNullOrEmpty(tokenTelefonoFront) || (t.Valor?.Trim() == tokenTelefonoFront.Trim() || t.Valor2?.Trim() == tokenTelefonoFront.Trim())) &&
          (t.Tipo.Equals("verificacion", StringComparison.OrdinalIgnoreCase) ||
           t.Tipo.Equals("correo", StringComparison.OrdinalIgnoreCase) ||
           t.Tipo.Equals("telefono", StringComparison.OrdinalIgnoreCase) ||
           t.Tipo.Equals("whatsapp", StringComparison.OrdinalIgnoreCase))
      );

      return esValido;
    }



  }
}
