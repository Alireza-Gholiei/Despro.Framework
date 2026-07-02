namespace Despro.Framework.Base.BaseExtensions;

public static class StringExtensions
{
    public static string ConvertNumPersianToEnglish(this string persianDigit)
    {
        var result = "";

        foreach (var t in persianDigit)
            result += t switch
            {
                '۰' => '0',
                '۱' => '1',
                '۲' => '2',
                '۳' => '3',
                '۴' => '4',
                '۵' => '5',
                '۶' => '6',
                '۷' => '7',
                '۸' => '8',
                '۹' => '9',
                _ => t
            };

        return result;
    }

    public static string ConvertArabicCharsToPersianChars(this string str)
    {
        return str.Replace("ﮎ", "ک").Replace("ﮏ", "ک").Replace("ﮐ", "ک").Replace("ﮑ", "ک").Replace("ك", "ک")
            .Replace("ي", "ی");
    }
}