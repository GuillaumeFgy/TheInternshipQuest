using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollower : MonoBehaviour {

    // Use this for initialization

    //this is the gameobject that is followed by the camera
    public Transform objective;
    public float distance;

	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        //we use lerp to move the camera and follow the car
        transform.position = Vector3.Lerp(transform.position, objective.position+new Vector3(0,0,-distance),0.05f);

	}
}
