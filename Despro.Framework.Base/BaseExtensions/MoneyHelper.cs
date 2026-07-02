namespace Despro.Framework.Base.BaseExtensions;

public static class MoneyHelper
{
    public static string MoneyTooman(this int price)
    {
        return $"{price:#,0} تومان";
    }

    public static string MoneyTooman(this int? price)
    {
        return $"{price:#,0} تومان";
    }

    public static string MoneySplitNumber(this int price)
    {
        return $"{price:#,0}";
    }

    public static string MoneySplitNumber(this int? price)
    {
        return $"{price:#,0}";
    }
}