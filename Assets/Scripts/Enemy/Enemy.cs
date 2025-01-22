using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Enemy
{
    public class Enemy : MonoBehaviour
    {
        [SerializeField] private Slider HPBar;
        
        private float enemyMaxHP = 50;
        public float currentHP = 0;

        private NavMeshAgent _navMeshAgent;
        private Animator _animator;

        private GameObject _targetPlayer;
        [SerializeField] private float targetDelay = 0.5f;
        
        // 애니메이터 파라미터
        private static readonly int Dead = Animator.StringToHash("Dead");
        private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");
        private static readonly int Attack = Animator.StringToHash("Attack");

        void Start()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _animator = GetComponent<Animator>();

            // 플레이어 태그 붙은 오브젝트를 타겟으로 세팅
            _targetPlayer = GameObject.FindWithTag("Player");
            
            InitEnemyHP();
        }

        void Update()
        {
            HPBar.value = currentHP / enemyMaxHP;
            if (currentHP <= 0)
            {
                StartCoroutine(EnemyDie());
                return;
            }
            
            if (_targetPlayer != null)
            {
                Debug.Log("타켓 찾음");
                
                float maxDelay = 0.5f;
                targetDelay += Time.deltaTime;
                
                if (targetDelay < maxDelay) return;
                
                // 적 AI의 목적지를 타겟의 위치로 설정
                _navMeshAgent.destination = _targetPlayer.transform.position;
                // 타겟을 바라보도록
                transform.LookAt(_targetPlayer.transform.position);

                bool isRange = Vector3.Distance(transform.position, _targetPlayer.transform.position) <= _navMeshAgent.stoppingDistance;

                if (isRange)
                {
                    _animator.SetTrigger(Attack);
                }
                else
                {
                    _animator.SetFloat(MoveSpeed, _navMeshAgent.velocity.magnitude);
                }

                targetDelay = 0;
            }
        }
        
        private void InitEnemyHP()
        {
            currentHP = enemyMaxHP;
        }

        IEnumerator EnemyDie()
        {
            _navMeshAgent.speed = 0;
            _animator.SetTrigger(Dead);

            yield return new WaitForSeconds(3f);
            Destroy(gameObject);
        }
    }
}
