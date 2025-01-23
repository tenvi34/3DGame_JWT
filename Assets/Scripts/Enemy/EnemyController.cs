using UnityEngine;
using UnityEngine.AI;

namespace Enemy
{
    // 적 상태에 대한 인터페이스
    public interface IEnemyState
    {
        void Enter(EnemyController context);   // 상태 진입 시 호출
        void Update(EnemyController context);  // 상태 업데이트 중 호출
        void Exit(EnemyController context);    // 상태 종료 시 호출
    }

    // 모든 상태의 기본 추상 클래스 - 적이 가지는 공통 동작
    public abstract class BaseEnemyState : IEnemyState
    {
        public virtual void Enter(EnemyController context) { }
        public virtual void Update(EnemyController context) { }
        public virtual void Exit(EnemyController context) { }
    }

    // 순찰 상태 - 정해진 지점들을 순회
    public class PatrolState : BaseEnemyState
    {
        private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");

        // 상태 진입 시 초기 이동 속도 설정
        public override void Enter(EnemyController context)
        {
            context.Animator.SetFloat(MoveSpeed, context.NavMeshAgent.velocity.magnitude);
        }

        // 순찰 지점 순환 및 이동
        public override void Update(EnemyController context)
        {
            // 순찰 지점이 없으면 종료
            if (context.PatrolPoints.Length == 0) return;

            // 현재 순찰 지점에 도착했다면 다음 지점으로 변경
            if (Vector3.Distance(context.transform.position, context.PatrolPoints[context.CurrentPatrolIndex].position) < 1f)
            {
                context.CurrentPatrolIndex = (context.CurrentPatrolIndex + 1) % context.PatrolPoints.Length;
            }

            // 다음 순찰 지점으로 이동
            context.NavMeshAgent.destination = context.PatrolPoints[context.CurrentPatrolIndex].position;
        }
    }

    // 조사 상태 - 수상한 소리가 들린 위치를 조사
    public class InvestigateState : BaseEnemyState
    {
        private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");
        private float _investigationTimer; // 조사 시간 추적

        // 마지막으로 알려진 플레이어 위치로 이동 시작
        public override void Enter(EnemyController context)
        {
            _investigationTimer = 0f;
            context.NavMeshAgent.destination = context.LastKnownPlayerPosition;
        }

        // 일정 시간 후 순찰 상태로 복귀
        public override void Update(EnemyController context)
        {
            _investigationTimer += Time.deltaTime;

            // 조사 시간 초과 시 순찰 상태로 전환
            if (_investigationTimer >= context.InvestigationTime)
            {
                context.TransitionToState(new PatrolState());
                return;
            }

            // 이동 애니메이션 업데이트
            context.Animator.SetFloat(MoveSpeed, context.NavMeshAgent.velocity.magnitude);
        }
    }

    // 추격 상태 - 플레이어를 직접 추적
    public class ChaseState : BaseEnemyState
    {
        // 애니메이터 파라미터 해시값 캐싱
        private static readonly int Attack = Animator.StringToHash("Attack");
        private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");

        public override void Update(EnemyController context)
        {
            if (context.Player == null) return;

            // 플레이어 위치로 이동 및 방향 전환
            context.NavMeshAgent.destination = context.Player.transform.position;
            context.transform.LookAt(context.Player.transform.position);

            // 플레이어와의 거리 계산
            float distanceToPlayer = Vector3.Distance(context.transform.position, context.Player.transform.position);

            // 공격 범위 내 여부 확인
            bool isInAttackRange = distanceToPlayer <= context.NavMeshAgent.stoppingDistance;

            // 공격 범위 내면 공격, 아니면 이동
            if (isInAttackRange)
            {
                context.Animator.SetTrigger(Attack);
            }
            else
            {
                context.Animator.SetFloat(MoveSpeed, context.NavMeshAgent.velocity.magnitude);
            }

            // 플레이어가 감지 범위를 벗어나면 조사 상태로 전환
            if (distanceToPlayer > context.DetectionRadius)
            {
                context.TransitionToState(new InvestigateState());
            }
        }
    }

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
    }
}