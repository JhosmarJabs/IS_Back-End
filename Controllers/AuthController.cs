using Microsoft.AspNetCore.Mvc;
using IS_Back_End.Models;
using IS_Back_End.Services;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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

    [HttpPost("")] // Funcionando
    public IActionResult Prueba()
    {
      try
      {
        Console.WriteLine("✅ Endpoint de prueba ejecutado con éxito");
        return Ok(new { message = "Endpoint de prueba ejecutado con éxito" });
      }
      catch (Exception ex)
      { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPost("registro")] // Funcionando
    public IActionResult Registrar([FromBody] Persona p)
    {
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

    [HttpPost("generar-validacion")]
    public async Task<IActionResult> GenerarTokenVerificacion([FromBody] TokenVerificacionRequest request)
    {
      try
      {
        var resultado = await tokenService.GenerarToken(-1, request.Correo, request.Telefono, request.Tipo);

        var response = new JsonWebTokenResponse
        {
          Message = $"Tokens generados y enviado",
          TokenCorreo = resultado.TokenCorreo,
          TokenTelefono = resultado.TokenTelefono,
          Tipo = resultado.Tipo,
          Id = -1
        };

        return Ok(response);
      }
      catch (Exception ex)
      {
        return BadRequest(new { error = ex.Message });
      }
    }

    [HttpPost("verificacion-tokens")]
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


    [HttpPost("verificar-correo")] // Funcionando
    public IActionResult VerificarCorreo([FromBody] string correo)
    {
      try
      {
        if (string.IsNullOrWhiteSpace(correo))
          return BadRequest(new { error = "El correo es requerido" });

        bool existe = auth.VerificarCorreoExiste(correo);

        if (!existe)
          return NotFound(new { error = "El correo no está registrado" });

        return Ok(new { message = "Correo encontrado", correo = correo });
      }
      catch (Exception ex)
      { return StatusCode(500, new { error = ex.Message }); }
    }
    [HttpPost("metodo-sesion")]
    public async Task<IActionResult> MetodoSesion([FromBody] SesionRequest request)
    {
      try
      {
        var personas = auth.GetUsuarios();
        var user = personas.FirstOrDefault(x => x.Id == request.UsuarioId);

        if (user == null)
          return NotFound(new { error = "Usuario no encontrado" });

        var tokenCreado = await tokenService.GenerarToken(user.Id, user.CorreoElectronico, user.NumeroTelefono, request.Metodo);

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


    [HttpPost("login")] // 
    public IActionResult Login([FromBody] LoginRequest User)
    {
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
          "biometrico" => LoginConBiometrico(User.Correo, User.DataBiometrica!),
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
      if (string.IsNullOrEmpty(user.Token))
        return BadRequest(new { error = "El token es requerido" });

      var resultado = auth.IniciarSesionConToken(user);
      if (!resultado.EntradaValida)
        return Unauthorized(new { error = resultado.Mensaje });

      return Ok(new { message = resultado.Mensaje, token = resultado.Jwt, tipoAuth = "token" });
    }

    private IActionResult LoginConBiometrico(string correo, string dataBiometrica)
    {
      // var resultado = auth.ProcesarLoginConBiometrico(correo, dataBiometrica);
      // if (!resultado.EntradaValida)
      //   return BadRequest(new { error = resultado.Mensaje });

      // return Ok(new { message = resultado.Mensaje, token = resultado.Jwt, tipoAuth = "biometrico" });
      return Ok(new { error = "Autenticación biométrica no implementada aún" });
    }

    // Generar token de recuperación de contraseña
    [HttpPost("solicitar-recuperacion")]
    public async Task<IActionResult> SolicitarRecuperacion([FromBody] SolicitarRecuperacionRequest request)
    {
      try
      {
        // Validaciones básicas.
        if (string.IsNullOrWhiteSpace(request.Correo))
          return BadRequest(new { error = "El correo es requerido" });

        // Buscar usuario.
        var usuario = auth.GetUsuarios().FirstOrDefault(u => u.CorreoElectronico.Equals(request.Correo, StringComparison.OrdinalIgnoreCase));
        if (usuario == null)
          return NotFound(new { error = "Usuario no encontrado" });

        // Generar y guardar token
        var tokenRecuperacion = await tokenService.GenerarToken(usuario.Id, usuario.CorreoElectronico, usuario.NumeroTelefono, request.Tipo.ToLower());

        // Aquí puedes enviar el token por el canal correspondiente. Si tu función GenerarToken lo hace, no hace falta más.

        // Responder con dato genérico (nunca devuelvas en un REST el token real).
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

    [HttpPost("verificar-token-recuperacion")]
    public IActionResult VerificarTokenRecuperacion([FromBody] VerificarTokenRequest request)
    {
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
    [HttpPost("recuperar-password")]
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

  }
}

