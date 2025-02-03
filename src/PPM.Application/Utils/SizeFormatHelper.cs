using System;
using System.Globalization;

namespace Affinity_manager.Utils
{
    internal class SizeFormatHelper
    {
        public static string SizeToString(long value)
        {
            string format;
            double readable;
            switch (Math.Abs(value))
            {
                case >= 0x40000000:
                    format = Strings.PPM.GiBFormat;
                    readable = value >> 20;
                    break;
                case >= 0x100000:
                    format = Strings.PPM.MiBFormat;
                    readable = value >> 10;
                    break;
                case >= 0x400:
                    format = Strings.PPM.KiBFormat;
                    readable = value;
                    break;
                default:
                    return string.Format(Strings.PPM.ByteFormat, value);
            }

            return string.Format(format, (readable / 1024).ToString("0.##", CultureInfo.CurrentCulture));
        }
    }
}
