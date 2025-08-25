using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// 해야할 일 정리
/// - Player를 생성한다
/// - Shop을 생성한다
/// - 스테이지와 상점 간의 전환을 관리한다
/// --- 게임을 시작하면 우선 shop을 보여준다.
/// --- 그 상태에서 게임스테이트를 설정하여 게임을 일시정지한다.
/// --- 그 상태에서 맵을 생성하고 플레이어를 생성한다.
/// --- 만약 생성이 끝났다면 상점에 Next 버튼을 추가하여 게임으로 넘어갈 수 있도록 한다.
/// - 스테이지 전환을 관리한다.
/// --- 스테이지 전환 시 Shop과 플레이어는 DontDestroyOnLoad로 관리한다.
/// --- 마찬가지로 Shop를 띄운채 현재 있는 맵을 모두 제거 한 뒤 새로운 맵을 생성한다.
/// --- 이때, 스테이지 단계에 따라 맵의 난이도 및 상점의 가격을 조정한다.
/// - 게임 시작, 종료, 일시정지 등의 게임 상태를 관리한다
/// </summary>
public class GameSceneController : MonoBehaviour
{
    public enum GameState
    {
        Paused,
        Playing
    }
    public static GameState CurrentGameState { get; private set; } = GameState.Paused;
    public int BossStage = 4;
    // 싱글톤 패턴을 사용하여 GameSceneController의 인스턴스를 관리
    public static GameSceneController Instance;
    public GameObject playerPrefab;
    public GameObject shopPrefab;
    public GameObject miniMapPrefab; // 미니맵 프리팹
    public GameObject startButtonPrefab;
    public MapManager mapManager;
    public string sceneToLoad; // 게임 시작 시 로드할 씬 이름
    public GameObject playerInstance;
    public GameObject shopInstance;
    public GameObject miniMapInstance; // 미니맵 인스턴스
    public GameObject warningObject;


    public int CurrentStage { get; private set; } = 1; // 현재 스테이지 단계

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        Debug.Log("GameSceneController Awake called");
        if (playerInstance == null)
        {
            playerInstance = Instantiate(playerPrefab);
            DontDestroyOnLoad(playerInstance);
        }
        else
        {
            playerInstance.SetActive(true); // 이미 존재하는 경우 활성화
        }
        if (miniMapInstance == null)
        {
            miniMapInstance = Instantiate(miniMapPrefab);
            DontDestroyOnLoad(miniMapInstance);
        }
        else
        {
            miniMapInstance.SetActive(true); // 이미 존재하는 경우 활성화
        }
        Debug.Log("Player instance created or reused");
        playerInstance.GetComponent<PlayerInput>().enabled = false; // 플레이어 입력 비활성화
        if (shopInstance == null)
        {
            shopInstance = Instantiate(shopPrefab);
            DontDestroyOnLoad(shopInstance);
        }
        else
        {
            shopInstance.SetActive(true); // 이미 존재하는 경우 활성화
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            NextStage();
        }
    }

    private void InitializeGame()
    {
        playerInstance.GetComponent<PlayerInput>().enabled = false;
        if (shopInstance != null)
        {
            shopInstance.SetActive(true);
            shopInstance.GetComponent<ShopItemPlacement>().Initialize();
        }
        // 게임 시작 시 Shop을 띄우고 일시정지 상태로 설정
        ShowShop();
        playerInstance.transform.position = Vector3.zero; // 플레이어 초기 위치 설정
        mapManager = FindObjectOfType<MapManager>();
        if (mapManager != null)
        {
            mapManager.MapGen();
        }
        if (miniMapInstance != null)
        {
            ResetMiniMap(); // 미니맵 초기화
        }

        ShowStartButton();

    }

    private void ShowShop()
    {
        if (shopInstance != null)
        {
            miniMapInstance.SetActive(false); // 미니맵 비활성화
            CurrentGameState = GameState.Paused; // 게임 상태를 일시정지로 설정
            Time.timeScale = 0f; // 게임 시간 정지
        }

    }

    public void ShowStartButton()
    {
        GameObject startButton = shopInstance.GetComponent<ShopItemPlacement>().NextButtonPrefab;
        startButton.SetActive(true);
        startButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(StartGame);
    }

    public void StartGame()
    {
        if (shopInstance != null)
        {
            shopInstance.SetActive(false);
            miniMapInstance.SetActive(true); // 미니맵 활성화
            playerInstance.GetComponent<PlayerInput>().enabled = true; // 플레이어 입력 활성화
            CurrentGameState = GameState.Playing; // 게임 상태를 재생 중으로 설정
            Time.timeScale = 1f; // 게임 시간 재개
        }
    }

    public void NextStage()
    {
        CurrentStage++;
        ShowShop(); // 다음 스테이지로 넘어가기 전에 Shop을 다시 보여줌
        if (CurrentStage < BossStage)
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            SceneManager.LoadScene("BossTest"); // 예시로 메인 메뉴 씬으로 이동
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Scene loaded: " + scene.name);
        if (scene.name == sceneToLoad || scene.name == "BossTest")
        {
            InitializeGame(); // 새로운 씬이 로드되면 게임 초기화
        }
    }


    /// <summary>
    /// 미니맵 관련 함수들
    /// </summary>
    public void ResetMiniMap()
    {
        if (miniMapInstance != null)
        {
            miniMapInstance.GetComponent<MiniMap>().ClearMiniMap();
        }
    }
    public void SetCurrentRoom(Vector2Int position)
    {
        if (miniMapInstance != null)
        {
            // 현재 방 위치를 미니맵에 설정하는 로직 구현
            miniMapInstance.GetComponent<MiniMap>().SetCurrentRoom(position);
        }
        else
        {
            Debug.LogWarning("MiniMap instance is not set.");
        }
    }
    public void AddMiniMapIcon(Vector2Int position, int openBitMask)
    {
        if (miniMapInstance != null)
        {
            // 미니맵에 아이콘 추가 로직 구현
            miniMapInstance.GetComponent<MiniMap>().AddMiniMapIcon(position, openBitMask);
            Debug.Log($"MiniMap icon added at position: {position}");
        }
        else
        {
            Debug.LogWarning("MiniMap instance is not set.");
        }
    }
    public void AddExitRoomIcon(Vector2Int position)
    {
        if (miniMapInstance != null)
        {
            // Exit Room 아이콘 추가 로직 구현
            miniMapInstance.GetComponent<MiniMap>().AddExitRoomIcon(position);
            Debug.Log($"Exit Room icon added at position: {position}");
        }
        else
        {
            Debug.LogWarning("MiniMap instance is not set.");
        }
    }
    public void GameOver()
    {
        Time.timeScale = 0f; // 게임 시간 정지
        //모든 인스턴스 삭제
        Destroy(playerInstance);
        Destroy(shopInstance);
        Destroy(miniMapInstance);
        Destroy(gameObject); // GameSceneController 인스턴스 삭제

    }
}
