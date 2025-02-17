using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// 게임의 UI를 관리하는 매니저 클래스
public class UIManager : MonoBehaviour
{
    // 싱글톤 패턴을 위한 인스턴스
    private static UIManager _instance;
    public static UIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<UIManager>();
            }
            return _instance;
        }
    }

    [Header("HUD Elements")]
    [SerializeField] private Text ammoText; // 탄약 표시 텍스트
    [SerializeField] private Text scoreText; // 점수 표시 텍스트
    [SerializeField] private GameObject gameOverPanel; // 게임 오버 패널
    [SerializeField] private GameObject victoryPanel; // 승리 패널

    private void Awake()
    {
        // 싱글톤 패턴 구현
        if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    // 탄약 정보 업데이트
    public void UpdateAmmoText(int current, int max)
    {
        ammoText.text = $"{current} / {max}";
    }

    // 점수 정보 업데이트
    public void UpdateScore(int score)
    {
        scoreText.text = $"Score: {score}";
    }

    // 게임 오버 UI 표시
    public void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
    }

    // 승리 UI 표시
    public void ShowVictory()
    {
        victoryPanel.SetActive(true);
    }

    // 게임 재시작
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // 메인 메뉴로 돌아가기
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}