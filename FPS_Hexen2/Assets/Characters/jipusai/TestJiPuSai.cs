using UnityEngine;
using System.Collections;

public class TestJiPuSai : MonoBehaviour
{

    Animator ani;
    void Start()
    {
        ani = GetComponent<Animator>();
       
       
       
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            ani.SetBool("SkillTwo", true);
        }
        AnimatorStateInfo asi = ani.GetCurrentAnimatorStateInfo(0);
        if (asi.fullPathHash == Animator.StringToHash("Base Layer.Grounded Strafe"))
        {
         
        }
        if (asi.IsName("Base Layer.Grounded Strafe"))
        {
            print(1);
            ani.speed = 1;
          //  print(ani.speed);
            print(asi.speed);
        }
    }
}
