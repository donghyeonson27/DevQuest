using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private FadeCanvas fadeCanvas;

    // "게임 시작" 버튼 OnClick 에 연결
    public void OnClickStartGame()
    {
        StartCoroutine(LoadGameSceneRoutine());
    }

    private IEnumerator LoadGameSceneRoutine()
    {
        // 1) 메뉴 위에 검은 페이드
        if (fadeCanvas != null)
            yield return fadeCanvas.FadeOut();

        // 2) 게임 씬 로드
        AsyncOperation op = SceneManager.LoadSceneAsync("Assignment");
        while (!op.isDone)
            yield return null;

        // 3) 새 씬에서 FadeCanvas 찾아서
        FadeCanvas newFade = FindObjectOfType<FadeCanvas>();
        if (newFade != null)
        {
            // 4) 천천히 밝게 (게임 장면 등장)
            yield return newFade.FadeIn();
        }
    }
}
