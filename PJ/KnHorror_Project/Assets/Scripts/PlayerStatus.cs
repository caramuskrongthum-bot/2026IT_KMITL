using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerStatus : MonoBehaviour
{
    public Slider HealthBar;
    public UnityEvent Down;
    public GameObject VFX_Blood;
    public Transform VFX_Point;
    public AudioSource SFX_Source;
    public AudioClip SFX_Damage;
    public void DamageToPlayer(int Damage)
    {
        Instantiate(VFX_Blood).transform.position = VFX_Point.transform.position;
        HealthBar.value -= Damage;
        SFX_Source.PlayOneShot(SFX_Damage);
        HealthBar.value = Mathf.Clamp(HealthBar.value,0,100);
        if (HealthBar.value == 0)
        {
            Animator A = GetComponent<Animator>();
            A.Play("Down");
            Down.Invoke();
        }
        if (HealthBar.value <= 50f)
        {
            Animator A = GetComponent<Animator>();
            A.SetLayerWeight(1, 1);
        }
        else if (HealthBar.value > 50f)
        {
            Animator A = GetComponent<Animator>();
            A.SetLayerWeight(1, 0);
        }
    }
    public void HealToPlayer(int Heal)
    {
        HealthBar.value += Heal;
        HealthBar.value = Mathf.Clamp(HealthBar.value, 0, 100);

        if (HealthBar.value <= 50f)
        {
            Animator A = GetComponent<Animator>();
            A.SetLayerWeight(1, 1);
        }
        else if (HealthBar.value > 50f)
        {
            Animator A = GetComponent<Animator>();
            A.SetLayerWeight(1, 0);
        }
    }

    public void GotCarry(GameObject B)
    {
        Animator A = GetComponent<Animator>();
        A.Play("P_Carry");
        transform.position = B.transform.position;
        transform.rotation = B.transform.rotation;
        transform.parent = B.transform;
    }
}
