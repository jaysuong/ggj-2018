using System.Text.RegularExpressions;

namespace InitialPrefabs.DANIEditor {
    public static class RegexTools {
        /// <summary>
        /// Converts a piece of text into a more readable text
        /// </summary>
        /// <param name="text">The text to convert</param>
        /// <returns></returns>
        public static string GetReadableText(string text) {
            return Regex.Replace(text, 
                @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", " $0");
        }
    }
}
