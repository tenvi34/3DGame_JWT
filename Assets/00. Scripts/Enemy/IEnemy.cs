using UnityEngine;

namespace Enemy
{
    public interface IEnemy
    {
        float AttackRange { get; }
        float WalkSpeed { get; }
        float RunSpeed { get; }
        void TryAttack();
        bool IsInAttackRange(Vector3 targetPosition);
    }
}