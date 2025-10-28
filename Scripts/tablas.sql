-- ============================================
-- SCRIPT DE CREACIÓN DE TABLAS
-- Sistema de Autenticación Multi-método
-- Base de datos: PostgreSQL
-- ============================================

-- Tabla principal de personas/usuarios
CREATE TABLE IF NOT EXISTS personas (
    id SERIAL PRIMARY KEY,
    nombre VARCHAR(50) NOT NULL,
    apellido_paterno VARCHAR(50) NOT NULL,
    apellido_materno VARCHAR(50),
    fecha_nacimiento DATE NOT NULL,
    sexo VARCHAR(20) NOT NULL, -- 'Masculino', 'Femenino', 'Otro'
    numero_telefono VARCHAR(15),
    correo_electronico VARCHAR(100) UNIQUE NOT NULL,
    password_hash TEXT, -- Hash de la contraseña
    huella_dactilar TEXT, -- Hash de la huella
    face_id TEXT, -- Hash del face ID
    verificado_correo BOOLEAN DEFAULT FALSE,
    verificado_telefono BOOLEAN DEFAULT FALSE,
    fecha_registro TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    activo BOOLEAN DEFAULT TRUE
);

-- Índices para búsquedas rápidas
CREATE INDEX idx_personas_correo ON personas(correo_electronico);
CREATE INDEX idx_personas_telefono ON personas(numero_telefono);

-- ============================================
-- Tabla para tokens temporales (Email, SMS, WhatsApp)
CREATE TABLE IF NOT EXISTS tokens_temporales (
    id SERIAL PRIMARY KEY,
    correo_electronico VARCHAR(100) NOT NULL, -- Puede ser correo sin usuario registrado aún
    numero_telefono VARCHAR(15), -- Opcional
    token VARCHAR(6) NOT NULL, -- Token de 6 dígitos
    tipo VARCHAR(20) NOT NULL, -- 'email', 'sms', 'whatsapp', 'login_email', 'login_sms', 'login_whatsapp'
    expiracion TIMESTAMP NOT NULL,
    usado BOOLEAN DEFAULT FALSE,
    intentos INT DEFAULT 0, -- Contador de intentos fallidos
    fecha_creacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Índice para búsqueda de tokens
CREATE INDEX idx_tokens_correo ON tokens_temporales(correo_electronico);
CREATE INDEX idx_tokens_token ON tokens_temporales(token);
CREATE INDEX idx_tokens_expiracion ON tokens_temporales(expiracion);

-- ============================================
-- Tabla para sesiones activas (JWT)
CREATE TABLE IF NOT EXISTS sesiones (
    id SERIAL PRIMARY KEY,
    persona_id INT NOT NULL REFERENCES personas(id) ON DELETE CASCADE,
    token_jwt TEXT NOT NULL,
    fecha_expiracion TIMESTAMP NOT NULL,
    activo BOOLEAN DEFAULT TRUE,
    ip_address VARCHAR(50),
    user_agent TEXT,
    fecha_creacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    fecha_ultimo_acceso TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Índices para sesiones
CREATE INDEX idx_sesiones_persona ON sesiones(persona_id);
CREATE INDEX idx_sesiones_token ON sesiones(token_jwt);
CREATE INDEX idx_sesiones_activo ON sesiones(activo);

-- ============================================
-- Función para limpiar tokens expirados automáticamente
CREATE OR REPLACE FUNCTION limpiar_tokens_expirados()
RETURNS void AS $$
BEGIN
    DELETE FROM tokens_temporales 
    WHERE expiracion < NOW() OR usado = TRUE;
END;
$$ LANGUAGE plpgsql;

-- ============================================
-- Función para limpiar sesiones expiradas
CREATE OR REPLACE FUNCTION limpiar_sesiones_expiradas()
RETURNS void AS $$
BEGIN
    UPDATE sesiones 
    SET activo = FALSE 
    WHERE fecha_expiracion < NOW() AND activo = TRUE;
END;
$$ LANGUAGE plpgsql;

-- ============================================
-- COMENTARIOS EN LAS TABLAS
COMMENT ON TABLE personas IS 'Tabla principal de usuarios del sistema';
COMMENT ON TABLE tokens_temporales IS 'Tokens temporales para verificación y login';
COMMENT ON TABLE sesiones IS 'Sesiones activas de usuarios autenticados';

COMMENT ON COLUMN personas.password_hash IS 'Hash BCrypt de la contraseña';
COMMENT ON COLUMN personas.huella_dactilar IS 'Hash de la huella dactilar';
COMMENT ON COLUMN personas.face_id IS 'Hash del Face ID';
COMMENT ON COLUMN tokens_temporales.tipo IS 'Tipo de token: email, sms, whatsapp, login_email, login_sms, login_whatsapp';
COMMENT ON COLUMN tokens_temporales.expiracion IS 'Fecha y hora de expiración del token (5 minutos)';
COMMENT ON COLUMN sesiones.token_jwt IS 'Token JWT para mantener la sesión activa';

-- ============================================
-- DATOS DE PRUEBA (OPCIONAL - Comentar en producción)
-- INSERT INTO personas (nombre, apellido_paterno, apellido_materno, fecha_nacimiento, sexo, numero_telefono, correo_electronico)
-- VALUES ('Juan', 'Pérez', 'García', '1990-05-15', 'Masculino', '5551234567', 'juan.perez@example.com');