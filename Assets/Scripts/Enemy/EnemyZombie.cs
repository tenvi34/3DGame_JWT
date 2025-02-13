using UnityEngine;
using UnityEngine.UI;

namespace Enemy
{
    public class EnemyZombie : LivingEntity
    {
        [SerializeField] private Slider HPBar;
        private EnemyController _enemyController;
        private Animator _animator;
        private CapsuleCollider _enemyCollider;
        
        private AudioSource _enemyAudioPlayer; // 오디오 소스 컴포넌트
        public AudioClip deathSound; // 사망시 재생할 소리
        public AudioClip hitSound; // 피격시 재생할 소리
        public ParticleSystem hitEffect; // 피격시 재생할 파티클 효과

        private static readonly int Dead = Animator.StringToHash("Dead");
        
        private void Start()
        {
            _enemyController = GetComponent<EnemyController>();
            _animator = GetComponent<Animator>();
            _enemyCollider = GetComponent<CapsuleCollider>();
            _enemyAudioPlayer = GetComponent<AudioSource>();

            HPBar.gameObject.SetActive(false);
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
    }
}