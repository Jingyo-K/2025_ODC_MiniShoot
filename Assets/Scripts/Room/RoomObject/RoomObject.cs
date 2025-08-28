using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Room/RoomObject")]
public class RoomObject : ScriptableObject
{
    public Vector2Int position;
    public GameObject prefab;

    public int width;
    public int height;
    public int openDoor; // 열려있는 문 비트마스크
    public GameObject NorthDoorPrefab;
    public GameObject SouthDoorPrefab;
    public GameObject EastDoorPrefab;
    public GameObject WestDoorPrefab;
}
