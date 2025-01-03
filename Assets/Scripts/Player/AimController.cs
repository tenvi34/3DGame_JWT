using Cinemachine;
using StarterAssets;
using UnityEngine;

namespace Player
{
    public class AimController : MonoBehaviour
    {
        [Header("Cinemachine Cameras")]
        public CinemachineVirtualCamera thirdPersonCamera; // 기본 카메라
        public CinemachineVirtualCamera shoulderAimCamera; // 견착 카메라

        [Header("Input")]
        private StarterAssetsInputs _inputs; // 입력 시스템
        private bool _isAiming; // 견착 상태

        private void Start()
        {
            _inputs = GetComponent<StarterAssetsInputs>(); // 입력 클래스 가져오기
        }

        private void Update()
        {
            HandleAim();
        }

        private void HandleAim()
        {
            // 우클릭 입력 상태 확인
            if (_inputs.aim && Input.GetMouseButton(1)) // 우클릭 유지
            {
                _isAiming = true;
                ActivateCamera(shoulderAimCamera); // 견착 카메라 활성화
            }
            else // 우클릭 해제 시
            {
                _isAiming = false;
                ActivateCamera(thirdPersonCamera); // 기본 카메라로 복귀
            }
        }

        private void ActivateCamera(CinemachineVirtualCamera camera)
        {
            // 모든 카메라의 우선순위를 기본값으로 설정
            thirdPersonCamera.Priority = 10;
            shoulderAimCamera.Priority = 10;

            // 활성화할 카메라의 우선순위를 높임
            camera.Priority = 20;
        }
    }
}