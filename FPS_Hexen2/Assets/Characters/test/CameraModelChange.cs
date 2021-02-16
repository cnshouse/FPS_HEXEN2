using UnityEngine;
using System.Collections;

public class CameraModelChange : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Camera cameraS = gameObject.GetComponent<Camera>();
        cameraS.clearFlags = CameraClearFlags.Depth;
        cameraS.transparencySortMode = TransparencySortMode.Orthographic;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
