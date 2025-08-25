using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManage : MonoBehaviour
{
    public Vector2Int Position; // 방의 위치
    public GameObject NorthDoor;
    public GameObject EastDoor;
    public GameObject SouthDoor;
    public GameObject WestDoor;
    public int DoorBitmask; //1:North 2:East 4:South 8:West
    public Enemy[] Enemies; // 적 리스트
    Enemy[] AliveEnemies;
    public bool isClear = false;
    public bool isWakeUp = false; // 적이 깨어났는지 여부

    void Start()
    {
        StartCoroutine(SleepEnemies());
    }

    void Update()
    {
        if (isClear)
        {
            return; // 방이 클리어된 경우 더 이상 업데이트하지 않음
        }

        else if (Enemies == null || Enemies.Length == 0)
        {
            isClear = true; // 적이 없으면 방 클리어

            OpenDoor(); // 문을 엶
            ScrapObject[] scraps = FindObjectsOfType<ScrapObject>();

            foreach (var scrap in scraps)
            {
                scrap.SetGoToPlayer(true);
            }
        }
        //enemies에 missing이 있을 경우 해당 적을 삭제
        AliveEnemies = System.Array.FindAll(Enemies, enemy => enemy != null);
        Enemies = AliveEnemies;
    }

    public void enemyWakeUp()
    {
        if(!isWakeUp)
        {
            isWakeUp = true;
            StartCoroutine(enemyWakeUpCoroutine());
        }
    }

    private IEnumerator enemyWakeUpCoroutine()
    {
        Vector2 warningPosition;
        foreach (Enemy enemy in Enemies)
        {
            warningPosition = enemy.transform.position;
            Instantiate(GameSceneController.Instance.warningObject, warningPosition, Quaternion.identity);
        }
        yield return new WaitForSeconds(1f); // 1초 대기
        foreach (Enemy enemy in Enemies)
        {
            enemy.gameObject.SetActive(true);
            enemy.WakeUp();
        }
    }

    public void SetBitmask(int mask)
    {
        DoorBitmask = mask;
    }

    public void OpenDoor()
    {
        if ((DoorBitmask & 1) != 0) // North
        {
            NorthDoor.SetActive(false);
        }
        if ((DoorBitmask & 2) != 0) // East
        {
            EastDoor.SetActive(false);
        }
        if ((DoorBitmask & 4) != 0) // South
        {
            SouthDoor.SetActive(false);
        }
        if ((DoorBitmask & 8) != 0) // West
        {
            WestDoor.SetActive(false);
        }
    }

    public void CloseDoor()
    {
        NorthDoor.SetActive(true);
        EastDoor.SetActive(true);
        SouthDoor.SetActive(true);
        WestDoor.SetActive(true);
    }

    IEnumerator SleepEnemies()
    {
        yield return new WaitForSeconds(0.01f); // 1초 대기
        foreach (Enemy enemy in Enemies)
        {
            enemy.Sleep();
            enemy.GetComponent<EnemyHP>().SetStageScale(GameSceneController.Instance.CurrentStage); // 적의 HP를 초기화
            enemy.gameObject.SetActive(false);
        }
    }
}
