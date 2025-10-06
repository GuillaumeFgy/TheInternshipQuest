using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BikeDamper : MonoBehaviour {

    // Use this for initialization

    //these are the two line renderers used
    public LineRenderer rearDamper, frontDamper;

    public Transform rearW, frontW, damperRearPosition, damperFrontPosition;

	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        //set the positions of the linerenderer
        rearDamper.SetPosition(0, rearW.position);
        rearDamper.SetPosition(1, damperRearPosition.position);

        frontDamper.SetPosition(0, frontW.position);
        frontDamper.SetPosition(1, damperFrontPosition.position);


    }
}
