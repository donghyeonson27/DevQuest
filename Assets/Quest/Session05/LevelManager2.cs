using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager2 : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "Assignment";

    // "게임 시작" 버튼 OnClick에 연결할 함수
    public void OnClickStartGame()
    {
        if (FadeCanvas.Instance != null)
        {
            // 페이드 연출 + 씬 전환
            FadeCanvas.Instance.LoadSceneWithFade(gameSceneName);
        }
        else
        {
            // 혹시 모를 비상용: FadeCanvas 못 찾으면 그냥 바로 씬 전환
            SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
        }
    }
}
