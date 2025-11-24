using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FadeCanvas : MonoBehaviour
{
    public static FadeCanvas Instance { get; private set; }

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 1f;

    private void Awake()
    {
        // 싱글톤 + 씬 넘어가도 유지
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (canvasGroup == null)
            canvasGroup = GetComponentInChildren<CanvasGroup>();

        // 시작할 때는 투명 + 클릭 막지 않기
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    // ===== 단순 페이드 함수들 =====
    public Coroutine FadeOut()
    {
        return StartCoroutine(FadeRoutine(1f));   // 0 → 1 (검정)
    }

    public Coroutine FadeIn()
    {
        return StartCoroutine(FadeRoutine(0f));   // 1 → 0 (투명)
    }

    private IEnumerator FadeRoutine(float targetAlpha)
    {
        float startAlpha = canvasGroup.alpha;
        float timer = 0f;

        // 목표가 1(완전 검정)이면, 이때부터는 클릭 막기
        if (targetAlpha > 0.9f)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = false;
        }

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeDuration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;

        // 완전 투명해졌으면 클릭 안 막게
        if (Mathf.Approximately(targetAlpha, 0f))
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
    }

    // ===== 여기서부터 "씬 전환 + 페이드" 담당 =====
    public void LoadSceneWithFade(string sceneName)
    {
        StartCoroutine(LoadSceneWithFadeRoutine(sceneName));
    }

    private IEnumerator LoadSceneWithFadeRoutine(string sceneName)
    {
        // 1) 현재 화면에서 검게 페이드 아웃
        yield return FadeOut();

        // 2) 씬 로드
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        while (!op.isDone)
        {
            yield return null;
        }

        // 3) 새 씬에서 서서히 페이드 인
        yield return FadeIn();
    }
}
