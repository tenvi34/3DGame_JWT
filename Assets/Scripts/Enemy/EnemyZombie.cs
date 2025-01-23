using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Enemy
{
    public class EnemyZombie : MonoBehaviour
    {
        [SerializeField] private Slider HPBar;
        private EnemyController _enemyController;
        
        private float enemyMaxHP = 50;
        public float currentHP = 0;

        private Animator _animator;
        private CapsuleCollider _enemyCollider;

        private static readonly int Dead = Animator.StringToHash("Dead");

        // 스폰 포인트 인덱스를 추적하기 위한 변수 추가
        private int _spawnPointIndex;

        // 죽음 이벤트
        public event Action<int> OnEnemyDeath;

        private void Start()
        {
            _enemyController = GetComponent<EnemyController>();
            _animator = GetComponent<Animator>();
            _enemyCollider = GetComponent<CapsuleCollider>();

            InitEnemyHP();
        }

        private void Update()
        {
            HPBar.value = currentHP / enemyMaxHP;
            
            if (currentHP <= 0)
            {
                Die();
            }
        }

        private void InitEnemyHP()
        {
            currentHP = enemyMaxHP;
        }

        private void Die()
        {
            _enemyController.NavMeshAgent.speed = 0;
            _animator.SetTrigger(Dead);
            _enemyCollider.enabled = false;

            // 스폰 포인트 카운트 감소 이벤트 호출
            OnEnemyDeath?.Invoke(_spawnPointIndex);

            Destroy(gameObject, 3f);
        }

        // 스폰 포인트 인덱스 설정 메서드 추가
        public void SetSpawnPointIndex(int index)
        {
            _spawnPointIndex = index;
        }

        // 외부에서 호출 가능한 메서드들
        public void TakeDamage(float damage)
        {
            currentHP -= damage;
            _enemyController.StartChasing(); // 공격받으면 즉시 추격 상태로 전환
        }

        public void ReactToSound(Vector3 soundPosition)
        {
            _enemyController.LastKnownPlayerPosition = soundPosition;
            _enemyController.ReactToPlayerSound();
        }
    }
}