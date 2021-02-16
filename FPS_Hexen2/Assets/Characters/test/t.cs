using UnityEngine;
using System.Collections;

public class t : MonoBehaviour {

	// Use this for initialization
    Vector3 v3DaodiPos;
	void Start () 
    {
        CharacterJoint charaJ;
        Rigidbody rigBody;
        Rigidbody zhixinObj = null;
        foreach (Collider col in gameObject.GetComponentsInChildren<Collider>())
        {
            charaJ = col.gameObject.GetComponent<CharacterJoint>();
            rigBody = col.gameObject.GetComponent<Rigidbody>();
            //rigBody.useGravity = false;
            rigBody.isKinematic = true;
            if (col.gameObject.name == "Bip001 Pelvis") zhixinObj = rigBody;
        }
        v3DaodiPos = zhixinObj.gameObject.transform.localPosition;
	}
	
	// Update is called once per frame
	void Update () 
    {
        CharacterJoint charaJ;
        Rigidbody rigBody;
        Rigidbody zhixinObj = null;
        if (Input.GetKeyDown(KeyCode.D))
        {
            Animator playerani = gameObject.GetComponent<Animator>();
            playerani.enabled = false;
            foreach (Collider col in gameObject.GetComponentsInChildren<Collider>())
            {
                charaJ = col.gameObject.GetComponent<CharacterJoint>();
                rigBody = col.gameObject.GetComponent<Rigidbody>();
                rigBody.isKinematic = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            
            foreach (Collider col in gameObject.GetComponentsInChildren<Collider>())
            {
                charaJ = col.gameObject.GetComponent<CharacterJoint>();
                rigBody = col.gameObject.GetComponent<Rigidbody>();
                rigBody.isKinematic = true;
                if (col.gameObject.name == "Bip001 Pelvis") zhixinObj = rigBody;
            }
            zhixinObj.gameObject.transform.localPosition = v3DaodiPos;
            Animator playerani = gameObject.GetComponent<Animator>();
            playerani.enabled = true;
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            float fRandomx = Random.Range(-0.9f, 0.9f);
            float fRandomy = Random.Range(-0.9f, 0.9f);
            float fRandomz = Random.Range(-0.9f, 0.9f);
            Vector3 v3Random = new Vector3(fRandomx, fRandomy, fRandomz);
            v3Random = v3Random + Vector3.up;
            v3Random = v3Random.normalized;
            //v3Random = new Vector3(0,1,1);
            //Debug.Log(fRandom+"===="+v3Random);
            Animator playerani = gameObject.GetComponent<Animator>();
            playerani.enabled = false;
            foreach (Collider col in gameObject.GetComponentsInChildren<Collider>())
            {
                charaJ = col.gameObject.GetComponent<CharacterJoint>();
                rigBody = col.gameObject.GetComponent<Rigidbody>();
                rigBody.isKinematic = false;
                if (col.gameObject.name == "Bip001 Pelvis") zhixinObj = rigBody;
                rigBody.AddForce(v3Random * 10, ForceMode.Impulse);
            }


            //zhixinObj.AddForce(v3Random * 10, ForceMode.Impulse);
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            float fRandomx = Random.Range(-0.9f, 0.9f);
            float fRandomy = Random.Range(-0.9f, 0.9f);
            float fRandomz = Random.Range(-0.9f, 0.9f);
            Vector3 v3Random = new Vector3(fRandomx, fRandomy, fRandomz);
            v3Random = v3Random + Vector3.up;
            v3Random = v3Random.normalized;
            //v3Random = new Vector3(0,1,1);
            //Debug.Log(fRandom+"===="+v3Random);
            Animator playerani = gameObject.GetComponent<Animator>();
            playerani.enabled = false;
            foreach (Collider col in gameObject.GetComponentsInChildren<Collider>())
            {
                charaJ = col.gameObject.GetComponent<CharacterJoint>();
                rigBody = col.gameObject.GetComponent<Rigidbody>();
                rigBody.isKinematic = false;
                if (col.gameObject.name == "Bip001 Pelvis") zhixinObj = rigBody;
            }
            zhixinObj.AddForce(v3Random * 10, ForceMode.Impulse);
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            float fRandomx = Random.Range(-0.9f, 0.9f);
            float fRandomy = Random.Range(-0.9f, 0.9f);
            float fRandomz = Random.Range(-0.9f, 0.9f);
            Vector3 v3Random = new Vector3(fRandomx, fRandomy, fRandomz);
            v3Random = v3Random + Vector3.up;
            v3Random = v3Random.normalized;
            //v3Random = new Vector3(0,1,1);
            //Debug.Log(fRandom+"===="+v3Random);
            Animator playerani = gameObject.GetComponent<Animator>();
            playerani.enabled = false;
            foreach (Collider col in gameObject.GetComponentsInChildren<Collider>())
            {
                charaJ = col.gameObject.GetComponent<CharacterJoint>();
                rigBody = col.gameObject.GetComponent<Rigidbody>();
                rigBody.isKinematic = false;
                if (col.gameObject.name == "Bip001 Pelvis") zhixinObj = rigBody;
                rigBody.AddForce(v3Random * 5, ForceMode.Impulse);
            }
        }
	}
}
