using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace MapExportExtension
{
    public static class ArmaSerializer
    {
        public static string Escape(string str)
        {
            return str.Replace("\"", "\"\"");
        }

        public static double? ParseDouble(string str)
        {
            if (string.IsNullOrEmpty(str) || str == "null")
            {
                return null;
            }
            return double.Parse(str.Trim(), CultureInfo.InvariantCulture);
        }

        public static string ParseString(string str)
        {
            if (str == "null")
            {
                return null;
            }
            return ReadString(new StringReader(str));
        }

        private static string ReadString(StringReader str)
        {
            if (str.Peek() == '"')
            {
                var sb = new StringBuilder();
                str.Read(); // Consume '"'
                while (str.Peek() != -1)
                {
                    char c = (char)str.Read();
                    if (c == '"')
                    {
                        if (str.Peek() == '"')
                        {
                            str.Read(); // Consume second '"'
                            sb.Append(c);
                        }
                        else
                        {
                            return sb.ToString();
                        }
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
                return sb.ToString();
            }
            return null;
        }

        private static double? ReadNumber(StringReader str)
        {
            var sb = new StringBuilder();
            int i;
            while ((i = str.Peek()) != -1)
            {
                char c = (char)i;
                if (char.IsDigit(c) || c == '.' || c == '-')
                {
                    str.Read();
                    sb.Append(c);
                }
                else
                {
                    return ParseDouble(sb.ToString());
                }
            }
            return ParseDouble(sb.ToString());
        }

        public static int[] ParseIntegerArray(string str)
        {
            return ParseMixedArray(str).Cast<double?>().Select(n => (int)n).ToArray();
        }

        public static double[] ParseDoubleArray(string str)
        {
            return ParseMixedArray(str).Cast<double?>().Select(n => (double)n).ToArray();
        }

        public static object[] ParseMixedArray(string str)
        {
            Trace.TraceInformation("ParseMixedArray: '{0}'", str);
            return ReadArray(new StringReader(str));
        }

        private static object[] ReadArray(StringReader str)
        {
            if (str.Peek() == '[')
            {
                var data = new List<object>();
                var expectItem = true;
                str.Read(); // Consume '['

                int i;
                while ((i = str.Peek()) != -1)
                {
                    char c = (char)i;
                    if (c == ']')
                    {
                        str.Read();
                        return data.ToArray();
                    }
                    if (c == ',')
                    {
                        str.Read();
                        expectItem = true;
                    }
                    else if (c != ' ' && expectItem)
                    {
                        if (c == '"')
                        {
                            data.Add(ReadString(str));
                        }
                        else if (c == '[')
                        {
                            data.Add(ReadArray(str));
                        }
                        else if (char.IsDigit(c) || c == '-')
                        {
                            data.Add(ReadNumber(str));
                        }
                        else if (c == 'n')
                        {
                            str.Read();
                            data.Add(null);
                        }
                        else if (c == 't')
                        {
                            str.Read();
                            data.Add(true);
                        }
                        else if (c == 'f')
                        {
                            str.Read();
                            data.Add(false);
                        }
                        expectItem = false;
                    }
                    else
                    {
                        str.Read();
                    }
                }
                return data.ToArray();
            }
            return null;
        }
    }
}
