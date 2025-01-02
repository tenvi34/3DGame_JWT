using Cinemachine;
using UnityEngine;

namespace Player
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Player Settings")] public float moveSpeed = 5f; // 캐릭터 이동 속도
        public float rotationSpeed = 10f; // 캐릭터 회전 속도
        public float mouseSensitivity = 100f; // 마우스 회전 감도

        [Header("Cinemachine")] public CinemachineVirtualCamera thirdPersonCamera; // 일반 상태 카메라
        public CinemachineVirtualCamera aimCamera; // 견착 상태 카메라

        [Header("Camera Settings")] public Transform cameraRoot; // 카메라가 따라갈 대상 (플레이어 머리 또는 어깨)
        public Transform characterRoot; // 캐릭터의 기준 위치

        private CharacterController _characterController;
        private Vector2 _moveInput;
        private Vector2 _mouseInput;
        private bool _isAiming;

        private float _yaw; // 카메라 회전을 위한 Y축 값
        private float _pitch; // 카메라 회전을 위한 X축 값

        private void Start()
        {
            _characterController = GetComponent<CharacterController>();
            Cursor.lockState = CursorLockMode.Locked; // 마우스 고정
        }

        private void Update()
        {
            HandleInput(); // 입력 처리
            HandleMovement(); // 캐릭터 이동 처리
            HandleRotation(); // 캐릭터와 카메라 회전 처리
            HandleAim(); // 견착 상태 처리
        }

        private void HandleInput()
        {
            // 이동 입력
            _moveInput.x = Input.GetAxis("Horizontal");
            _moveInput.y = Input.GetAxis("Vertical");

            // 마우스 입력
            _mouseInput.x = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            _mouseInput.y = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            // 견착 상태 입력
            _isAiming = Input.GetMouseButton(1); // 마우스 오른쪽 버튼
        }

        private void HandleMovement()
        {
            // WASD 입력으로 캐릭터 이동
            Vector3 moveDirection = transform.forward * _moveInput.y + transform.right * _moveInput.x;
            _characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
        }

        private void HandleRotation()
        {
            // 마우스 입력으로 카메라와 캐릭터 회전
            _yaw += _mouseInput.x;
            _pitch -= _mouseInput.y;
            _pitch = Mathf.Clamp(_pitch, -30f, 70f); // 상하 회전 제한

            // 캐릭터 회전
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, _yaw, 0f),
                rotationSpeed * Time.deltaTime);

            // 카메라 회전
            cameraRoot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }

        private void HandleAim()
        {
            // 견착 상태 전환
            if (_isAiming)
            {
                aimCamera.Priority = 20; // 견착 카메라 활성화
                thirdPersonCamera.Priority = 10; // 일반 카메라 비활성화
            }
            else
            {
                aimCamera.Priority = 10; // 견착 카메라 비활성화
                thirdPersonCamera.Priority = 20; // 일반 카메라 활성화
            }
        }
    }
}