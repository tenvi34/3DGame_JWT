using UnityEngine;
using Player;

// 게임의 전반적인 상태와 점수를 관리하는 게임 매니저
public class GameManager : MonoBehaviour
{
    // 싱글톤 패턴을 위한 인스턴스
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            // 만약 싱글톤 변수에 아직 오브젝트가 할당되지 않았다면
            if (_instance == null)
            {
                // 씬에서 GameManager 오브젝트를 찾아 할당
                _instance = FindObjectOfType<GameManager>();
            }
            return _instance;
        }
    }

    private int _score; // 현재 게임 점수
    private bool _isGameOver; // 게임 오버 상태
    // private PlayerHealth _player; // 플레이어 캐릭터

    private void Awake()
    {
        // 씬에 싱글톤 오브젝트가 된 다른 GameManager 오브젝트가 있다면
        if (Instance != this)
        {
            // 자신을 파괴
            Destroy(gameObject);
            return;
        }
        
        // 플레이어 캐릭터의 사망 이벤트 발생시 게임 오버
        // _player = FindObjectOfType<PlayerHealth>();
        // _player.onDeath += EndGame;
    }

    // 점수를 추가하고 UI 갱신
    public void AddScore(int points)
    {
        // 게임 오버가 아닌 상태에서만 점수 증가 가능
        if (_isGameOver) return;
        
        _score += points;
        UIManager.Instance.UpdateScore(_score);
    }

    // 게임 오버 처리
    public void EndGame()
    {
        // 게임 오버 상태가 아닐 때만 실행
        if (_isGameOver) return;
        
        _isGameOver = true;
        UIManager.Instance.ShowGameOver();
    }

    // 게임 승리 처리
    public void Victory()
    {
        // 게임 오버 상태가 아닐 때만 실행
        if (_isGameOver) return;
        
        _isGameOver = true;
        UIManager.Instance.ShowVictory();
    }
}