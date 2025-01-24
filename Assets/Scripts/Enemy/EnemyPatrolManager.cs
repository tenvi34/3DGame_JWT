using System.Linq;
using UnityEngine;

namespace Enemy
{
    public class EnemyPatrolManager : MonoBehaviour
    {
        // 전체 맵의 순찰 포인트들을 저장할 배열
        public Transform[] GlobalPatrolPoints;

        // 가장 가까운 순찰 포인트들을 찾아 할당
        public void AssignNearestPatrolPoints(EnemyController enemyController, int patrolPointCount = 3)
        {
            if (GlobalPatrolPoints.Length == 0) return;

            // 현재 좀비 위치에서 가장 가까운 순찰 포인트들 선택
            var nearestPoints = GlobalPatrolPoints
                .OrderBy(point => Vector3.Distance(enemyController.transform.position, point.position))
                .Take(patrolPointCount)
                .ToArray();

            enemyController.PatrolPoints = nearestPoints;
        }
    }
}