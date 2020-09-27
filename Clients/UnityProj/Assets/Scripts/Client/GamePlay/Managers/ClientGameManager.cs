using System.Collections.Generic;
using BiangStudio.GamePlay;
using BiangStudio.GamePlay.UI;
using BiangStudio.Singleton;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientGameManager : MonoSingleton<ClientGameManager>
{
    #region Managers

    #region Mono

    private AudioManager AudioManager => AudioManager.Instance;
    private CameraManager CameraManager => CameraManager.Instance;
    private UIManager UIManager => UIManager.Instance;

    #endregion

    #region TSingletonBaseManager

    #region Resources

    private LayerManager LayerManager => LayerManager.Instance;
    private PrefabManager PrefabManager => PrefabManager.Instance;
    private GameObjectPoolManager GameObjectPoolManager => GameObjectPoolManager.Instance;

    #endregion

    #region Framework

    private GameStateManager GameStateManager => GameStateManager.Instance;
    private RoutineManager RoutineManager => RoutineManager.Instance;

    #endregion

    #region GamePlay

    #region Level

    private FXManager FXManager => FXManager.Instance;
    private ProjectileManager ProjectileManager => ProjectileManager.Instance;
    private LevelManager LevelManager => LevelManager.Instance;

    #endregion

    #endregion

    #endregion

    #endregion

    public float FirstStepTime = 14f;

    public Animator EasyButtonAnim;
    public Animator HardButtonAnim;

    public GameObject HardChoosePanel;
    public AudioSource BGMAudioSource;

    private void Awake()
    {
        UIManager.Init(
            (prefabName) => Instantiate(PrefabManager.GetPrefab(prefabName)),
            Debug.LogError,
            () => Input.GetMouseButtonDown(0),
            () => Input.GetMouseButtonDown(1),
            () => Input.GetKeyDown(KeyCode.Escape),
            () => Input.GetKeyDown(KeyCode.Return),
            () => Input.GetKeyDown(KeyCode.Tab)
        );

        LayerManager.Awake();
        PrefabManager.Awake();
        if (!GameObjectPoolManager.IsInit)
        {
            Transform root = new GameObject("GameObjectPool").transform;
            DontDestroyOnLoad(root.gameObject);
            GameObjectPoolManager.Init(root);
            GameObjectPoolManager.Awake();
        }

        RoutineManager.LogErrorHandler = Debug.LogError;
        RoutineManager.Awake();
        GameStateManager.Awake();

        FXManager.Awake();
        ProjectileManager.Awake();
        ProjectileManager.Init(new GameObject("ProjectileRoot").transform);
        LevelManager.Awake();
    }

    private void Start()
    {
        LayerManager.Start();
        PrefabManager.Start();
        GameObjectPoolManager.Start();

        RoutineManager.Start();
        GameStateManager.Start();

        FXManager.Start();
        ProjectileManager.Start();
        LevelManager.Start();

        UIManager.Instance.ShowUIForms<DebugPanel>();
#if !DEBUG
        UIManager.Instance.CloseUIForm<DebugPanel>();
#endif
        StartGame();
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.F10))
        {
            ReloadGame();
            return;
        }

        if (Input.GetKey(KeyCode.C))
        {
            Time.timeScale = 5f;
        }
        else
        {
            Time.timeScale = 1f;
        }

        LayerManager.Update(Time.deltaTime);
        PrefabManager.Update(Time.deltaTime);
        GameObjectPoolManager.Update(Time.deltaTime);

        RoutineManager.Update(Time.deltaTime, Time.frameCount);
        GameStateManager.Update(Time.deltaTime);

        FXManager.Update(Time.deltaTime);
        ProjectileManager.Update(Time.deltaTime);
        LevelManager.Update(Time.deltaTime);
    }

    void LateUpdate()
    {
        LayerManager.LateUpdate(Time.deltaTime);
        PrefabManager.LateUpdate(Time.deltaTime);
        GameObjectPoolManager.LateUpdate(Time.deltaTime);

        RoutineManager.LateUpdate(Time.deltaTime);
        GameStateManager.LateUpdate(Time.deltaTime);

        FXManager.LateUpdate(Time.deltaTime);
        ProjectileManager.LateUpdate(Time.deltaTime);
        LevelManager.LateUpdate(Time.deltaTime);
    }

    void FixedUpdate()
    {
        LayerManager.FixedUpdate(Time.fixedDeltaTime);
        PrefabManager.FixedUpdate(Time.fixedDeltaTime);
        GameObjectPoolManager.FixedUpdate(Time.fixedDeltaTime);

        RoutineManager.FixedUpdate(Time.fixedDeltaTime);
        GameStateManager.FixedUpdate(Time.fixedDeltaTime);

        FXManager.FixedUpdate(Time.fixedDeltaTime);
        ProjectileManager.FixedUpdate(Time.fixedDeltaTime);
        LevelManager.FixedUpdate(Time.fixedDeltaTime);
    }

    public void StartGame()
    {
        UIManager.Instance.ShowUIForms<ScorePanel>();
        LevelManager.Instance.Score = 0;
        LevelManager.Instance.Combo = 0;
    }

    public void EasyButtonShow()
    {
        EasyButtonAnim.SetTrigger("Show");
    }

    public void HardButtonShow()
    {
        HardButtonAnim.SetTrigger("Show");
    }

    public void HideHardChoosePanel()
    {
        HardChoosePanel.SetActive(false);
    }

    public void ShowHighestScore()
    {
        UIManager.Instance.GetBaseUIForm<ScorePanel>().ShowHighestScore();
    }

    public void ReloadGame()
    {
        ShutDownGame();
        SceneManager.LoadScene(0);
    }

    private void ShutDownGame()
    {
        LevelManager.ShutDown();
        FXManager.ShutDown();
        ProjectileManager.ShutDown();

        GameStateManager.ShutDown();
        RoutineManager.ShutDown();

        GameObjectPoolManager.ShutDown();
        PrefabManager.ShutDown();
        LayerManager.ShutDown();
    }

    public void SwitchToHard()
    {
        LevelManager.Instance.SwitchToHard();
    }
}