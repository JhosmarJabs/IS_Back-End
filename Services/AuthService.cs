using Backend.Services;
using IS_Back_End.Helpers;
using IS_Back_End.Models;
using System;
using System.Linq;

namespace IS_Back_End.Services
{
  public class AuthService
  {
    private readonly DataService data;
    private readonly TokenService tokenService;
    private readonly JwtService jwtService;

    public AuthService(DataService ds, TokenService ts, JwtService js)
    {
      data = ds;
      tokenService = ts;
      jwtService = js;
    }
    public List<Persona> GetUsuarios()
    {
      return data.Load<Persona>("personas.json");
    }

    // 🔹 Verifica si un correo ya existe en personas.json
    public bool VerificarCorreoExiste(string correo)
    {
      var personas = data.Load<Persona>("personas.json");
      return personas.Any(p =>
          p.CorreoElectronico.Equals(correo, StringComparison.OrdinalIgnoreCase));
    }

    // 🔹 Registrar nuevo usuario
    public Persona RegistrarUsuario(Persona ePersona)
    {
      // Leer usuarios existentes desde personas.json
      var personas = data.Load<Persona>("personas.json");

      if (personas.Any(x => x.CorreoElectronico == ePersona.CorreoElectronico))
        throw new Exception("Correo ya está registrado.");

      // Encriptar la contraseña antes de guardar
      ePersona.PasswordHash = PasswordHasher.Hash(ePersona.PasswordHash);
      ePersona.Id = personas.Count > 0 ? personas.Max(u => u.Id) + 1 : 1;

      personas.Add(ePersona);
      data.Save("personas.json", personas);
      return ePersona;
    }

    // 🔹 Iniciar sesión
    // arreglar
    public string IniciarSesionConContraseña(string correo, string password)
    {
      var personas = data.Load<Persona>("personas.json");
      var user = personas.FirstOrDefault(x => x.CorreoElectronico == correo);
      if (user == null)
        throw new Exception("Usuario no encontrado.");

      if (!PasswordHasher.Verify(password, user.PasswordHash))
        throw new Exception("Contraseña incorrecta.");

      return jwtService.GenerarToken(user.Id);
    }
    public ValidacionTokenResult ProcesarLoginConPassword(string correo, string password)
    {
      if (string.IsNullOrEmpty(password))
        return new ValidacionTokenResult { EntradaValida = false, Mensaje = "La contraseña es requerida" };

      var personas = data.Load<Persona>("personas.json");
      var user = personas.FirstOrDefault(x => x.CorreoElectronico == correo);

      if (user == null)
        return new ValidacionTokenResult { EntradaValida = false, Mensaje = "Usuario no encontrado" };

      if (!PasswordHasher.Verify(password, user.PasswordHash))
        return new ValidacionTokenResult { EntradaValida = false, Mensaje = "Contraseña incorrecta" };

      var jwt = jwtService.GenerarToken(user.Id);
      return new ValidacionTokenResult { EntradaValida = true, Mensaje = "Login exitoso", Jwt = jwt };
    }

    public ValidacionTokenResult IniciarSesionConToken(LoginRequest eUsuario)
    {
      var personas = data.Load<Persona>("personas.json");
      var user = personas.FirstOrDefault(x => x.CorreoElectronico == eUsuario.Correo);
      if (user == null)
        return new ValidacionTokenResult { EntradaValida = false, Mensaje = "Usuario no encontrado." };

      if (string.IsNullOrEmpty(eUsuario.Token) || string.IsNullOrEmpty(eUsuario.TypeToken))
        return new ValidacionTokenResult { EntradaValida = false, Mensaje = "Token y tipo de token son requeridos." };

      var tokens = data.Load<TokenTemporal>("tokens_temporales.json");
      var tokenEncontrado = tokens.FirstOrDefault(t =>
          t.UsuarioId == user.Id &&
          t.Tipo.Equals(eUsuario.TypeToken, StringComparison.OrdinalIgnoreCase) &&
          (
              // Para tipo verificacion, valida que ambos tokens coincidan
              (t.Tipo.Equals("verificacion", StringComparison.OrdinalIgnoreCase) &&
                (string.Equals(t.Valor, eUsuario.Token, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(t.Valor2, eUsuario.Token, StringComparison.OrdinalIgnoreCase))
              )
              // Para otros tipos, valida solo el valor principal
              || (t.Tipo != "verificacion" &&
                  string.Equals(t.Valor, eUsuario.Token, StringComparison.OrdinalIgnoreCase))
          ) &&
          !t.Usado &&
          t.FechaExpiracion > DateTime.UtcNow);


      if (tokenEncontrado == null)
        return new ValidacionTokenResult { EntradaValida = false, Mensaje = $"Token {eUsuario.TypeToken} inválido o expirado." };

      tokenEncontrado.Usado = true;
      data.Save("tokens_temporales.json", tokens);

      var jwt = jwtService.GenerarToken(user.Id);
      return new ValidacionTokenResult
      {
        EntradaValida = true,
        Jwt = jwt,
        Mensaje = $"Login exitoso con token {eUsuario.TypeToken}"
      };
    }

    public bool VerificarTokenRecuperacion(string correo, string tokenRecibido)
    {
      var personas = data.Load<Persona>("personas.json");
      var user = personas.FirstOrDefault(x => x.CorreoElectronico.Equals(correo, StringComparison.OrdinalIgnoreCase));
      if (user == null)
        return false;

      var tokens = data.Load<TokenTemporal>("tokens_temporales.json");

      // Buscar token que pertenezca al usuario y coincida con uno o ambos valores si es verificacion
      var tokenEncontrado = tokens.FirstOrDefault(t =>
          t.UsuarioId == user.Id &&
          !t.Usado &&
          t.FechaExpiracion > DateTime.UtcNow &&
          (t.Valor == tokenRecibido || t.Valor2 == tokenRecibido)
      );

      return tokenEncontrado != null;
    }


    public ValidacionTokenResult ProcesarLoginConBiometrico(string correo, string dataBiometrica)
    {
      if (string.IsNullOrEmpty(dataBiometrica))
        return new ValidacionTokenResult { EntradaValida = false, Mensaje = "Los datos biométricos son requeridos" };

      var personas = data.Load<Persona>("personas.json");
      var user = personas.FirstOrDefault(x => x.CorreoElectronico == correo);

      if (user == null)
        return new ValidacionTokenResult { EntradaValida = false, Mensaje = "Usuario no encontrado" };

      // Aquí implementar tu lógica biométrica

      var jwt = jwtService.GenerarToken(user.Id);
      return new ValidacionTokenResult { EntradaValida = true, Mensaje = "Login exitoso con biometría", Jwt = jwt };
    }

    // 🔹 Verificar token de recuperación de contraseña


    // 🔹 Cambiar contraseña del usuario
    public bool CambiarPassword(string correo, string nuevaPassword)
    {
      if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(nuevaPassword))
        return false;

      var personas = data.Load<Persona>("personas.json");
      var user = personas.FirstOrDefault(x => x.CorreoElectronico.Equals(correo, StringComparison.OrdinalIgnoreCase));

      if (user == null)
        return false;

      // Encriptar la nueva contraseña
      user.PasswordHash = PasswordHasher.Hash(nuevaPassword);

      // Guardar cambios
      data.Save("personas.json", personas);

      // Marcar el token como usado
      var tokens = data.Load<TokenTemporal>("tokens_temporales.json");
      var tokenRecuperacion = tokens.FirstOrDefault(t =>
          t.UsuarioId == user.Id &&
          t.Tipo.Equals("recuperacion", StringComparison.OrdinalIgnoreCase) &&
          !t.Usado &&
          t.FechaExpiracion > DateTime.UtcNow);

      if (tokenRecuperacion != null)
      {
        tokenRecuperacion.Usado = true;
        data.Save("tokens_temporales.json", tokens);
      }

      return true;
    }

  }
}
