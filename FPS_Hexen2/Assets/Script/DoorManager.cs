using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorManager : MonoBehaviour
{
    public GameObject[] Doors;
    public GameObject[] DoorEffects;
    // Start is called before the first frame update
    void Start()
    {
        foreach(GameObject d in Doors)
		{
            Animation anim = d.GetComponent<Animation>();
            AnimationClip open = anim.GetClip("open");
            anim.clip = open;
            anim.Play();
            Debug.Log("Played animation");
		}

        foreach(GameObject e in DoorEffects)
		{
            e.SetActive(false);
		}
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
