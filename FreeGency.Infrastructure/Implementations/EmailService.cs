using FreeGency.Infrastructure.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Net;
using System.Text.RegularExpressions;

namespace FreeGency.Infrastructure.Implementations
{
    public class EmailService(IOptions<EmailBinding> options, IConfiguration configuration) : IEmailService
    {
        private readonly EmailBinding _options = options.Value;
        private readonly IConfiguration _configuration = configuration;

        public async Task<bool> SendMassege(string Email, string Messege, string? reason)
        {
            try
            {
                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(_options.Host, _options.Port, SecureSocketOptions.StartTls);
                    client.Authenticate(_options.Mail, _options.Password);

                    var bodyBuilder = BuildBody(Messege, reason);
                    var message = new MimeMessage
                    {
                        Body = bodyBuilder.ToMessageBody()
                    };
                    message.From.Add(new MailboxAddress(_options.DisplayName, _options.Mail));
                    message.To.Add(new MailboxAddress("testing", Email));
                    message.Subject = reason == null ? "No Submitted" : reason;

                    await client.SendAsync(message);
                    client.Disconnect(true);
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email error: {ex}");
                return false;
            }
        }

        private BodyBuilder BuildBody(string messege, string? reason)
        {
            var bodyBuilder = new BodyBuilder();
            var assetsPath = Path.Combine(AppContext.BaseDirectory, "Email", "Assets");
            var logoCid = "freegency-logo";
            var mailCid = "freegency-mail";

            var logoPath = Path.Combine(assetsPath, "Logo.png");
            var mailPath = Path.Combine(assetsPath, "Mail.png");

            if (File.Exists(logoPath))
            {
                var logo = bodyBuilder.LinkedResources.Add(logoPath);
                logo.ContentId = logoCid;
            }

            if (File.Exists(mailPath))
            {
                var mail = bodyBuilder.LinkedResources.Add(mailPath);
                mail.ContentId = mailCid;
            }

            var isLink = messege.StartsWith("http", StringComparison.OrdinalIgnoreCase);
            var isReset = reason?.Contains("Reset", StringComparison.OrdinalIgnoreCase) == true;
            var isConfirm = reason?.Contains("Confirm", StringComparison.OrdinalIgnoreCase) == true;
            var code = ExtractResetCode(messege);

            var heading = isConfirm
                ? "Confirm your email"
                : isReset || !string.IsNullOrEmpty(code)
                    ? "Reset your password"
                    : (string.IsNullOrWhiteSpace(reason) ? "Message from FreeGency" : reason);

            var subtitle = isConfirm
                ? "You're almost in. Click the button below to finish setting up your FreeGency account."
                : isReset || !string.IsNullOrEmpty(code)
                    ? "Enter this 6-digit code to reset your password."
                    : "Here's a message from the FreeGency team.";

            var ctaLabel = isConfirm ? "Confirm email" : "Continue";
            var loginUrl = (_configuration["FrontendUrl"] ?? "http://localhost:4200").TrimEnd('/') + "/auth/login";

            string contentBlock;
            if (isLink)
            {
                var safeUrl = WebUtility.HtmlEncode(messege);
                contentBlock = $"""
                    <p style="margin:0 0 32px;font-family:Arial,Helvetica,sans-serif;font-size:16px;line-height:26px;color:#464555;">
                      {WebUtility.HtmlEncode(subtitle)}
                    </p>
                    <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="margin:0 0 24px;">
                      <tr>
                        <td align="center" style="border-radius:999px;background-color:#5b4ff0;">
                          <a href="{messege}"
                             style="display:block;padding:16px 24px;font-family:Arial,Helvetica,sans-serif;
                                    font-size:16px;font-weight:700;line-height:24px;color:#ffffff;text-decoration:none;border-radius:999px;">
                            {WebUtility.HtmlEncode(ctaLabel)}
                          </a>
                        </td>
                      </tr>
                    </table>
                    <p style="margin:0;font-family:Arial,Helvetica,sans-serif;font-size:12px;line-height:20px;color:#777587;word-break:break-all;">
                      Or paste this link into your browser:<br />
                      <a href="{messege}" style="color:#5b4ff0;text-decoration:underline;">{safeUrl}</a>
                    </p>
                    """;
            }
            else if (!string.IsNullOrEmpty(code))
            {
                var spacedCode = string.Join("  ", code.ToCharArray());
                contentBlock = $"""
                    <p style="margin:0 0 28px;font-family:Arial,Helvetica,sans-serif;font-size:16px;line-height:26px;color:#464555;">
                      {WebUtility.HtmlEncode(subtitle)}
                    </p>
                    <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="margin:0 0 20px;">
                      <tr>
                        <td align="center" style="background-color:#f6f3f5;border:2px solid #5b4ff0;border-radius:16px;padding:28px 20px;">
                          <p style="margin:0 0 10px;font-family:Arial,Helvetica,sans-serif;font-size:12px;font-weight:700;letter-spacing:0.1em;text-transform:uppercase;color:#777587;">
                            Your code
                          </p>
                          <p style="margin:0;font-family:Consolas,'Courier New',monospace;font-size:36px;line-height:44px;font-weight:700;letter-spacing:0.12em;color:#4130d7;">
                            {WebUtility.HtmlEncode(spacedCode)}
                          </p>
                        </td>
                      </tr>
                    </table>
                    <p style="margin:0;font-family:Arial,Helvetica,sans-serif;font-size:13px;line-height:20px;color:#777587;">
                      If you didn't request this, you can safely ignore this email.
                    </p>
                    """;
            }
            else
            {
                contentBlock = $"""
                    <p style="margin:0 0 16px;font-family:Arial,Helvetica,sans-serif;font-size:16px;line-height:26px;color:#464555;">
                      {WebUtility.HtmlEncode(subtitle)}
                    </p>
                    <p style="margin:0;font-family:Arial,Helvetica,sans-serif;font-size:16px;line-height:26px;color:#1b1b1d;font-weight:600;">
                      {WebUtility.HtmlEncode(messege)}
                    </p>
                    """;
            }

            bodyBuilder.HtmlBody = $"""
                <!DOCTYPE html>
                <html lang="en">
                <head>
                  <meta charset="utf-8" />
                  <meta name="viewport" content="width=device-width, initial-scale=1" />
                  <title>{WebUtility.HtmlEncode(heading)}</title>
                </head>
                <body style="margin:0;padding:0;background-color:#ffffff;">
                  <div style="display:none;max-height:0;overflow:hidden;opacity:0;">
                    {WebUtility.HtmlEncode(subtitle)}
                  </div>
                  <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="background-color:#ffffff;">
                    <tr>
                      <td align="center" style="padding:40px 16px;">
                        <table role="presentation" width="100%" cellspacing="0" cellpadding="0"
                               style="max-width:500px;border-collapse:separate;border:1px solid #c7c4d8;border-radius:16px;background-color:#ffffff;">
                          <tr>
                            <td align="center" style="padding:48px 32px 40px;">

                              <img src="cid:{logoCid}" alt="FreeGency" width="160" height="37"
                                   style="display:block;margin:0 auto 40px;border:0;outline:none;height:auto;" />

                              <img src="cid:{mailCid}" alt="" width="180"
                                   style="display:block;margin:0 auto 40px;border:0;outline:none;height:auto;max-width:100%;" />

                              <h1 style="margin:0 0 16px;font-family:Arial,Helvetica,sans-serif;font-size:28px;line-height:36px;font-weight:700;letter-spacing:-0.02em;color:#1b1b1d;">
                                {WebUtility.HtmlEncode(heading)}
                              </h1>

                              {contentBlock}

                              <p style="margin:40px 0 0;font-family:Arial,Helvetica,sans-serif;font-size:14px;line-height:20px;color:#464555;">
                                <a href="{loginUrl}" style="color:#5b4ff0;font-weight:700;text-decoration:underline;">← Back to Login</a>
                              </p>

                            </td>
                          </tr>
                        </table>
                      </td>
                    </tr>
                  </table>
                </body>
                </html>
                """;

            bodyBuilder.TextBody = !string.IsNullOrEmpty(code)
                ? $"{heading}\n\n{subtitle}\n\nYour code: {code}\n\n— FreeGency"
                : isLink
                    ? $"{heading}\n\n{subtitle}\n\nOpen this link:\n{messege}\n\n— FreeGency"
                    : $"{heading}\n\n{subtitle}\n\n{messege}\n\n— FreeGency";

            return bodyBuilder;
        }

        private static string? ExtractResetCode(string messege)
        {
            var match = Regex.Match(messege ?? string.Empty, @"(\d{6})");
            return match.Success ? match.Groups[1].Value : null;
        }
    }
}
