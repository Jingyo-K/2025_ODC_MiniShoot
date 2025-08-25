using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 실제 맵을 생성
/// </summary>
public class MapManager : MonoBehaviour
{
    public List<RoomObject> roomPrefabs;
    public GameObject horizontalDoorPrefab;
    public GameObject verticalDoorPrefab;
    public GameObject exitZonePrefab;
    public int minRoomCount = 10;
    public int maxRoomCount = 15;
    public int xMargin = 2;
    public int yMargin = 2;
    private GameObject Map;
    public void MapGen()
    {
        // 최소 방 개수와 최대 방 개수를 비교하여 유효한 범위로 설정
        int roomCount = Random.Range(minRoomCount, maxRoomCount + 1);
        var nodeGraph = new RoomGraph(roomCount);
        List<RoomNode> nodes = nodeGraph.Generate(Vector2Int.zero);
        Map = new GameObject("Map");
        var generator = Map.AddComponent<RoomGen>();
        generator.roomPrefabs = roomPrefabs;
        generator.horizontalDoorPrefab = horizontalDoorPrefab;
        generator.verticalDoorPrefab = verticalDoorPrefab;
        generator.exitZonePrefab = exitZonePrefab;
        generator.xMargin = xMargin;
        generator.yMargin = yMargin;

        generator.MapGen(nodes);
    }
}