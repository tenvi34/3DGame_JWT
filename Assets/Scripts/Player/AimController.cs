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
        
        private StarterAssetsInputs _starterAssetsInputs;
        private ThirdPersonController _playerController;

        private void Awake()
        {
            _starterAssetsInputs = GetComponent<StarterAssetsInputs>();
            _playerController = GetComponent<ThirdPersonController>();
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
                Vector3 aimDir = (mouseWorldPosition - bulletSpawnPoint.position).normalized;
                Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.LookRotation(aimDir, Vector3.up));
                _starterAssetsInputs.shoot = false;
            }
            else
            {
                _starterAssetsInputs.shoot = false;
            }
        }
    }
}