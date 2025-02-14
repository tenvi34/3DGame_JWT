using UnityEngine;

namespace Enemy
{
    // 적 상태에 대한 인터페이스
    public interface IEnemyState
    {
        void Enter(EnemyController controller);   // 상태 진입 시 호출
        void Update(EnemyController controller);  // 상태 업데이트 중 호출
        void Exit(EnemyController controller);    // 상태 종료 시 호출
    }

    // 모든 상태의 기본 추상 클래스 - 적이 가지는 공통 동작
    public abstract class BaseEnemyState : IEnemyState
    {
        public virtual void Enter(EnemyController controller) { }
        public virtual void Update(EnemyController controller) { }
        public virtual void Exit(EnemyController controller) { }
    }
    
    /// <summary>
    /// ================================== 상태 추가 ==================================
    /// </summary>

    // 순찰 상태 - 정해진 지점들을 순회
    public class PatrolState : BaseEnemyState
    {
        // 애니메이터 파라미터 해시값 캐싱
        private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");

        // 상태 진입 시 초기 이동 속도 설정
        public override void Enter(EnemyController controller)
        {
            controller.NavMeshAgent.destination = controller.GetCurrentPatrolPoint().position;
            controller.Animator.SetFloat(MoveSpeed, controller.NavMeshAgent.velocity.magnitude);
        }

        // 순찰 지점 순환 및 이동
        public override void Update(EnemyController controller)
        {
            if (controller.PatrolPoints == null || controller.PatrolPoints.Length == 0) return;

            // NavMeshAgent가 경로의 끝에 도달했는지 확인
            if (!controller.NavMeshAgent.pathPending && 
                controller.NavMeshAgent.remainingDistance <= controller.NavMeshAgent.stoppingDistance)
            {
                // 다음 순찰 지점으로 이동
                controller.MoveToNextPatrolPoint();
            }

            // 이동 애니메이션 업데이트
            controller.Animator.SetFloat(MoveSpeed, controller.NavMeshAgent.velocity.magnitude);
            
            // 플레이어 감지 레이캐스트
            Vector3 directionToPlayer = (controller.Player.transform.position - controller.transform.position).normalized;
            float distanceToPlayer = Vector3.Distance(controller.transform.position, controller.Player.transform.position);

            // 장애물 레이어 설정
            LayerMask obstacleLayer = LayerMask.GetMask("Default");

            // 감지 반경 내에 있고 장애물이 없는지 확인
            bool isPlayerVisible = !Physics.Raycast(
                controller.transform.position + Vector3.up, // 약간 높은 위치에서 레이 발사
                directionToPlayer, 
                distanceToPlayer, 
                obstacleLayer
            );

            // 플레이어가 감지되면 추격 상태로 전환
            if (distanceToPlayer <= controller.DetectionRadius && isPlayerVisible)
            {
                controller.TransitionToState(new ChaseState());
            }
        }
    }

    // 조사 상태 - 수상한 소리가 들린 위치를 조사
    public class InvestigateState : BaseEnemyState
    {
        private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");
        private float _investigationTimer; // 조사 시간 추적

        // 마지막으로 알려진 플레이어 위치로 이동 시작
        public override void Enter(EnemyController controller)
        {
            _investigationTimer = 0f;
            controller.NavMeshAgent.destination = controller.LastKnownPlayerPosition;
        }

        // 일정 시간 후 순찰 상태로 복귀
        public override void Update(EnemyController controller)
        {
            _investigationTimer += Time.deltaTime;

            // 조사 시간 초과 시 순찰 상태로 전환
            if (_investigationTimer >= controller.InvestigationTime)
            {
                controller.TransitionToState(new PatrolState());
                return;
            }

            // 이동 애니메이션 업데이트
            controller.Animator.SetFloat(MoveSpeed, controller.NavMeshAgent.velocity.magnitude);
        }
    }

    // 추격 상태 - 플레이어를 직접 추적하고 공격 시도
    public class ChaseState : BaseEnemyState
    {
        private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");
        private LayerMask _obstacleLayer = LayerMask.GetMask("Default");

        public override void Update(EnemyController controller)
        {
            if (controller.Player == null) return;

            Vector3 directionToPlayer = controller.Player.transform.position - controller.transform.position;
            float distanceToPlayer = directionToPlayer.magnitude;

            bool isPlayerVisible = !Physics.Raycast(
                controller.transform.position + Vector3.up,
                directionToPlayer.normalized, 
                distanceToPlayer, 
                _obstacleLayer
            );

            if (distanceToPlayer <= controller.DetectionRadius && isPlayerVisible)
            {
                // 플레이어 위치로 이동
                controller.NavMeshAgent.destination = controller.Player.transform.position;
                controller.transform.LookAt(controller.Player.transform.position);
                controller.Animator.SetFloat(MoveSpeed, controller.NavMeshAgent.velocity.magnitude);

                // 공격 범위 확인
                var enemy = controller.GetComponent<IEnemy>();
                if (enemy != null && enemy.IsInAttackRange(controller.Player.transform.position))
                {
                    // 공격 범위 안에 들어오면 공격 상태로 전환
                    controller.TransitionToState(new AttackState());
                }
            }
            else
            {
                controller.LastKnownPlayerPosition = controller.Player.transform.position;
                controller.TransitionToState(new InvestigateState());
            }
        }
    }
    
    // 공격 상태 - 기본 공격 로직
    public class AttackState : BaseEnemyState
    {
        private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");

        public override void Enter(EnemyController controller)
        {
            // 공격 시작 시 플레이어를 바라보도록 설정
            if(controller.Player != null)
            {
                controller.transform.LookAt(controller.Player.transform.position);
            }
        
            // 이동 정지
            controller.NavMeshAgent.isStopped = true;
            controller.Animator.SetFloat(MoveSpeed, 0);
        }

        public override void Update(EnemyController controller)
        {
            if (controller.Player == null)
            {
                controller.TransitionToState(new PatrolState());
                return;
            }

            // 적 컴포넌트 가져오기
            var enemy = controller.GetComponent<IEnemy>();
            if (enemy == null) return;

            float distanceToPlayer = Vector3.Distance(controller.transform.position, controller.Player.transform.position);

            // 공격 범위를 벗어나면 추격 상태로 전환
            if (!enemy.IsInAttackRange(controller.Player.transform.position))
            {
                controller.TransitionToState(new ChaseState());
                return;
            }

            // 플레이어 방향 계속 바라보기
            controller.transform.LookAt(controller.Player.transform.position);
        
            // 공격 시도
            enemy.TryAttack();
        }

        public override void Exit(EnemyController controller)
        {
            // 이동 재개
            controller.NavMeshAgent.isStopped = false;
        }
    }
}