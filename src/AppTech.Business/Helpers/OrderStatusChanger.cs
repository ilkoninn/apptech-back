using AppTech.Core.Enums;

namespace AppTech.Business.Helpers
{
    public static class OrderStatusChanger
    {
        public static string Change(EOrderStatus orderStatus)
        {
            return orderStatus switch
            {
                EOrderStatus.ONPAYMENT => "onpayment",
                EOrderStatus.CANCELLED => "cancelled",
                EOrderStatus.FULLYPAID => "fullypaid",
                EOrderStatus.DECLINED => "declined",
                _ => throw new ArgumentOutOfRangeException(nameof(orderStatus), orderStatus, null)
            };
        }
    }
}
