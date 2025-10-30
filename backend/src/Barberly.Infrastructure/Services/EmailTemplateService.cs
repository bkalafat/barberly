using Barberly.Application.Models;
using Barberly.Application.Notifications.Interfaces;
using System.Globalization;

namespace Barberly.Infrastructure.Services;

/// <summary>
/// Service for rendering email templates with Turkish localization.
/// </summary>
public sealed class EmailTemplateService : IEmailTemplateService
{
    private static readonly CultureInfo TurkishCulture = new("tr-TR");

    public Task<string> RenderAppointmentConfirmationAsync(
        AppointmentDto appointment,
        UserDto user,
        BarberDto barber,
        ServiceDto service,
        BarberShopDto shop)
    {
        var appointmentDate = appointment.Start.ToString("dd MMMM yyyy dddd", TurkishCulture);
        var appointmentTime = appointment.Start.ToString("HH:mm", TurkishCulture);
        var duration = service.DurationInMinutes;
        var price = service.Price.ToString("N2", TurkishCulture);

        var html = $@"
<!DOCTYPE html>
<html lang=""tr"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Randevunuz Onaylandı</title>
</head>
<body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0;"">
    <div style=""max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 8px; background-color: #ffffff;"">
        <div style=""text-align: center; padding: 20px 0; border-bottom: 2px solid #2c3e50;"">
            <h1 style=""color: #2c3e50; margin: 0;"">Barberly</h1>
        </div>
        
        <h2 style=""color: #2c3e50; margin-top: 30px;"">Randevunuz Onaylandı!</h2>
        <p>Merhaba <strong>{EscapeHtml(user.FullName)}</strong>,</p>
        <p>Randevunuz başarıyla oluşturuldu. Detaylar aşağıdadır:</p>
        
        <div style=""background-color: #f8f9fa; padding: 20px; border-radius: 5px; margin: 20px 0;"">
            <h3 style=""margin-top: 0; color: #2c3e50; border-bottom: 2px solid #3498db; padding-bottom: 10px;"">Randevu Detayları</h3>
            <table style=""width: 100%; border-collapse: collapse;"">
                <tr>
                    <td style=""padding: 8px 0; color: #555;""><strong>Berber:</strong></td>
                    <td style=""padding: 8px 0;"">{EscapeHtml(barber.FullName)}</td>
                </tr>
                <tr>
                    <td style=""padding: 8px 0; color: #555;""><strong>Salon:</strong></td>
                    <td style=""padding: 8px 0;"">{EscapeHtml(shop.Name)}</td>
                </tr>
                <tr>
                    <td style=""padding: 8px 0; color: #555;""><strong>Hizmet:</strong></td>
                    <td style=""padding: 8px 0;"">{EscapeHtml(service.Name)}</td>
                </tr>
                <tr>
                    <td style=""padding: 8px 0; color: #555;""><strong>Tarih:</strong></td>
                    <td style=""padding: 8px 0;"">{appointmentDate}</td>
                </tr>
                <tr>
                    <td style=""padding: 8px 0; color: #555;""><strong>Saat:</strong></td>
                    <td style=""padding: 8px 0;"">{appointmentTime}</td>
                </tr>
                <tr>
                    <td style=""padding: 8px 0; color: #555;""><strong>Süre:</strong></td>
                    <td style=""padding: 8px 0;"">{duration} dakika</td>
                </tr>
                <tr>
                    <td style=""padding: 8px 0; color: #555;""><strong>Ücret:</strong></td>
                    <td style=""padding: 8px 0;"">{price} TL</td>
                </tr>
            </table>
        </div>
        
        <div style=""background-color: #e8f4f8; padding: 15px; border-radius: 5px; margin: 20px 0; border-left: 4px solid #3498db;"">
            <p style=""margin: 0; color: #2c3e50;""><strong>📍 Adres:</strong></p>
            <p style=""margin: 5px 0 0 0;"">{EscapeHtml(shop.Address.Street)}, {EscapeHtml(shop.Address.City)}</p>
            <p style=""margin: 5px 0 0 0;""><strong>📞 Telefon:</strong> {EscapeHtml(shop.Phone)}</p>
        </div>
        
        <p style=""margin-top: 30px;"">Randevunuzu iptal veya değiştirmek isterseniz, lütfen bizimle iletişime geçin.</p>
        
        <div style=""text-align: center; margin-top: 40px; padding-top: 20px; border-top: 1px solid #ddd;"">
            <p style=""color: #7f8c8d; font-size: 12px; margin: 5px 0;"">
                Bu otomatik bir mesajdır. Lütfen yanıtlamayın.
            </p>
            <p style=""color: #7f8c8d; font-size: 12px; margin: 5px 0;"">
                © 2025 Barberly - Tüm hakları saklıdır.
            </p>
        </div>
    </div>
</body>
</html>";

        return Task.FromResult(html);
    }

    public Task<string> RenderAppointmentReminderAsync(
        AppointmentDto appointment,
        UserDto user,
        BarberDto barber,
        ServiceDto service,
        BarberShopDto shop)
    {
        var appointmentDate = appointment.Start.ToString("dd MMMM yyyy dddd", TurkishCulture);
        var appointmentTime = appointment.Start.ToString("HH:mm", TurkishCulture);

        var html = $@"
<!DOCTYPE html>
<html lang=""tr"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Randevu Hatırlatması</title>
</head>
<body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0;"">
    <div style=""max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 8px; background-color: #ffffff;"">
        <div style=""text-align: center; padding: 20px 0; border-bottom: 2px solid #2c3e50;"">
            <h1 style=""color: #2c3e50; margin: 0;"">Barberly</h1>
        </div>
        
        <h2 style=""color: #f39c12; margin-top: 30px;"">⏰ Randevu Hatırlatması</h2>
        <p>Merhaba <strong>{EscapeHtml(user.FullName)}</strong>,</p>
        <p>Yarın randevunuz var! Lütfen zamanında gelin.</p>
        
        <div style=""background-color: #fff3cd; padding: 20px; border-radius: 5px; margin: 20px 0; border-left: 4px solid #ffc107;"">
            <h3 style=""margin-top: 0; color: #856404;"">Randevu Detayları</h3>
            <table style=""width: 100%; border-collapse: collapse;"">
                <tr>
                    <td style=""padding: 8px 0; color: #555;""><strong>Berber:</strong></td>
                    <td style=""padding: 8px 0;"">{EscapeHtml(barber.FullName)}</td>
                </tr>
                <tr>
                    <td style=""padding: 8px 0; color: #555;""><strong>Salon:</strong></td>
                    <td style=""padding: 8px 0;"">{EscapeHtml(shop.Name)}</td>
                </tr>
                <tr>
                    <td style=""padding: 8px 0; color: #555;""><strong>Tarih:</strong></td>
                    <td style=""padding: 8px 0;"">{appointmentDate}</td>
                </tr>
                <tr>
                    <td style=""padding: 8px 0; color: #555;""><strong>Saat:</strong></td>
                    <td style=""padding: 8px 0;"">{appointmentTime}</td>
                </tr>
            </table>
        </div>
        
        <div style=""background-color: #e8f4f8; padding: 15px; border-radius: 5px; margin: 20px 0; border-left: 4px solid #3498db;"">
            <p style=""margin: 0; color: #2c3e50;""><strong>📍 Adres:</strong></p>
            <p style=""margin: 5px 0 0 0;"">{EscapeHtml(shop.Address.Street)}, {EscapeHtml(shop.Address.City)}</p>
            <p style=""margin: 5px 0 0 0;""><strong>📞 Telefon:</strong> {EscapeHtml(shop.Phone)}</p>
        </div>
        
        <p style=""margin-top: 30px;"">Randevunuzu iptal veya değiştirmek isterseniz, lütfen en kısa sürede bizimle iletişime geçin.</p>
        
        <p style=""margin-top: 20px; text-align: center; font-size: 16px; color: #2c3e50;"">
            <strong>Görüşmek üzere! 👋</strong>
        </p>
        
        <div style=""text-align: center; margin-top: 40px; padding-top: 20px; border-top: 1px solid #ddd;"">
            <p style=""color: #7f8c8d; font-size: 12px; margin: 5px 0;"">
                Bu otomatik bir mesajdır. Lütfen yanıtlamayın.
            </p>
            <p style=""color: #7f8c8d; font-size: 12px; margin: 5px 0;"">
                © 2025 Barberly - Tüm hakları saklıdır.
            </p>
        </div>
    </div>
</body>
</html>";

        return Task.FromResult(html);
    }

    public Task<string> RenderAppointmentCancellationAsync(
        AppointmentDto appointment,
        UserDto user,
        BarberDto barber,
        ServiceDto service,
        BarberShopDto shop,
        DateTimeOffset cancelledAt)
    {
        var appointmentDate = appointment.Start.ToString("dd MMMM yyyy dddd", TurkishCulture);
        var appointmentTime = appointment.Start.ToString("HH:mm", TurkishCulture);
        var cancellationDate = cancelledAt.ToString("dd MMMM yyyy HH:mm", TurkishCulture);

        var html = $@"
<!DOCTYPE html>
<html lang=""tr"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Randevu İptal Edildi</title>
</head>
<body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0;"">
    <div style=""max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 8px; background-color: #ffffff;"">
        <div style=""text-align: center; padding: 20px 0; border-bottom: 2px solid #2c3e50;"">
            <h1 style=""color: #2c3e50; margin: 0;"">Barberly</h1>
        </div>
        
        <h2 style=""color: #c0392b; margin-top: 30px;"">❌ Randevunuz İptal Edildi</h2>
        <p>Merhaba <strong>{EscapeHtml(user.FullName)}</strong>,</p>
        <p>Aşağıdaki randevunuz iptal edilmiştir:</p>
        
        <div style=""background-color: #f8d7da; padding: 20px; border-radius: 5px; margin: 20px 0; border-left: 4px solid #dc3545;"">
            <h3 style=""margin-top: 0; color: #721c24;"">İptal Edilen Randevu</h3>
            <table style=""width: 100%; border-collapse: collapse;"">
                <tr>
                    <td style=""padding: 8px 0; color: #555;""><strong>Berber:</strong></td>
                    <td style=""padding: 8px 0;"">{EscapeHtml(barber.FullName)}</td>
                </tr>
                <tr>
                    <td style=""padding: 8px 0; color: #555;""><strong>Salon:</strong></td>
                    <td style=""padding: 8px 0;"">{EscapeHtml(shop.Name)}</td>
                </tr>
                <tr>
                    <td style=""padding: 8px 0; color: #555;""><strong>Tarih:</strong></td>
                    <td style=""padding: 8px 0;"">{appointmentDate}</td>
                </tr>
                <tr>
                    <td style=""padding: 8px 0; color: #555;""><strong>Saat:</strong></td>
                    <td style=""padding: 8px 0;"">{appointmentTime}</td>
                </tr>
                <tr>
                    <td style=""padding: 8px 0; color: #555;""><strong>İptal Tarihi:</strong></td>
                    <td style=""padding: 8px 0;"">{cancellationDate}</td>
                </tr>
            </table>
        </div>
        
        <div style=""background-color: #d4edda; padding: 15px; border-radius: 5px; margin: 20px 0; border-left: 4px solid #28a745;"">
            <p style=""margin: 0; color: #155724;""><strong>💡 Yeni Randevu Oluşturun</strong></p>
            <p style=""margin: 10px 0 0 0;"">İsterseniz yeni bir randevu oluşturabilirsiniz. Sizinle tekrar görüşmek isteriz!</p>
        </div>
        
        <div style=""text-align: center; margin-top: 40px; padding-top: 20px; border-top: 1px solid #ddd;"">
            <p style=""color: #7f8c8d; font-size: 12px; margin: 5px 0;"">
                Bu otomatik bir mesajdır. Lütfen yanıtlamayın.
            </p>
            <p style=""color: #7f8c8d; font-size: 12px; margin: 5px 0;"">
                © 2025 Barberly - Tüm hakları saklıdır.
            </p>
        </div>
    </div>
</body>
</html>";

        return Task.FromResult(html);
    }

    private static string EscapeHtml(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#39;");
    }
}
