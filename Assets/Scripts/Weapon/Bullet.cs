using System;
using UnityEngine;

namespace Weapon
{
    public class Bullet : MonoBehaviour
    {
        public int damage;
        public int destroyDelay = 1;

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Environment"))
            {
                // Destroy(gameObject); // 즉시 삭제
                Destroy(gameObject, destroyDelay); // 딜레이 후 삭제
            }
        }
    }
}
