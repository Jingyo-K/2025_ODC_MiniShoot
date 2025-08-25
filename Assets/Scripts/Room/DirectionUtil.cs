using UnityEngine;

public enum DirectionType { North, East, South, West }

public static class DirectionUtil
{
    public static Vector2Int ToVector(this DirectionType dir)
    {
        return dir switch
        {
            DirectionType.North => new Vector2Int(0, 1),
            DirectionType.East  => new Vector2Int(1, 0),
            DirectionType.South => new Vector2Int(0, -1),
            DirectionType.West  => new Vector2Int(-1, 0),
            _ => Vector2Int.zero,
        };
    }

    public static DirectionType Opposite(this DirectionType dir)
    {
        return dir switch
        {
            DirectionType.North => DirectionType.South,
            DirectionType.East  => DirectionType.West,
            DirectionType.South => DirectionType.North,
            DirectionType.West  => DirectionType.East,
            _ => dir,
        };
    }

    public static DoorFlag ToDoorFlag(this DirectionType dir)
    {
        return dir switch
        {
            DirectionType.North => DoorFlag.North,
            DirectionType.East  => DoorFlag.East,
            DirectionType.South => DoorFlag.South,
            DirectionType.West  => DoorFlag.West,
            _ => DoorFlag.None,
        };
    }
}

