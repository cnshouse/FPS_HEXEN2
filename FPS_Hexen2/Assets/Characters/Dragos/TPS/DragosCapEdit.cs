using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragosCapEdit : MonoBehaviour
{
    public GameObject CapBoneOne;
    public GameObject CapBoneTwo;
    public Animator CharacterAnimator;
    public Quaternion left;
    public Quaternion right;
    // Start is called before the first frame update
    void Start()
    {
        //CapBoneOne.transform.Rotate(left);  
    }

    // Update is called once per frame
    void Update()
    {
        // 45.093 9.516 96.904 
        Quaternion  current = CapBoneOne.transform.rotation;
        if(left != current)
		{
            CapBoneOne.transform.SetPositionAndRotation(CapBoneOne.transform.position ,left);
		}
        Quaternion currentR = CapBoneTwo.transform.rotation;
        if (right != current)
        {
            CapBoneTwo.transform.SetPositionAndRotation(CapBoneTwo.transform.position, right);
        }
        // -35.283 7.715 -96.151 
    }
}
