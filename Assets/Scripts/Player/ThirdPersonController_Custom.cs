using StarterAssets;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/* 참고: 애니메이션은 캐릭터와 캡슐 모두 컨트롤러를 통해 애니메이터의 null 체크를 사용하여 호출됩니다 */

namespace Player
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonControllerCustom : MonoBehaviour
    {
        [Header("Player")] 
        [Tooltip("캐릭터의 이동 속도 (초당 m/s)")]
        public float MoveSpeed = 2.0f;

        [Tooltip("캐릭터의 스프린트 속도 (초당 m/s)")] 
        public float SprintSpeed = 5.335f;

        [Tooltip("캐릭터가 이동 방향으로 회전하는 속도")] 
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("가속 및 감속")] 
        public float SpeedChangeRate = 10.0f;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Space(10)] 
        [Tooltip("플레이어가 점프할 수 있는 높이")]
        public float JumpHeight = 1.2f;

        [Tooltip("캐릭터가 자체 중력 값을 사용합니다. 엔진의 기본값은 -9.81f입니다.")]
        public float Gravity = -15.0f;

        [Space(10)] [Tooltip("다시 점프할 수 있기까지 필요한 시간. 0f로 설정하면 즉시 점프가 가능합니다.")]
        public float JumpTimeout = 0.50f;

        [Tooltip("낙하 상태로 진입하기까지 필요한 시간. 계단을 내려갈 때 유용합니다.")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")] 
        [Tooltip("캐릭터가 땅에 닿아 있는지 여부. CharacterController에 내장된 Grounded 체크 기능과는 별도입니다.")]
        public bool Grounded = true;

        [Tooltip("울퉁불퉁한 지면에서 유용합니다.")] 
        public float GroundedOffset = -0.14f;

        [Tooltip("Grounded 체크의 반경입니다. CharacterController의 반경과 일치해야 합니다.")]
        public float GroundedRadius = 0.28f;

        [Tooltip("캐릭터가 지면으로 사용하는 레이어입니다.")] 
        public LayerMask GroundLayers;

        [Header("Cinemachine")] [Tooltip("시네머신 가상 카메라에서 따라갈 대상 설정")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("카메라를 위쪽으로 이동할 수 있는 최대 각도 (도 단위)")]
        public float TopClamp = 70.0f;

        [Tooltip("카메라를 아래쪽으로 이동할 수 있는 최대 각도 (도 단위)")]
        public float BottomClamp = -30.0f;

        [Tooltip("카메라를 덮어 쓸 추가 각도 (고정된 카메라 위치 미세 조정에 유용)")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("모든 축에서 카메라 위치를 고정")] 
        public bool LockCameraPosition = false;

        // 시네머신 관련
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        // 플레이어 관련
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // 타임아웃 관련
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // 애니메이션 ID
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;
        private int _animIDDirection;

#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif
        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;

        private const float _threshold = 0.01f;

        private bool _hasAnimator;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse"; // 현재 입력 장치가 키보드/마우스인지 확인
#else
                return false;
#endif
            }
        }

        private void Awake()
        {
            // 메인 카메라에 대한 참조 가져오기
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        private void Start()
        {
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
#else
            Debug.LogError("Starter Assets 패키지에서 종속성이 누락되었습니다." +
                           "Tools/Starter Assets/Reinstall Dependencies를 사용하여 수정해주세요.");
#endif

            AssignAnimationIDs();

            // 타임아웃 초기화
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }

        private void Update()
        {
            _hasAnimator = TryGetComponent(out _animator);

            JumpAndGravity(); // 점프 및 중력 처리
            GroundedCheck(); // 지면 확인
            Move(); // 이동 처리
        }

        private void LateUpdate()
        {
            CameraRotation(); // 카메라 회전 처리
        }

        // 애니메이션 파라미터의 해시 값을 초기화
        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
            _animIDDirection = Animator.StringToHash("Direction");
        }

        // 캐릭터가 지면에 닿아 있는지 확인하고 애니메이터를 업데이트
        private void GroundedCheck()
        {
            // 오프셋을 가진 구체 위치 설정
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

            // 캐릭터가 있을 경우 애니메이터 업데이트
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        // 카메라 회전 처리
        private void CameraRotation()
        {
            // 입력이 있고 카메라 위치가 고정되지 않은 경우
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                // 마우스 입력에 Time.deltaTime을 곱하지 않음
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            }

            // 360도로 제한
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // 시네머신이 이 타겟을 따라감
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw, 0.0f);

            // 캐릭터 회전 동기화
            float characterYaw = _cinemachineTargetYaw;
            transform.rotation = Quaternion.Euler(0.0f, characterYaw, 0.0f);
        }

        // 캐릭터 이동 처리
        private void Move()
        {
            // 이동 속도, 스프린트 속도, 스프린트 입력에 따라 목표 속도 설정
            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

            // 간단한 가속 및 감속 설계
            // 참고: Vector2의 == 연산자는 근사치를 사용하므로 부동소수점 오류에 민감하지 않으며, magnitude보다 효율적임
            // 입력이 없을 경우 목표 속도를 0으로 설정
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            // 플레이어의 현재 수평 속도 참조
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            // 목표 속도까지 가속 또는 감속
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // 선형 대신 곡선을 생성하여 유기적인 속도 변화를 제공
                // Lerp의 T는 클램프되므로 속도를 제한할 필요 없음
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

                // 속도를 소수점 3자리로 반올림
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // 입력 방향 정규화
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            // 참고: Vector2의 != 연산자는 근사치를 사용하여 부동소수점 오류에 민감하지 않음
            // 이동 입력이 있을 경우 플레이어를 입력 방향으로 회전
            if (_input.move != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);

                // 카메라를 기준으로 입력 방향을 바라보도록 회전
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            // 플레이어 이동
            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            // 애니메이터 업데이트
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
                
                // Direction 값 업데이트 (좌우 이동 감지)
                // float direction = _input.move.x; // A: -1, D: 1
                // _animator.SetFloat(_animIDDirection, direction);
            }
        }

        // 점프 및 중력 처리
        private void JumpAndGravity()
        {
            if (Grounded)
            {
                // 낙하 타임아웃 타이머 초기화
                _fallTimeoutDelta = FallTimeout;

                // 캐릭터가 있으면 애니메이터 업데이트
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                // Grounded 상태에서 속도가 무한히 감소하지 않도록 방지
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // 점프
                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    // sqrt(H * -2 * G) = 원하는 높이에 도달하기 위해 필요한 속도 계산
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    // 캐릭터가 있으면 애니메이터 업데이트
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                    }
                }

                // 점프 타임아웃
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // 점프 타임아웃 타이머 초기화
                _jumpTimeoutDelta = JumpTimeout;

                // 낙하 타임아웃
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    // 캐릭터가 있으면 애니메이터 업데이트
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
                }

                // 지면에 있지 않을 경우 점프 입력 비활성화
                _input.jump = false;
            }

            // 터미널 속도 이하라면 중력을 계속 적용 
            // (DeltaTime을 두 번 곱해 시간이 지날수록 선형적으로 속도 증가)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        // 각도를 최소 및 최대 값으로 제한
        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            // 각도가 -360° 미만이거나 360° 초과하는 경우 정규화
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;

            // 각도를 주어진 최소와 최대 값으로 제한
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        // 캐릭터가 지면에 닿아 있는지 확인하는 구체 그리기
        private void OnDrawGizmosSelected()
        {
            // 투명한 초록색과 빨간색 색상 정의
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            // Grounded 상태에 따라 색상을 변경
            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // 선택되었을 때, Grounded 확인 구체와 동일한 위치 및 반지름으로 Gizmo 그리기
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }

        // 애니메이션 이벤트를 통해 발소리 재생
        private void OnFootstep(AnimationEvent animationEvent)
        {
            // 애니메이션 이벤트의 클립 가중치가 0.5 초과인 경우
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                // 발자국 오디오 클립이 존재할 경우
                if (FootstepAudioClips.Length > 0)
                {
                    // 랜덤하게 오디오 클립을 선택
                    var index = Random.Range(0, FootstepAudioClips.Length);

                    // 클립을 현재 캐릭터 위치에서 재생
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center),
                        FootstepAudioVolume);
                }
            }
        }

        // 애니메이션 이벤트를 통해 착지 소리 재생
        private void OnLand(AnimationEvent animationEvent)
        {
            // 애니메이션 이벤트의 클립 가중치가 0.5 초과인 경우
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                // 착지 효과음을 현재 캐릭터 위치에서 재생
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center),
                    FootstepAudioVolume);
            }
        }
    }
}