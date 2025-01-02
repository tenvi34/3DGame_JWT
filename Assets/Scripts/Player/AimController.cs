using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using Cinemachine;
using StarterAssets;

namespace Player
{
    public class AimController : MonoBehaviour
    {
        [Header("Cinemachine")]
        public CinemachineVirtualCamera thirdPersonCamera; // 기존 3인칭 카메라
        public CinemachineVirtualCamera aimCamera; // 견착 카메라

        private StarterAssetsInputs _inputs;
        private Transform _playerTransform; // 플레이어 Transform 참조
        private Transform _aimCameraTransform; // 견착 카메라 Transform 참조

        private void Start()
        {
            _inputs = GetComponent<StarterAssetsInputs>();
            _playerTransform = transform; // 플레이어 Transform 초기화
            _aimCameraTransform = aimCamera.transform; // 견착 카메라 Transform 초기화
        }

        private void Update()
        {
            HandleAim();
        }

        private void HandleAim()
        {
            if (_inputs.aim) // 우클릭으로 견착 활성화
            {
                aimCamera.Priority = 20; // 견착 카메라 활성화
                thirdPersonCamera.Priority = 10;

                // 캐릭터의 방향에 카메라 방향을 맞춤
                AlignCameraWithPlayer();
            }
            else
            {
                aimCamera.Priority = 10; // 3인칭 카메라 활성화
                thirdPersonCamera.Priority = 20;
            }
        }

        private void AlignCameraWithPlayer()
        {
            // 견착 카메라가 캐릭터의 정면을 바라보도록 설정
            _aimCameraTransform.position = _playerTransform.position + _playerTransform.forward * 0.5f + _playerTransform.up * 1.5f;
            _aimCameraTransform.rotation = Quaternion.LookRotation(_playerTransform.forward);
        }
    }
}