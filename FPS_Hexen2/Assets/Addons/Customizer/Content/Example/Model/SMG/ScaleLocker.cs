using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleLocker : MonoBehaviour
{
    public Vector3 _scale;

    // Start is called before the first frame update
    void Start()
    {
        _scale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        if(transform.localScale != _scale)
		{
            transform.localScale = _scale;
		}
    }
}
