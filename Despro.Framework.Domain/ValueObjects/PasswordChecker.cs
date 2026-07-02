using Despro.Framework.Domain.DomainExceptions;
using Despro.Framework.Domain.ValueObjects.Auth;

namespace Despro.Framework.Domain.ValueObjects;

public static class PasswordChecker
{
    public static bool ValidatePassword(this AuthPasswordOptions options, string password)
    {
        List<string>? errors = null;

        if (string.IsNullOrWhiteSpace(password) || password.Length < options.RequiredLength)
        {
            errors ??= [];
            errors.Add($"رمز عبور باید حداقل {options.RequiredLength} کاراکتر باشد");
        }
        if (options.RequireNonAlphanumeric && password.All(IsLetterOrDigit))
        {
            errors ??= [];
            errors.Add("رمز عبور باید حداقل یک کاراکتر غیر الفبایی داشته باشد.");
        }
        if (options.RequireDigit && !password.Any(IsDigit))
        {
            errors ??= [];
            errors.Add("رمز عبور باید حداقل یک رقم ('0'-'9') داشته باشد.");
        }
        if (options.RequireLowercase && !password.Any(IsLower))
        {
            errors ??= [];
            errors.Add("رمز عبور باید حداقل یک حروف کوچک ('a'-'z') داشته باشد.");
        }
        if (options.RequireUppercase && !password.Any(IsUpper))
        {
            errors ??= [];
            errors.Add("رمز عبور باید حداقل یک حروف بزرگ ('A'-'Z') داشته باشد.");
        }
        if (options.RequiredUniqueChars >= 1 && password.Distinct().Count() < options.RequiredUniqueChars)
        {
            errors ??= [];
            errors.Add($"گذرواژه‌ها باید حداقل از {options.RequiredUniqueChars} نویسه مختلف استفاده کنند.");
        }

        if (!(errors?.Count > 0))
            return true;

        var allError = string.Join(", \n", errors);

        throw new InvalidValueObjectException(allError);
    }

    public static bool Validate(AuthPasswordOptions options, string password)
    {
        List<string>? errors = null;

        if (string.IsNullOrWhiteSpace(password) || password.Length < options.RequiredLength)
        {
            errors ??= [];
            errors.Add($"رمز عبور باید حداقل {options.RequiredLength} کاراکتر باشد");
        }
        if (options.RequireNonAlphanumeric && password.All(IsLetterOrDigit))
        {
            errors ??= [];
            errors.Add("رمز عبور باید حداقل یک کاراکتر غیر الفبایی داشته باشد.");
        }
        if (options.RequireDigit && !password.Any(IsDigit))
        {
            errors ??= [];
            errors.Add("رمز عبور باید حداقل یک رقم ('0'-'9') داشته باشد.");
        }
        if (options.RequireLowercase && !password.Any(IsLower))
        {
            errors ??= [];
            errors.Add("رمز عبور باید حداقل یک حروف کوچک ('a'-'z') داشته باشد.");
        }
        if (options.RequireUppercase && !password.Any(IsUpper))
        {
            errors ??= [];
            errors.Add("رمز عبور باید حداقل یک حروف بزرگ ('A'-'Z') داشته باشد.");
        }
        if (options.RequiredUniqueChars >= 1 && password.Distinct().Count() < options.RequiredUniqueChars)
        {
            errors ??= [];
            errors.Add($"گذرواژه‌ها باید حداقل از {options.RequiredUniqueChars} نویسه مختلف استفاده کنند.");
        }

        if (!(errors?.Count > 0))
            return true;

        var allError = string.Join(", \n", errors);

        throw new InvalidValueObjectException(allError);

    }

    private static bool IsDigit(char c)
    {
        return c is >= '0' and <= '9';
    }

    private static bool IsLower(char c)
    {
        return c is >= 'a' and <= 'z';
    }

    private static bool IsUpper(char c)
    {
        return c is >= 'A' and <= 'Z';
    }

    private static bool IsLetterOrDigit(char c)
    {
        return IsUpper(c) || IsLower(c) || IsDigit(c);
    }
}