using UnityEngine;

namespace Enemy
{
    public class EnemyAnimationController
    {
        private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");

        // 애니메이션 상수 값 정의
        private const float IDLE_SPEED = 0f; // idle 애니메이션
        private const float WALK_SPEED = 1f; // 걷기 애니메이션
        private const float RUN_SPEED = 2f; // 달리기 애니메이션

        // 부드러운 애니메이션 전환을 위한 보간 속도
        private const float ANIMATION_SMOOTHING = 5f;

        // 속도에 따른 애니메이션 파라미터 값 계산 (개선된 버전)
        public static float ConvertVelocityToAnimParameter(float currentSpeed, float walkSpeed, float runSpeed)
        {
            // 정지 상태면 idle
            if (currentSpeed < 0.1f) return IDLE_SPEED;

            // 걷기와 달리기 사이의 비율 계산
            float speedRatio;

            // 현재 속도 <= 걷기 속도
            if (currentSpeed <= walkSpeed)
            {
                // 0(idle) ~ 1(walk) 사이로 보간
                speedRatio = Mathf.Clamp01(currentSpeed / walkSpeed);
                return Mathf.Lerp(IDLE_SPEED, WALK_SPEED, speedRatio);
            }
            // 현재 속도 > 걷기 속도
            else
            {
                // 1(walk) ~ 2(run) 사이로 보간
                speedRatio = Mathf.Clamp01((currentSpeed - walkSpeed) / (runSpeed - walkSpeed));
                return Mathf.Lerp(WALK_SPEED, RUN_SPEED, speedRatio);
            }
        }

        // 부드러운 애니메이션 업데이트
        public static void UpdateAnimation(EnemyController controller)
        {
            var enemy = controller.GetComponent<IEnemy>();
            if (enemy == null) return;

            // 실제 이동 속도 측정
            float currentSpeed = controller.NavMeshAgent.velocity.magnitude;

            // 목표 애니메이션 파라미터 값 계산
            float targetAnimParameter = ConvertVelocityToAnimParameter(currentSpeed, enemy.WalkSpeed, enemy.RunSpeed);

            // 현재 애니메이션 파라미터 값 가져오기
            float currentAnimParameter = controller.Animator.GetFloat(MoveSpeed);

            // 부드러운 보간 적용
            float newAnimParameter = Mathf.Lerp(
                currentAnimParameter,
                targetAnimParameter,
                Time.deltaTime * ANIMATION_SMOOTHING
            );

            // 애니메이터에 값 설정
            controller.Animator.SetFloat(MoveSpeed, newAnimParameter);
        }

        // 상태 전환 시 즉각적인 애니메이션 설정
        public static void SetAnimation(EnemyController controller, bool isRunning)
        {
            var enemy = controller.GetComponent<IEnemy>();
            if (enemy == null) return;

            // NavMeshAgent 속도와 애니메이션 동기화
            if (isRunning)
            {
                controller.NavMeshAgent.speed = enemy.RunSpeed;
                controller.Animator.SetFloat(MoveSpeed, RUN_SPEED);
            }
            else
            {
                controller.NavMeshAgent.speed = enemy.WalkSpeed;
                controller.Animator.SetFloat(MoveSpeed, WALK_SPEED);
            }
        }

        // 정지 상태 애니메이션 설정
        public static void SetIdleAnimation(EnemyController controller)
        {
            controller.Animator.SetFloat(MoveSpeed, IDLE_SPEED);
        }
    }
}