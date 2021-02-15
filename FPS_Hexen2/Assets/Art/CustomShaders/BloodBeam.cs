using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodBeam : MonoBehaviour
{
    public int _BeamMaxRange;               //Distance that the weapon can shoot...
    public GameObject _BeamTarget;          //The Target that the player is Trying to hit
    public float _beamSpeed;                //The Speed at which the front point of the beam will travel towards target

    public GameObject BeamStart;
    public GameObject BeamEndPoint;
    public LineRenderer _BeamReneder;
    
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        float distance = Vector3.Distance(BeamEndPoint.transform.position, _BeamTarget.transform.position);
        Debug.Log("Distance " + distance);

        if(distance > 0.01)
		{
            // Move our position a step closer to the target.
            float step = _beamSpeed * Time.deltaTime; // calculate distance to move
            BeamEndPoint.transform.position = Vector3.MoveTowards(BeamEndPoint.transform.position, _BeamTarget.transform.position, step);
        }
        _BeamReneder.SetPosition(0, BeamStart.transform.position);
        _BeamReneder.SetPosition(1, BeamEndPoint.transform.position);
    }
}
