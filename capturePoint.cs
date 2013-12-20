using UnityEngine;
using System.Collections;

// this class just tells the soldiers they have reached their destination

public class capturePoint : MonoBehaviour {
	
	int objectLayer, blueLayer, orangeLayer;

	// Use this for initialization
	void Start () {
	
		blueLayer = LayerMask.NameToLayer("Blue");
		orangeLayer = LayerMask.NameToLayer("Orange");
	}
	
	// if a blue or orange unit enters the area, change their state
	void OnTriggerEnter(Collider collidee)
	{
//		Debug.Log(collidee);
		objectLayer = collidee.gameObject.layer;
		
		if (objectLayer == blueLayer || objectLayer == orangeLayer)
		{
			collidee.SendMessage("setState", "inPosition", SendMessageOptions.RequireReceiver);
		}
	}
}
