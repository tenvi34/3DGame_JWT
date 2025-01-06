using System;
using Cinemachine;
using StarterAssets;
using UnityEngine;

namespace Player
{
    public class AimController : MonoBehaviour
    {
        // 애니메이션 ID
        private static readonly int IsAiming = Animator.StringToHash("IsAiming");

        [Header("Cinemachine Cameras")]
        public CinemachineVirtualCamera thirdPersonCamera; // 기본 카메라
        public CinemachineVirtualCamera shoulderAimCamera; // 견착 카메라

        [Header("Input")]
        private StarterAssetsInputs _inputs;
        private bool _isAiming;
        
        [Header("Camera Rotation")]
        public float rotationSpeed = 5.0f; // 회전 속도
        private Transform _characterTransform;
        private float _currentYaw;
        private float _currentPitch;

        [Header("Camera Clamp")]
        public float minPitch = -30f; // 카메라 아래쪽 제한
        public float maxPitch = 60f;  // 카메라 위쪽 제한
        
        public bool CheckAiming => _isAiming; // 견착 상태 확인

        private void Start()
        {
            _inputs = GetComponent<StarterAssetsInputs>(); // 입력 클래스 가져오기
            _characterTransform = transform; // 캐릭터 트랜스폼 가져오기
        }

        private void Update()
        {
            HandleAim();
        }

        private void LateUpdate()
        {
            HandleCharacterRotation();
        }

        private void HandleAim()
        {
            // 우클릭 입력 상태 확인
            if (_inputs.aim && Input.GetMouseButton(1) && !_inputs.sprint) // 우클릭 유지
            {
                _isAiming = true;
                ActivateCamera(shoulderAimCamera); // 견착 카메라 활성화
                
                // 캐릭터 방향과 카메라 동기화
                AlignCameraToCharacter();
                
                // 견착 상태 애니메이션 트리거
                TriggerAimAnimation(true);
            }
            else // 우클릭 해제 시
            {
                _isAiming = false;
                ActivateCamera(thirdPersonCamera); // 기본 카메라로 복귀
                
                // 견착 상태 애니메이션 트리거
                TriggerAimAnimation(false);
            }
        }

        // 견착 상태에서 캐릭터 회전
        private void HandleCharacterRotation()
        {
            if (_isAiming) // 견착 상태일 때만 캐릭터 회전
            {
                // 마우스 입력 값
                float mouseX = _inputs.look.x * rotationSpeed * Time.deltaTime;
                float mouseY = _inputs.look.y * rotationSpeed * Time.deltaTime;

                // 카메라 회전 값 업데이트
                _currentYaw += mouseX;
                _currentPitch = Mathf.Clamp(_currentPitch - mouseY, minPitch, maxPitch);

                // 카메라 회전 적용
                shoulderAimCamera.transform.rotation = Quaternion.Euler(_currentPitch, _currentYaw, 0);

                // 캐릭터의 회전도 카메라의 Y축 회전 값에 동기화
                _characterTransform.rotation = Quaternion.Euler(0, shoulderAimCamera.transform.eulerAngles.y, 0);
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
        
        private void AlignCameraToCharacter()
        {
            // 캐릭터가 바라보고있는 방향으로 카메라 설정
            var characterRotation = transform.root.eulerAngles.y;
            shoulderAimCamera.transform.rotation = Quaternion.Euler(0, characterRotation, 0);
        }
        
        private void TriggerAimAnimation(bool isAiming)
        {
            Animator animator = GetComponent<Animator>();
            if (animator) animator.SetBool(IsAiming, isAiming);
        }
        
        public void CancelAiming()
        {
            _isAiming = false;
            ActivateCamera(thirdPersonCamera); // 기본 카메라로 전환
            TriggerAimAnimation(false); // 견착 상태 해제 애니메이션
        }
    }
}