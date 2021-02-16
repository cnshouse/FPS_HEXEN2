using UnityEngine;
using System.Collections;

public class Chain_Animator_Handle : MonoBehaviour
{
    private Animator _Animator_Component = null;

    public Animator Animator_Component
    {
        get 
        {
            if (_Animator_Component == null)
            { 
                _Animator_Component = gameObject.GetComponent<Animator>();
            }

            return _Animator_Component; 
        }

        set { _Animator_Component = value; }
    }

    public void Open()
    {
        Animator_Component.SetBool("State", true);
    }

    public void Close()
    {
        Animator_Component.SetBool("State", false);
    }

    public void Play()
    {
        float _Temp = Random_Number;

        Animator_Component.SetFloat("Random", 0.0f);

        Animator_Component.SetFloat("Speed", 1);
    }

    public void Play_Reverse()
    {
        float _Temp = Random_Number;

        Animator_Component.SetFloat("Speed", -1);
    }

    public void Stop()
    {
        float _Temp = Random_Number;

        Animator_Component.SetFloat("Speed", 0.0f);
    }

    private void Update()
    {
        Animator_Component.SetFloat("Random", Mathf.Lerp(Animator_Component.GetFloat("Random"), 1, Time.deltaTime));
    }

    private float _Random_Number = 0.0f;

    public float Random_Number
    {
        get 
        {
            _Random_Number = Random.Range(-10.0f, 11.0f);

            return _Random_Number; 
        }

        set { _Random_Number = value; }
    }
}