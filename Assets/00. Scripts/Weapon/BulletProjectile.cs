using UnityEngine;

namespace Weapon
{
    // 발사체의 동작을 담당하는 클래스
    public class BulletProjectile : MonoBehaviour
    {
        [Header("Visual Effects")]
        [SerializeField] private Transform hitVFXNormal; // 일반 물체 피격 효과
        [SerializeField] private Transform hitVFXEnemy; // 적 피격 효과

        [Header("Settings")]
        [SerializeField] private float speed = 50f; // 발사체 속도
        [SerializeField] private float damage = 20f; // 발사체 데미지
        [SerializeField] private float lifeTime = 3f; // 발사체 생존 시간

        private Rigidbody _rigidbody; // 물리 처리를 위한 리지드바디
        private bool _hasHit; // 충돌 여부

        private void Awake()
        {
            // 컴포넌트 초기화
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            // 초기 속도 설정
            _rigidbody.velocity = transform.forward * speed;
            // 일정 시간 후 자동 제거
            Destroy(gameObject, lifeTime);
        }

        // 충돌 처리
        private void OnTriggerEnter(Collider other)
        {
            // 이미 충돌했다면 처리하지 않음
            if (_hasHit) return;
            _hasHit = true;

            // 데미지를 받을 수 있는 대상인지 확인
            if (other.TryGetComponent<IDamageable>(out var target))
            {
                // 충돌 지점과 방향 계산
                Vector3 hitPoint = other.ClosestPoint(transform.position);
                Vector3 hitNormal = transform.position - other.transform.position;
                
                // 데미지 처리
                target.OnDamage(damage, hitPoint, hitNormal);
                
                // 적중 효과 재생
                SpawnHitVFX(hitVFXEnemy, hitPoint, hitNormal);
            }
            else
            {
                // 일반 물체에 적중한 경우
                SpawnHitVFX(hitVFXNormal, transform.position, transform.forward);
            }

            // 발사체 제거
            Destroy(gameObject);
        }

        // 적중 효과 생성
        private void SpawnHitVFX(Transform vfxPrefab, Vector3 position, Vector3 normal)
        {
            if (vfxPrefab == null) return;
            
            // 효과의 회전값 계산 및 생성
            Quaternion rotation = Quaternion.LookRotation(normal);
            Instantiate(vfxPrefab, position, rotation);
        }
    }
}