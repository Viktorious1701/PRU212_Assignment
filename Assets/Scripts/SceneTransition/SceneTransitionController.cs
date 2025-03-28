using UnityEngine;
using Cinemachine;
using UnityEngine.SceneManagement;

public class SceneTransitionController : MonoBehaviour
{
    [Header("Transition Settings")]
    [SerializeField] private float transitionDuration = 1f;
    [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Prefabs")]
    [SerializeField] private GameObject blackHoleMaskPrefab;
    [SerializeField] private float initialMaskScale = 0.1f; // Starting small

    private static SceneTransitionController instance;
    private CinemachineVirtualCamera virtualCamera;
    private SpriteRenderer blackHoleMask;
    private float transitionProgress = 0f;
    private TransitionState currentState = TransitionState.Idle;
    private string targetSceneName;
    private Vector3 initialCameraSize;

    private enum TransitionState
    {
        Idle,
        GrowingMask,
        LoadingScene,
        ShrinkingMask
    }

    void Awake()
    {
        // Singleton pattern to persist across scenes
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // Subscribe to scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Find camera in new scene
        virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();

        // Create mask if it doesn't exist
        if (blackHoleMask == null && blackHoleMaskPrefab != null)
        {
            GameObject maskObject = Instantiate(blackHoleMaskPrefab);
            blackHoleMask = maskObject.GetComponent<SpriteRenderer>();
            blackHoleMask.gameObject.SetActive(false);
            // Ensure mask covers the entire screen when needed
            if (Camera.main != null)
            {
                float height = Camera.main.orthographicSize * 2f;
                float width = height * Camera.main.aspect;
                blackHoleMask.transform.position = Camera.main.transform.position + new Vector3(0, 0, 10);
            }

            // Ensure mask is on top of everything
            blackHoleMask.sortingOrder = 1000;
        }

        // Reset mask for shrinking
        if (blackHoleMask != null)
        {
            // Start small when first created
            blackHoleMask.transform.localScale = Vector3.one * initialMaskScale;
        }

        // Store initial camera size
        if (virtualCamera != null)
        {
            initialCameraSize = Vector3.one * virtualCamera.m_Lens.OrthographicSize;
        }

        // Continue transition if we were in the middle of one
        if (currentState == TransitionState.LoadingScene)
        {
            currentState = TransitionState.ShrinkingMask;
            transitionProgress = 0f;
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from event to prevent memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Update()
    {
        if (currentState == TransitionState.Idle || virtualCamera == null || blackHoleMask == null)
            return;

        // Increment transition progress
        transitionProgress += Time.deltaTime / transitionDuration;

        switch (currentState)
        {
            case TransitionState.GrowingMask:
                HandleGrowingMask();
                break;
            case TransitionState.LoadingScene:
                HandleLoadingScene();
                break;
            case TransitionState.ShrinkingMask:
                HandleShrinkingMask();
                break;
        }
    }

    void HandleGrowingMask()
    {
        blackHoleMask.gameObject.SetActive(true);
        // Grow black hole mask from small to full screen
        float growProgress = transitionCurve.Evaluate(transitionProgress);
        float maskScale = Mathf.Lerp(initialMaskScale, 2f, growProgress);

        blackHoleMask.transform.localScale = Vector3.one * maskScale;
        
        // Zoom in slightly during growth
        virtualCamera.m_Lens.OrthographicSize = initialCameraSize.x * Mathf.Lerp(1f, 1.2f, growProgress);

        // Transition to loading scene when fully covered
        if (transitionProgress >= 1f)
        {
            currentState = TransitionState.LoadingScene;
            transitionProgress = 0f;
            SceneManager.LoadScene(targetSceneName);
        }
    }

    void HandleLoadingScene()
    {
        // Brief pause between scenes
        if (transitionProgress >= 0.1f)
        {
            currentState = TransitionState.ShrinkingMask;
            transitionProgress = 0f;
        }
    }

    void HandleShrinkingMask()
    {
        blackHoleMask.gameObject.SetActive(true);
        // Shrink black hole mask to a small size
        float shrinkProgress = transitionCurve.Evaluate(transitionProgress);
        float maskScale = Mathf.Lerp(1f, initialMaskScale, shrinkProgress);

        blackHoleMask.transform.localScale = Vector3.one * maskScale;

        // Zoom out slightly during shrink
        virtualCamera.m_Lens.OrthographicSize = initialCameraSize.x * Mathf.Lerp(1.2f, 1f, shrinkProgress);

      
        // Reset when transition complete
        if (transitionProgress >= 1f)
        {
             Destroy(blackHoleMask.gameObject);
        }
    }

    public void TransitionToScene(string sceneName)
    {
        if (currentState != TransitionState.Idle) return;

        targetSceneName = sceneName;
        currentState = TransitionState.GrowingMask;
        transitionProgress = 0f;
    }
}