using UnityEngine;
using System.Collections;

public class xxx : MonoBehaviour {

    public float stat = 100f;
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        stat = Mathf.Lerp(stat, 1f, Time.deltaTime);
    }
}
