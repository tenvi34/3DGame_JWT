using System.Collections;
using UnityEngine;
using System.Collections.Generic;

namespace Enemy 
{
    public class EnemySpawnAndPatrolManager : MonoBehaviour
    {
        // 적 및 소환 수 설정
        [System.Serializable]
        public class EnemyTypeConfig
        {
            public GameObject enemyPrefab;
            [Range(1, 10)] public int spawnCount;
            [HideInInspector] public int currentSpawnCount; // 현재 스폰된 수를 추적
        }

        // 순찰 포인트 설정
        [System.Serializable]
        public class SpawnPointConfig
        {
            public Transform spawnPoint;
            public Transform[] localPatrolPoints;
            public EnemyTypeConfig[] enemyTypes;
        }

        [SerializeField] private SpawnPointConfig[] spawnPointConfigs;

        private void Start()
        {
            // 현재 스폰 카운트 초기화
            foreach (var config in spawnPointConfigs)
            {
                foreach (var enemyType in config.enemyTypes)
                {
                    enemyType.currentSpawnCount = 0;
                }
            }
            
            StartCoroutine(EnemySpawnRoutine());
        }

        private IEnumerator EnemySpawnRoutine()
        {
            while (true)
            {
                foreach (var config in spawnPointConfigs)
                {
                    foreach (var enemyType in config.enemyTypes)
                    {
                        // 설정된 수만큼만 스폰
                        if (enemyType.currentSpawnCount < enemyType.spawnCount)
                        {
                            SpawnEnemy(config, enemyType);
                            enemyType.currentSpawnCount++;
                        }
                    }
                }

                yield return new WaitForSeconds(2f);
            }
        }

        private void SpawnEnemy(SpawnPointConfig spawnConfig, EnemyTypeConfig enemyType)
        {
            // 랜덤한 순찰 포인트 선택
            int randomPatrolIndex = Random.Range(0, spawnConfig.localPatrolPoints.Length);
            Vector3 spawnPosition = spawnConfig.localPatrolPoints[randomPatrolIndex].position + Random.insideUnitSphere * 2;
            
            GameObject spawnedEnemy = Instantiate(enemyType.enemyPrefab, spawnPosition, Quaternion.identity);
            EnemyController enemyController = spawnedEnemy.GetComponent<EnemyController>();

            // 순찰 포인트 할당
            enemyController.PatrolPoints = spawnConfig.localPatrolPoints;
        }
    }
}