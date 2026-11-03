using System.Globalization;

namespace BranchFlow.Web.Components.Orders;

public static class Display
{
    private static readonly CultureInfo Vietnamese = CultureInfo.GetCultureInfo("vi-VN");
    public static string Money(decimal value) => value.ToString(value == decimal.Truncate(value) ? "N0" : "N2", Vietnamese) + "đ";
    public static string VietnamTime(DateTimeOffset value) => value.ToOffset(TimeSpan.FromHours(7)).ToString("dd/MM/yyyy HH:mm", Vietnamese);
}
