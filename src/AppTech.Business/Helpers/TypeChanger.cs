using AppTech.Core.Enums;

namespace AppTech.Business.Helpers
{
    public static class TypeChanger
    {
        public static EGiftCardType Change(string type)
        {
            return type.ToLower() switch
            {
                "standard" => EGiftCardType.Standard,
                "premium" => EGiftCardType.Premium,
                "vip" => EGiftCardType.VIP,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }
}

