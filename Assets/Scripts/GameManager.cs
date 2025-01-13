using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // 싱글턴
    public static GameManager Instance;

    private ShooterController _shooterController;
    
    [SerializeField] private Text bulletText;
    
    void Start()
    {
        Instance = this;
        _shooterController = FindObjectOfType<ShooterController>();
    }

    void Update()
    {
        bulletText.text = _shooterController.currentBullet + " / " + _shooterController.maxBullet;
    }
}
