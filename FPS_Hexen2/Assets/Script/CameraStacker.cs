using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CameraStacker : MonoBehaviour
{
    private Camera _otherCamera;
    private Camera _overlayCamera;

    //cameraData.cameraStack.Add(myOverlayCamera);

    // Start is called before the first frame update
    void Start()
    {
        _overlayCamera = this.GetComponent<Camera>();

        Camera[] Cameras = FindObjectsOfType<Camera>();
        foreach(Camera c in Cameras)
		{
            if(c != _overlayCamera)
			{
                _otherCamera = c;

                var cameraData = _otherCamera.GetUniversalAdditionalCameraData();
                if(c != null)
                    cameraData?.cameraStack?.Add(_overlayCamera);
            }
		}

    }

	private void OnDisable()
	{
        Camera[] Cameras = FindObjectsOfType<Camera>();
        foreach (Camera c in Cameras)
        {
            if (c != _overlayCamera)
            {
                _otherCamera = c;

                var cameraData = _otherCamera.GetUniversalAdditionalCameraData();
                cameraData.cameraStack.Remove(_overlayCamera);
            }
        }
    }

	// Update is called once per frame
	void Update()
    {
        
    }

}
