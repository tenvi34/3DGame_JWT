using UnityEngine;

namespace Weapon
{
    public class BulletProjectile : MonoBehaviour
    {
        [SerializeField] private Transform vfxHitGreen;
        [SerializeField] private Transform vfxHitRed;
        [SerializeField] private float speed = 50f;
        [SerializeField] private float destroyTime = 3f;
        [SerializeField] private float damage = 20f;

        private Rigidbody _bulletRigidbody;

        private void Awake()
        {
            _bulletRigidbody = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            destroyTime -= Time.deltaTime;
            if (destroyTime <= 0)
            {
                DestroyBullet();
            }
            
            BulletShot();
        }

        private void BulletShot()
        {
            _bulletRigidbody.velocity = transform.forward * speed;
        }
        
        // 허공에 쏴서 충돌 감지가 되지 않는 경우를 대비하여 일정 시간이 지나면 총알을 제거
        private void DestroyBullet()
        {
            Destroy(gameObject);
            destroyTime = 3;
        }

        private void OnTriggerEnter(Collider other)
        {
            // if (other.GetComponent<BulletTarget>() != null)
            // {
            //     // 목표물에 맞았을 때 (Ex. 적)
            //     Instantiate(vfxHitGreen, transform.position, Quaternion.identity);
            // }
            // else
            // {
            //     // 이외에 맞았을 때 (Ex. 벽, 오브젝트 등..)
            //     Instantiate(vfxHitRed, transform.position, Quaternion.identity);
            // }

            
            // 적에게 맞췄을 때
            if (other.CompareTag("Enemy"))
            {
                other.gameObject.GetComponent<Enemy.EnemyZombie>().currentHP -= damage;
            }

            Debug.Log("Hit: " + other.name);
            Destroy(gameObject);
        }
    }
}