using UnityEngine;
using UnityEngine.AI;

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
        private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");

        // 상태 진입 시 초기 이동 속도 설정
        public override void Enter(EnemyController controller)
        {
            controller.Animator.SetFloat(MoveSpeed, controller.NavMeshAgent.velocity.magnitude);
        }

        // 순찰 지점 순환 및 이동
        public override void Update(EnemyController controller)
        {
            // 순찰 지점이 없으면 종료
            if (controller.PatrolPoints.Length == 0) return;

            // 현재 순찰 지점에 도착했다면 다음 지점으로 변경
            if (Vector3.Distance(controller.transform.position, controller.PatrolPoints[controller.CurrentPatrolIndex].position) < 1f)
            {
                controller.CurrentPatrolIndex = (controller.CurrentPatrolIndex + 1) % controller.PatrolPoints.Length;
            }

            // 다음 순찰 지점으로 이동
            controller.NavMeshAgent.destination = controller.PatrolPoints[controller.CurrentPatrolIndex].position;
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

    // 추격 상태 - 플레이어를 직접 추적
    public class ChaseState : BaseEnemyState
    {
        // 애니메이터 파라미터 해시값 캐싱
        private static readonly int Attack = Animator.StringToHash("Attack");
        private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");

        public override void Update(EnemyController controller)
        {
            if (controller.Player == null) return;

            // 플레이어 위치로 이동 및 방향 전환
            controller.NavMeshAgent.destination = controller.Player.transform.position;
            controller.transform.LookAt(controller.Player.transform.position);

            // 플레이어와의 거리 계산
            float distanceToPlayer = Vector3.Distance(controller.transform.position, controller.Player.transform.position);

            // 공격 범위 내 여부 확인
            bool isInAttackRange = distanceToPlayer <= controller.NavMeshAgent.stoppingDistance;

            // 공격 범위 내면 공격, 아니면 이동
            if (isInAttackRange)
            {
                controller.Animator.SetTrigger(Attack);
            }
            else
            {
                controller.Animator.SetFloat(MoveSpeed, controller.NavMeshAgent.velocity.magnitude);
            }

            // 플레이어가 감지 범위를 벗어나면 조사 상태로 전환
            if (distanceToPlayer > controller.DetectionRadius)
            {
                controller.TransitionToState(new InvestigateState());
            }
        }
    }
    
    // 공격 상태 - 근접 공격
    public class MeleeAttackState : BaseEnemyState
    {
        private static readonly int Attack = Animator.StringToHash("Attack");
        private float _attackCoolTime = 1f;
        private float _lastAttackTime;

        public override void Enter(EnemyController controller)
        {
            // 플레이어를 바라보도록 설정
            controller.transform.LookAt(controller.Player.transform.position);
        }

        public override void Update(EnemyController controller)
        {
            // 플레이어가 없거나 공격 범위를 벗어나면 순찰 상태로 전환
            if (controller.Player == null 
                || Vector3.Distance(controller.transform.position, controller.Player.transform.position) > controller.NavMeshAgent.stoppingDistance)
            {
                controller.TransitionToState(new PatrolState());
                return;
            }
            
            // 공격 쿨타임 확인
            if (Time.time - _lastAttackTime >= _attackCoolTime)
            {
                controller.Animator.SetTrigger(Attack);
                _lastAttackTime = Time.time;
                
                // 플레이어에게 데미지 주는 로직 구현 예정
                
            }
        }
    }
    
    // 원거리 공격 상태 - 발사체 공격
    public class RangedAttackState : BaseEnemyState
    {
        private static readonly int RangedAttack = Animator.StringToHash("RangedAttack");
        private float _attackCooldown = 2f;
        private float _lastAttackTime;
        public GameObject ProjectilePrefab; // 발사체 프리팹

        public override void Enter(EnemyController controller)
        {
            // 플레이어를 바라보도록 설정
            controller.transform.LookAt(controller.Player.transform.position);
        }

        public override void Update(EnemyController controller)
        {
            // 플레이어가 없거나 공격 범위를 벗어나면 순찰 상태로 전환
            if (controller.Player == null || 
                Vector3.Distance(controller.transform.position, controller.Player.transform.position) > controller.RangedAttackRadius)
            {
                controller.TransitionToState(new PatrolState());
                return;
            }

            // 공격 쿨다운 체크
            if (Time.time - _lastAttackTime >= _attackCooldown)
            {
                // 원거리 공격 애니메이션 트리거
                controller.Animator.SetTrigger(RangedAttack);
                _lastAttackTime = Time.time;

                // 발사체 생성
                SpawnProjectile(controller);
            }
        }

        private void SpawnProjectile(EnemyController controller)
        {
            if (ProjectilePrefab == null) return;

            // 발사체 생성 및 방향 설정
            GameObject projectile = Object.Instantiate(
                ProjectilePrefab, 
                controller.transform.position + controller.transform.forward + Vector3.up, 
                Quaternion.LookRotation(controller.Player.transform.position - controller.transform.position)
            );

            // 발사체에 속도/방향 부여 (추가 로직 필요)
            Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();
            if (projectileRb != null)
            {
                projectileRb.velocity = controller.transform.forward * 10f;
            }
        }
    }
    
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    // 적 AI의 주요 컨트롤러 클래스
    public class EnemyController : MonoBehaviour
    {
        // 현재 상태 저장
        private IEnemyState _currentState;

        public NavMeshAgent NavMeshAgent { get; private set; }
        public Animator Animator { get; private set; }
        public GameObject Player { get; private set; }

        // 순찰 관련 변수
        public Transform[] PatrolPoints;  // 순찰할 지점들
        public int CurrentPatrolIndex = 0;  // 현재 순찰 지점 인덱스

        // 감지 및 행동 설정 변수
        public float DetectionRadius = 10f;  // 플레이어 감지 반경
        public float InvestigationTime = 5f;  // 조사 지속 시간
        public Vector3 LastKnownPlayerPosition;  // 마지막으로 알려진 플레이어 위치
        
        // 공격 관련 변수
        public float RangedAttackRadius = 15f;

        // 공격 타입
        public enum AttackType
        {
            Melee,
            Ranged
        }

        // 초기화 - 기본 상태를 순찰 상태로 설정
        private void Start()
        {
            NavMeshAgent = GetComponent<NavMeshAgent>();
            Animator = GetComponent<Animator>();
            Player = GameObject.FindGameObjectWithTag("Player");

            TransitionToState(new PatrolState());
        }

        // 현재 상태 업데이트
        private void Update()
        {
            _currentState?.Update(this);
        }

        // 상태 전환 메서드
        public void TransitionToState(IEnemyState newState)
        {
            _currentState?.Exit(this);  // 현재 상태 종료
            _currentState = newState;   // 새 상태로 변경
            _currentState?.Enter(this); // 새 상태 진입
        }

        // 소리에 반응하여 조사 상태로 전환
        public void ReactToPlayerSound()
        {
            LastKnownPlayerPosition = Player.transform.position;
            TransitionToState(new InvestigateState());
        }

        // 플레이어 추격 시작
        public void StartChasing()
        {
            TransitionToState(new ChaseState());
        }
        
        // 현재 적의 공격 유형
        public AttackType CurrentAttackType = AttackType.Melee;

        // 공격 상태로 전환하는 메서드 추가
        public void StartAttacking()
        {
            switch (CurrentAttackType)
            {
                case AttackType.Melee:
                    TransitionToState(new MeleeAttackState());
                    break;
                case AttackType.Ranged:
                    TransitionToState(new RangedAttackState());
                    break;
            }
        }
    }
}