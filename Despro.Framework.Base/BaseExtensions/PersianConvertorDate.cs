using System.Globalization;

namespace Despro.Framework.Base.BaseExtensions;

public static class PersianConvertorDate
{
    public static string ToShamsi(this DateTime value)
    {
        PersianCalendar pc = new();
        return (pc.GetYear(value).ToString().PadLeft(4, '0') + "/" + pc.GetMonth(value).ToString("00").PadLeft(2, '0') +
                "/" +
                pc.GetDayOfMonth(value).ToString("00").PadLeft(2, '0')).ConvertNumPersianToEnglish();
    }

    public static string ToShamsiWithTime(this DateTime value)
    {
        PersianCalendar pc = new();
        return pc.GetYear(value) + "/" + pc.GetMonth(value).ToString("00").PadLeft(2, '0') + "/" +
               pc.GetDayOfMonth(value).ToString("00").PadLeft(2, '0') + " " +
               pc.GetHour(value).ToString().PadLeft(2, '0') + ":" + pc.GetMinute(value).ToString().PadLeft(2, '0') +
               ":" + pc.GetSecond(value).ToString().PadLeft(2, '0');
    }

    public static string ToTime(this DateTime value)
    {
        PersianCalendar pc = new();
        return pc.GetHour(value) + ":" + pc.GetMinute(value) + ":" + pc.GetSecond(value);
    }

    public static DateTime ShamsiToMiladi(this string Date)
    {
        var date = Date.Replace("/", "").Replace("-", "");

        PersianCalendar pc = new();
        var year = int.Parse(date[..4]);
        var Month = int.Parse(date.Substring(4, 2));
        var Day = int.Parse(date.Substring(6, 2));
        DateTime dt = new(year, Month, Day, pc);
        return dt;
    }

    public static long GetCurrentSeasonfirstMonth(this DateTime value)
    {
        var ShamsiDate = value.ToShamsi();

        //Example(=> 1401/04/30 get shamsi month => ( ShamsiDate = 04)

        var ShamsiList = ShamsiDate.Split("/");

        if (ShamsiList[1] == "01" || ShamsiList[1] == "02" || ShamsiList[1] == "03")
        {
            ShamsiList[1] = "01";
            ShamsiList[2] = "01";
            ShamsiDate = ShamsiList[0] + "/" + ShamsiList[1] + "/" + ShamsiList[2];
            return ShamsiDate.ShamsiToMiladi().Ticks;
        }

        if (ShamsiList[1] == "04" || ShamsiList[1] == "05" || ShamsiList[1] == "06")
        {
            ShamsiList[1] = "04";
            ShamsiList[2] = "01";
            ShamsiDate = ShamsiList[0] + "/" + ShamsiList[1] + "/" + ShamsiList[2];
            return ShamsiDate.ShamsiToMiladi().Ticks;
        }

        if (ShamsiList[1] == "07" || ShamsiList[1] == "08" || ShamsiList[1] == "09")
        {
            ShamsiList[1] = "07";
            ShamsiList[2] = "01";
            ShamsiDate = ShamsiList[0] + "/" + ShamsiList[1] + "/" + ShamsiList[2];
            return ShamsiDate.ShamsiToMiladi().Ticks;
        }

        if (ShamsiList[1] == "10" || ShamsiList[1] == "11" || ShamsiList[1] == "12")
        {
            ShamsiList[1] = "10";
            ShamsiList[2] = "01";
            ShamsiDate = ShamsiList[0] + "/" + ShamsiList[1] + "/" + ShamsiList[2];
            return ShamsiDate.ShamsiToMiladi().Ticks;
        }

        return 0;
    }

    public static string ToShamsiWhitTime(this DateTime value)
    {
        PersianCalendar pc = new();
        return pc.GetHour(value) + ":" + pc.GetMinute(value) + ":" + pc.GetSecond(value) + " " + pc.GetYear(value) +
               "/" + pc.GetMonth(value).ToString("00") + "/" +
               pc.GetDayOfMonth(value).ToString("00");
    }

    public static string ToWhatch(this DateTime value)
    {
        PersianCalendar pc = new();
        return pc.GetHour(value) + ":" + pc.GetMinute(value) + ":" + pc.GetSecond(value);
    }

    public static DateTime? ShamsiToMiladi2(this string Date, string Time = "")
    {
        if (string.IsNullOrEmpty(Date)) return null;

        if (!Date.Contains($"/")) return Date.ShamsiToMiladi(Time);

        var dates = Date.Split("/");

        PersianCalendar pc = new();
        var year = int.Parse(dates[0]);
        var Month = int.Parse(dates[1]);
        var Day = int.Parse(dates[2]);
        var Hours = 0;
        var Minutes = 0;

        if (!string.IsNullOrEmpty(Time))
        {
            var Times = Time.Split(":").ToList();
            Hours = int.Parse(Times[0]);
            Minutes = int.Parse(Times[1]);
        }

        DateTime dt = new(year, Month, Day, Hours, Minutes, 0, pc);

        return dt;
    }

    public static DateTime ShamsiToMiladi(this string Date, string Time = "")
    {
        var date = Date.Replace("/", "").Replace("-", "");

        PersianCalendar pc = new();
        var year = int.Parse(date[..4]);
        var Month = int.Parse(date.Substring(4, 2));
        var Day = int.Parse(date.Substring(6, 2));
        var Hours = 0;
        var Minutes = 0;

        if (!string.IsNullOrEmpty(Time))
        {
            var Times = Time.Split(":").ToList();
            Hours = int.Parse(Times[0]);
            Minutes = int.Parse(Times[1]);
        }

        DateTime dt = new(year, Month, Day, Hours, Minutes, 0, pc);

        return dt;
    }

    public static string PersionDayOfWeek(this DateTime date)
    {
        return date.DayOfWeek switch
        {
            DayOfWeek.Saturday => "شنبه",
            DayOfWeek.Sunday => "یکشنبه",
            DayOfWeek.Monday => "دوشنبه",
            DayOfWeek.Tuesday => "سه شنبه",
            DayOfWeek.Wednesday => "چهارشنبه",
            DayOfWeek.Thursday => "پنج شنبه",
            DayOfWeek.Friday => "جمعه",
            _ => throw new Exception()
        };
    }

    public static long ToTimeTicks(this string date)
    {
        long Time = 0;

        Time += int.Parse(date[..2]) * TimeSpan.TicksPerHour;
        Time += int.Parse(date.Substring(3, 2)) * TimeSpan.TicksPerMinute;

        return Time;
    }

    public static string ToTimeString(this long date)
    {
        var Time = new DateTime(date).ToString("HH:mm");

        return Time;
    }

    public static long RemoveTimeFromDate(this long date)
    {
        DateTime dateTime = new(date);
        var T2 = DateTime.Parse(dateTime.ToShortDateString()).Ticks;

        return T2;
    }

    public static DateTime GetMiladiDate(int Year, int Month, int Day)
    {
        var _Month = Month.ToString();
        var _Day = Day.ToString();

        if (_Month.Length < 2) _Month = "0" + _Month;

        if (_Day.Length < 2) _Day = "0" + _Day;

        var PersianDate = Year + _Month + _Day;

        var time = PersianDate.ShamsiToMiladi();

        return time;
    }

    public static int ToWeekDayNumber(this long date)
    {
        DateTime dateTime = new(date);
        var DayOfWeek = dateTime.DayOfWeek;
        var Day = DayOfWeek.ToString();
        var dayNumber = (int)Enum.Parse(typeof(DayOfWeek), Day);

        return dayNumber;
    }

    public static string CurrentPMonth(this int month)
    {
        return month switch
        {
            1 => "فروردین",
            2 => "اردیبهشت",
            3 => "خرداد",
            4 => "تیر",
            5 => "مرداد",
            6 => "شهریور",
            7 => "مهر",
            8 => "آبان",
            9 => "آذر",
            10 => "دی",
            11 => "بهمن",
            12 => "اسفند",
            _ => ""
        };
    }

    public static long GetFirstDayInMonth(this DateTime value)
    {
        var pc = value.ToShamsi();
        var first = pc[..8] + "01";
        return first.ShamsiToMiladi().Ticks;
    }

    public static long GetLastDayInMonth(this DateTime value)
    {
        var pc = value.ToShamsi();
        var end = "";

        var month = Convert.ToInt32(pc.Substring(5, 2));

        end = month switch
        {
            <= 6 => pc[..8] + "31",
            > 6 and < 12 => pc[..8] + "30",
            _ => end
        };

        PersianCalendar PersianCalendar = new();

        if (PersianCalendar.IsLeapYear(Convert.ToInt32(pc[..2])))
        {
            if (month == 12) end = pc[..8] + "30";
        }
        else
        {
            if (month == 12) end = pc[..8] + "29";
        }

        return end.ShamsiToMiladi().Ticks;
    }

    public static string ToPersianDayOfWeek(this DayOfWeek value)
    {
        return value switch
        {
            DayOfWeek.Sunday => "یکشنبه",
            DayOfWeek.Monday => "دوشنبه",
            DayOfWeek.Tuesday => "سه شنبه",
            DayOfWeek.Wednesday => "چهارشنبه",
            DayOfWeek.Thursday => "پنجشنبه",
            DayOfWeek.Friday => "جمعه",
            DayOfWeek.Saturday => "شنبه",
            _ => string.Empty
        };
    }
}