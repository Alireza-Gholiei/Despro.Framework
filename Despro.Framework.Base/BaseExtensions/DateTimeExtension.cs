namespace Despro.Framework.Base.BaseExtensions;

public static class DateTimeExtension
{
    public static bool IsBetween(this DateTime date, DateTime startDate, DateTime endDate, bool compareTime = false)
    {
        if (!compareTime)
        {
            if (date.Date >= startDate.Date)
            {
                return date.Date <= endDate.Date;
            }

            return false;
        }

        if (date >= startDate)
        {
            return date <= endDate;
        }

        return false;
    }

    public static bool IsBetween(this DateTime date, long startDate, long endDate, bool compareTime = false)
    {
        var startDateConvert = new DateTime(startDate);
        var endDateConvert = new DateTime(endDate);

        if (!compareTime)
        {
            if (date.Date >= startDateConvert.Date)
            {
                return date.Date <= endDateConvert.Date;
            }

            return false;
        }

        if (date >= startDateConvert)
        {
            return date <= endDateConvert;
        }

        return false;
    }

    public static bool IsBetween(this long date, long startDate, long endDate, bool compareTime = false)
    {
        var dateConvert = new DateTime(date);
        var startDateConvert = new DateTime(startDate);
        var endDateConvert = new DateTime(endDate);

        if (!compareTime)
        {
            if (dateConvert.Date >= startDateConvert.Date)
            {
                return dateConvert.Date <= endDateConvert.Date;
            }

            return false;
        }

        if (dateConvert >= startDateConvert)
        {
            return dateConvert <= endDateConvert;
        }

        return false;
    }
}