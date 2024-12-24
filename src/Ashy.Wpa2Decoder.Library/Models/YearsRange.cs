namespace Ashy.Wpa2Decoder.Library.Models;

public class YearsRange
{
    public YearsRange(string yearsRangeString)
    {
        YearsRangeString = yearsRangeString;
    }

    public string YearsRangeString { get; init; }
    public bool YearsRangeSpecified => !string.IsNullOrWhiteSpace(YearsRangeString);
    public (int start, int end) YearsRangeRange {
        get
        {
            if (YearsRangeSpecified)
            {
                return TryParseRange(YearsRangeString, out var start, out var end) 
                    ? (start, end)
                    : (-1, -1);
            }
            return (-1, -1);
        }
    }

    public string[] GetYearsArray()
    {
        if (!YearsRangeSpecified || YearsRangeRange == (-1, -1))
        {
            return [];
        }
        var result = new List<string>();
        for (int year = YearsRangeRange.start; year <= YearsRangeRange.end; year++)
        {
            result.Add(year.ToString());
        }
        return result.ToArray();
    }
        
    private static bool TryParseRange(string range, out int start, out int end)
    {
        start = 0;
        end = 0;

        // Split the range string by the hyphen
        var parts = range.Split('-');
        if (parts.Length == 2 && int.TryParse(parts[0], out start) && int.TryParse(parts[1], out end))
        {
            return true;
        }

        return false;
    }
}