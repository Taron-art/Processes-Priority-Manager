using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Vanara.PInvoke;

namespace Affinity_manager.Utils
{
    internal static class RusDetector
    {
        private static CultureInfo[] LanguagesToCheck
        {
            get
            {
                return
                [
                    new CultureInfo("ru-RU"),
                    new CultureInfo("tt-RU"),
                    new CultureInfo("ba-RU"),
                    new CultureInfo("cv-RU"),
                    new CultureInfo("ce-RU"),
                    new CultureInfo("mhr-RU"),
                    new CultureInfo("mrj-RU"),
                    new CultureInfo("udm-RU"),
                    new CultureInfo("sah-RU"),
                    new CultureInfo("krl-RU"),
                    new CultureInfo("nog-RU")
                ];
            }

        }

        public static bool Check()
        {
            CultureInfo[] languages = GetInputLanguges().ToArray();
            CultureInfo[] russianLanguagesFound = languages.Intersect(LanguagesToCheck).ToArray();
            if (russianLanguagesFound.Length > 0 && languages.Except(LanguagesToCheck).All(language => language.TwoLetterISOLanguageName.Equals("en", System.StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            return true;
        }

        private static IEnumerable<CultureInfo> GetInputLanguges()
        {
            int size = User32.GetKeyboardLayoutList(0, null);
            User32.HKL[] locales = new User32.HKL[size];
            User32.GetKeyboardLayoutList(size, locales);
            foreach (User32.HKL locale in locales)
            {
                yield return CultureInfo.GetCultureInfo(locale.LangId);
            }
        }
    }
}
