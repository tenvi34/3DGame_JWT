using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace Weapon
{
    public enum WeaponType
    {
        AssaultRifle,
        Handgun,
        Shotgun,
        SniperRifle,
        Grenade,
    }
    public class WeaponController : MonoBehaviour
    {
        public WeaponType weaponType;
        public Transform bulletPos;
        public GameObject bullet;

        public void Use()
        {
            if (weaponType == WeaponType.AssaultRifle)
            {
                StartCoroutine(Shot());
            }
        }

        IEnumerator Shot()
        {
            // 총알 생성 후 발사
            GameObject instantBullet = Instantiate(bullet, bulletPos.position, bulletPos.rotation);
            Rigidbody bulletRigidbody = instantBullet.GetComponent<Rigidbody>();
            bulletRigidbody.velocity = bulletPos.forward * 50;
            
            yield return null;
        }
    }
}