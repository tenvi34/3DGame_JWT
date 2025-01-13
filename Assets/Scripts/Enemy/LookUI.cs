using UnityEngine;

namespace Enemy
{
    public class LookUI : MonoBehaviour
    {
        private Camera _camera;
        
        void Start()
        {
            // 메인 카메라를 찾아서 가져온다
            if (_camera == null)
            {
                _camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
            }
        }
        
        void Update()
        {
            if (_camera != null)
            {
                transform.LookAt(transform.position + _camera.transform.rotation * Vector3.forward,
                    _camera.transform.rotation * Vector3.up);
            }
        }
    }
}
