using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MiniMap : MonoBehaviour
{
    [Header("Canvas Target")]
    [SerializeField] private RectTransform mapContainer; // 미니맵이 들어갈 부모(RectTransform)
    [SerializeField] private GameObject visitedIcon; //방문한 방을 담을 부모
    [SerializeField] private GameObject unvisitedIcon; //방문하지 않은 방을 담을 부모
    [SerializeField] private GameObject ExitRoomIcon;

    [Header("Icon Appearance")]
    [SerializeField] private Sprite iconSprite;          // 방 아이콘 스프라이트(없으면 기본 사각형)
    [SerializeField] private Color iconColor = Color.white;
    [SerializeField] private Color unvisitedIconColor = Color.gray; // 방문하지 않은 방 아이콘 색상
    [SerializeField] private Vector2 cellSize = new Vector2(14, 14); // 한 칸 크기(px)
    [SerializeField] private Vector2 spacing = new Vector2(2, 2);   // 칸 간격(px)

    [Header("Grid Options")]
    [Tooltip("미니맵 원점 보정. (0,0)이 중앙이 아니라면 여기로 밀어두기")]
    [SerializeField] private Vector2Int gridOffset = Vector2Int.zero;
    [Tooltip("화면 좌상단 원점 기준, 위로 갈수록 y가 감소하도록 뒤집을지")]
    [SerializeField] private bool invertY = true; // Isaac처럼 위쪽이 +y 좌표라면 true 추천

    void Awake()
    {
        if (mapContainer == null)
        {
            Debug.LogError("[MiniMap] mapContainer가 설정되지 않았습니다!");
            return;
        }
        visitedIcon.GetComponent<RectTransform>().anchoredPosition = gridOffset;
        unvisitedIcon.GetComponent<RectTransform>().anchoredPosition = gridOffset;
        ExitRoomIcon.GetComponent<RectTransform>().anchoredPosition = gridOffset;
    }

    // --- 여기 구현부 ---
    public void AddMiniMapIcon(Vector2Int position, int openBitMask)
    {
        if (mapContainer == null)
        {
            Debug.LogWarning("[MiniMap] mapContainer가 비어있어요!");
            return;
        }

        foreach (Transform child in visitedIcon.transform)
        {
            // 이미 같은 위치에 아이콘이 있다면 중복 생성 방지
            if (child.name == $"Icon_{position.x}_{position.y}")
            {
                Debug.LogWarning($"[MiniMap] 이미 {position} 위치에 아이콘이 있어요!");
                return;
            }
        }

        foreach (Transform child in unvisitedIcon.transform)
        {
            if (child.name == $"Icon_{position.x}_{position.y}")
            {
                Destroy(child.gameObject); // 방문하지 않은 방 아이콘이 있다면 제거
            }
        }

        if ((openBitMask & 1) != 0) // North
        {
            AddNextRoomIcon(position + Vector2Int.up);
        }
        if ((openBitMask & 2) != 0) // East
        {
            AddNextRoomIcon(position + Vector2Int.right);
        }
        if ((openBitMask & 4) != 0) // South
        {
            AddNextRoomIcon(position + Vector2Int.down);
        }
        if ((openBitMask & 8) != 0) // West
        {
            AddNextRoomIcon(position + Vector2Int.left);
        }

        // 1) GO & Image 생성
        var go = new GameObject($"Icon_{position.x}_{position.y}",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));

        go.transform.SetParent(visitedIcon.transform, worldPositionStays: false);

        var rt = (RectTransform)go.transform;
        var img = go.GetComponent<Image>();
        img.raycastTarget = false; // 클릭 막기
        img.sprite = iconSprite;
        img.color = iconColor;
        img.type = Image.Type.Simple;
        img.preserveAspect = false;

        // 2) 앵커/피벗: 좌상단 기준으로 찍기(Isaac 느낌)
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f); 
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = cellSize;

        // 3) 격자 → 앵커 좌표 변환
        float stepX = cellSize.x + spacing.x;
        float stepY = cellSize.y + spacing.y;

        int gx = position.x + gridOffset.x;
        int gy = (invertY ? -position.y : position.y) + gridOffset.y;

        // 좌상단 앵커이므로, 아래로 갈수록 y는 음수 방향
        float x = gx * stepX;
        float y = -(gy * stepY);

        rt.anchoredPosition = new Vector2(x, y);
    }
    
    private void AddNextRoomIcon(Vector2Int position) //1:North, 2:East, 4:South, 8:West
    {
        if (mapContainer == null)
        {
            Debug.LogWarning("[MiniMap] mapContainer가 비어있어요!");
            return;
        }

        foreach (Transform child in visitedIcon.transform)
        {
            if (child.name == $"Icon_{position.x}_{position.y}")
            {
                Debug.LogWarning($"[MiniMap] 이미 {position} 위치에 아이콘이 있어요!");
                return;
            }
        }

        // 1) GO & Image 생성
        var go = new GameObject($"Icon_{position.x}_{position.y}",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));

        go.transform.SetParent(unvisitedIcon.transform, worldPositionStays: false);

        var rt = (RectTransform)go.transform;
        var img = go.GetComponent<Image>();
        img.raycastTarget = false; // 클릭 막기
        img.sprite = iconSprite;
        img.color = unvisitedIconColor;
        img.type = Image.Type.Simple;
        img.preserveAspect = false;

        // 2) 앵커/피벗: 좌상단 기준으로 찍기(Isaac 느낌)
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = cellSize;

        // 3) 격자 → 앵커 좌표 변환
        float stepX = cellSize.x + spacing.x;
        float stepY = cellSize.y + spacing.y;

        int gx = position.x + gridOffset.x;
        int gy = (invertY ? -position.y : position.y) + gridOffset.y;

        // 좌상단 앵커이므로, 아래로 갈수록 y는 음수 방향
        float x = gx * stepX;
        float y = -(gy * stepY);

        rt.anchoredPosition = new Vector2(x, y);
    }

    public void AddExitRoomIcon(Vector2Int position)
    {
        var go = new GameObject($"Icon_{position.x}_{position.y}",
        typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));

        go.transform.SetParent(ExitRoomIcon.transform, worldPositionStays: false);

        var rt = (RectTransform)go.transform;
        var img = go.GetComponent<Image>();
        img.raycastTarget = false; // 클릭 막기
        img.sprite = iconSprite;
        img.color = Color.green;
        img.type = Image.Type.Simple;
        img.preserveAspect = false;

        // 2) 앵커/피벗: 좌상단 기준으로 찍기(Isaac 느낌)
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f); 
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = cellSize;

        // 3) 격자 → 앵커 좌표 변환
        float stepX = cellSize.x + spacing.x;
        float stepY = cellSize.y + spacing.y;

        int gx = position.x + gridOffset.x;
        int gy = (invertY ? -position.y : position.y) + gridOffset.y;

        // 좌상단 앵커이므로, 아래로 갈수록 y는 음수 방향
        float x = gx * stepX;
        float y = -(gy * stepY);

        rt.anchoredPosition = new Vector2(x, y);
    }
    public void SetCurrentRoom(Vector2Int position)
    {
        if (mapContainer == null)
        {
            Debug.LogWarning("[MiniMap] mapContainer가 비어있어요!");
            return;
        }

        foreach (Transform child in visitedIcon.transform)
        {
            if (child.name == $"Icon_{position.x}_{position.y}")
            {
                // 현재 방 아이콘을 강조 표시
                Image img = child.GetComponent<Image>();
                img.color = Color.red; // 강조 색상으로 변경
            }
            else
            {
                // 다른 방 아이콘은 기본 색상으로 되돌리기
                Image img = child.GetComponent<Image>();
                img.color = iconColor;
            }
        }
    }

    public void ClearMiniMap()
    {
        if (mapContainer == null)
        {
            Debug.LogWarning("[MiniMap] mapContainer가 비어있어요!");
            return;
        }

        // 기존 아이콘 모두 제거
        foreach (Transform child in visitedIcon.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in unvisitedIcon.transform)
        {
            Destroy(child.gameObject);
        }
        if(ExitRoomIcon.transform.childCount > 1)
        {
            Destroy(ExitRoomIcon.transform.GetChild(0).gameObject);
        }
    }
}
