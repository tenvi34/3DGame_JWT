using Cinemachine;
using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;
using Weapon;

namespace Player
{
    public class AimController : MonoBehaviour
    {
        [SerializeField] private CinemachineVirtualCamera _aimVirtualCamera;
        [SerializeField] private float normalSensitivity;
        [SerializeField] private float aimSensitivity;
        [SerializeField] private LayerMask aimColliderLayerMask;
        [SerializeField] private Transform debugTransform;
        [SerializeField] private Transform bulletPrefab;
        [SerializeField] private Transform bulletSpawnPoint;
        
        private bool _isReloading = false; // 장전중인걸 확인
        
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
            // 화면 중심에서 레이캐스트를 발사하여 조준점이 닿는 월드 좌표를 계산하고,
            // 충돌 지점 정보를 디버깅용 Transform에 반영
            Vector3 mouseWorldPosition = Vector3.zero;
            Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
            if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimColliderLayerMask))
            {
                debugTransform.position = raycastHit.point;
                mouseWorldPosition = raycastHit.point;
            }

            if (_starterAssetsInputs.aim) // 조준(견착) 상태일 경우
            {
                // 조준용 가상 카메라를 활성화하고, 조준 시의 민감도를 설정하며,
                // 캐릭터가 이동 시 회전하지 않도록 설정
                _aimVirtualCamera.gameObject.SetActive(true);
                _playerController.SetSensitivity(aimSensitivity);
                _playerController.SetRotateOnMove(false);
                _starterAssetsInputs.sprint = false;

                // 조준점의 위치를 기준으로 캐릭터가 해당 방향을 부드럽게 바라보도록 설정
                Vector3 worldAimTarget = mouseWorldPosition;
                worldAimTarget.y = transform.position.y;
                Vector3 aimDirection = (worldAimTarget - transform.position).normalized;
                transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 20f);
            }
            else // 비조준 상태일 경우
            {
                // 조준용 가상 카메라를 비활성화하고, 일반 민감도를 복구하며,
                // 이동 시 캐릭터가 회전하도록 설정
                _aimVirtualCamera.gameObject.SetActive(false);
                _playerController.SetSensitivity(normalSensitivity);
                _playerController.SetRotateOnMove(true);
            }

            if (_starterAssetsInputs.shoot && _starterAssetsInputs.aim) // 조준 상태에서만 발사 가능
            {
                _starterAssetsInputs.sprint = false;
                
                // 조준점과 총알 발사 위치를 기준으로 총알을 생성하고, 발사 애니메이션을 재생
                Vector3 aimDir = (mouseWorldPosition - bulletSpawnPoint.position).normalized;
                Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.LookRotation(aimDir, Vector3.up));
                _animator.SetTrigger(DoShoot);
                
                _starterAssetsInputs.shoot = false;
            }
            else
            {
                _starterAssetsInputs.shoot = false;
            }

            if (_starterAssetsInputs.reload && !_starterAssetsInputs.shoot && !_isReloading)
            {
                Debug.Log("장전 시작");
                Reload();
            }
        }
        
        // 장전
        private void Reload()
        {
            if (_isReloading) return; // 장전 중이면 실행하지 않음

            _isReloading = true; // 장전 상태 활성화
            _animator.SetTrigger(DoReload); // 장전 애니메이션 실행
            // _starterAssetsInputs.reload = false; // 입력 초기화
            
            // 애니메이션 길이 가져오기
            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0); // 0은 Base Layer
            float reloadAnimationLength = stateInfo.length; // 현재 재생 중인 상태의 애니메이션 길이

            Invoke(nameof(ReloadOut), reloadAnimationLength); // 애니메이션 재생이 끝나면 다시 장전 가능 및 탄창 교체 완료
        }
        
        private void ReloadOut()
        {
            // _weaponController.curAmmo = _weaponController.maxAmmo;
            _starterAssetsInputs.reload = false;
            _isReloading = false;
        }
    }
}