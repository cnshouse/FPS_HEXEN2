using UnityEngine;
using System.Collections;

public class fpsChange : MonoBehaviour {

    public int fps = -1;
	void Start () {
        ani = GetComponent<Animator>();
        //ani.linearVelocityBlending = true;
        //ani.pivotWeight
    }
	
	// Update is called once per frame
	void Update () {
	    
	}
    void OnEnable()
    {
        Application.targetFrameRate = fps;


    }

    Animator ani;

    float allTime = 0f;
    float allDis = 0f;
    Vector3 startV3 = Vector3.zero;
    float nowspeed = 0f;
    string fpsTxt = "";
    private void jisuanMoveSpeed()
    {
        
        
        if (Time.deltaTime == 0)
            return;
        Vector3 move = Vector3.zero;
        move.x = ani.GetFloat("Right");
        move.z = ani.GetFloat("Forward");
        if (move.magnitude <= 0.1f || ani.GetFloat("MouseX")>0.1f)
        {
            allTime = 0f;
            allDis = 0f;
            startV3 = transform.position;
            return;
        }
        Vector3 at = transform.position;
        allTime += Time.deltaTime;
        allDis = Vector3.Distance(at, startV3);
        float spd = allDis / allTime;
        
        nowspeed = allDis / allTime;

        fpsTxt = System.String.Format("FPS:{0:F0} , Pools:{1} , moveSpeed:{2:F2}", 0, 0, nowspeed);//保留两位小数  
    }

    void OnGUI()
    {
        jisuanMoveSpeed();
        GUI.Label(new Rect(0, 0, 680, 50), fpsTxt);
    }

}
