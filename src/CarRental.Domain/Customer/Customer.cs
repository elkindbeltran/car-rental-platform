using CarRental.SharedKernel.Domain;
using CarRental.SharedKernel.Exceptions;
using System.Net.Mail;

namespace CarRental.Domain.Customer;

public sealed class Customer : AggregateRoot<Guid>, IAuditableEntity, IConcurrencyTracked, ISoftDeletable
{
    public const int MaximumNameLength = 100;
    public const int MaximumEmailLength = 256;
    public const int MaximumPhoneLength = 32;
    public const int MaximumExternalUserIdLength = 256;

    private Customer(Guid id, string firstName, string lastName, string email, string? phone) : base(id)
    {
        SetDetails(firstName, lastName, email, phone);
        IsActive = true;
    }

    private Customer() { }

    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public string? ExternalUserId { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? LastModifiedAtUtc { get; set; }
    public string? LastModifiedBy { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }

    public static Customer Create(Guid id, string firstName, string lastName, string email, string? phone)
    {
        if (id == Guid.Empty)
        {
            throw new BusinessException("Customer.InvalidId", "A customer identifier is required.");
        }

        return new Customer(id, firstName, lastName, email, phone);
    }

    public void UpdateDetails(string firstName, string lastName, string email, string? phone) =>
        SetDetails(firstName, lastName, email, phone);

    public void Deactivate() => IsActive = false;

    public void LinkToUser(string externalUserId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(externalUserId);
        if (externalUserId.Trim().Length > MaximumExternalUserIdLength)
        {
            throw new BusinessException("Customer.InvalidExternalUserId", "The external user identifier is too long.");
        }

        if (ExternalUserId is not null && !string.Equals(ExternalUserId, externalUserId.Trim(), StringComparison.Ordinal))
        {
            throw new BusinessException("Customer.AlreadyLinked", "The customer is already linked to another user.");
        }

        ExternalUserId = externalUserId.Trim();
    }

    private void SetDetails(string firstName, string lastName, string email, string? phone)
    {
        FirstName = NormalizeRequired(firstName, MaximumNameLength, "first name");
        LastName = NormalizeRequired(lastName, MaximumNameLength, "last name");
        var normalizedEmail = NormalizeRequired(email, MaximumEmailLength, "email").ToLowerInvariant();
        if (!MailAddress.TryCreate(normalizedEmail, out var parsedEmail) ||
            !string.Equals(parsedEmail.Address, normalizedEmail, StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException("Customer.InvalidEmail", "A valid customer email address is required.");
        }

        Email = normalizedEmail;

        var normalizedPhone = phone?.Trim();
        if (normalizedPhone?.Length > MaximumPhoneLength)
        {
            throw new BusinessException("Customer.InvalidPhone", $"Phone cannot exceed {MaximumPhoneLength} characters.");
        }

        Phone = string.IsNullOrEmpty(normalizedPhone) ? null : normalizedPhone;
    }

    private static string NormalizeRequired(string value, int maximumLength, string fieldName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        var normalized = value.Trim();
        if (normalized.Length > maximumLength)
        {
            throw new BusinessException("Customer.InvalidValue", $"Customer {fieldName} cannot exceed {maximumLength} characters.");
        }

        return normalized;
    }
}
