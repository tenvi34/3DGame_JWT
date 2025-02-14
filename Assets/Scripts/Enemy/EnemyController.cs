using UnityEngine;
using UnityEngine.AI;

namespace Enemy
{
    // 적 AI의 주요 컨트롤러 클래스
    public class EnemyController : MonoBehaviour
    {
        // 현재 상태 저장
        private IEnemyState _currentState;

        // 컴포넌트 참조
        public NavMeshAgent NavMeshAgent { get; private set; }
        public Animator Animator { get; private set; }
        public GameObject Player { get; private set; }

        // 순찰 관련 변수
        public Transform[] PatrolPoints;  // 순찰할 지점들
        private int[] _patrolOrder;       // 각 몬스터별 독립적인 순찰 순서
        public int CurrentPatrolIndex = 0;  // 현재 순찰 지점 인덱스

        // 감지 및 행동 설정 변수
        public float DetectionRadius = 10f;  // 플레이어 감지 반경
        public float InvestigationTime = 5f;  // 조사 지속 시간
        public Vector3 LastKnownPlayerPosition;  // 마지막으로 알려진 플레이어 위치

        // 초기화 - 기본 상태를 순찰 상태로 설정
        private void Start()
        {
            // 컴포넌트 초기화
            NavMeshAgent = GetComponent<NavMeshAgent>();
            Animator = GetComponent<Animator>();
            Player = GameObject.FindGameObjectWithTag("Player");
            
            InitializePatrolOrder();
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
        
        // 순찰 경로 초기화 및 랜덤화
        private void InitializePatrolOrder()
        {
            _patrolOrder = new int[PatrolPoints.Length];
            for (int i = 0; i < PatrolPoints.Length; i++)
            {
                _patrolOrder[i] = i;
            }
            
            // Fisher-Yates 셔플로 순찰 순서 랜덤화
            for (int i = _patrolOrder.Length - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                (_patrolOrder[i], _patrolOrder[randomIndex]) = (_patrolOrder[randomIndex], _patrolOrder[i]);
            }
        }
        
        // 현재 순찰 포인트 반환
        public Transform GetCurrentPatrolPoint()
        {
            return PatrolPoints[_patrolOrder[CurrentPatrolIndex]];
        }

        // 다음 순찰 포인트로 이동
        public void MoveToNextPatrolPoint()
        {
            CurrentPatrolIndex = (CurrentPatrolIndex + 1) % _patrolOrder.Length;
            NavMeshAgent.destination = GetCurrentPatrolPoint().position;
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