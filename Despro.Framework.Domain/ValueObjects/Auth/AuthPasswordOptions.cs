using System.ComponentModel.DataAnnotations;

namespace Despro.Framework.Domain.ValueObjects.Auth;

public class AuthPasswordOptions
{
    public const string ConfigName = "AuthPasswordOptions";

    [Range(4, 128, ErrorMessage = "حداقل طول رمز عبور باید بین 4 تا 128 باشد.")]
    public int RequiredLength { get; set; } = 6;
    [Range(1, 128, ErrorMessage = "تعداد نویسه‌های متمایز باید حداقل 1 باشد.")]
    public int RequiredUniqueChars { get; set; } = 1;
    public bool RequireNonAlphanumeric { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireDigit { get; set; } = true;
}