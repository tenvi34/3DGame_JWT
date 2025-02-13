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

        private static readonly int Dead = Animator.StringToHash("Dead");
        
        private void Start()
        {
            _enemyController = GetComponent<EnemyController>();
            _animator = GetComponent<Animator>();
            _enemyCollider = GetComponent<CapsuleCollider>();

            HPBar.gameObject.SetActive(false);
        }

        private void Update()
        {
            HPBar.value = health / startingHealth;
        }

        public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
        {
            base.OnDamage(damage, hitPoint, hitNormal);
            
            HPBar.gameObject.SetActive(true);
            
            if (!dead)
            {
                _enemyController.LastKnownPlayerPosition = _enemyController.Player.transform.position;
                _enemyController.StartChasing();
            }
        }

        public override void Die()
        {
            base.Die();
            
            _enemyController.NavMeshAgent.speed = 0;
            _animator.SetTrigger(Dead);
            _enemyCollider.enabled = false;

            Destroy(gameObject, 3f);
        }

        public void ReactToSound(Vector3 soundPosition)
        {
            _enemyController.LastKnownPlayerPosition = soundPosition;
            _enemyController.ReactToPlayerSound();
        }
    }
}