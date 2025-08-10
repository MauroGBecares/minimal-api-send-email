using Microsoft.Extensions.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Text.Json;
using MailMinimalApi;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();
builder.Services.AddCors();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));
builder.Services.Configure<RecaptchaSettings>(builder.Configuration.GetSection("Recaptcha"));
builder.Services.AddHttpClient();

var app = builder.Build();


app.UseCors(p => p
    .WithOrigins("https://baldurgroup.com/")
    .AllowAnyHeader()
    .AllowAnyMethod());

app.MapGet("/", () => Results.Ok("API is running"));

app.MapPost("/send-email", async (
    HttpRequest request,
    IOptions<EmailSettings> emailOptions,
    IOptions<RecaptchaSettings> recaptchaOptions,
    IHttpClientFactory httpClientFactory
) =>
{
    try
    {
        var form = await request.ReadFormAsync();

        var from = form["from"].ToString();
        var subject = form["subject"].ToString();
        var body = form["body"].ToString();
        var captchaToken = form["g-recaptcha-response"].ToString();
        var files = form.Files;

        Console.WriteLine($"from: {from}");
        Console.WriteLine($"subject: {subject}");
        Console.WriteLine($"body: {body}");
        Console.WriteLine($"g-recaptcha-response: {captchaToken}");
        Console.WriteLine($"files count: {files.Count}");

        if (string.IsNullOrEmpty(captchaToken))
        {
            return Results.BadRequest(new { error = "Captcha no completado." });
        }

        var captchaSecretKey = recaptchaOptions.Value.SecretKey;

        var client = httpClientFactory.CreateClient();
        var response = await client.PostAsync(
            "https://www.google.com/recaptcha/api/siteverify",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "secret", captchaSecretKey },
                { "response", captchaToken }
            }));

        var resultJson = await response.Content.ReadAsStringAsync();
        var captchaResult = JsonSerializer.Deserialize<ReCaptchaResponse>(resultJson);

        if (captchaResult is null || !captchaResult.success)
            return Results.BadRequest(new { error = "Captcha inválido." });

        var settings = emailOptions.Value;

        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(settings.FromName, from));
        email.To.Add(MailboxAddress.Parse(settings.User));
        email.Subject = subject;

        var builder = new BodyBuilder { TextBody = body };

        foreach (var file in files)
        {
            if (!settings.AllowedFileTypes.Contains(file.ContentType))
                return Results.BadRequest(new { error = "Tipo de archivo no permitido." });

            if (file.Length > settings.MaxFileSize)
                return Results.BadRequest(new { error = "El archivo supera el tamaño máximo permitido." });

            using var stream = file.OpenReadStream();
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            builder.Attachments.Add(file.FileName, ms.ToArray(), ContentType.Parse(file.ContentType));
        }

        email.Body = builder.ToMessageBody();

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(settings.Host, settings.Port, SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(settings.User, settings.Password);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);

        return Results.Ok(new { success = true, message = "Correo enviado correctamente" });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR] {ex}");
        return Results.Problem(
            detail: ex.Message,
            title: "Error al enviar el correo",
            statusCode: 500
        );
    }
});

var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Run($"http://0.0.0.0:{port}");

