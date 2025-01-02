using Cinemachine;
using StarterAssets;
using UnityEngine;

namespace Player
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Player Settings")]
        public float moveSpeed = 5.0f; // 기본 이동 속도
        public float sprintSpeed = 10.0f; // 스프린트 속도
        public float rotationSpeed = 10.0f; // 회전 속도
        public float jumpHeight = 1.2f; // 점프 높이
        public float gravity = -15.0f; // 중력 값

        [Header("Camera Settings")]
        public CinemachineVirtualCamera thirdPersonCamera; // 일반 상태 카메라
        public CinemachineVirtualCamera aimCamera; // 견착 상태 카메라
        public Transform cameraRoot; // 카메라가 따라갈 대상 (플레이어 머리 또는 어깨)
        public float mouseSensitivity = 100f; // 마우스 감도
        public float topClamp = 70.0f; // 카메라 위쪽 각도 제한
        public float bottomClamp = -30.0f; // 카메라 아래쪽 각도 제한

        [Header("Sound Settings")]
        public AudioClip landingAudioClip; // 착지 소리
        public AudioClip[] footstepAudioClips; // 발소리
        [Range(0, 1)] public float footstepAudioVolume = 0.5f; // 발소리 볼륨

        private CharacterController _characterController;
        private StarterAssetsInputs _inputs;
        private Animator _animator;

        private float _speed;
        private float _verticalVelocity;
        private float _yaw;
        private float _pitch;
        private bool _isAiming;
        private bool _grounded;
        
        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;

        private void Start()
        {
            _characterController = GetComponent<CharacterController>();
            _inputs = GetComponent<StarterAssetsInputs>();
            _animator = GetComponent<Animator>();
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            HandleInput();
            HandleMovement();
            HandleRotation();
            HandleAim();
            HandleGravityAndJump();
        }
        
        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        private void HandleInput()
        {
            _isAiming = _inputs.aim; // 마우스 우클릭 입력
        }

        private void HandleMovement()
        {
            // 속도 설정 (스프린트 여부 확인)
            float targetSpeed = _inputs.sprint ? sprintSpeed : moveSpeed;
            Vector3 moveDirection = transform.forward * _inputs.move.y + transform.right * _inputs.move.x;

            // 이동 처리
            _characterController.Move(moveDirection.normalized * targetSpeed * Time.deltaTime + new Vector3(0, _verticalVelocity, 0) * Time.deltaTime);

            // 애니메이터 업데이트
            if (_animator != null)
            {
                _animator.SetFloat("Speed", targetSpeed);
                _animator.SetBool("Grounded", _grounded);
            }
        }

        private void HandleRotation()
        {
            // 마우스 입력에 따른 회전 처리
            _yaw += _inputs.look.x * mouseSensitivity * Time.deltaTime;
            _pitch -= _inputs.look.y * mouseSensitivity * Time.deltaTime;
            _pitch = Mathf.Clamp(_pitch, bottomClamp, topClamp);

            // 캐릭터 회전
            transform.rotation = Quaternion.Euler(0.0f, _yaw, 0.0f);

            // 카메라 회전
            cameraRoot.localRotation = Quaternion.Euler(_pitch, 0.0f, 0.0f);
        }

        private void HandleAim()
        {
            // 견착 상태 전환
            if (_isAiming)
            {
                aimCamera.Priority = 20;
                thirdPersonCamera.Priority = 10;
            }
            else
            {
                aimCamera.Priority = 10;
                thirdPersonCamera.Priority = 20;
            }
        }

        private void HandleGravityAndJump()
        {
            if (_grounded)
            {
                _verticalVelocity = -2f;

                // 점프 처리
                if (_inputs.jump)
                {
                    _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                    if (_animator != null)
                    {
                        _animator.SetTrigger(_animIDJump);
                    }
                }
            }
            else
            {
                // 공중 상태
                _verticalVelocity += gravity * Time.deltaTime;
            }
        }

        private void OnAnimatorMove()
        {
            // 발소리 재생
            if (_grounded && _characterController.velocity.magnitude > 0.1f && footstepAudioClips.Length > 0)
            {
                int index = Random.Range(0, footstepAudioClips.Length);
                AudioSource.PlayClipAtPoint(footstepAudioClips[index], transform.position, footstepAudioVolume);
            }
        }
    }
}
