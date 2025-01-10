using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;
using Weapon;
using Random = UnityEngine.Random;

namespace Player
{
    public class ShooterController : MonoBehaviour
    {
        // 설정값
        [SerializeField] private CinemachineVirtualCamera _aimVirtualCamera;
        [SerializeField] private float normalSensitivity;
        [SerializeField] private float aimSensitivity;
        [SerializeField] private LayerMask aimColliderLayerMask;
        [SerializeField] private Transform debugTransform;
        
        [Header("Bullet")]
        [SerializeField] private Transform bulletPrefab;
        [SerializeField] private Transform bulletSpawnPoint;
        [SerializeField] private GameObject[] muzzleFlashPrefabs; // 총구 화염 프리팹 배열
        
        [Header("Sound")]
        [SerializeField] private AudioSource audioSource; // AudioSource 컴포넌트
        [SerializeField] private AudioClip shootSound;    // 총 발사 사운드 클립

        // 체크
        private bool _isReloading = false; // 장전중인지 확인

        private StarterAssetsInputs _starterAssetsInputs;
        private ThirdPersonController _playerController;
        private Animator _animator;

        // 애니메이터
        private static readonly int DoShoot = Animator.StringToHash("DoShoot"); // 사격
        private static readonly int DoReload = Animator.StringToHash("DoReload"); // 장전

        private void Awake()
        {
            _starterAssetsInputs = GetComponent<StarterAssetsInputs>();
            _playerController = GetComponent<ThirdPersonController>();
            _animator = GetComponent<Animator>();
        }

        private void Update()
        {
            AimMode(); // 조준 모드
            Shoot(); // 사격

            if (_starterAssetsInputs.reload && !_starterAssetsInputs.shoot && !_isReloading)
            {
                Debug.Log("장전 시작");
                Reload();
            }
        }

        // 조준 모드
        private void AimMode()
        {
            // 화면 중심에서 레이캐스트를 발사하여 조준점이 닿는 월드 좌표를 계산
            Vector3 mouseWorldPosition = Vector3.zero;
            Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
            if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimColliderLayerMask))
            {
                debugTransform.position = raycastHit.point;
                mouseWorldPosition = raycastHit.point;
            }

            if (_starterAssetsInputs.aim) // 조준 상태일 경우
            {
                _aimVirtualCamera.gameObject.SetActive(true);
                _playerController.SetSensitivity(aimSensitivity);
                _playerController.SetRotateOnMove(false);
                _starterAssetsInputs.sprint = false;
                _animator.SetLayerWeight(1, 1);

                // 캐릭터가 조준 방향을 부드럽게 바라보도록 설정
                Vector3 worldAimTarget = mouseWorldPosition;
                worldAimTarget.y = transform.position.y;
                Vector3 aimDirection = (worldAimTarget - transform.position).normalized;
                transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 20f);
            }
            else // 비조준 상태
            {
                _aimVirtualCamera.gameObject.SetActive(false);
                _playerController.SetSensitivity(normalSensitivity);
                _playerController.SetRotateOnMove(true);
                _animator.SetLayerWeight(1, 0);
            }
        }

        // 사격
        private void Shoot()
        {
            if (_starterAssetsInputs.shoot && _starterAssetsInputs.aim) // 조준 상태에서만 발사 가능
            {
                _starterAssetsInputs.sprint = false;

                // 총알 생성 및 발사 애니메이션 재생
                Vector3 mouseWorldPosition = debugTransform.position; // 조준점 위치를 활용
                Vector3 aimDir = (mouseWorldPosition - bulletSpawnPoint.position).normalized;
                SpawnRandomMuzzleFlash(); // 총구 화염 표시
                PlayShootSound(); // 총 발사 사운드 재생
                Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.LookRotation(aimDir, Vector3.up));
                _animator.SetTrigger(DoShoot);

                _starterAssetsInputs.shoot = false;
            }
            else
            {
                _starterAssetsInputs.shoot = false;
            }
        }

        // 장전
        private void Reload()
        {
            if (_isReloading) return; // 이미 장전 중이면 실행하지 않음

            _isReloading = true; // 장전 상태 활성화
            _animator.SetTrigger(DoReload); // 장전 애니메이션 실행
            _starterAssetsInputs.reload = false; // 입력 초기화

            // 애니메이션 길이 가져오기
            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0); // 0은 Base Layer
            float reloadAnimationLength = stateInfo.length; // 현재 재생 중인 상태의 애니메이션 길이

            Invoke(nameof(ReloadOut), reloadAnimationLength);
        }

        private void ReloadOut()
        {
            // _weaponController.curAmmo = _weaponController.maxAmmo;
            _starterAssetsInputs.reload = false;
            _isReloading = false; // 장전 상태 초기화
        }
        
        private void SpawnRandomMuzzleFlash()
        {
            // 총구 화염 프리팹 배열 중 랜덤으로 선택
            int randomIndex = Random.Range(0, muzzleFlashPrefabs.Length);
            GameObject selectedMuzzleFlash = muzzleFlashPrefabs[randomIndex];

            // 선택된 총구 화염을 활성화
            GameObject muzzleFlashInstance = Instantiate(
                selectedMuzzleFlash, 
                bulletSpawnPoint.position, 
                bulletSpawnPoint.rotation
            );

            // 일정 시간 후 자동 삭제
            Destroy(muzzleFlashInstance, 0.05f); // 총구 화염은 0.05초 뒤에 삭제
        }
        
        private void PlayShootSound()
        {
            audioSource.PlayOneShot(shootSound);
        }
    }
}