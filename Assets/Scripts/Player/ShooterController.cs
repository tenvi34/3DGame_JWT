using Cinemachine;
using StarterAssets;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Player
{
    public class ShooterController : MonoBehaviour
    {
        [Header("Camera Settings")] [SerializeField]
        private CinemachineVirtualCamera aimCamera; // 조준 카메라

        [SerializeField] private float normalSensitivity; // 일반 상태 감도
        [SerializeField] private float aimSensitivity; // 조준 상태 감도
        [SerializeField] private LayerMask aimMask; // 조준 가능한 레이어

        [Header("Weapon Settings")] [SerializeField]
        private Transform bulletPrefab; // 발사체 프리팹

        [SerializeField] private Transform bulletSpawnPoint; // 발사 위치
        [SerializeField] private GameObject[] muzzleFlashPrefabs; // 총구 화염 효과

        [Header("Audio")] [SerializeField] private AudioSource audioSource; // 오디오 소스
        [SerializeField] private AudioClip shootSound; // 발사 소리
        [SerializeField] private AudioClip reloadSound; // 재장전 소리

        public int maxBullet = 30; // 최대 탄약 수
        public int currentBullet { get; private set; } // 현재 탄약 수

        private bool _isReloading; // 재장전 중 여부
        private Animator _animator; // 애니메이터
        private StarterAssetsInputs _input; // 입력 시스템

        // 애니메이터 해시값 캐싱
        private static readonly int ShootTrigger = Animator.StringToHash("DoShoot");
        private static readonly int ReloadTrigger = Animator.StringToHash("DoReload");

        private void Awake()
        {
            // 컴포넌트 초기화
            _animator = GetComponent<Animator>();
            _input = GetComponent<StarterAssetsInputs>();
            currentBullet = maxBullet;
        }

        private void Update()
        {
            // 매 프레임 처리할 기능들
            HandleAiming();
            HandleShooting();
            HandleReload();
        }

        // 조준 처리
        private void HandleAiming()
        {
            // 화면 중앙에서 레이캐스트를 발사하여 조준점 위치 계산
            Vector3 mouseWorldPosition = Vector3.zero;
            Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Ray ray = Camera.main.ScreenPointToRay(screenCenter);

            if (Physics.Raycast(ray, out RaycastHit hit, 999f, aimMask))
            {
                mouseWorldPosition = hit.point;
            }

            // 조준 상태일 때
            if (_input.aim)
            {
                // 조준 카메라 활성화 및 감도 조정
                aimCamera.gameObject.SetActive(true);
                GetComponent<ThirdPersonController>().SetSensitivity(aimSensitivity);
                GetComponent<ThirdPersonController>().SetRotateOnMove(false);
                _input.sprint = false;
                _animator.SetLayerWeight(1, 1);

                // 캐릭터를 조준 방향으로 부드럽게 회전
                Vector3 worldAimTarget = mouseWorldPosition;
                worldAimTarget.y = transform.position.y;
                Vector3 aimDirection = (worldAimTarget - transform.position).normalized;
                transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 20f);
            }
            else
            {
                // 일반 상태로 전환
                aimCamera.gameObject.SetActive(false);
                GetComponent<ThirdPersonController>().SetSensitivity(normalSensitivity);
                GetComponent<ThirdPersonController>().SetRotateOnMove(true);
                _animator.SetLayerWeight(1, 0);
            }
        }

        // 발사 처리
        private void HandleShooting()
        {
            if (_input.shoot && _input.aim && !_isReloading && currentBullet > 0)
            {
                // 조준점 위치 계산
                Vector3 mouseWorldPosition = Vector3.zero;
                Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
                Ray ray = Camera.main.ScreenPointToRay(screenCenter);

                if (Physics.Raycast(ray, out RaycastHit hit, 999f, aimMask))
                {
                    mouseWorldPosition = hit.point;
                }

                // 발사 처리
                Vector3 aimDir = (mouseWorldPosition - bulletSpawnPoint.position).normalized;
                SpawnMuzzleFlash();
                audioSource.PlayOneShot(shootSound);

                // 발사체 생성
                Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.LookRotation(aimDir, Vector3.up));
                currentBullet -= 1;
                _animator.SetTrigger(ShootTrigger);

                // UI 갱신
                UIManager.Instance.UpdateAmmoText(currentBullet, maxBullet);
            }

            _input.shoot = false;
        }

        // 재장전 처리
        private void HandleReload()
        {
            if (_input.reload && !_isReloading && currentBullet < maxBullet)
            {
                StartReload();
            }
        }

        // 재장전 시작
        private void StartReload()
        {
            _isReloading = true;
            _animator.SetTrigger(ReloadTrigger);
            audioSource.PlayOneShot(reloadSound);
            Invoke(nameof(CompleteReload), 1f);
        }

        // 재장전 완료
        private void CompleteReload()
        {
            currentBullet = maxBullet;
            _isReloading = false;
            _input.reload = false;
            UIManager.Instance.UpdateAmmoText(currentBullet, maxBullet);
        }

        // 총구 화염 효과 생성
        private void SpawnMuzzleFlash()
        {
            int randomIndex = Random.Range(0, muzzleFlashPrefabs.Length);
            GameObject muzzleFlash = Instantiate(
                muzzleFlashPrefabs[randomIndex],
                bulletSpawnPoint.position,
                bulletSpawnPoint.rotation
            );

            // 짧은 시간 후 제거
            Destroy(muzzleFlash, 0.05f);
        }
    }
}