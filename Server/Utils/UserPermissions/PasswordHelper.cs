namespace Server.Utils.UserPermissions
{
    public static class PasswordHelper
    {
        /// <summary>
        /// Hash a plaintext password using BCrypt
        /// </summary>
        public static string HashPassword(string plainTextPassword)
        {
            if (string.IsNullOrWhiteSpace(plainTextPassword))
            {
                throw new ArgumentException("Password cannot be empty", nameof(plainTextPassword));
            }

            return BCrypt.Net.BCrypt.HashPassword(plainTextPassword);
        }

        /// <summary>
        /// Verify a plaintext password against a hashed password
        /// </summary>
        public static bool VerifyPassword(string plainTextPassword, string hashedPassword)
        {
            if (string.IsNullOrWhiteSpace(plainTextPassword) || string.IsNullOrWhiteSpace(hashedPassword))
            {
                return false;
            }

            return BCrypt.Net.BCrypt.Verify(plainTextPassword, hashedPassword);
        }
    }
}

