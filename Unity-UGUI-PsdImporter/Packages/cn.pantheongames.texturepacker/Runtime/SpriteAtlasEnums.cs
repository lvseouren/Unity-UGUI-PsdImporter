#if UNITY_EDITOR
namespace PantheonGames.TexturePacker
{
    public enum Algorithm
    {
        Basic,
        MaxRects,
        Polygon
    }

    public enum BasicSorting
    {
        Best,
        Name,
        Width,
        Height,
        Area,
        Circumference
    }

    public enum BasicSortingOrder
    {
        Ascending,
        Descending
    }

    public enum PackMode
    {
        Fast,
        Good,
        Best
    }

    public enum HeuristicsMode
    {
        Best,
        ShortSideFit,
        LongSideFit,
        AreaFit,
        BottomLeft,
        ContactPoint
    }

    public enum TrimMode
    {
        None,
        Crop,
        CropKeepPos,
        Polygon
    }

    public enum SizeConstraints
    {
        POT,
        WordAligned,
        AnySize
    }
}
#endif