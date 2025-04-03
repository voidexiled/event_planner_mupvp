// classes/Utils/StringUtils.cs

using System.Globalization;
using System.Text;

namespace event_planner_mupvp.classes.utils
{
    /// <summary>
    /// Proporciona métodos de utilidad para manipulación de strings.
    /// </summary>
    public static class StringUtils
    {
        /// <summary>
        /// Elimina los diacríticos (acentos, etc.) de un string.
        /// </summary>
        /// <param name="text">El texto de entrada.</param>
        /// <returns>El texto sin diacríticos.</returns>
        public static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            string normalizedString = text.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder(capacity: normalizedString.Length);

            for (int i = 0; i < normalizedString.Length; i++)
            {
                char c = normalizedString[i];
                UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}