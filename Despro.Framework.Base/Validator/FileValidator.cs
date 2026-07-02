using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Despro.Framework.Base.Validator;

public class FileValidator : AbstractValidator<IFormFile>
{
    private readonly string[] _allowedExtensions = [".jpg", ".png", ".jpeg", ".pdf"];
    private const long MaxFileSize = 10 * 1024 * 1024;

    /// <summary>
    /// use this constructor for default settings
    /// </summary>
    public FileValidator(bool isRequired = true)
    {
        ApplyRules(MaxFileSize, _allowedExtensions, isRequired);
    }

    /// <summary>
    /// use this constructor to set custom max file size
    /// </summary>
    /// <param name="maxFileSize">Mb</param>
    /// <param name="isRequired"></param>
    public FileValidator(long maxFileSize, bool isRequired = true)
    {
        ApplyRules(maxFileSize * 1024 * 1024, _allowedExtensions, isRequired);
    }

    /// <summary>
    /// use this constructor to set custom allowed extensions (Use ["*"] to allow all extensions)
    /// </summary>
    /// <param name="allowedExtensions"></param>
    /// <param name="isRequired"></param>
    public FileValidator(string[] allowedExtensions, bool isRequired = true)
    {
        ApplyRules(MaxFileSize, allowedExtensions, isRequired);
    }

    /// <summary>
    /// use this constructor to set custom allowed extensions and max file size (Use ["*"] to allow all extensions)
    /// </summary>
    /// <param name="maxFileSize">Mb</param>
    /// <param name="allowedExtensions"></param>
    /// <param name="isRequired"></param>
    public FileValidator(long maxFileSize, string[] allowedExtensions, bool isRequired = true)
    {
        ApplyRules(maxFileSize * 1024 * 1024, allowedExtensions, isRequired);
    }

    private void ApplyRules(long maxBytes, string[] extensions, bool isRequired)
    {
        RuleFor(x => x)
            .Cascade(CascadeMode.Stop)
            .Custom((file, context) =>
            {
                if (file == null || file.Length == 0)
                {
                    if (isRequired)
                    {
                        context.AddFailure("فایل الزامی است.");
                    }
                    return;
                }

                if (file.Length > maxBytes)
                {
                    context.AddFailure($"حجم فایل نباید بیشتر از {maxBytes / 1024 / 1024} مگابایت باشد.");
                    return;
                }

                if (extensions.Contains("*"))
                {
                    return;
                }

                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!extensions.Contains(ext))
                {
                    context.AddFailure($"فقط فرمت‌ های {string.Join(", ", extensions)} مجاز هستند.");
                }
            });
    }
}
