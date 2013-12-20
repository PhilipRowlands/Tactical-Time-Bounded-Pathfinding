using UnityEngine;
using System.Collections;

// this is attached to an invisible cone that acts as a human's field of view

public class HumanFieldOfView : MonoBehaviour {
	
	string objectLayer;
	string teamLayer;
	string enemyLayer;
	
	public Transform threat;
	Transform soldier;
	Vector3 threatPosition;
	
	Ray lineOfSight;		// check to see if the object can be seen
	RaycastHit hit;
	public float sightDistance = 15f;
	
	public float rotationLimit = 30f;
	public float rotationAmount = 20f;
	float yRotation, yStartRotation;
	
	void Start()
	{
		sightDistance = 15f;
		teamLayer = LayerMask.LayerToName( transform.parent.gameObject.layer );
		
		if (teamLayer.Equals("Blue") )
			enemyLayer = "Orange";
		else
			enemyLayer = "Blue";
		
		soldier = transform.parent;
		
		InvokeRepeating("changeDirection", 0f, 5f);
	}
	
	// called once a frame
	void Update()
	{
		// rotate around the y-axis to sweep an area
		yRotation = transform.rotation.y;
		
		transform.Rotate(Vector3.up, rotationAmount * Time.deltaTime);
		
	}
	
	void changeDirection()
	{
		rotationAmount *= -1;
	}
	
	void OnTriggerEnter(Collider coll)
	{
		// for some reason, casting a line to the object's position doesn't work
		if (Physics.Linecast(transform.position, coll.bounds.center, out hit) )
		{
			GameObject hitObject = hit.collider.gameObject;
			objectLayer = LayerMask.LayerToName(hitObject.layer);
			
//			Debug.Log(transform.parent.name + " thinks " + hitObject.name + " is in layer " + objectLayer);
			
			if (objectLayer == enemyLayer)
			{
				// the object has been identified as the enemy...now what?
				threat = hitObject.transform;
				transform.parent.LookAt(threat);
				transform.parent.SendMessage("setState", "isFiring");
				transform.parent.SendMessage("setThreat", threat);
			}
		}
		
	}
	
	void OnTriggerStay(Collider coll)
	{
		
//		if (Physics.Raycast(lineOfSight, out hit, sightDistance) )
		if (Physics.Linecast(transform.position, coll.bounds.center, out hit) )
		{
			GameObject hitObject = hit.collider.gameObject;
			objectLayer = LayerMask.LayerToName(hitObject.layer);
			
//			Debug.Log(hitObject.name + " is in layer " + objectLayer);
			
			if (objectLayer == enemyLayer)
			{
				// the object has been identified as the enemy...now what?
				threat = hitObject.transform;
				transform.parent.LookAt(threat);
				transform.parent.SendMessage("setState", "isFiring");
				transform.parent.SendMessage("setThreat", threat);
			}
		}
	
	}
	
	
	void OnTriggerExit()
	{
		CancelInvoke("checkTarget");
		threat = null;
		transform.parent.SendMessage("setState", "isMoving");
		transform.parent.SendMessage("resetSpeed");		// slow down
	}
	
	// if the threat is dead, stop firing
	void checkTarget()
	{
		if (threat == null)
		{
			BroadcastMessage("stopFiring");
		}
	}
	
}