using System.Collections.Generic;

public enum PlatformType
{
    Normal,
    Falling,
    Broken,
    TNT,
    Spear,
    Obstacle,
    Cloud
}

public static class PlatformTypeExtensions
{
    private static readonly Dictionary<PlatformType, string> _names = new Dictionary<PlatformType, string>
    {
        { PlatformType.Normal, "Platform_Normal" },
        { PlatformType.Falling, "Platform_Falling" },
        { PlatformType.Broken, "Platform_Broken" },
        { PlatformType.TNT, "Platform_TNT" },
        { PlatformType.Spear, "Platform_Spear" },
        { PlatformType.Obstacle, "Platform_Obstacle" },
        { PlatformType.Cloud, "Platform_Cloud" },
    };

    public static string GetName(this PlatformType type)
    {
        return _names[type];
    }
}