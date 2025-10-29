# Email Notifications MVP Plan

## Overview

This document outlines the minimum viable product (MVP) implementation for email notifications in the Barberly platform. The goal is to send automated email notifications to users when key events occur in the appointment lifecycle.

## Business Requirements

### Core User Stories

1. **As a customer**, I want to receive a confirmation email when I book an appointment, so I have a record of my booking.
2. **As a customer**, I want to receive a reminder email 24 hours before my appointment, so I don't forget.
3. **As a customer**, I want to receive a notification when my appointment is cancelled, so I'm aware of the cancellation.
4. **As a barber**, I want to receive notifications when new appointments are booked with me, so I can prepare.
5. **As a barber**, I want to receive notifications when appointments are cancelled, so I can update my schedule.

## MVP Scope

### In Scope (Phase 1 - MVP)

- ✅ Appointment confirmation emails (to customer and barber)
- ✅ Appointment reminder emails (24 hours before)
- ✅ Appointment cancellation emails (to customer and barber)
- ✅ Basic email template system with HTML formatting
- ✅ Outbox pattern for reliable delivery
- ✅ Background worker for processing email queue
- ✅ SMTP integration for email delivery
- ✅ Basic email configuration management

### Out of Scope (Future Phases)

- ❌ SMS notifications (Phase 2)
- ❌ WhatsApp notifications (Phase 2)
- ❌ Push notifications (Phase 2)
- ❌ Azure Service Bus integration (Phase 2)
- ❌ Advanced email templates with user customization (Phase 2)
- ❌ Email delivery tracking and analytics (Phase 3)
- ❌ Notification preferences management UI (Phase 3)

## Technical Architecture

### Domain-Driven Design Approach

Following the existing Barberly architecture (DDD + Clean Architecture + CQRS), we will implement:

#### 1. Domain Layer (`Barberly.Domain`)

**New Entities:**

- `NotificationOutbox` - Outbox pattern for reliable message delivery
  - `Id` (Guid)
  - `EventType` (string) - "AppointmentBooked", "AppointmentCancelled", "AppointmentReminder"
  - `RecipientEmail` (string)
  - `RecipientName` (string)
  - `Subject` (string)
  - `Body` (string) - HTML content
  - `Metadata` (string) - JSON with appointment details
  - `Status` (enum) - Pending, Processing, Sent, Failed
  - `RetryCount` (int)
  - `MaxRetries` (int) - default 3
  - `CreatedAtUtc` (DateTimeOffset)
  - `ProcessedAtUtc` (DateTimeOffset?)
  - `ErrorMessage` (string?)

**Domain Events:**

- `AppointmentBookedEvent`
- `AppointmentCancelledEvent`
- `AppointmentRescheduledEvent`

#### 2. Application Layer (`Barberly.Application`)

**New Commands:**
- None directly - notifications are triggered by domain events

**New Queries:**
- `GetPendingNotificationsQuery` - Fetch pending notifications from outbox

**New Handlers:**

- `AppointmentBookedEventHandler` - Creates notification outbox entries
- `AppointmentCancelledEventHandler` - Creates notification outbox entries
- `AppointmentReminderHandler` - Triggered by background job

**New Services (Interfaces):**

- `IEmailService` - Interface for email sending
  - `Task<bool> SendEmailAsync(string to, string subject, string htmlBody, CancellationToken ct)`
  
- `IEmailTemplateService` - Interface for email template generation
  - `Task<string> RenderAppointmentConfirmationAsync(AppointmentDto appointment, UserDto user, BarberDto barber)`
  - `Task<string> RenderAppointmentReminderAsync(AppointmentDto appointment, UserDto user, BarberDto barber)`
  - `Task<string> RenderAppointmentCancellationAsync(AppointmentDto appointment, UserDto user, BarberDto barber)`

#### 3. Infrastructure Layer (`Barberly.Infrastructure`)

**New Implementations:**

- `SmtpEmailService` - SMTP implementation of `IEmailService`
  - Uses `MailKit` library for robust email sending
  - Supports TLS/SSL
  - Configurable via `appsettings.json`

- `EmailTemplateService` - Implementation of `IEmailTemplateService`
  - Simple string-based template system (no external dependencies for MVP)
  - HTML email templates with inline CSS
  - Localization-ready (Turkish support)

- `NotificationOutboxRepository` - Repository for outbox persistence
  - Standard EF Core repository pattern
  - Queries for pending notifications
  - Atomic status updates

**New Background Services:**

- `EmailNotificationWorker` - Hosted service (`IHostedService`)
  - Runs every 30 seconds
  - Queries pending notifications from outbox
  - Processes batch of 10 notifications at a time
  - Updates status based on delivery result
  - Implements exponential backoff for retries

- `AppointmentReminderWorker` - Hosted service
  - Runs every hour
  - Queries appointments starting in 24 hours
  - Creates reminder notifications in outbox
  - Prevents duplicate reminders (tracks sent reminders)

#### 4. API Layer (`Barberly.Api`)

**Configuration:**

- Add email configuration section to `appsettings.json`:
  ```json
  {
    "EmailSettings": {
      "SmtpHost": "smtp.gmail.com",
      "SmtpPort": 587,
      "UseSsl": true,
      "FromEmail": "noreply@barberly.com",
      "FromName": "Barberly",
      "Username": "your-email@gmail.com",
      "Password": "your-app-password"
    },
    "NotificationSettings": {
      "ReminderHours": 24,
      "BatchSize": 10,
      "ProcessingIntervalSeconds": 30
    }
  }
  ```

**New Endpoints (Admin/Testing):**

- `POST /api/v1/admin/notifications/test` - Send test email (admin only)
- `GET /api/v1/admin/notifications/outbox` - View outbox status (admin only)

## Implementation Plan

### Phase 1: Domain & Application Setup (Day 1)

1. ✅ Create `NotificationOutbox` entity in Domain layer
2. ✅ Define domain events (`AppointmentBookedEvent`, etc.)
3. ✅ Create `IEmailService` and `IEmailTemplateService` interfaces
4. ✅ Create `INotificationOutboxRepository` interface
5. ✅ Update `Appointment` entity to raise domain events

### Phase 2: Infrastructure Implementation (Day 2)

1. ✅ Implement `SmtpEmailService` using MailKit
2. ✅ Implement `EmailTemplateService` with HTML templates
3. ✅ Implement `NotificationOutboxRepository`
4. ✅ Add `NotificationOutbox` to `BarberlyDbContext`
5. ✅ Create EF Core migration for NotificationOutbox table
6. ✅ Update existing appointment handlers to publish domain events

### Phase 3: Background Workers (Day 3)

1. ✅ Implement `EmailNotificationWorker`
2. ✅ Implement `AppointmentReminderWorker`
3. ✅ Register background services in `Program.cs`
4. ✅ Add health checks for background workers
5. ✅ Configure settings in `appsettings.json`

### Phase 4: Testing & Validation (Day 4)

1. ✅ Unit tests for domain events
2. ✅ Unit tests for email template service
3. ✅ Integration tests for outbox repository
4. ✅ Integration tests for email sending (mock SMTP)
5. ✅ Integration tests for background workers
6. ✅ Manual testing with real SMTP server

### Phase 5: Documentation & Deployment (Day 5)

1. ✅ Update README with email notification features
2. ✅ Document email configuration settings
3. ✅ Add email templates documentation
4. ✅ Update API documentation
5. ✅ Deploy to staging for testing

## Email Templates

### 1. Appointment Confirmation (Turkish)

**Subject:** Randevunuz Onaylandı - Barberly

**Body:**
```html
<html>
<body style="font-family: Arial, sans-serif; line-height: 1.6; color: #333;">
  <div style="max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 8px;">
    <h2 style="color: #2c3e50;">Randevunuz Onaylandı!</h2>
    <p>Merhaba {{CustomerName}},</p>
    <p>Randevunuz başarıyla oluşturuldu.</p>
    
    <div style="background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;">
      <h3 style="margin-top: 0; color: #2c3e50;">Randevu Detayları</h3>
      <p><strong>Berber:</strong> {{BarberName}}</p>
      <p><strong>Salon:</strong> {{ShopName}}</p>
      <p><strong>Hizmet:</strong> {{ServiceName}}</p>
      <p><strong>Tarih:</strong> {{AppointmentDate}}</p>
      <p><strong>Saat:</strong> {{AppointmentTime}}</p>
      <p><strong>Süre:</strong> {{Duration}} dakika</p>
      <p><strong>Ücret:</strong> {{Price}} TL</p>
    </div>
    
    <p>Randevunuzu iptal veya değiştirmek için <a href="{{AppointmentLink}}">buraya tıklayın</a>.</p>
    
    <p style="color: #7f8c8d; font-size: 12px; margin-top: 30px;">
      Bu otomatik bir mesajdır. Lütfen yanıtlamayın.
    </p>
  </div>
</body>
</html>
```

### 2. Appointment Reminder (Turkish)

**Subject:** Yarın Randevunuz Var - Barberly

**Body:**
```html
<html>
<body style="font-family: Arial, sans-serif; line-height: 1.6; color: #333;">
  <div style="max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 8px;">
    <h2 style="color: #2c3e50;">Randevu Hatırlatması</h2>
    <p>Merhaba {{CustomerName}},</p>
    <p>Yarın randevunuz var!</p>
    
    <div style="background-color: #fff3cd; padding: 15px; border-radius: 5px; margin: 20px 0; border-left: 4px solid #ffc107;">
      <h3 style="margin-top: 0; color: #856404;">Randevu Detayları</h3>
      <p><strong>Berber:</strong> {{BarberName}}</p>
      <p><strong>Salon:</strong> {{ShopName}}</p>
      <p><strong>Tarih:</strong> {{AppointmentDate}}</p>
      <p><strong>Saat:</strong> {{AppointmentTime}}</p>
    </div>
    
    <p>Randevunuzu iptal veya değiştirmek için <a href="{{AppointmentLink}}">buraya tıklayın</a>.</p>
    
    <p>Görüşmek üzere!</p>
    
    <p style="color: #7f8c8d; font-size: 12px; margin-top: 30px;">
      Bu otomatik bir mesajdır. Lütfen yanıtlamayın.
    </p>
  </div>
</body>
</html>
```

### 3. Appointment Cancellation (Turkish)

**Subject:** Randevunuz İptal Edildi - Barberly

**Body:**
```html
<html>
<body style="font-family: Arial, sans-serif; line-height: 1.6; color: #333;">
  <div style="max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 8px;">
    <h2 style="color: #c0392b;">Randevunuz İptal Edildi</h2>
    <p>Merhaba {{CustomerName}},</p>
    <p>Aşağıdaki randevunuz iptal edildi:</p>
    
    <div style="background-color: #f8d7da; padding: 15px; border-radius: 5px; margin: 20px 0; border-left: 4px solid #dc3545;">
      <p><strong>Berber:</strong> {{BarberName}}</p>
      <p><strong>Salon:</strong> {{ShopName}}</p>
      <p><strong>Tarih:</strong> {{AppointmentDate}}</p>
      <p><strong>Saat:</strong> {{AppointmentTime}}</p>
      <p><strong>İptal Tarihi:</strong> {{CancellationDate}}</p>
    </div>
    
    <p>Yeni bir randevu oluşturmak için <a href="{{BookingLink}}">buraya tıklayın</a>.</p>
    
    <p style="color: #7f8c8d; font-size: 12px; margin-top: 30px;">
      Bu otomatik bir mesajdır. Lütfen yanıtlamayın.
    </p>
  </div>
</body>
</html>
```

## Configuration

### Development Configuration

For local development, use a test email service like Ethereal Email (https://ethereal.email/) or Gmail with app passwords:

```json
{
  "EmailSettings": {
    "SmtpHost": "smtp.ethereal.email",
    "SmtpPort": 587,
    "UseSsl": true,
    "FromEmail": "test@barberly.com",
    "FromName": "Barberly (Test)",
    "Username": "your-ethereal-username",
    "Password": "your-ethereal-password"
  }
}
```

### Production Configuration

Use Azure SendGrid or similar production email service:

```json
{
  "EmailSettings": {
    "SmtpHost": "smtp.sendgrid.net",
    "SmtpPort": 587,
    "UseSsl": true,
    "FromEmail": "noreply@barberly.com",
    "FromName": "Barberly",
    "Username": "apikey",
    "Password": "your-sendgrid-api-key"
  }
}
```

## Testing Strategy

### Unit Tests

1. **Domain Tests:**
   - `NotificationOutbox` creation and state transitions
   - Domain event publication from `Appointment` entity

2. **Application Tests:**
   - Event handlers create correct outbox entries
   - Email template rendering with correct data
   - Template parameter substitution

3. **Infrastructure Tests:**
   - Outbox repository queries and updates
   - Email template service renders valid HTML

### Integration Tests

1. **Repository Tests:**
   - CRUD operations on `NotificationOutbox`
   - Batch retrieval of pending notifications
   - Status updates are atomic

2. **Email Service Tests:**
   - Mock SMTP server integration
   - Email format validation
   - Error handling and retries

3. **Background Worker Tests:**
   - Worker processes notifications correctly
   - Worker handles errors gracefully
   - Worker respects retry limits
   - Reminder worker doesn't create duplicates

### Manual Testing Checklist

- [ ] Book appointment → Receive confirmation email
- [ ] Cancel appointment → Receive cancellation email
- [ ] Wait 24 hours before appointment → Receive reminder email
- [ ] Test email templates render correctly in Gmail, Outlook, Apple Mail
- [ ] Test email delivery failures trigger retries
- [ ] Test max retry limit prevents infinite loops
- [ ] Test background workers restart after crash

## Security Considerations

1. **Email Credentials:**
   - Store SMTP credentials in Azure Key Vault (production)
   - Use environment variables for local development
   - Never commit credentials to source control

2. **Email Content:**
   - Sanitize all user-provided data before including in emails
   - Prevent HTML injection in email bodies
   - Validate email addresses before sending

3. **Rate Limiting:**
   - Implement rate limiting to prevent email spam
   - Batch processing to respect SMTP provider limits
   - Monitor for abnormal email volume

4. **Privacy:**
   - Don't include sensitive information in email subjects
   - Use secure links for appointment management
   - Implement unsubscribe mechanism (Phase 2)

## Monitoring & Observability

### Metrics to Track

1. **Email Delivery Metrics:**
   - Total emails sent
   - Success rate
   - Failure rate by error type
   - Average delivery time
   - Retry rate

2. **Queue Metrics:**
   - Pending notifications count
   - Processing queue depth
   - Average processing time
   - Failed notifications count

3. **Worker Health:**
   - Last successful run timestamp
   - Worker uptime
   - Exception count

### Logging

Log the following events:

- Email send attempts (success/failure)
- Notification outbox creation
- Background worker execution
- SMTP connection errors
- Template rendering errors

## Performance Considerations

1. **Batch Processing:**
   - Process notifications in batches of 10
   - Configurable batch size

2. **Database Queries:**
   - Index on `NotificationOutbox.Status` and `CreatedAtUtc`
   - Use pagination for large result sets

3. **SMTP Connection:**
   - Reuse SMTP connections when possible
   - Implement connection pooling

4. **Worker Scheduling:**
   - Configurable processing interval (default: 30 seconds)
   - Avoid overlapping worker executions

## Dependencies

### NuGet Packages Required

- `MailKit` (>= 4.3.0) - SMTP email sending
- `MimeKit` (>= 4.3.0) - Email message construction
- Existing packages: MediatR, EF Core, etc.

## Rollout Strategy

### Phase 1: Development & Testing
- Implement on feature branch
- Test with Ethereal Email
- Code review and testing

### Phase 2: Staging Deployment
- Deploy to staging environment
- Configure with test SendGrid account
- Run integration tests
- Manual testing by QA team

### Phase 3: Production Rollout
- Deploy to production with feature flag disabled
- Monitor logs and metrics
- Enable for 10% of users
- Gradual rollout to 100%

### Rollback Plan
- Feature flag to disable email sending
- Database migration rollback script
- Disable background workers via configuration

## Success Criteria

✅ Email notifications are delivered within 1 minute of event
✅ Email delivery success rate > 95%
✅ Reminder emails are sent 24 hours before appointments
✅ Zero duplicate reminder emails
✅ Failed emails are retried up to 3 times
✅ Background workers recover from failures
✅ All tests pass with >80% code coverage
✅ Email templates render correctly in major email clients

## Future Enhancements (Post-MVP)

### Phase 2: Enhanced Notifications
- SMS notifications via Twilio
- WhatsApp notifications
- Push notifications for mobile app
- User notification preferences UI
- Multiple reminder time options (24h, 2h, 30m)

### Phase 3: Advanced Features
- Email delivery tracking and analytics
- A/B testing of email templates
- Personalized recommendations in emails
- Marketing campaign support
- Azure Service Bus integration

### Phase 4: Enterprise Features
- White-label email templates per shop
- Custom email domains per shop
- Advanced scheduling rules
- Notification analytics dashboard
- Multi-language support (EN, TR)

## References

- [Barberly Architecture Guide](.github/copilot-instructions.md)
- [Project Plan](.github/plan/plan.md)
- [MailKit Documentation](https://github.com/jstedfast/MailKit)
- [Outbox Pattern](https://microservices.io/patterns/data/transactional-outbox.html)
- [Email Template Best Practices](https://www.campaignmonitor.com/dev-resources/guides/coding/)

---

**Document Version:** 1.0  
**Last Updated:** 2025-10-29  
**Status:** Ready for Implementation  
**Owner:** Development Team
