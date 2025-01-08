using UnityEngine;

namespace Weapon
{
    public class BulletProjectile : MonoBehaviour
    {
        [SerializeField] private Transform vfxHitGreen;
        [SerializeField] private Transform vfxHitRed;

        private Rigidbody _bulletRigidbody;

        private void Awake()
        {
            _bulletRigidbody = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            float speed = 50f;
            _bulletRigidbody.velocity = transform.forward * speed;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<BulletTarget>() != null)
            {
                // 목표물에 맞았을 때 (Ex. 적)
                Instantiate(vfxHitGreen, transform.position, Quaternion.identity);
            }
            else
            {
                // 이외에 맞았을 때 (Ex. 벽, 오브젝트 등..)
                Instantiate(vfxHitRed, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }
    }
}