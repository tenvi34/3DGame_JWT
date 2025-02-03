using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace Enemy
{
    public class EnemySpawnAndPatrolManager : MonoBehaviour
    {
        [System.Serializable]
        public class EnemyTypeConfig
        {
            public GameObject enemyPrefab;
            [Range(1, 10)] public int spawnCount;
        }

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
            StartCoroutine(EnemySpawnRoutine());
        }

        private IEnumerator EnemySpawnRoutine()
        {
            while (true)
            {
                foreach (var config in spawnPointConfigs)
                {
                    
                }
            }
        }
    }
}