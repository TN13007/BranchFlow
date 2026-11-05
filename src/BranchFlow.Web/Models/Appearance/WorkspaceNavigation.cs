using System.Security.Claims;

namespace BranchFlow.Web.Models.Appearance;

public sealed record WorkspaceLink(string Key, string Label, string Url, string Icon, bool Mobile = false);

public static class WorkspaceNavigation
{
    public static string Role(ClaimsPrincipal user) => user.FindFirst(ClaimTypes.Role)?.Value ?? "";
    public static string RoleLabel(string role) => role switch
    {
        "OWNER" => "Chủ sở hữu", "SYSTEM_ADMIN" => "Quản trị tổng",
        "BRANCH_ADMIN" => "Quản trị chi nhánh", "EMPLOYEE" => "Nhân viên", _ => "Tài khoản"
    };
    public static string Home(string role) => role switch
    {
        "OWNER" => "/owner", "SYSTEM_ADMIN" or "BRANCH_ADMIN" => "/admin",
        "EMPLOYEE" => "/employee", _ => "/account"
    };
    public static IReadOnlyList<WorkspaceLink> Links(string role)
    {
        const string icons = "/images/christmas/icons/";
        WorkspaceLink account = new("account", "Tài khoản", "/account", icons + "icon-user.svg", true);
        return role switch
        {
            "OWNER" => [
                new("overview", "Tổng quan", "/owner", "/images/icons/house.svg", true),
                new("orders", "Đơn hàng", "/owner?tab=orders", icons + "icon-orders.svg", true),
                new("employees", "Nhân viên", "/owner?tab=employees", icons + "icon-user.svg", true),
                new("inventory", "Kho", "/owner?tab=inventory", icons + "icon-box.svg", true),
                new("payroll", "Bảng lương", "/owner?tab=payroll", icons + "icon-orders.svg"),
                new("analytics", "Phân tích", "/owner?tab=analytics", icons + "icon-owner.svg"), account],
            "SYSTEM_ADMIN" => [
                new("overview", "Tổng quan", "/admin", "/images/icons/house.svg", true),
                new("system-menu", "Danh mục", "/admin?tab=system-menu", "/images/icons/coffee.svg", true),
                new("branches", "Chi nhánh", "/admin?tab=branches", icons + "icon-admin.svg", true),
                new("inventory", "Kho", "/admin?tab=inventory", icons + "icon-box.svg", true),
                new("transfer", "Điều chuyển", "/admin?tab=transfer", icons + "icon-box.svg"), account],
            "BRANCH_ADMIN" => [
                new("overview", "Tổng quan", "/admin", "/images/icons/house.svg", true),
                new("branch-menu", "Menu", "/admin?tab=branch-menu", "/images/icons/coffee.svg", true),
                new("orders", "Đơn hàng", "/admin?tab=orders", icons + "icon-orders.svg"),
                new("inventory", "Kho", "/admin?tab=inventory", icons + "icon-box.svg", true),
                new("stocktake", "Kiểm kho", "/admin?tab=stocktake", icons + "icon-stocktake.svg", true),
                new("transfer", "Điều chuyển", "/admin?tab=transfer", icons + "icon-box.svg"),
                new("issue", "Xuất kho", "/admin?tab=issue", icons + "icon-box.svg"),
                new("employees", "Nhân viên", "/admin?tab=employees", icons + "icon-user.svg"),
                new("payroll", "Bảng lương", "/admin?tab=payroll", icons + "icon-orders.svg"), account],
            "EMPLOYEE" => [
                new("home", "Trang chủ", "/employee", "/images/icons/house.svg", true),
                new("create", "Gọi món", "/orders/create", "/images/icons/coffee.svg", true),
                new("mine", "Đơn của tôi", "/orders/mine", icons + "icon-orders.svg", true),
                new("stock", "Kho", "/employee?tab=stock", icons + "icon-box.svg", true),
                new("issue", "Xuất kho", "/employee?tab=issue", icons + "icon-box.svg"),
                new("stocktake", "Kiểm kho", "/employee?tab=stocktake", icons + "icon-stocktake.svg"),
                new("payroll", "Lương", "/employee?tab=payroll", icons + "icon-orders.svg"), account],
            _ => [account]
        };
    }
    public static bool IsCurrent(WorkspaceLink link, string relativePath, string? tab)
    {
        var path = relativePath.Split('?', '#')[0].Trim('/');
        var target = link.Url.Split('?')[0].Trim('/');
        if (link.Key == "mine" && path.StartsWith("orders/", StringComparison.OrdinalIgnoreCase) && path != "orders/create") return true;
        if (!string.Equals(path, target, StringComparison.OrdinalIgnoreCase)) return false;
        var query = link.Url.Contains('?') ? link.Url.Split("tab=")[1] : null;
        return string.Equals(query, string.IsNullOrWhiteSpace(tab) ? null : tab, StringComparison.OrdinalIgnoreCase);
    }
}
