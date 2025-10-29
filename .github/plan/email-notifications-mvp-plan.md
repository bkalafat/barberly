# Email Notifications MVP Plan (No Payments)

**Date:** October 29, 2025  
**Status:** Planning Phase

## Overview

Build email notification system for appointment lifecycle using SendGrid with dead-letter queue (exponential backoff, max 3 retries). Use GitHub Actions scheduled workflows for background jobs. Barbers collect payments in-person. Enforce 24-hour cancellation policy.

---

## Implementation Steps

### Step 1: Build Email Service Infrastructure

**Objective:** Create core email service abstraction and SendGrid implementation

**Tasks:**
- Create `IEmailService` interface in [`Barberly.Application/Interfaces/IEmailService.cs`](../../backend/src/Barberly.Application/Interfaces/IEmailService.cs) with:
  - `SendAppointmentConfirmationAsync(Guid appointmentId, CancellationToken ct)`
  - `SendAppointmentReminderAsync(Guid appointmentId, CancellationToken ct)`
  - `SendAppointmentCancellationAsync(Guid appointmentId, CancellationToken ct)`
  - Return type: `Task<SendEmailResult>` where `SendEmailResult(bool Success, string? MessageId, string? ErrorMessage)`

- Implement [`SendGridEmailService`](../../backend/src/Barberly.Infrastructure/Services/SendGridEmailService.cs) that:
  - Queries appointment/barber/shop data via repositories
  - Sends via SendGrid client using dynamic templates
  - Returns success/failure with message ID or error details

- Install `SendGrid` NuGet package (v9.29+) in `Barberly.Infrastructure.csproj`

- Configure SendGrid settings:
  - API key in User Secrets (development) and Azure Key Vault (production)
  - From email address (e.g., `noreply@barberly.com`)
  - Template IDs for confirmation, reminder, and cancellation emails
  - Add to `appsettings.json` structure

- Register `IEmailService` as scoped service in [`Program.cs`](../../backend/src/Barberly.Api/Program.cs)

**Deliverable:** Email service ready to send emails via SendGrid

---

### Step 2: Create Dead-Letter Queue Infrastructure

**Objective:** Implement retry mechanism for failed email sends

**Tasks:**
- Create [`EmailFailureQueue`](../../backend/src/Barberly.Domain/Entities/EmailFailureQueue.cs) entity with fields:
  - `Id` (Guid, PK)
  - `AppointmentId` (Guid, FK)
  - `EmailType` (enum: Confirmation/Reminder/Cancellation)
  - `RecipientEmail` (string, max 200 chars)
  - `ErrorMessage` (string, max 1000 chars)
  - `RetryCount` (int, default 0)
  - `NextRetryAt` (DateTimeOffset, calculated with exponential backoff)
  - `CreatedAt` (DateTimeOffset)
  - `LastAttemptAt` (DateTimeOffset)

- Create EF Core migration: `dotnet ef migrations add AddEmailFailureQueue`

- Create [`IEmailFailureQueueRepository`](../../backend/src/Barberly.Application/Interfaces/IEmailFailureQueueRepository.cs) interface with methods:
  - `AddAsync(EmailFailureQueue item, CancellationToken ct)`
  - `GetDueForRetryAsync(CancellationToken ct)` - returns items where `NextRetryAt <= UtcNow` and `RetryCount < 3`
  - `UpdateAsync(EmailFailureQueue item, CancellationToken ct)`
  - `RemoveAsync(Guid id, CancellationToken ct)`

- Implement repository in [`EmailFailureQueueRepository.cs`](../../backend/src/Barberly.Infrastructure/Persistence/EmailFailureQueueRepository.cs)

- Exponential backoff calculation:
  - Retry 1: 5 minutes
  - Retry 2: 30 minutes
  - Retry 3: 2 hours
  - After 3 attempts: Remove from queue (log only)

**Deliverable:** Dead-letter queue infrastructure ready for failed email retry

---

### Step 3: Trigger Emails with Dead-Letter Fallback

**Objective:** Integrate email sending into appointment lifecycle

**Tasks:**
- Update [`CreateAppointment`](../../backend/src/Barberly.Api/Endpoints/SchedulingEndpoints.cs#L104) handler:
  - Call `SendAppointmentConfirmationAsync` after successful booking
  - On failure, add to `EmailFailureQueue` with `RetryCount = 0` and `NextRetryAt = UtcNow + 5 minutes`
  - Log error but don't block appointment creation

- Update [`CancelAppointment`](../../backend/src/Barberly.Api/Endpoints/SchedulingEndpoints.cs#L155) handler:
  - Call `SendAppointmentCancellationAsync` with refund eligibility flag
  - Calculate refund eligibility using `appointment.IsEligibleForRefund()` method
  - On failure, queue to dead-letter
  - Log error but don't block cancellation

- Add optional `CancellationReason` string parameter to `CancelAppointmentRequest` model
  - Pass to `Cancel(reason)` method
  - Include in cancellation email if provided

- Wrap all email calls in try-catch blocks with detailed logging

**Deliverable:** Emails sent on appointment creation and cancellation with fallback retry

---

### Step 4: Create GitHub Actions Scheduled Jobs

**Objective:** Implement background jobs for reminders and retry logic using free GitHub Actions

**Tasks:**
- Create [`.github/workflows/email-reminder-job.yml`](../../.github/workflows/email-reminder-job.yml):
  - Schedule: Runs hourly via cron (`0 * * * *`)
  - Triggers API endpoint: `POST /api/v1/jobs/send-reminders`
  - Authentication: Uses shared secret from GitHub repository secrets
  - Timeout: 5 minutes max

- Create API endpoint `/api/v1/jobs/send-reminders` in new [`JobEndpoints.cs`](../../backend/src/Barberly.Api/Endpoints/JobEndpoints.cs):
  - Requires API key authentication (separate from JWT)
  - Queries appointments where:
    - `Start` between 23-24 hours from now
    - `ReminderSent = false`
    - `IsCancelled = false`
  - Sends reminder emails via `SendAppointmentReminderAsync`
  - Marks `ReminderSent = true` on success
  - Queues to dead-letter on failure
  - Returns summary: `{ sent: 10, failed: 2 }`

- Create [`.github/workflows/email-retry-job.yml`](../../.github/workflows/email-retry-job.yml):
  - Schedule: Runs every 5 minutes via cron (`*/5 * * * *`)
  - Triggers API endpoint: `POST /api/v1/jobs/retry-failed-emails`
  - Authentication: Same API key as reminder job
  - Timeout: 5 minutes max

- Create API endpoint `/api/v1/jobs/retry-failed-emails`:
  - Queries `EmailFailureQueue` where `NextRetryAt <= UtcNow` and `RetryCount < 3`
  - Retries sending email
  - On success: Remove from queue
  - On failure: Increment `RetryCount`, calculate next retry time with exponential backoff
  - After 3 attempts: Remove from queue and log permanently failed
  - Returns summary: `{ retried: 5, succeeded: 3, permanentlyFailed: 2 }`

- Add API key middleware for job endpoints:
  - Check `X-Job-Api-Key` header
  - Compare with secret from configuration
  - Return 401 if invalid

- Exempt job endpoints from rate limiting

**Deliverable:** Automated reminder sending and email retry via GitHub Actions

---

### Step 5: Enhance Appointment Domain & API

**Objective:** Add cancellation policy enforcement and appointment tracking

**Tasks:**
- Update [`Appointment`](../../backend/src/Barberly.Domain/Entities/Appointment.cs) entity:
  - Add `ReminderSent` boolean property (default false)
  - Add optional `CancellationReason` string property (max 500 chars, nullable)
  - Add `IsEligibleForRefund()` method:
    - Returns `true` if `Start - CancelledAtUtc >= TimeSpan.FromHours(24)`
    - Returns `false` otherwise
  - Update `Cancel(string? reason)` method signature to accept optional reason parameter
  - Store reason in `CancellationReason` property

- Create EF Core migration:
  ```bash
  dotnet ef migrations add AddReminderAndCancellationFields --project Barberly.Infrastructure --startup-project Barberly.Api
  ```

- Update `GET /appointments/{id}` endpoint response to include:
  - `isEligibleForRefund` (calculated property)
  - `cancellationReason` (if cancelled)
  - `reminderSent` (boolean)

- Create new endpoint `GET /api/v1/users/{id}/appointments` in [`SchedulingEndpoints.cs`](../../backend/src/Barberly.Api/Endpoints/SchedulingEndpoints.cs):
  - Requires authentication (user can only view their own appointments)
  - Filter by last 12 months: `CreatedAtUtc >= DateTime.UtcNow.AddMonths(-12)`
  - Return all appointments sorted by `Start` descending
  - Include related data: barber name, shop name, service name

- Create query handler for user appointments in Application layer

**Deliverable:** Enhanced domain model with cancellation policy and user appointment history

---

### Step 6: Build User Profile & Appointment History UI

**Objective:** Create customer-facing UI for managing appointments

**Tasks:**
- Create [`ProfilePage.tsx`](../../web/barberly-web/src/pages/ProfilePage.tsx):
  - Display user information (name, email)
  - Two tabs: "Upcoming Appointments" and "Past Appointments"
  - Upcoming: appointments where `Start > now && !IsCancelled`
  - Past: all other appointments (cancelled or completed)

- Implement `useUserAppointments(userId)` hook in [`hooks.ts`](../../web/barberly-web/src/lib/api/hooks.ts):
  - Fetches from `GET /api/v1/users/{id}/appointments`
  - Separates into upcoming and past arrays
  - Caches with TanStack Query
  - Query key: `['users', userId, 'appointments']`

- Create appointment card component showing:
  - Barber name and profile image
  - Service name
  - Date and time (formatted)
  - Shop name and address
  - Status badge (Upcoming/Cancelled/Completed)

- Add "Cancel Appointment" button on upcoming appointments:
  - Shows refund eligibility notice based on 24-hour rule
  - Optional textarea for cancellation reason
  - Confirmation dialog before cancelling
  - Disables button and shows loading state during mutation

- Implement `useCancelAppointment()` mutation hook:
  - Accepts `{ appointmentId: string, reason?: string }`
  - Calls `DELETE /api/v1/appointments/{id}` with optional body
  - On success: Shows success toast and invalidates queries
  - Invalidates: `['users', userId, 'appointments']` and `['appointments', appointmentId]`
  - Optimistic update: Immediately marks as cancelled in UI

- Show past appointments as read-only:
  - Display "Cancelled" badge if `isCancelled = true`
  - Display "Completed" badge if `Start < now && !isCancelled`
  - No action buttons

- Add route in App.tsx: `/profile` -> `<ProfilePage />`

**Deliverable:** Complete user profile page with appointment management

---

## Configuration Details

### SendGrid Configuration

**appsettings.json:**
```json
{
  "SendGrid": {
    "ApiKey": "",
    "FromEmail": "noreply@barberly.com",
    "FromName": "Barberly",
    "Templates": {
      "AppointmentConfirmation": "d-xxxxxxxxxxxxxxxxxxxxx",
      "AppointmentReminder": "d-xxxxxxxxxxxxxxxxxxxxx",
      "AppointmentCancellation": "d-xxxxxxxxxxxxxxxxxxxxx"
    }
  }
}
```

**User Secrets (Development):**
```json
{
  "SendGrid": {
    "ApiKey": "SG.xxxxxxxxxxxxxxxxxxxxx"
  },
  "JobApiKey": "dev-job-secret-key-change-in-production"
}
```

### SendGrid Template Variables

**Confirmation Template:**
- `{{customer_name}}` - Full name of customer
- `{{barber_name}}` - Full name of barber
- `{{shop_name}}` - Name of barber shop
- `{{shop_address}}` - Full shop address
- `{{shop_phone}}` - Shop contact phone
- `{{appointment_date}}` - Formatted date (e.g., "Monday, October 30, 2025")
- `{{appointment_time}}` - Formatted time (e.g., "2:00 PM")
- `{{service_name}}` - Service being booked
- `{{service_duration}}` - Duration in minutes
- `{{cancellation_policy}}` - "Free cancellation up to 24 hours before appointment"

**Reminder Template:**
- `{{customer_name}}`
- `{{barber_name}}`
- `{{shop_name}}`
- `{{shop_address}}`
- `{{appointment_date}}` - Tomorrow's date
- `{{appointment_time}}`

**Cancellation Template:**
- `{{customer_name}}`
- `{{barber_name}}`
- `{{appointment_date}}` - Original appointment date
- `{{appointment_time}}` - Original appointment time
- `{{is_refund_eligible}}` - "Yes" or "No"
- `{{cancellation_reason}}` - Optional reason provided by customer

### GitHub Actions Configuration

**Repository Secrets to Add:**
- `JOB_API_KEY` - Shared secret for job endpoint authentication
- `API_BASE_URL` - Production API URL (e.g., `https://api.barberly.com`)

**Workflow Files:**
- `.github/workflows/email-reminder-job.yml`
- `.github/workflows/email-retry-job.yml`

---

## Open Questions

### 1. GitHub Actions API Auth?
**Options:**
- Use shared secret in repository secrets ✅ **SELECTED**
- Bearer token
- IP whitelist only

### 2. Job Endpoint Rate Limiting?
**Options:**
- Exempt job endpoints from global rate limit ✅ **SELECTED**
- Separate stricter limit
- Same 100/min limit

### 3. Email Template Hosting?
**Options:**
- Create templates in SendGrid dashboard first ✅ **SELECTED**
- Use inline HTML in code
- Mix of both

---

## Testing Strategy

### Unit Tests
- `SendGridEmailService` - Mock SendGrid client, verify template data
- `Appointment.IsEligibleForRefund()` - Test 24-hour boundary conditions
- `Appointment.Cancel(reason)` - Verify reason storage

### Integration Tests
- `POST /api/v1/jobs/send-reminders` - Verify reminder logic and query
- `POST /api/v1/jobs/retry-failed-emails` - Verify exponential backoff
- `GET /api/v1/users/{id}/appointments` - Verify 12-month filter
- Email sending with dead-letter queue fallback

### E2E Tests (Playwright)
- Book appointment -> Verify confirmation email sent (mock)
- Cancel appointment within 24h -> Verify refund eligible
- Cancel appointment after 24h -> Verify not refund eligible
- View appointment history -> Verify upcoming/past separation

---

## Success Metrics

- **Email Delivery Rate:** Target 98%+ (monitor SendGrid analytics)
- **Dead-Letter Queue Success Rate:** 80%+ emails succeed within 3 retries
- **Reminder Send Rate:** 100% of eligible appointments get reminders 24h before
- **Customer Satisfaction:** Reduction in "Did I book successfully?" support inquiries
- **No-Show Rate:** Expected 30-50% reduction with reminders
- **Cancellation Policy Compliance:** 100% accurate refund eligibility calculation

---

## Timeline Estimate

- **Step 1 (Email Service):** 2 days
- **Step 2 (Dead-Letter Queue):** 1 day
- **Step 3 (Email Triggers):** 1 day
- **Step 4 (GitHub Actions Jobs):** 2 days
- **Step 5 (Domain Enhancements):** 1 day
- **Step 6 (User Profile UI):** 2 days
- **Testing & Bug Fixes:** 2 days

**Total:** ~11 days (2 weeks with buffer)

---

## Dependencies

### NuGet Packages
- `SendGrid` (v9.29.0+) - Email sending
- `Hangfire` (optional, if not using GitHub Actions) - Background jobs

### External Services
- SendGrid account with API key
- GitHub Actions (free tier sufficient)

### Infrastructure
- PostgreSQL database (already configured)
- Azure Key Vault (production only, for secrets)

---

## Deployment Checklist

- [ ] Create SendGrid account and verify sender email
- [ ] Create all 3 email templates in SendGrid dashboard
- [ ] Test templates with sample data
- [ ] Store SendGrid API key in Azure Key Vault (production)
- [ ] Configure repository secrets in GitHub (`JOB_API_KEY`, `API_BASE_URL`)
- [ ] Run database migrations in production
- [ ] Deploy API with new endpoints
- [ ] Test GitHub Actions workflows manually
- [ ] Monitor dead-letter queue for first 48 hours
- [ ] Verify first reminder emails sent successfully

---

**End of Plan**
