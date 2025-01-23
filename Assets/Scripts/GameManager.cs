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

    [Header("Enemy Spawn")] [SerializeField]
    private GameObject enemyPrefab;

    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private int[] maxEnemiesPerSpawnPoint; // 각 스폰 지점별 최대 몬스터 수

    private int[] currentEnemiesAtSpawnPoint; // 현재 각 스폰 지점의 몬스터 수 추적

    [Header("Bullet")] [SerializeField] private Text bulletText;

    void Start()
    {
        Instance = this;
        _shooterController = FindObjectOfType<ShooterController>();

        // 현재 몬스터 수 배열 초기화
        currentEnemiesAtSpawnPoint = new int[spawnPoints.Length];

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
        // 소환 가능한 스폰 포인트 찾기
        List<int> availableSpawnPoints = new List<int>();
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (currentEnemiesAtSpawnPoint[i] < maxEnemiesPerSpawnPoint[i])
            {
                availableSpawnPoints.Add(i);
            }
        }

        // 소환 가능한 지점이 있다면
        if (availableSpawnPoints.Count > 0)
        {
            int selectedSpawnIndex = availableSpawnPoints[Random.Range(0, availableSpawnPoints.Count)];

            // 몬스터 생성
            GameObject spawnedEnemy = Instantiate(
                enemyPrefab,
                spawnPoints[selectedSpawnIndex].position,
                Quaternion.identity
            );

            // 몬스터 수 증가
            currentEnemiesAtSpawnPoint[selectedSpawnIndex]++;

            // 몬스터 파괴 시 카운트 감소
            spawnedEnemy.GetComponent<Enemy.EnemyZombie>().OnEnemyDeath += DecreaseEnemyCount;
        }

        yield return new WaitForSeconds(2f);

        // 2초마다 적 생성 시도
        StartCoroutine(EnemySpawn());
    }

    // 몬스터 카운트 감소 메서드
    void DecreaseEnemyCount(int spawnPointIndex)
    {
        currentEnemiesAtSpawnPoint[spawnPointIndex]--;
    }
}