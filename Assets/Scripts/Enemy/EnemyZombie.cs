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

            Destroy(gameObject, 3f);
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