using System.Net;
using CareNote.Models;

namespace CareNote.Services
{
    public static class NoteFormatter
    {
        private const string DateFormat = "yyyy-MM-dd HH:mm";

        public static string FormatText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            // Skyddar mot XSS
            return WebUtility.HtmlEncode(text);
        }

        public static string FormatDate(DateTime dateTime)
        {
            return dateTime.ToString(DateFormat);
        }

        public static string FormatPriority(Priority priority)
        {
            return priority switch
            {
                Priority.High => "Hög",
                Priority.Low => "Låg",
                _ => "Normal"
            };
        }
    }
}