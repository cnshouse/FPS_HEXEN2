using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MimicJoints : MonoBehaviour
{
    public Transform[] localJoints;
    public Transform[] RemoteJoints;
    int jointNumber = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        foreach(Transform t in RemoteJoints)
		{
            t.position = localJoints[jointNumber].position;
            t.rotation = localJoints[jointNumber].rotation;
            jointNumber++;
		}
        jointNumber = 0;
    }
}
