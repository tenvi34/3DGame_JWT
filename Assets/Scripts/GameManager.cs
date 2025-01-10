using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // 싱글턴
    public static GameManager Instance;
    
    void Start()
    {
        Instance = this;
    }

    void Update()
    {
        
    }
}
