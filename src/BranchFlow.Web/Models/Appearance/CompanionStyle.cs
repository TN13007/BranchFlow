namespace BranchFlow.Web.Models.Appearance;

public sealed record CompanionStyle(string Key, string Name, string ShortName, string Mood)
{
    private string AssetDirectory => $"/images/christmas/characters/companions/{Key}";
    public string ImagePath => $"{AssetDirectory}/idle.png";
    public string PullImagePath => $"{AssetDirectory}/reach.png";

    // Only these directed pairs have supplied footage. Never reverse a clip.
    public string? TransitionTo(string targetKey) => (Key, targetKey) switch
    {
        ("ginger", "santa" or "reindeer" or "barista") => $"{AssetDirectory}/transitions/to-{targetKey}.webm",
        ("santa", "reindeer") => $"{AssetDirectory}/transitions/to-reindeer.webm",
        _ => null
    };

    public static IReadOnlyList<CompanionStyle> All { get; } = Array.AsReadOnly(new[]
    {
        new CompanionStyle("ginger", "Bạn Bánh quy", "Bánh quy", "Một chút quế, một chút ấm áp"),
        new CompanionStyle("santa", "Ông già Noel", "Noel", "Ghé quầy, nhận một chút niềm vui"),
        new CompanionStyle("reindeer", "Bạn Tuần lộc", "Tuần lộc", "Trạm dừng ấm giữa rừng đông"),
        new CompanionStyle("barista", "Cô pha chế", "Pha chế", "Một ly ngon, một ngày dịu dàng")
    });

    public static CompanionStyle FromKey(string? key) => All.FirstOrDefault(style => style.Key == key) ?? All[0];
}
