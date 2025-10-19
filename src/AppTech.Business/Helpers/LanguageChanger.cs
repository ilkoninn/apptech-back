using AppTech.Core.Enums;

namespace AppTech.Business.Helpers
{
    public static class LanguageChanger
    {
        public static ELanguage Change(string language)
        {
            return language.ToLower() switch
            {
                "en" => ELanguage.En,
                "ru" => ELanguage.Ru,
                "az" => ELanguage.Az,
                _ => throw new ArgumentOutOfRangeException(nameof(language), language, null)
            };
        }
    }
}
