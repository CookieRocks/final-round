using UnityEngine;

public class RunBadge
{
    public string BadgeName { get; }
    public string ShortDescription { get; }
    public RunBadgeType BadgeType { get; }
    public Color AccentColor { get; }

    public RunBadge(string badgeName, string shortDescription, RunBadgeType badgeType, Color accentColor)
    {
        BadgeName = badgeName;
        ShortDescription = shortDescription;
        BadgeType = badgeType;
        AccentColor = accentColor;
    }
}

public enum RunBadgeType
{
    Positive,
    Performance,
    Style,
    Risky,
    Funny,
    Warning
}
