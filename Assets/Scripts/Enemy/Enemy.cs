using UnityEngine;
using UnityEngine.UI;

namespace Enemy
{
    public class Enemy : MonoBehaviour
    {
        [SerializeField] private Slider HPBar;
        
        private float enemyMaxHP = 100;
        public float currentHP = 0;
        
        void Start()
        {
            InitEnemyHP();
        }

        void Update()
        {
            HPBar.value = currentHP / enemyMaxHP;
            if (currentHP <= 0)
            {
                Destroy(gameObject);
            }
        }
        
        private void InitEnemyHP()
        {
            currentHP = enemyMaxHP;
        }
    }
}
