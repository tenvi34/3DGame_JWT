using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;

/* Note: animations are called via the controller for both the character and capsule using animator null checks
   참고: 애니메이션은 캐릭터와 캡슐 모두 컨트롤러를 통해 애니메이터의 null 체크를 사용하여 호출됩니다.
 */

namespace Player
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM 
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonControllerCustom : MonoBehaviour
    {
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")] // 캐릭터의 이동 속도 (초당 미터)
        public float MoveSpeed = 2.0f;

        [Tooltip("Sprint speed of the character in m/s")] // 캐릭터의 스프린트 속도 (초당 미터)
        public float SprintSpeed = 5.335f;

        [Tooltip("How fast the character turns to face movement direction")] // 캐릭터가 이동 방향으로 회전하는 속도
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")] // 가속 및 감속
        public float SpeedChangeRate = 10.0f;

        public AudioClip LandingAudioClip; // 착지 소리 클립
        public AudioClip[] FootstepAudioClips; // 발소리 클립
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f; // 발소리 볼륨

        [Space(10)]
        [Tooltip("The height the player can jump")] // 플레이어가 점프할 수 있는 높이
        public float JumpHeight = 1.2f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")] // 캐릭터의 자체 중력 값 (엔진 기본값은 -9.81f)
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")] // 다시 점프하기까지 필요한 시간. 0f로 설정하면 즉시 점프 가능
        public float JumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")] // 낙하 상태로 진입하기 전 필요한 시간. 계단을 내려갈 때 유용
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")] // 캐릭터가 지면에 있는지 여부. CharacterController의 내장 확인 기능과 별개
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")] // 울퉁불퉁한 지면에서 유용
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")] // 지면 확인의 반경. CharacterController의 반경과 일치해야 함
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")] // 캐릭터가 지면으로 사용하는 레이어
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")] // 시네머신 가상 카메라에서 카메라가 따라갈 타겟
        public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")] // 카메라를 위로 이동할 수 있는 최대 각도
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")] // 카메라를 아래로 이동할 수 있는 최대 각도
        public float BottomClamp = -30.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")] // 카메라를 덮어쓰는 추가 각도. 카메라 위치를 세부 조정할 때 유용
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")] // 모든 축에서 카메라 위치를 고정
        public bool LockCameraPosition = false;

        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        // player
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;

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
                return _playerInput.currentControlScheme == "KeyboardMouse"; // 현재 입력 장치가 키보드 및 마우스인지 확인
#else
				return false;
#endif
            }
        }

        private void Awake()
        {
            // get a reference to our main camera
            // 메인 카메라에 대한 참조를 가져옵니다.
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        private void Start()
        {
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
            
            _hasAnimator = TryGetComponent(out _animator); // 애니메이터가 있는지 확인
            _controller = GetComponent<CharacterController>(); // CharacterController 컴포넌트 가져오기
            _input = GetComponent<StarterAssetsInputs>(); // 입력 컴포넌트 가져오기
#if ENABLE_INPUT_SYSTEM 
            _playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            AssignAnimationIDs();

            // reset our timeouts on start
            // 시작 시 타임아웃 값 초기화
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }

        private void Update()
        {
            _hasAnimator = TryGetComponent(out _animator);

            JumpAndGravity(); // 점프 및 중력 처리
            GroundedCheck();  // 지면 상태 확인
            Move();           // 캐릭터 이동
        }

        private void LateUpdate()
        {
            CameraRotation(); // 카메라 회전 처리
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        private void GroundedCheck()
        {
            // set sphere position, with offset
            // 오프셋과 함께 구체의 위치 설정
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

            // update animator if using character
            // 캐릭터가 있을 경우 애니메이터 업데이트
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        private void CameraRotation()
        {
            // if there is an input and camera position is not fixed
            // 입력이 있고 카메라 위치가 고정되지 않은 경우
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                //Don't multiply mouse input by Time.deltaTime;
                // 마우스 입력에는 Time.deltaTime을 곱하지 않습니다.
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            }

            // clamp our rotations so our values are limited 360 degrees
            // 회전을 제한하여 값이 360도를 초과하지 않도록 함
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Cinemachine will follow this target
            // 시네머신이 이 타겟을 따릅니다.
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw, 0.0f);
        }

                private void Move()
        {
            // set target speed based on move speed, sprint speed and if sprint is pressed
            // 이동 속도, 스프린트 속도, 그리고 스프린트 입력 여부에 따라 목표 속도를 설정합니다.
            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon
            // 간단한 가속 및 감속 로직으로, 쉽게 교체하거나 수정할 수 있도록 설계됨

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // 참고: Vector2의 == 연산자는 근사값을 사용하므로 부동소수점 오류에 민감하지 않으며, magnitude 계산보다 더 저렴함
            // if there is no input, set the target speed to 0
            // 입력이 없으면 목표 속도를 0으로 설정합니다.
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            // 플레이어의 현재 수평 속도를 참조
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f; // 속도 변화 임계값
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f; // 입력 강도

            // accelerate or decelerate to target speed
            // 목표 속도까지 가속 또는 감속
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // 선형이 아닌 곡선형 결과를 만들어 더 자연스러운 속도 변화를 제공
                // note T in Lerp is clamped, so we don't need to clamp our speed
                // Lerp의 T 값은 이미 클램핑되므로 속도를 따로 제한할 필요 없음
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);

                // round speed to 3 decimal places
                // 속도를 소수점 세 자리로 반올림
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // normalize input direction
            // 입력 방향을 정규화
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // 참고: Vector2의 != 연산자는 근사값을 사용하므로 부동소수점 오류에 민감하지 않으며, magnitude 계산보다 더 저렴함
            // if there is a move input rotate player when the player is moving
            // 이동 입력이 있을 경우 플레이어를 이동 방향으로 회전
            if (_input.move != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime);

                // rotate to face input direction relative to camera position
                // 카메라 위치를 기준으로 입력 방향을 바라보도록 회전
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            // move the player
            // 플레이어 이동
            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            // update animator if using character
            // 캐릭터를 사용할 경우 애니메이터 업데이트
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                // reset the fall timeout timer
                // 낙하 타임아웃 타이머 초기화
                _fallTimeoutDelta = FallTimeout;

                // update animator if using character
                // 캐릭터를 사용할 경우 애니메이터 업데이트
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                // stop our velocity dropping infinitely when grounded
                // 지면에 있을 때 속도가 무한히 감소하지 않도록 방지
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // Jump
                // 점프
                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    // H * -2 * G의 제곱근 = 원하는 높이에 도달하기 위해 필요한 속도
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    // update animator if using character
                    // 캐릭터를 사용할 경우 애니메이터 업데이트
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                    }
                }

                // jump timeout
                // 점프 타임아웃
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // reset the jump timeout timer
                // 점프 타임아웃 타이머 초기화
                _jumpTimeoutDelta = JumpTimeout;

                // fall timeout
                // 낙하 타임아웃
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    // update animator if using character
                    // 캐릭터를 사용할 경우 애니메이터 업데이트
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
                }

                // if we are not grounded, do not jump
                // 지면에 없을 경우 점프 불가
                _input.jump = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            // 중력 값을 시간이 지남에 따라 적용 (터미널 속도 이하일 경우)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            // 각도를 제한
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f); // 투명한 초록색
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f); // 투명한 빨간색

            if (Grounded) Gizmos.color = transparentGreen; // 지면 상태일 경우 초록색
            else Gizmos.color = transparentRed; // 공중 상태일 경우 빨간색

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            // 선택된 경우, 지면 충돌 반경과 위치에 맞는 기즈모를 그립니다.
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            // 애니메이션 이벤트를 통해 발소리 재생
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length); // 랜덤 발소리 선택
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            // 애니메이션 이벤트를 통해 착지 소리 재생
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }
    }
}