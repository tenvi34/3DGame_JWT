using UnityEngine;
using UnityEngine.UI;

namespace Enemy
{
    public class EnemyZombie : LivingEntity, IEnemy
    {
        // 좀비 스탯
        [Header("Zombie Stats")]
        [SerializeField] private float zombieHealth = 100f; // 좀비 체력
        [SerializeField] private float zombieDamage = 20f; // 공격력
        [SerializeField] private float zombieSpeed = 3f; // 이동 속도
        [SerializeField] private float attackRange = 0.5f; // 공격 범위
        public float timeBetAttack = 3f; // 공격 간격
        private float lastAttackTime; // 마지막 공격 시간
        
        [SerializeField] private Slider HPBar;
        private EnemyController _enemyController;
        private Animator _animator;
        private CapsuleCollider _enemyCollider;
        
        private AudioSource _enemyAudioPlayer; // 오디오 소스 컴포넌트
        public AudioClip deathSound; // 사망시 재생할 소리
        public AudioClip hitSound; // 피격시 재생할 소리
        public ParticleSystem hitEffect; // 피격시 재생할 파티클 효과
        
        private static readonly int Dead = Animator.StringToHash("Dead");
        
        // 적 AI의 초기 스펙을 결정하는 셋업 메서드
        public void Setup(float newHealth, float newDamage, float newSpeed)
        {
            // 체력 설정
            startingHealth = newHealth;
            health = newHealth;
            // 공격력 설정
            zombieDamage = newDamage;
        
            // 이동 속도 설정
            if(_enemyController != null)
            {
                _enemyController.NavMeshAgent.speed = newSpeed;
            }
        }
        
        private void Start()
        {
            _enemyController = GetComponent<EnemyController>();
            _animator = GetComponent<Animator>();
            _enemyCollider = GetComponent<CapsuleCollider>();
            _enemyAudioPlayer = GetComponent<AudioSource>();

            HPBar.gameObject.SetActive(false);
            
            // 적 능력치 설정
            Setup(zombieHealth, zombieDamage, zombieSpeed);
        }

        private void Update()
        {
            HPBar.value = health / startingHealth;
        }

        public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
        {
            // 피격되면 체력바 활성화
            HPBar.gameObject.SetActive(true);
            
            if (!dead)
            {
                _enemyController.LastKnownPlayerPosition = _enemyController.Player.transform.position;
                _enemyController.StartChasing();
                
                // 공격받은 지점과 방향으로 파티클 효과 재생
                hitEffect.transform.position = hitPoint;
                hitEffect.transform.rotation = Quaternion.LookRotation(hitNormal);
                hitEffect.Play();
                
                // 피격 효과음 재생
                _enemyAudioPlayer.PlayOneShot(hitSound);
            }
            
            // LivingEntity의 OnDamage()를 실행하여 데미지 적용
            base.OnDamage(damage, hitPoint, hitNormal);
        }

        public override void Die()
        {
            base.Die();

            // AI 추적을 중지하고 내비메시 컴포넌트, 콜라이더 비활성화
            _enemyController.NavMeshAgent.isStopped = true;
            _enemyController.NavMeshAgent.enabled = false;
            _enemyCollider.enabled = false;
            
            // 죽음 애니메이션 및 효과음 재생
            _animator.SetTrigger(Dead);
            _enemyAudioPlayer.PlayOneShot(deathSound);

            Destroy(gameObject, 3f);
        }

        public void ReactToSound(Vector3 soundPosition)
        {
            _enemyController.LastKnownPlayerPosition = soundPosition;
            _enemyController.ReactToPlayerSound();
        }

        // 공격
        public float AttackRange => attackRange;
        // 공격 범위안에 타겟이 있는지 확인
        public bool IsInAttackRange(Vector3 targetPosition)
        {
            return Vector3.Distance(transform.position, targetPosition) <= attackRange;
        }
        // 공격 실행
        public void TryAttack()
        {
            if (Time.time - lastAttackTime >= timeBetAttack)
            {
                // 공격 애니메이션만 실행
                _animator.SetTrigger("Attack");
                lastAttackTime = Time.time;
            }
        }
        // 애니메이션 클립에서 이벤트로 호출할 메서드
        // 애니메이션의 특정 프레임에서 이 메서드가 호출됨
        public void OnAttackPoint()
        {
            // 데미지 처리 전에 플레이어가 여전히 공격 범위 내에 있는지 체크
            if (_enemyController.Player != null && IsInAttackRange(_enemyController.Player.transform.position))
            {
                if (_enemyController.Player.TryGetComponent<IDamageable>(out var target))
                {
                    Vector3 hitPoint = _enemyController.Player.transform.position;
                    Vector3 hitNormal = transform.position - hitPoint;
                    target.OnDamage(zombieDamage, hitPoint, hitNormal);
                }
            }
        }
    }
}