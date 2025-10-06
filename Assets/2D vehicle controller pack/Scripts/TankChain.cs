using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankChain : MonoBehaviour {

    // Use this for initialization
    //this is the line renderer
    public LineRenderer linRend;
    public Transform startLinR, endLinR;

    //these are the wheels
    public Transform[] wheels;

	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        int k = 0;
        //starting position
        linRend.SetPosition(k, startLinR.position);
        k++;
        //wheel positions
        for (int ii=0; ii<wheels.Length;ii++)
        {  linRend.SetPosition(k, wheels[ii].position+new Vector3(0,0.25f,0));
            k++;
        }

        //end position
        linRend.SetPosition(k, endLinR.position);
        k++;

        //wheel positions
        for (int ii = wheels.Length-1; ii >=0; ii--)
        {
            linRend.SetPosition(k, wheels[ii].position + new Vector3(0, -0.25f, 0));
            k++;
        }

        //start position again
        linRend.SetPosition(k, startLinR.position);
    }
}
