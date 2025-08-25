using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 방 노드 클래스
/// - 위치, 방문 여부, 문 상태, 이웃 노드 리스트를 포함
/// - 문 상태는 비트마스크 연산을 사용하여 관리
/// - 방향에 따른 문 열기, 확인 메서드 제공
/// </summary>
[System.Flags]
public enum DoorFlag
{
    None  = 0,
    North = 1 << 0,  // 0001
    East  = 1 << 1,  // 0010
    South = 1 << 2,  // 0100
    West  = 1 << 3   // 1000
}
public enum RoomType
{
    Start,
    Normal,
    Exit
}
public class RoomNode
{
    public Vector2Int Position { get; private set; }
    public Vector2 RealPosition { get; private set; }
    public bool IsVisited { get; set; }
    public DoorFlag Doors { get; set; }
    public List<RoomNode> Neighbors { get; private set; }
    public RoomType Tag { get; set; }
    public RoomObject RoomObject { get; set; }

    public RoomNode(Vector2Int position)
    {
        Position = position;
        RealPosition = new Vector2(0, 0);
        IsVisited = false;
        Doors = DoorFlag.None;
        Neighbors = new List<RoomNode>();
        Tag = RoomType.Normal;
        RoomObject = null;
    }

    public void SetRealPosition(Vector2 realPosition)
    {
        RealPosition = realPosition;
    }
    public void OpenDoor(DirectionType dir)
    {
        Doors |= dir.ToDoorFlag();
    }

    public bool HasDoor(DirectionType dir)
    {
        return (Doors & dir.ToDoorFlag()) == 0;
    }
    public bool CompareDoors(int bitMask)
    {
        return Doors == (DoorFlag)bitMask;
    }
    public int getBitMask()
    {
        return (int)Doors;
    }
    public bool isLastNode()
    {
        return Neighbors.Count == 0;
    }
}