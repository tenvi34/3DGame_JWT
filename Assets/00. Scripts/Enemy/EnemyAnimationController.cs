using UnityEngine;

namespace Enemy
{
    public class EnemyAnimationController
    {
        private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");

        // 애니메이션 상수 값 정의
        private const float IDLE_SPEED = 0f; //  idle 애니메이션
        private const float WALK_SPEED = 1f; //  걷기 애니메이션
        private const float RUN_SPEED = 2f; // 달리기 애니메이션

        // 속도에 따른 애니메이션 파라미터 값 계산
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
                speedRatio = currentSpeed / walkSpeed;
                return Mathf.Lerp(IDLE_SPEED, WALK_SPEED, speedRatio);
            }
            // 현재 속도 > 걷기 속도
            else
            {
                // 1(walk) ~ 2(run) 사이로 보간
                speedRatio = (currentSpeed - walkSpeed) / (runSpeed - walkSpeed);
                return Mathf.Lerp(WALK_SPEED, RUN_SPEED, speedRatio);
            }
        }

        // 현재 속도에 따른 애니메이션 업데이트
        public static void UpdateAnimation(EnemyController controller)
        {
            var enemy = controller.GetComponent<IEnemy>();
            if (enemy == null) return;

            float currentSpeed = controller.NavMeshAgent.velocity.magnitude;
            float animatorParameter = ConvertVelocityToAnimParameter(currentSpeed, enemy.WalkSpeed, enemy.RunSpeed);

            controller.Animator.SetFloat(MoveSpeed, animatorParameter);
        }

        // 애니메이션 상태 설정(Idle-Walk-Run 상태 전환 시 사용)
        public static void SetAnimation(EnemyController controller, bool isRunning)
        {
            var enemy = controller.GetComponent<IEnemy>();
            if (enemy == null) return;

            float targetSpeed = isRunning ? enemy.RunSpeed : enemy.WalkSpeed;
            float animationParameter = ConvertVelocityToAnimParameter(targetSpeed, enemy.WalkSpeed, enemy.RunSpeed);

            controller.Animator.SetFloat(MoveSpeed, animationParameter);
        }
    }
}