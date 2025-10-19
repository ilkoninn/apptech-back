using AppTech.Core.Enums;

namespace AppTech.Business.Helpers
{
    public static class EnumExtensions
    {
        public static string EnumToString(this Enum enumType)
        {
            return enumType switch
            {
                ELanguage.Az => "AZ",
                ELanguage.En => "EN",
                ELanguage.Ru => "RU",
                EGiftCardType.Standard => "standard",
                EGiftCardType.Premium => "premium",
                EGiftCardType.VIP => "vip",
                EQuestionType.SingleChoice => "single",
                EQuestionType.MultipleChoice => "multi",
                EQuestionType.DragAndDrop => "dnd",
                EQuestionType.Terminal => "term",
                _ => throw new ArgumentOutOfRangeException(nameof(enumType), enumType, null)
            };
        }
    }
}
