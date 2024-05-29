using AJ.Engine.Interfaces.Util.Strings;
using AJ.Logging.Interfaces;
using System;
using System.Collections.Generic;

namespace AJ.LoggingV2
{
    public static class TitleFormatter
    {
        private static readonly string[] NUMBERS_DECA = new string[]
        {
            "00",
            "01",
            "02",
            "03",
            "04",
            "05",
            "06",
            "07",
            "08",
            "09",
            "10",
            "11",
            "12",
            "13",
            "14",
            "15",
            "16",
            "17",
            "18",
            "19",
            "20",
            "21",
            "22",
            "23",
            "24",
            "25",
            "26",
            "27",
            "28",
            "29",
            "30",
            "31",
            "32",
            "33",
            "34",
            "35",
            "36",
            "37",
            "38",
            "39",
            "40",
            "41",
            "42",
            "43",
            "44",
            "45",
            "46",
            "47",
            "48",
            "49",
            "50",
            "51",
            "52",
            "53",
            "54",
            "55",
            "56",
            "57",
            "58",
            "59",
        };
        private static readonly Dictionary<int, string> YEARS = new Dictionary<int, string>();

        private static readonly char[][] LOGTYPES_STRING = new char[][]
        {
            "INFO".ToCharArray(),
            "WARNING".ToCharArray(),
            "ERROR".ToCharArray(),
            "FATAL".ToCharArray(),
            "DEBUG".ToCharArray()
        };

        private static readonly int[] LOGTYPES_LENGTH = new int[]
        {
            4,
            7,
            5,
            5,
            5
        };

        private static readonly char[] DATETIME = "00/00/0000 00:00:00".ToCharArray();

        private const int DEFAULT_NUM_OF_CHARS = 6;

        public static int GetLengthInBytes(LogTypes logType, int titleSizeInBytes)
        {
            return (LOGTYPES_LENGTH[GetLogTypeIndex(logType)] + DATETIME.Length + DEFAULT_NUM_OF_CHARS + NewLine.Length) * 2 + titleSizeInBytes;
        }

        public static void SetTitle(Span<char> data, LogTypes logTypes, ReadOnlySpan<char> title, DateTime dateTime)
        {
            int offset = 0;
            SetLogType(data, logTypes, ref offset);
            SetTitle(data, title, ref offset);
            SetDateTime(data, dateTime, ref offset);
            SetNewLine(data, offset);
        }

        private static void SetLogType(Span<char> data, LogTypes logType, ref int offset)
        {
            var index = GetLogTypeIndex(logType);
            var length = LOGTYPES_LENGTH[index];
            LOGTYPES_STRING[index].CopyTo(data.Slice(1));
            data[0] = '[';
            data[1 + length] = ']';
            offset = offset + length + 2;
        }

        private static void SetTitle(Span<char> data, ReadOnlySpan<char> title, ref int offset)
        {
            title.CopyTo(data.Slice(offset + 1));
            data[offset] = '[';
            data[offset + title.Length + 1] = ']';
            offset = offset + title.Length + 2;
        }

        private static void SetDateTime(Span<char> data, DateTime dateTime, ref int offset)
        {
            data[offset] = '[';
            DATETIME.CopyTo(data.Slice(offset + 1));
            data[offset + 1 + DATETIME.Length] = ']';
            var slice = data.Slice(offset + 1, DATETIME.Length);
            SetDateTimeDay(slice, dateTime.Day);
            SetDateTimeMonth(slice, dateTime.Month);
            SetDateTimeYear(slice, dateTime.Year);
            SetDateTimeHours(slice, dateTime.Hour);
            SetDateTimeMinutes(slice, dateTime.Minute);
            SetDateTimeSeconds(slice, dateTime.Second);
            offset = offset + 2 + DATETIME.Length;
        }

        private static void SetDateTimeDay(Span<char> data, int day)
        {
            NUMBERS_DECA[day].CopyTo(data.Slice(0, 2));
        }

        private static void SetDateTimeMonth(Span<char> data, int month)
        {
            NUMBERS_DECA[month].CopyTo(data.Slice(3, 2));
        }

        private static void SetDateTimeYear(Span<char> data, int year)
        {
            if (!YEARS.TryGetValue(year, out var yeardata))
            {
                yeardata = year.ToString().PadLeft(4, '0');
                YEARS.Add(year, yeardata);
            }
            yeardata.CopyTo(data.Slice(6, 4));
        }

        private static void SetDateTimeHours(Span<char> data, int hours)
        {
            NUMBERS_DECA[hours].CopyTo(data.Slice(11, 2));
        }

        private static void SetDateTimeMinutes(Span<char> data, int minutes)
        {
            NUMBERS_DECA[minutes].CopyTo(data.Slice(14, 2));
        }

        private static void SetDateTimeSeconds(Span<char> data, int seconds)
        {
            NUMBERS_DECA[seconds].CopyTo(data.Slice(17, 2));
        }

        private static void SetNewLine(Span<char> data, int offset)
        {
            NewLine.CopyTo(data.Slice(offset));
        }

        private static int GetLogTypeIndex(LogTypes logType)
        {
            return logType switch
            {
                LogTypes.INFO => 0,
                LogTypes.WARNING => 1,
                LogTypes.ERROR => 2,
                LogTypes.FATAL => 3,
                LogTypes.DEBUG => 4
            };
        }
    }
}