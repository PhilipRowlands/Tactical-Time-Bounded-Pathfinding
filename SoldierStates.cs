using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// this is a finite-state machine for the AI movement. It is so called
// because I am using the default soldier models from the Unity Bootcamp scene.

public class SoldierStates : MonoBehaviour {
	
	// all figues are integers for speed of calculation
	public int health = 100, criticalHealth = 25;	// they will retreat at critical health levels
	
	public Vector3 destination;
	public Transform home;
	public Transform enemyBase;
	public Transform threat;

	public int pathPosition = 0;
	
	public enum agentState
	{
		isIdle, 			// does nothing
		isDead, 			// freezes movement and increases the enemy's kills by 1
		isFiring, 			// shooting at a target. Decreases ammo.
		isTakingCover,		// putting themselves between a solid object and what's shooting them	
		isMovingToLocation, // using A* to move to a location (or an adjusted version)
		atBase,				// home sweet home...getting ammo and health
		inPosition,		// not sure what this will do...
		lowOnAmmo,			// fall back to base
		lowOnHealth,		// fall back to base
	};
	
	public agentState currentState = agentState.isIdle;		// they are idle at the start
	agentState previousState;							// just in case I need to record this
	
	public Transform gun;		// the gun itself
	FireAI gunAI;				// this is the script that handles shooting
	
	// these variables concern navigation and movement
	AStarOnly aStarScript;			// change to var navScript once the alternative script is created
	public List<Vector3> pathToFollow;
	MoveAgent movement;
	
	// Use this for initialization
	void Start () {
		
		// get the external scripts and variables
		gun = transform.FindChild("Hips").FindChild("M4");
		gunAI = (FireAI) gun.GetComponent("FireAI");
		aStarScript = (AStarOnly) transform.GetComponent("AStarOnly");
		movement = (MoveAgent) transform.GetComponent("MoveAgent");
	
		
		// find the home automatically
		if (gameObject.layer == LayerMask.NameToLayer("Blue") )
		{
			home = GameObject.Find("Blue Base").transform;
			enemyBase = GameObject.Find("Orange Base").transform;
		}
		else
		{
			home = GameObject.Find("Orange Base").transform;
			enemyBase = GameObject.Find("Blue Base").transform;
		}
	}
	
	// Update is called once per frame
	void Update () {
		
//		Debug.Log(gameObject.name + " is in state " + currentState);
		
		switch(currentState)
		{
		case agentState.isDead:
			Debug.Log(gameObject.name + " says \" I am deaded!\"");
			Destroy(gameObject);
			enemyBase.SendMessage("addStats", "enemyKilled");
			home.SendMessage("respawn", gameObject);	// tell the base to respawn me!
			
			break;
			
		case agentState.isFiring:
			if (threat != null)
			{
				transform.LookAt(threat);
				gunAI.SendMessage("startFiring");
				movement.speed = 5f;	// start running
				followPath();		// follow the path anyway
			}
			else
			{
				// a null threat means the threat has been destroyed
				gunAI.SendMessage("stopFiring");
				setState("isMoving");
				movement.speed = 2.5f;
			}
			// add in method to find cover
			
			break;
			
		case agentState.atBase:
//			Debug.Log("At base, restocking");
			replenishHealth();
			gunAI.SendMessage("replenishAmmo", 5);
			setState("isMoving");
			break;
			
		case agentState.inPosition:
			Debug.Log(gameObject + " in position");
			home.SendMessage("coordinatePaths", this);
			break;
			
		case agentState.isIdle:
			gunAI.SendMessage("stopFiring");
			setState("isMoving");
			break;
			
		case agentState.isMovingToLocation:
			
			// follow the path by calling the MoveAgent class - using a key to make the movements visible
			if (pathPosition < pathToFollow.Count
//				&& Input.GetKey(KeyCode.F)
				)
			{
				gunAI.SendMessage("stopFiring");
				followPath();
				
			}
			break;
			
		case agentState.isTakingCover:
			// find the nearest object that can act as cover
			Debug.Log(gameObject + " is under fire!");
			movement.speed = 5f;		// speed up
			setState("isMoving");
			break;
			
		case agentState.lowOnAmmo:
//			setTarget (home.position);		// set the base as the target
			pathToFollow.Reverse();			// quick way to move back towards the base
			setState("isMoving");
			gunAI.SendMessage("stopFiring");
			break;
			
		case agentState.lowOnHealth:
//			setTarget (home.position);
			pathToFollow.Reverse();
			setState("isMoving");
			break;
			
		}
		
		if (health < criticalHealth)
		{
			setState("lowOnHealth");
			if (health <= 0)
			{
				setState ("isDead");
			}
		}
		
	}
	
	// the navigation script will send the path to this method
	// which in turn will inform the base of the soldier's paths
	void setPath(List<Vector3> route)
	{
		pathToFollow = route;		// reset the path
		pathPosition = 0;
		setState("isMoving");
	}
	
	// follow the path
	void followPath()
	{
		Vector3 nextStep = pathToFollow[pathPosition];
		nextStep.y = transform.position.y;
		movement.moveTarget = nextStep;
		
		// if not firing, look at the next step
		if (currentState != agentState.isFiring)
			transform.LookAt(nextStep);
		
		// check if the next position has been reached
		float distance = Vector3.Distance(transform.position, nextStep);
		if (distance < 0.5f)
			pathPosition++;
	}
	
	
	// allows the Human FOV script to set a threat
	void setThreat(Transform target)
	{
		threat = target;
	}
	
	// get more health
	void replenishHealth()
	{
		health = 100;
	}
	
	// just changes starte
	void setState(string newState)
	{
		previousState = currentState;
		
		if (newState.Equals("isIdle") )
			currentState = agentState.isIdle;
		
		if (newState.Equals("isFiring") )
			currentState = agentState.isFiring;
		
		if (newState.Equals("isAtBase") )
			currentState = agentState.atBase;
		
		if (newState.Equals("inPosition") )
			currentState = agentState.inPosition;
		
		if (newState.Equals("isDead") )
			currentState = agentState.isDead;
		
		if (newState.Equals("lowOnHealth") )
			currentState = agentState.lowOnHealth;
		
		if (newState.Equals("lowOnAmmo") )
			currentState = agentState.lowOnAmmo;
		
		if (newState.Equals("isMoving") )
			currentState = agentState.isMovingToLocation;
		
		if (newState.Equals("isHiding") || newState.Equals("isUnderFire") )
			currentState = agentState.isTakingCover;
		
	}
	
	// gives the soldier a destination to reach
	// should cancel the old path just in case
	public void setTarget(Vector3 position)
	{
		destination = position;
		transform.SendMessage("setDestination", destination);
	}
	
	void damage(int damage)
	{
		health -= damage;
	}
	
}
