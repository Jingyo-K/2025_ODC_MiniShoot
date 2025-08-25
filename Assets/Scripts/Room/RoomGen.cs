using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// RoomNode 리스트를 기반으로 프리팹을 인스턴스화하여 맵을 시각화함
/// </summary>
public class RoomGen : MonoBehaviour
{
    public List<RoomObject> roomPrefabs;
    public GameObject horizontalDoorPrefab;
    public GameObject verticalDoorPrefab;
    public GameObject exitZonePrefab;
    public int xMargin = 2;
    public int yMargin = 2;

    public void MapGen(List<RoomNode> nodes)
    {
        GenMap(nodes);
    }

    private void GenMap(List<RoomNode> nodes)
    {
        int count = 1;
        foreach (RoomNode node in nodes)
        {
            if (node.RoomObject == null)
            {
                node.RoomObject = roomPrefabs[0];
            }
            SetNeighborPosition(node);
            GameObject room = Instantiate(node.RoomObject.prefab, new Vector3(node.RealPosition.x, node.RealPosition.y, 0), Quaternion.identity);
            room.name = $"Room_{node.Position.x}_{node.Position.y}_{node.Tag}_{count++}";
            room.transform.parent = transform;
            room.GetComponent<RoomManage>().Position = node.Position;
            int DoorFlag = node.getBitMask();
            room.GetComponent<RoomManage>().SetBitmask(DoorFlag);
            room.GetComponent<RoomManage>().OpenDoor();

            /*if (DoorFlag != 0)
            {
                PlaceDoorOrHall(node, room, DirectionType.North);
                PlaceDoorOrHall(node, room, DirectionType.East);
                PlaceDoorOrHall(node, room, DirectionType.South);
                PlaceDoorOrHall(node, room, DirectionType.West);
            }
            */
            if (node.Tag == RoomType.Exit)
            {
                GameObject exitZone = Instantiate(exitZonePrefab, new Vector3(node.RealPosition.x, node.RealPosition.y, 0), Quaternion.identity);
                exitZone.name = $"ExitZone_{node.Position.x}_{node.Position.y}";
                exitZone.transform.parent = room.transform;
                GameSceneController.Instance.AddExitRoomIcon(node.Position);
            }
        }
    }
    private void PlaceDoorOrHall(RoomNode node, GameObject room, DirectionType direction)
    {
        Vector3 basePosition = node.RealPosition;
        Vector3 offset = Vector3.zero;
        GameObject selectedPrefab = null;

        float halfWidth = node.RoomObject.width / 2f;
        float halfHeight = node.RoomObject.height / 2f;
        float hallWidth = (32f - node.RoomObject.width) / 2f;
        float hallHeight = (32f - node.RoomObject.height) / 2f;

        bool hasDoor = node.HasDoor(direction);

        switch (direction)
        {
            case DirectionType.North:
                offset = new Vector3(0, halfHeight + (hasDoor ? -0.5f : hallHeight / 2), 0);
                selectedPrefab = hasDoor ? verticalDoorPrefab : node.RoomObject.verticalHallPrefab;
                break;
            case DirectionType.East:
                offset = new Vector3(halfWidth + (hasDoor ? -0.5f : hallWidth / 2), 0, 0);
                selectedPrefab = hasDoor ? horizontalDoorPrefab : node.RoomObject.horizontalHallPrefab;
                break;
            case DirectionType.South:
                offset = new Vector3(0, -halfHeight + (hasDoor ? 0.5f : -hallHeight / 2), 0);
                selectedPrefab = hasDoor ? verticalDoorPrefab : node.RoomObject.verticalHallPrefab;
                break;
            case DirectionType.West:
                offset = new Vector3(-halfWidth + (hasDoor ? 0.5f : -hallWidth / 2), 0, 0);
                selectedPrefab = hasDoor ? horizontalDoorPrefab : node.RoomObject.horizontalHallPrefab;
                break;
        }

        Vector3 spawnPosition = basePosition + offset;
        Instantiate(selectedPrefab, spawnPosition, Quaternion.identity).transform.parent = room.transform;
    }
    private void SetNeighborPosition(RoomNode node)
    {
        foreach (RoomNode neighbor in node.Neighbors)
        {
            neighbor.RoomObject = roomPrefabs[Random.Range(1, roomPrefabs.Count)];
            switch (neighbor.Position - node.Position)
            {
                case Vector2Int up when up == Vector2Int.up:
                    neighbor.SetRealPosition(node.RealPosition + new Vector2(0, CalHeight(node, neighbor) + yMargin));
                    break;
                case Vector2Int down when down == Vector2Int.down:
                    neighbor.SetRealPosition(node.RealPosition + new Vector2(0, -CalHeight(node, neighbor) - yMargin));
                    break;
                case Vector2Int right when right == Vector2Int.right:
                    neighbor.SetRealPosition(node.RealPosition + new Vector2(CalWidth(node, neighbor) + xMargin, 0));
                    break;
                case Vector2Int left when left == Vector2Int.left:
                    neighbor.SetRealPosition(node.RealPosition + new Vector2(-CalWidth(node, neighbor) - xMargin, 0));
                    break;
            }
        }
    }
    private float CalHeight(RoomNode node1, RoomNode node2)
    {
        return 32f; // Assuming each room height is 32 units, adjust as necessary
        //return Mathf.Abs(node1.RoomObject.height + node2.RoomObject.height) / 2;
    }
    private float CalWidth(RoomNode node1, RoomNode node2)
    {
        return 32f; // Assuming each room width is 32 units, adjust as necessary
        //return Mathf.Abs(node1.RoomObject.width + node2.RoomObject.width)/2;
    }
}
