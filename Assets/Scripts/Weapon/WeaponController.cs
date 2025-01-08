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

    public enum FireMode
    {
        Single, // 단발
        Burst, // 3점사
        Auto, // 연사
    }

    public class WeaponController : MonoBehaviour
    {
        // 무기 타입 및 사격 모드
        public WeaponType weaponType;
        public FireMode fireMode;

        // 총알
        public int maxAmmo;
        public int curAmmo;
        
        // 수류탄 투척
        private IEnumerator ThrowGrenade()
        {
            // GameObject grenade = Instantiate(bullet, bulletPos.position, bulletPos.rotation);
            // Rigidbody rb = grenade.GetComponent<Rigidbody>();
            // rb.AddForce(bulletPos.forward * 20, ForceMode.Impulse); // 수류탄 투척
            yield return null;
        }
    }
}