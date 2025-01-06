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
        public Transform bulletPos;
        public GameObject bullet;

        // 관련 변수
        private bool _isShooting = false;

        public void Use()
        {
            switch (weaponType)
            {
                case WeaponType.AssaultRifle:
                    HandleAssaultRifle();
                    break;
                case WeaponType.Handgun:
                    HandleHandgun();
                    break;
                case WeaponType.Shotgun:
                    HandleShotgun();
                    break;
                case WeaponType.SniperRifle:
                    HandleSniperRifle();
                    break;
                case WeaponType.Grenade:
                    HandleGrenade();
                    break;
            }
        }

        private void HandleAssaultRifle()
        {
            switch (fireMode)
            {
                case FireMode.Single:
                    StartCoroutine(SingleShot());
                    break;
                case FireMode.Burst:
                    StartCoroutine(BurstShot());
                    break;
                case FireMode.Auto:
                    if (!_isShooting)
                        StartCoroutine(AutoShot());
                    break;
            }
        }

        private void HandleHandgun()
        {
            StartCoroutine(SingleShot()); // 핸드건은 단발만
        }

        private void HandleShotgun()
        {
            StartCoroutine(ShotgunBlast()); // 여러 총알을 동시에 발사
        }

        private void HandleSniperRifle()
        {
            StartCoroutine(SingleShot()); // 단발, 느린 발사 속도
        }

        private void HandleGrenade()
        {
            StartCoroutine(ThrowGrenade()); // 투척 메커니즘
        }

        // 단발 사격
        private IEnumerator SingleShot()
        {
            FireBullet();
            yield return null;
        }

        // 3점사
        private IEnumerator BurstShot()
        {
            for (int i = 0; i < 3; i++)
            {
                FireBullet();
                yield return new WaitForSeconds(0.1f); // 발사 간격
            }
        }

        // 연사
        private IEnumerator AutoShot()
        {
            _isShooting = true;
            while (Input.GetMouseButton(0))
            {
                FireBullet();
                yield return new WaitForSeconds(0.1f);
            }

            _isShooting = false;
        }

        // 샷건 발사
        private IEnumerator ShotgunBlast()
        {
            for (int i = 0; i < 8; i++) // 샷건 총알 8개
            {
                FireBullet(RandomSpread()); // 랜덤하게 퍼지도록 설정
            }

            yield return null;
        }

        // 수류탄 투척
        private IEnumerator ThrowGrenade()
        {
            // GameObject grenade = Instantiate(bullet, bulletPos.position, bulletPos.rotation);
            // Rigidbody rb = grenade.GetComponent<Rigidbody>();
            // rb.AddForce(bulletPos.forward * 20, ForceMode.Impulse); // 수류탄 투척
            yield return null;
        }

        // 총알 발사
        private void FireBullet(Vector3 spread = default)
        {
            GameObject instantBullet = Instantiate(bullet, bulletPos.position, bulletPos.rotation);
            Rigidbody bulletRigidbody = instantBullet.GetComponent<Rigidbody>();
            bulletRigidbody.velocity = (bulletPos.forward + spread) * 50;
        }

        // 탄 퍼짐
        private Vector3 RandomSpread()
        {
            return new Vector3(
                Random.Range(-0.1f, 0.1f),
                Random.Range(-0.1f, 0.1f),
                0
            );
        }
        
        // 총 종류 설정
        public void SetWeaponType(WeaponType type)
        {
            weaponType = type;
        }
        
        // 사격 모드 설정
        public void SetFireMode(FireMode mode)
        {
            fireMode = mode;
        }
        
        // 총 종류 또는 사격 모드 변경 시 코드 사용 예시
        // _animator.SetInteger("WeaponType", (int)_weaponController.weaponType);
        // _animator.SetTrigger("DoShot");
    }
}