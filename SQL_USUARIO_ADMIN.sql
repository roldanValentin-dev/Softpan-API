-- Script para crear usuario Admin inicial
-- Email: admin@gmail.com
-- Password: admin123

-- Usar el endpoint de registro en su lugar
-- POST https://softpan-api.onrender.com/api/auth/register
-- Body:
-- {
--   "email": "admin@gmail.com",
--   "password": "admin123",
--   "confirmPassword": "admin123"
-- }

-- Si necesitas insertar directamente, usa AspNetUsers (Identity):
INSERT INTO "AspNetUsers" (
    "Id",
    "UserName",
    "NormalizedUserName",
    "Email",
    "NormalizedEmail",
    "EmailConfirmed",
    "PasswordHash",
    "SecurityStamp",
    "ConcurrencyStamp",
    "PhoneNumberConfirmed",
    "TwoFactorEnabled",
    "LockoutEnabled",
    "AccessFailedCount"
)
VALUES (
    gen_random_uuid()::text,
    'admin@gmail.com',
    'ADMIN@GMAIL.COM',
    'admin@gmail.com',
    'ADMIN@GMAIL.COM',
    true,
    'AQAAAAIAAYagAAAAEJ5Z5Z5Z5Z5Z5Z5Z5Z5Z5Z5Z5Z5Z5Z5Z5Z5Z5Z5Z5Z5Z5Z5Z5Z5Z5Z5Z5Z5Z5Z==',
    gen_random_uuid()::text,
    gen_random_uuid()::text,
    false,
    false,
    true,
    0
);
