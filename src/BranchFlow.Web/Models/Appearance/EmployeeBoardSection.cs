namespace BranchFlow.Web.Models.Appearance;

// UI sections of the Development preview; these links do not grant API roles.
public sealed record EmployeeBoardSection(string Key, string Label, string Title, string FigmaNode, string IconPath)
{
    public string Url => Key == "home" ? "/preview/employee" : $"/preview/employee?tab={Key}";

    public static IReadOnlyList<EmployeeBoardSection> All { get; } = Array.AsReadOnly(new[]
    {
        new EmployeeBoardSection("home", "Trang chủ ca làm", "Trang chủ ca làm việc", "165:8640", "/images/icons/house.svg"),
        new EmployeeBoardSection("stock", "Tồn kho tại quầy", "Tồn kho nguyên liệu", "165:10021", "/images/christmas/icons/icon-box.svg"),
        new EmployeeBoardSection("stocktake", "Kiểm kho cuối ca", "Kiểm kho cuối ca", "165:10405", "/images/christmas/icons/icon-stocktake.svg"),
        new EmployeeBoardSection("profile", "Hồ sơ cá nhân", "Thông tin cá nhân & Phân công", "165:10874", "/images/christmas/icons/icon-user.svg"),
        new EmployeeBoardSection("payroll", "Lương cá nhân", "Phiếu lương cá nhân", "165:11109", "/images/christmas/icons/icon-orders.svg")
    });

    public static EmployeeBoardSection FromKey(string? key) =>
        All.FirstOrDefault(section => string.Equals(section.Key, key, StringComparison.OrdinalIgnoreCase)) ?? All[0];
}
