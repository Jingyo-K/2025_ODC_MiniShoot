using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            RoomManage roomManage = GetComponentInParent<RoomManage>();
            if (roomManage != null)
            {
                if (!roomManage.isClear)
                {
                    roomManage.enemyWakeUp(); // 적을 깨움
                    roomManage.CloseDoor(); // 문을 닫음
                }
                GameSceneController.Instance.AddMiniMapIcon(roomManage.Position, roomManage.DoorBitmask);
                GameSceneController.Instance.SetCurrentRoom(roomManage.Position);
            }
        }
    }
}
