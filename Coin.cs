using System;
using UnityEngine;

// this is a shiny coin for the soldiers to pick up

public class Coin : Pickups
{
	public override void OnTriggerEnter (Collider coll)
	{
		GameObject collObject = coll.gameObject;
		string collLayer = LayerMask.LayerToName(collObject.layer);
		if (collLayer == "Blue" || collLayer == "Orange" )
		{
			GameObject mainCamera = Camera.main.gameObject;
			passMessage(mainCamera, collLayer);
		}
	}
	
	public override void passMessage (GameObject go, string layer)
	{	
		go.SendMessage("incrementStatistics", layer + " has picked up a coin");
		Destroy (gameObject);
		
	}
}


