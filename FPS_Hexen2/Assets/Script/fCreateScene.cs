using UnityEngine;
using System.Collections;

public class fCreateScene : MonoBehaviour {

	// Use this for initialization
    GameObject gameobj;
    public string scenename = "";
	public GameObject m_pJiaoSe_Light = null;
	void Start () {
         // 根据名字查找物件
        //Debug.Log("CreateScene" + scenename);
        //Debug.LogError("LoadLevel完成,fCreateScene: " + scenename + ",Time=" + Time.time);

        gameobj = GameObject.Find("_fGameManager");

        // 如果物件存在
        if (gameobj)
        {
            gameobj.SendMessage("OnSceneCreate", scenename);
        }
	}

}
