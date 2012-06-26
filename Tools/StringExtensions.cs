using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Tools
{
    public static class StringExtensions
    {
        public static bool IsEmpty(this string value)
        {
            if (value != null)
                return value.Length == 0;
            else
                return true;
        }



        public static string FormatWith(this string value, params object[] parameters)
        {
            return String.Format(value, parameters);
        }

        public static string TrimToMaxLength(this string value, int maxLength)
        {
            if (value != null && value.Length > maxLength)
                return value.Substring(0, maxLength);
            else
                return value;
        }

        public static string TrimToMaxLength(this string value, int maxLength, string suffix)
        {
            if (value != null && value.Length > maxLength)
                return value.Substring(0, maxLength) + suffix;
            else
                return value;
        }

        public static bool Contains(this string inputValue, string comparisonValue, StringComparison comparisonType)
        {
            return inputValue.IndexOf(comparisonValue, comparisonType) != -1;
        }

        public static XDocument ToXDocument(this string xml)
        {
            return XDocument.Parse(xml);
        }

        public static XmlDocument ToXmlDOM(this string xml)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xml);
            return xmlDocument;
        }

        public static XPathNavigator ToXPath(this string xml)
        {
            return new XPathDocument((TextReader) new StringReader(xml)).CreateNavigator();
        }


        public static string EnsureStartsWith(this string value, string prefix)
        {
            if (!value.StartsWith(prefix))
                return prefix + value;
            else
                return value;
        }

        public static string EnsureEndsWith(this string value, string suffix)
        {
            if (!value.EndsWith(suffix))
                return value + suffix;
            else
                return value;
        }


        public static bool IsNumeric(this string value)
        {
            float result;
            return Single.TryParse(value, out result);
        }

        public static string ExtractDigits(this string value)
        {
            return String.Join((string) null, Regex.Split(value, "[^\\d]"));
        }

        public static string ConcatWith(this string value, params string[] values)
        {
            return value + String.Concat(values);
        }

        public static Guid ToGuid(this string value)
        {
            return new Guid(value);
        }

        public static string GetBefore(this string value, string x)
        {
            int length = value.IndexOf(x);
            if (length != -1)
                return value.Substring(0, length);
            else
                return String.Empty;
        }

        public static string GetBetween(this string value, string x, string y)
        {
            int num1 = value.IndexOf(x);
            int num2 = value.LastIndexOf(y);
            if (num1 == -1 || num1 == -1)
                return String.Empty;
            int startIndex = num1 + x.Length;
            if (startIndex < num2)
                return value.Substring(startIndex, num2 - startIndex).Trim();
            else
                return String.Empty;
        }

        public static string GetAfter(this string value, string x)
        {
            int num = value.LastIndexOf(x);
            if (num == -1)
                return String.Empty;
            int startIndex = num + x.Length;
            if (startIndex < value.Length)
                return value.Substring(startIndex).Trim();
            else
                return String.Empty;
        }

        public static string Join<T>(string separator, T[] value)
        {
            if (value == null || value.Length == 0)
                return String.Empty;
            if (separator == null)
                separator = String.Empty;
            Converter<T, string> converter = (Converter<T, string>) (o => o.ToString());
            return String.Join(separator, Array.ConvertAll(value, converter));
        }

        public static string Remove(this string value, params string[] strings)
        {
            return Enumerable.Aggregate((IEnumerable<string>) strings, value, (Func<string, string, string>) ((current, c) => current.Replace(c, String.Empty)));
        }

        
        public static byte[] GetBytes(this string data)
        {
            return Encoding.Default.GetBytes(data);
        }

        public static byte[] GetBytes(this string data, Encoding encoding)
        {
            return encoding.GetBytes(data);
        }

        public static string ToTitleCase(this string value)
        {
            return CultureInfo.CurrentUICulture.TextInfo.ToTitleCase(value);
        }

        

        public static string ReplaceWith(this string value, string regexPattern, string replaceValue, RegexOptions options)
        {
            return Regex.Replace(value, regexPattern, replaceValue, options);
        }

        [Obsolete("Please use RemoveAllSpecialCharacters instead")]
        public static string AdjustInput(this string value)
        {
            return String.Join((string) null, Regex.Split(value, "[^a-zA-Z0-9]"));
        }

        public static string RemoveAllSpecialCharacters(this string value)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (char ch in Enumerable.Where((IEnumerable<char>) value, (Func<char, bool>) (c =>
                                                                                                          {
                                                                                                              if ((int) c >= 48 && (int) c <= 57 || (int) c >= 65 && (int) c <= 90)
                                                                                                                  return true;
                                                                                                              if ((int) c >= 97)
                                                                                                                  return (int) c <= 122;
                                                                                                              else
                                                                                                                  return false;
                                                                                                          })))
                stringBuilder.Append(ch);
            return ((object) stringBuilder).ToString();
        }

        public static string SpaceOnUpper(this string value)
        {
            return Regex.Replace(value, "([A-Z])(?=[a-z])|(?<=[a-z])([A-Z]|[0-9]+)", " $1$2").TrimStart(new char[0]);
        }


        public static byte[] ToBytes(this string value, Encoding encoding)
        {
            encoding = encoding ?? Encoding.Default;
            return encoding.GetBytes(value);
        }


        public static string EncodeBase64(this string value, Encoding encoding)
        {
            encoding = encoding ?? Encoding.UTF8;
            return Convert.ToBase64String(encoding.GetBytes(value));
        }


        public static string DecodeBase64(this string encodedValue, Encoding encoding)
        {
            encoding = encoding ?? Encoding.UTF8;
            byte[] bytes = Convert.FromBase64String(encodedValue);
            return encoding.GetString(bytes);
        }

        public static string ToValidFileName(this string s)
        {
            return RemoveAll(HttpUtility.HtmlDecode(s), Path.GetInvalidFileNameChars()).Replace("\"", "");
        }

        public static string ToValidDirName(this string s)
        {
            return RemoveAll(HttpUtility.HtmlDecode(s), Path.GetInvalidPathChars()).Replace("\"", "");
        }

        public static string RemoveAll(this string s, char[] toRemove)
        {
            string erg = "";
            foreach (var c in toRemove)
                erg = s.Replace(c.ToString(), String.Empty);

            return erg;
        }
    }
}