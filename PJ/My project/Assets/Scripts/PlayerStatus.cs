using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerStatus : MonoBehaviour
{
    public int Health;
    public UnityEvent Player_Dead;
    public GameObject[] Heart_Icon;
    public GameObject Damage_Ui_Prefab;
    public void Player_Got_Damage(int A)
    {
        if (Health > 1)
        {
            Instantiate(Damage_Ui_Prefab);
            Health -= A;
            Health = Mathf.Clamp(Health, 0, 3);
            Heart_Icon[Health-1].SetActive(false);
        }
        else if (Health == 1)
        {
            Player_Dead.Invoke();
        }
    }
}
