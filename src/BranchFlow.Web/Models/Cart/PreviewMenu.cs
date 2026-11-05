using BranchFlow.Application.DTOs.Sprint4;

namespace BranchFlow.Web.Models.Cart;

// Fixture chỉ dùng trang Development /preview/orders, không phải seed DB hay API giả.
public static class PreviewMenu
{
    public static OrderMenu Create() => new(new(1, "DEMO", "Chi nhánh minh họa"),
    [
        new(1, "Trà trái cây", [
            new(10, "Trà đào cam sả", "/images/menu/scenes/tea-winter-v2.jpg", 30000, [new(101, 1, "M", 30000), new(102, 2, "L", 38000)]),
            new(11, "Trà vải", "/images/menu/scenes/lychee-winter-v2.jpg", 42000, [new(103, 1, "M", 42000)])]),
        new(2, "Cà phê", [
            new(12, "Cà phê sữa đá", "/images/menu/scenes/coffee-winter-v2.jpg", 32000, [new(104, 1, "M", 32000)]),
            new(13, "Bạc xỉu", "/images/menu/scenes/milk-coffee-winter-v2.jpg", 38000, [new(105, 1, "M", 38000)])]),
        new(3, "Sữa tươi", [new(14, "Matcha latte", "/images/menu/scenes/matcha-winter-v2.jpg", 52000, [new(106, 1, "M", 52000)])])
    ],
    [new(1, "Topping", [new(5, "Trân châu", 5000, 3), new(6, "Thạch", 4000, 3)])],
    [new(1, "Đường", 1, [new(7, "Ít đường"), new(8, "Không đường")]), new(2, "Đá", 1, [new(9, "Ít đá"), new(10, "Không đá")])]);
}
