# Despro.Framework.Domain

> The **domain** layer of [Despro Framework](../README.md).

`Despro.Framework.Domain` builds on `Despro.Framework.Base` and provides the domain-modeling
building blocks and self-contained domain services: value objects, security primitives
(hashing, password policy) and domain-specific validators. It has no dependency on
infrastructure or presentation concerns.

- **Package:** `Despro.Framework.Domain`
- **Version:** `2.0.4`
- **Target framework:** `net10.0`
- **Depends on:** `Despro.Framework.Base`

---

## Installation

```bash
dotnet add package Despro.Framework.Domain
```

### DI registration

```csharp
using Despro.Framework.Domain;

builder.Services.AddFrameworkDomain(builder.Configuration);
```

`AddFrameworkDomain` binds the `AuthPasswordOptions` configuration section, validates its
data annotations, and additionally enforces `RequiredUniqueChars <= RequiredLength`.
Because validation runs with `ValidateOnStart()`, a misconfigured password policy fails the
application at boot instead of at first use.

---

## What's inside

### Value objects (`ValueObjects`)

| Type | Description |
| --- | --- |
| `ValueObject` | Abstract base implementing structural equality over public properties/fields via reflection. Members can be excluded with `[IgnoreMember]`. |
| `AuthPasswordOptions` | Password-policy options (`RequiredLength` 4–128, `RequiredUniqueChars`, `RequireDigit/Lowercase/Uppercase/NonAlphanumeric`). Bound from configuration section `AuthPasswordOptions`. |
| `PasswordChecker` | Validates a password against `AuthPasswordOptions` (`ValidatePassword` extension and static `Validate`), throwing `InvalidValueObjectException` with localized messages on failure. |
| `PasswordGenerator` | Generates policy-compliant passwords (`Generate`, `CreateRandomPassword`). |
| `IranianNationalCodeChecker` | `IsValid(nationalId)` checksum validation for Iranian national codes. |
| `TextHelper` | String utilities: `ToSlug`, `Subscribe` (truncate), `ConvertHtmlToText`, `SetUnReadableEmail`, `GenerateCode`, `IsText`, `IsUniCode`. |

### Security tools (`SecurityTools`)

- `Hasher` — SHA-256 helpers: `HashPassword(username, password)`, `GetHash(text)`,
  `GetHash(object)` (property-aware object hashing).

### Exceptions (`DomainExceptions`)

- `BaseDomainException` (abstract, extends `Base`'s `BaseException`).
- `InvalidValueObjectException` — thrown by value-object/password validation failures.

---

## Usage

```csharp
// A value object with structural equality:
public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }
    public Money(decimal amount, string currency) => (Amount, Currency) = (amount, currency);
}

// Password policy (bound + validated by AddFrameworkDomain):
public class ChangePasswordHandler(IOptions<AuthPasswordOptions> options)
{
    public void Handle(string newPassword)
    {
        options.Value.ValidatePassword(newPassword); // throws InvalidValueObjectException if weak
        var suggestion = PasswordGenerator.Generate(options.Value);
        // ...
    }
}
```

Example configuration:

```json
{
  "AuthPasswordOptions": {
    "RequiredLength": 8,
    "RequiredUniqueChars": 4,
    "RequireNonAlphanumeric": true,
    "RequireLowercase": true,
    "RequireUppercase": true,
    "RequireDigit": true
  }
}
```

---

## Project structure

```
Despro.Framework.Domain
├── DomainExceptions/    # BaseDomainException, InvalidValueObjectException
├── SecurityTools/       # Hasher (SHA-256)
├── ValueObjects/        # ValueObject, TextHelper, IranianNationalCodeChecker, Password*
│   └── Auth/            # AuthPasswordOptions
└── FrameworkDomainDi.cs # AddFrameworkDomain
```
