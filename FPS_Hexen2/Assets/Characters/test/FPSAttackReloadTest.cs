using UnityEngine;
using System.Collections;

public class FPSAttackReloadTest : MonoBehaviour {

    float lastFireTime;
    int iAttackNum;
    int iReloadNum;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () 
    {
        Animator ani = gameObject.GetComponent<Animator>();
        if (iAttackNum > 0)
        {
            iAttackNum--;
            if (iAttackNum == 0) ani.SetFloat("attack", 0f);
        }
        AnimatorStateInfo stateInfo = ani.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("Reload"))
        {
            if (stateInfo.normalizedTime >= 1f) ani.SetFloat("reload", 0f);
        }
        /*if (iReloadNum > 0)
        {
            iReloadNum--;
            if (iReloadNum == 0) ani.SetFloat("reload", 0f);
        }*/
        if (Input.GetMouseButton(0))
        {
            if (Time.time - lastFireTime > 0.1f)
            {
                iAttackNum = 3;
                lastFireTime = Time.time;
                ani.SetFloat("attack", 1f);
            }
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            iReloadNum = 30;
            ani.SetFloat("reload", 1f);
        }
	}
}
