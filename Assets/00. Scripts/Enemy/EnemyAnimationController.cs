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
        
        
    }
}