using Despro.Framework.Domain.DomainExceptions;
using Despro.Framework.Domain.ValueObjects.Auth;
using System.Text;

namespace Despro.Framework.Domain.ValueObjects;

public class PasswordGenerator
{
    private static readonly Random _random = new();

    private const string LowercaseChars = "abcdefghijklmnopqrstuvwxyz";
    private const string UppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string DigitChars = "0123456789";
    private const string NonAlphanumericChars = "!@#$%?_-&";
    //private const string NonAlphanumericChars = "!@#$%^&*()-_=+[]{}<>?";

    public static string Generate(AuthPasswordOptions options)
    {
        if (options is null)
            throw new InvalidValueObjectException(nameof(options));

        if (options.RequiredLength < 1)
            throw new InvalidValueObjectException("Password length must be greater than zero.");

        var allChars = new StringBuilder();

        if (options.RequireLowercase) allChars.Append(LowercaseChars);
        if (options.RequireUppercase) allChars.Append(UppercaseChars);
        if (options.RequireDigit) allChars.Append(DigitChars);
        if (options.RequireNonAlphanumeric) allChars.Append(NonAlphanumericChars);

        if (allChars.Length == 0)
            allChars.Append(LowercaseChars);

        var passwordChars = new List<char>();

        if (options.RequireLowercase)
            passwordChars.Add(GetRandomChar(LowercaseChars));
        if (options.RequireUppercase)
            passwordChars.Add(GetRandomChar(UppercaseChars));
        if (options.RequireDigit)
            passwordChars.Add(GetRandomChar(DigitChars));
        if (options.RequireNonAlphanumeric)
            passwordChars.Add(GetRandomChar(NonAlphanumericChars));

        while (passwordChars.Count < options.RequiredLength)
        {
            passwordChars.Add(GetRandomChar(allChars.ToString()));
        }

        passwordChars = EnsureUniqueChars(passwordChars, options, allChars.ToString());

        Shuffle(passwordChars);

        return new string(passwordChars.ToArray());
    }

    private static char GetRandomChar(string chars)
        => chars[_random.Next(chars.Length)];

    private static void Shuffle<T>(IList<T> list)
    {
        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = _random.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private static List<char> EnsureUniqueChars(List<char> chars, AuthPasswordOptions options, string allChars)
    {
        var unique = chars.Distinct().ToList();

        while (unique.Count < options.RequiredUniqueChars && unique.Count < allChars.Length)
        {
            var c = GetRandomChar(allChars);
            if (!unique.Contains(c))
                unique.Add(c);
        }

        while (unique.Count < options.RequiredLength)
            unique.Add(GetRandomChar(allChars));

        return unique;
    }

    public static string CreateRandomPassword(AuthPasswordOptions? opts = null)
    {
        opts ??= new AuthPasswordOptions()
        {
            RequiredLength = 8,
            RequiredUniqueChars = 0,
            RequireDigit = true,
            RequireLowercase = false,
            RequireUppercase = false,
            RequireNonAlphanumeric = false,
        };

        var randomChars = new[] {
            "ABCDEFGHJKLMNOPQRSTUVWXYZ",
            "abcdefghijkmnopqrstuvwxyz",
            "0123456789",
            "!@$?_-&"
        };

        Random rand = new(Environment.TickCount);
        List<char> chars = [];

        if (opts.RequireUppercase)
            chars.Insert(rand.Next(0, chars.Count),
                randomChars[0][rand.Next(0, randomChars[0].Length)]);

        if (opts.RequireLowercase)
            chars.Insert(rand.Next(0, chars.Count),
                randomChars[1][rand.Next(0, randomChars[1].Length)]);

        if (opts.RequireDigit)
            chars.Insert(rand.Next(0, chars.Count),
                randomChars[2][rand.Next(0, randomChars[2].Length)]);

        if (opts.RequireNonAlphanumeric)
            chars.Insert(rand.Next(0, chars.Count),
                randomChars[3][rand.Next(0, randomChars[3].Length)]);

        for (var i = chars.Count; i < opts.RequiredLength || chars.Distinct().Count() < opts.RequiredUniqueChars; i++)
        {
            if (opts.RequireUppercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[0][rand.Next(0, randomChars[0].Length)]);

            if (opts.RequireLowercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[1][rand.Next(0, randomChars[1].Length)]);

            if (opts.RequireDigit)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[2][rand.Next(0, randomChars[2].Length)]);

            if (opts.RequireNonAlphanumeric)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[3][rand.Next(0, randomChars[3].Length)]);
            //string rcs = randomChars[rand.Next(0, randomChars.Length)];
            //chars.Insert(rand.Next(0, chars.Count),
            //    rcs[rand.Next(0, rcs.Length)]);
        }

        return new string(chars.ToArray());
    }
}