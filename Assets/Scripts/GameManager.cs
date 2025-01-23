using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // 싱글턴
    public static GameManager Instance;

    private ShooterController _shooterController;

    [Header("Enemy")] 
    [SerializeField] private GameObject enemy;
    [SerializeField] private GameObject[] spawnPoint;
    
    [Header("Bullet")]
    [SerializeField] private Text bulletText;
    
    void Start()
    {
        Instance = this;
        _shooterController = FindObjectOfType<ShooterController>();
        
        StartCoroutine(EnemySpawn());
    }

    void Update()
    {
        ShowBulletCount();
    }

    void ShowBulletCount()
    {
        bulletText.text = _shooterController.currentBullet + " / " + _shooterController.maxBullet;
    }

    IEnumerator EnemySpawn()
    {
        // 적을 스폰포인트 중 랜덤으로 생성
        Instantiate(enemy, spawnPoint[Random.Range(0, spawnPoint.Length)].transform.position, Quaternion.identity);

        yield return new WaitForSeconds(2f);

        // 2초마다 적 계속 생성
        StartCoroutine(EnemySpawn());
    }
}
