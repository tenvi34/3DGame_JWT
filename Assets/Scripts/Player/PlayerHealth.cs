using System;
using StarterAssets;
using UnityEngine;
using UnityEngine.UI;

namespace Player
{
    public class PlayerHealth : LivingEntity
    {
        [Header("UI")]
        [SerializeField] private Slider healthSlider; // 체력을 표시할 UI 슬라이더
        
        [Header("Audio")]
        [SerializeField] private AudioClip deathClip; // 사망 소리
        [SerializeField] private AudioClip hitClip; // 피격 소리
        [SerializeField] private AudioClip healClip; // 회복 소리
        
        private AudioSource _playerAudio; // 오디오 소스 컴포넌트
        private Animator _animator;
        private ShooterController _shooterController;
        private ThirdPersonController _playerController;

        private void Awake()
        {
            _playerAudio = GetComponent<AudioSource>();
            _animator = GetComponent<Animator>();
            _shooterController = GetComponent<ShooterController>();
            _playerController = GetComponent<ThirdPersonController>();
        }

        // LivingEntity의 OnEnable() 실행 (상태 초기화)
        protected override void OnEnable()
        {
            base.OnEnable();
            
            // 체력바 초기화
            healthSlider.gameObject.SetActive(true);
            healthSlider.maxValue = startingHealth;
            healthSlider.value = health;
        }

        // 데미지 처리
        public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
        {
            // 죽지 않았을 경우에만 효과음 재생
            if (!dead)
            {
                _playerAudio.PlayOneShot(hitClip);
            }
            // LivingEntity의 OnDamage() 실행(데미지 적용)
            base.OnDamage(damage, hitPoint, hitNormal);
            
            // 체력바 갱신
            healthSlider.value = health;
        }
        
        // 체력 회복
        public override void RestoreHealth(float newHealth)
        {
            // LivingEntity의 RestoreHealth() 실행 (체력 증가)
            base.RestoreHealth(newHealth);
            
            // 체력바 갱신
            healthSlider.value = health;
            _playerAudio.PlayOneShot(healClip);
        }

        // 사망 처리
        public override void Die()
        {
            // LivingEntity의 Die() 실행(사망 적용)
            base.Die();
            
            // 체력바 비활성화
            // healthSlider.gameObject.SetActive(false);
            
            // 사망 애니메이션 재생 및 효과음 재생
            _playerAudio.PlayOneShot(deathClip);
            _animator.SetTrigger("Die");
            
            // 플레이어 조작 비활성화
            _playerController.enabled = false;
            _shooterController.enabled = false;
            
            // 게임 오버 처리
            GameManager.Instance.EndGame();
        }
    }
}