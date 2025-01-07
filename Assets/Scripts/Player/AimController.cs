using Cinemachine;
using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class AimController : MonoBehaviour
    {
        [SerializeField] private CinemachineVirtualCamera _aimVirtualCamera;
        [SerializeField] private float normalSensitivity;
        [SerializeField] private float aimSensitivity;
        [SerializeField] private LayerMask aimColliderLayerMask;
        [SerializeField] private Transform debugTransform;
        
        private StarterAssetsInputs _starterAssetsInputs;
        private ThirdPersonController _playerController;

        private void Awake()
        {
            _starterAssetsInputs = GetComponent<StarterAssetsInputs>();
            _playerController = GetComponent<ThirdPersonController>();
        }

        private void Update()
        {
            if (_starterAssetsInputs.aim)
            {
                _aimVirtualCamera.gameObject.SetActive(true);
                _playerController.SetSensitivity(aimSensitivity);
            }
            else
            {
                _aimVirtualCamera.gameObject.SetActive(false);
                _playerController.SetSensitivity(normalSensitivity);
            }

            Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
            if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimColliderLayerMask))
            {
                debugTransform.position = raycastHit.point;
            }
        }
    }
}