using System.Globalization;
using System.Reflection;

namespace Despro.Framework.Presentation.Utilites;

public static class DatePersian
{
    public static void InitializePersianCulture()
    {
        InitializeCulture("fa-ir", ["ی", "د", "س", "چ", "پ", "ج", "ش"],
            ["یکشنبه", "دوشنبه", "سه شنبه", "چهارشنبه", "پنجشنبه", "جمعه", "شنبه"],
            [
                "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی",
                "بهمن", "اسفند", ""
            ],
            [
                "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی",
                "بهمن", "اسفند", ""
            ], "ق.ظ. ", "ب.ظ. ", "yyyy/MM/dd", new PersianCalendar());
    }

    private static void InitializeCulture(string culture, string[] abbreviatedDayNames, string[] dayNames,
        string[] abbreviatedMonthNames, string[] monthNames, string amDesignator,
        string pmDesignator, string shortDatePattern, Calendar calendar)
    {
        CultureInfo calture = new(culture);
        var info = calture.DateTimeFormat;
        info.AbbreviatedDayNames = abbreviatedDayNames;
        info.DayNames = dayNames;
        info.AbbreviatedMonthNames = abbreviatedMonthNames;
        info.MonthNames = monthNames;
        info.AMDesignator = amDesignator;
        info.PMDesignator = pmDesignator;
        info.ShortDatePattern = shortDatePattern;
        info.FirstDayOfWeek = DayOfWeek.Saturday;
        var cal = calendar;
        var type = typeof(DateTimeFormatInfo);
        var fieldInfo = type.GetField("calendar", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)!;
        fieldInfo?.SetValue(info, cal);
        var field = typeof(CultureInfo).GetField("calendar", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)!;
        field?.SetValue(calture, cal);
        Thread.CurrentThread.CurrentCulture = calture;
        Thread.CurrentThread.CurrentUICulture = calture;
        CultureInfo.CurrentCulture.DateTimeFormat = info;
        CultureInfo.CurrentUICulture.DateTimeFormat = info;
        CultureInfo cultureInfo = new("fa-IR")
        {
            NumberFormat =
            {
                CurrencySymbol = "ريال"
            }
        };

        CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
    }
}

public class PersianCulture : CultureInfo
{
    private readonly Calendar _calendar;
    private DateTimeFormatInfo _dateTimeFormatInfo;

    public PersianCulture() : base("fa-IR")
    {
        _calendar = new PersianCalendar();

        OptionalCalendars = new List<Calendar>
        {
            new PersianCalendar(),
            new GregorianCalendar()
        }.ToArray();

        var dateTimeFormatInfo = CreateSpecificCulture("fa-IR").DateTimeFormat;
        dateTimeFormatInfo.Calendar = _calendar;
        _dateTimeFormatInfo = dateTimeFormatInfo;
    }

    public override Calendar Calendar => _calendar;

    public override Calendar[] OptionalCalendars { get; }

    public override DateTimeFormatInfo DateTimeFormat
    {
        get => _dateTimeFormatInfo;
        set => _dateTimeFormatInfo = value;
    }
}