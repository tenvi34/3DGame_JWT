using UnityEngine;

namespace Enemy
{
    public interface IEnemy
    {
        float AttackRange { get; }
        void TryAttack();
        bool IsInAttackRange(Vector3 targetPosition);
    }
}