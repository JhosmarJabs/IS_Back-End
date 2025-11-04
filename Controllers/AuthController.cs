using Microsoft.AspNetCore.Mvc;
using IS_Back_End.Models;
using IS_Back_End.Services;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Text.Json;
using Backend.Services;

namespace IS_Back_End.Controllers
{
  [ApiController]
  [Route("auth")]
  public class AuthController : ControllerBase
  {
    private readonly AuthService auth;
    private readonly TokenService tokenService;

    public AuthController(AuthService a, TokenService t)
    {
      auth = a;
      tokenService = t;
    }

    [HttpPost("")]
    public IActionResult TestEndpoint()
    {
      try
      {
        Console.WriteLine("✅ Endpoint de prueba ejecutado con éxito");
        return Ok(new { message = "Endpoint de prueba ejecutado con éxito" });
      }
      catch (Exception ex)
      { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPost("GenerateVerificationToken")]
    public async Task<IActionResult> GenerarTokenVerificacion([FromBody] TokenVerificacionRequest request)
    {
      try
      {
        var resultado = await tokenService.GenerarToken(request.Id, request.Correo, request.Telefono, request.Tipo);

        _ = Task.Run(() => tokenService.EnviarTokenN8n(resultado));

        var response = new JsonWebTokenResponse
        {
          Message = $"Tokens generados y enviado",
          TokenCorreo = resultado.TokenCorreo,
          TokenTelefono = resultado.TokenTelefono,
          Tipo = resultado.Tipo,
          Id = resultado.Id
        };

        return Ok(response);
      }
      catch (Exception ex)
      {
        return BadRequest(new { error = ex.Message });
      }
    }

    [HttpPost("RegisterUser")]
    public IActionResult Registrar([FromBody] Persona p)
    {
      // public int Id { get; set; }
      // public string Nombre { get; set; }
      // public string ApellidoPaterno { get; set; }
      // public string ApellidoMaterno { get; set; }
      // public string CorreoElectronico { get; set; }
      // public string NumeroTelefono { get; set; }
      // public string Sexo { get; set; }
      // public string FechaNacimiento { get; set; }
      // public string? HuellaDactilar { get; set; }
      // public string? FaceID { get; set; }
      // public string PasswordHash { get; set; }
      try
      {
        if (p == null)
          return BadRequest(new { error = "Se requiere un JSON válido en el body" });

        var user = auth.RegistrarUsuario(p);
        return Ok(new { message = "Usuario registrado", user });
      }
      catch (System.Exception ex)
      { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPost("CheckEmailExists")]
    public IActionResult VerificarCorreo([FromBody] string correo)
    {
      var (existe, usuario, error) = auth.ObtenerUsuarioPorCorreo(correo);

      if (!existe)
      {
        if (error.Contains("requerido"))
          return BadRequest(new { error });
        if (error.Contains("no está registrado"))
          return NotFound(new { error });
        return StatusCode(500, new { error });
      }

      return Ok(new
      {
        message = "Correo encontrado",
        usuario = new
        {
          usuario.Id,
          usuario.Nombre,
          usuario.CorreoElectronico,
          usuario.NumeroTelefono,
          usuario.ApellidoPaterno,
          usuario.ApellidoMaterno,
          usuario.Sexo
        }
      });
    }

    [HttpPost("GenerateSessionToken")]
    public async Task<IActionResult> MetodoSesion([FromBody] SesionRequest request)
    {
      // Modelo de request:
      // public int UsuarioId { get; set; }
      // public string Metodo { get; set; }  // "sms", "correo", "whatsapp"
      try
      {
        var personas = auth.GetUsuarios();
        var user = personas.FirstOrDefault(x => x.Id == request.UsuarioId);

        if (user == null)
          return NotFound(new { error = "Usuario no encontrado" });

        var tokenCreado = await tokenService.GenerarToken(user.Id, user.CorreoElectronico, user.NumeroTelefono, request.Metodo);

        // Envía la notificación a n8n sin bloquear la respuesta al usuario
        _ = Task.Run(() => tokenService.EnviarTokenN8n(tokenCreado));

        var response = new JsonWebTokenResponse
        {
          Message = $"Token generado y enviado por {request.Metodo}",
          TokenCorreo = tokenCreado.TokenCorreo,
          TokenTelefono = tokenCreado.TokenTelefono,
          Tipo = tokenCreado.Tipo,
          Id = user.Id
        };

        return Ok(response);
      }
      catch (System.Exception ex)
      {
        return BadRequest(new { error = ex.Message });
      }
    }

    [HttpPost("Login")]
    public IActionResult Login([FromBody] LoginRequest User)
    {
      // public string Correo { get; set; }             // siempre requerido
      // public string TipoAuth { get; set; }            // password, token, huella, faceid
      //    public string? Password { get; set; }          // para password
      //    public string? TypeToken { get; set; }         // Para el tipo de token SMS, WhatsApp, Correo
      //          public string? Token { get; set; }             // para token
      //    public string? DataBiometrica { get; set; }    // para huella o faceID
      try
      {
        string correo = User.Correo;
        string tipoAuth = User.TipoAuth ?? "password";

        // Verificar si el correo existe y obtener usuario
        var usuario = auth.GetUsuarios().FirstOrDefault(u => u.CorreoElectronico == correo);

        if (usuario == null)
          return NotFound(new { error = "El correo no está registrado" });

        // Ejecutar autenticación según el tipo
        return tipoAuth.ToLower() switch
        {
          "password" => LoginConPassword(usuario.CorreoElectronico, User.Password!),
          "token" => LoginConToken(User),
          "biometrico" => LoginConBiometrico(User),
          _ => BadRequest(new { error = "Tipo de autenticación no válido" })
        };
      }
      catch (Exception ex)
      { return BadRequest(new { error = ex.Message }); }
    }

    private IActionResult LoginConPassword(string correo, string password)
    {
      var resultado = auth.ProcesarLoginConPassword(correo, password);
      if (!resultado.EntradaValida)
        return BadRequest(new { error = resultado.Mensaje });

      return Ok(new { message = resultado.Mensaje, token = resultado.Jwt, tipoAuth = "password" });
    }

    private IActionResult LoginConToken(LoginRequest user)
    {
      // public string Correo { get; set; }             // siempre requerido
      // public string TipoAuth { get; set; }            // password, token, huella, faceid
      // public string? Password { get; set; }          // para password
      // public string? TypeToken { get; set; }         // Para el tipo de token SMS, WhatsApp, Correo
      // public string? Token { get; set; }             // para token
      // public string? DataBiometrica { get; set; }    // para huella o faceID
      if (string.IsNullOrEmpty(user.Token))
        return BadRequest(new { error = "El token es requerido" });

      var resultado = auth.IniciarSesionConToken(user);
      if (!resultado.EntradaValida)
        return Unauthorized(new { error = resultado.Mensaje });

      return Ok(new { message = resultado.Mensaje, token = resultado.Jwt, tipoAuth = "token" });
    }

    public IActionResult LoginConBiometrico([FromBody] LoginRequest user)
    {
      try
      {
        // Aquí accedes correctamente a model.DataBiometrica
        var resultado = auth.ProcesarLoginConBiometrico(user.Correo, user.FaceID);
        if (!resultado.EntradaValida)
          return BadRequest(new { error = resultado.Mensaje });

        return Ok(new { message = resultado.Mensaje, token = resultado.Jwt, tipoAuth = "biometrico" });
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { error = ex.Message });
      }
    }
    // Generar token de recuperación de contraseña
    [HttpPost("RequestPasswordRecovery")]
    public async Task<IActionResult> SolicitarRecuperacion([FromBody] SolicitarRecuperacionRequest request)
    {
      // Modelo de request:
      // public string Correo { get; set; }
      // public string Tipo { get; set; } // "correo", "telefono", "whatsapp"
      try
      {
        if (string.IsNullOrWhiteSpace(request.Correo))
          return BadRequest(new { error = "El correo es requerido" });

        var usuario = auth.GetUsuarios().FirstOrDefault(u => u.CorreoElectronico.Equals(request.Correo, StringComparison.OrdinalIgnoreCase));
        if (usuario == null)
          return NotFound(new { error = "Usuario no encontrado" });

        var tokenRecuperacion = await tokenService.GenerarToken(
            usuario.Id,
            usuario.CorreoElectronico,
            usuario.NumeroTelefono,
            request.Tipo.ToLower()
        );

        // Desacoplar el envío a n8n
        _ = Task.Run(() => tokenService.EnviarTokenN8n(tokenRecuperacion));

        // Responder genéricamente
        return Ok(new
        {
          message = $"Token de recuperación enviado por {request.Tipo}",
          correo = usuario.CorreoElectronico
        });
      }
      catch (Exception ex)
      {
        return BadRequest(new { error = ex.Message });
      }
    }

    [HttpPost("VerifyRecoveryToken")]
    public IActionResult VerificarTokenRecuperacion([FromBody] VerificarTokenRequest request)
    {
      // Modelo de request:
      // public string Correo { get; set; }
      // public string Token { get; set; }
      try
      {
        bool esValido = auth.VerificarTokenRecuperacion(request.Correo, request.Token);

        if (!esValido)
          return BadRequest(new { error = "Token inválido o expirado" });

        return Ok(new { message = "Token válido" });
      }
      catch (Exception ex)
      {
        return BadRequest(new { error = ex.Message });
      }
    }

    // Cambiar contraseña con token de recuperación
    [HttpPost("RecoverPassword")]
    public IActionResult RecuperarPassword([FromBody] RecuperarPasswordRequest request)
    {
      try
      {
        if (!auth.VerificarCorreoExiste(request.Correo))
          return NotFound(new { error = "El correo no está registrado" });

        var cambioExitoso = auth.CambiarPassword(request.Correo, request.NuevaPassword);

        if (!cambioExitoso)
          return StatusCode(500, new { error = "Error interno al cambiar la contraseña" });

        return Ok(new { message = "Contraseña actualizada correctamente" });
      }
      catch (Exception ex)
      {
        return BadRequest(new { error = ex.Message });
      }
    }

    [HttpPost("VerifyTokens")]
    public IActionResult VerificarTokens([FromBody] TokenVerificacionRequest request)
    {
      try
      {
        bool esValido = false;

        switch (request.Tipo?.ToLower())
        {
          case "correo":
            esValido = tokenService.VerificarTokens(request.Correo, null);
            break;
          case "telefono":
          case "whatsapp":
            esValido = tokenService.VerificarTokens(null, request.Telefono);
            break;
          case "verificacion":
            esValido = tokenService.VerificarTokens(request.Correo, request.Telefono);
            break;
          default:
            return BadRequest(new { error = "Tipo de token no válido" });
        }

        if (!esValido)
          return BadRequest(new { error = "Token(s) inválido(s)" });

        return Ok(new { message = "Token(s) válido(s)" });
      }
      catch (Exception ex)
      {
        return BadRequest(new { error = ex.Message });
      }
    }















    // === LEER PERSONAS.JSON ===
    [HttpPost("debug/read-personas")]
    public IActionResult ReadPersonas([FromBody] Admin admin)
    {
      if (!auth.EsRootAdmin(admin.Usuario, admin.Contraseña))
        return StatusCode(-1014, new { error = "No tienes permisos para ejecutar esto" });

      var (success, contenido, error) = auth.LeerPersonasJson();

      if (!success)
        return StatusCode(500, new { error = "Error al leer personas.json", detalle = error });

      return Ok(new { archivo = "personas.json", contenido });
    }

    // === LEER TOKENS_TEMPORALES.JSON ===
    [HttpPost("debug/read-tokens")]
    public IActionResult ReadTokens([FromBody] Admin admin)
    {
      if (!auth.EsRootAdmin(admin.Usuario, admin.Contraseña))
        return StatusCode(-1014, new { error = "No tienes permisos para ejecutar esto" });

      var (success, contenido, error) = auth.LeerTokensJson();

      if (!success)
        return StatusCode(500, new { error = "Error al leer tokens_temporales.json", detalle = error });

      return Ok(new { archivo = "tokens_temporales.json", contenido });
    }

    // === LIMPIAR TOKENS_TEMPORALES.JSON ===
    [HttpPost("debug/clean-tokens")]
    public IActionResult CleanTokens([FromBody] Admin admin)
    {
      if (!auth.EsRootAdmin(admin.Usuario, admin.Contraseña))
        return StatusCode(-1014, new { error = "No tienes permisos para ejecutar esto" });

      var (success, error) = auth.LimpiarTokens();

      if (!success)
        return StatusCode(500, new { error = "Error al limpiar tokens_temporales.json", detalle = error });

      return Ok(new { message = "tokens_temporales.json limpiado", contenido = "[]" });
    }


  }
}

