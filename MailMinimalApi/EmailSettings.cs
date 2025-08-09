namespace MailMinimalApi
{
    public class EmailSettings
    {
        public string User { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }

        // Nuevos campos sugeridos:
        public string FromName { get; set; } = "Baldur Group"; // Nombre del remitente
        public string ReplyTo { get; set; } = string.Empty; // Email de respuesta
        public long MaxFileSize { get; set; } = 5242880; // 5MB en bytes
        public string[] AllowedFileTypes { get; set; } = { "application/pdf" };
    }
}
