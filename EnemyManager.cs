using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// this handles the squads - their destinations, positions and so on

public class EnemyManager : MonoBehaviour {
	
	public Transform observer;			// this is the camera
	public Transform enemyBase, target1, target2, target3, target4;		// destinations
	public GameObject soldierTemplate;	// a template for the soldiers
	public GameObject soldier1, soldier2, soldier3, soldier4, soldier5;		// the squad
	
	// keep track of the agents' paths
	List<Transform> targetList;
	List<GameObject> roster;
	
	int enemyKilled = 0, enemyBaseCaptured = 0;
	
	int baseLayer;
	int enemyLayer;

	// Use this for initialization
	void Start () {
		baseLayer = gameObject.layer;
		
		if (baseLayer == LayerMask.NameToLayer("Blue") )
			enemyLayer = LayerMask.NameToLayer("Orange");
		else
			enemyLayer = LayerMask.NameToLayer("Blue");
		
//		Debug.Log(gameObject.name + " is in layer " + LayerMask.LayerToName(baseLayer) + 
//			". The enemy base is in " + LayerMask.LayerToName(enemyLayer) );
		
		targetList = new List<Transform>();
		roster = new List<GameObject>(5);
		
		for (int i = 0; i < 3; i++)
		{
			spawnUnit(i);
		}
		
		Invoke("setTargets", 0f);
		
	}
	
	// what happens if something enters the base area
	void OnTriggerEnter(Collider collidee)
	{
		int objectLayer = collidee.gameObject.layer;
		
		if (objectLayer == enemyLayer)
		{
			observer.SendMessage("incrementStatistics", objectLayer + " hasCapturedEnemyBase");
			collidee.SendMessage("setState", "inPosition");
//			Debug.Log("The base " + gameObject.name + " has been captured");
			
		}
	}
	
	// what happens if that object stays there
	void OnTriggerStay(Collider collidee)
	{
		int objectLayer = collidee.gameObject.layer;
		
		if (objectLayer == baseLayer )
		{
			collidee.SendMessage("setState", "isAtBase");
		}
	}
	
	
	void setTargets()
	{
		soldier1 = roster[0];
		soldier2 = roster[1];
		soldier3 = roster[2];
//		soldier4 = roster[3];
//		soldier5 = roster[4];
		
		soldier1.SendMessage("setTarget", target1.position);
		soldier2.SendMessage("setTarget", target2.position);
		soldier3.SendMessage("setTarget", target3.position);
//		soldier4.SendMessage("setTarget", target4.position);
//		soldier5.SendMessage("setTarget", enemyBase.position);
		
		targetList.Add(target1);
		targetList.Add(target2);
		targetList.Add(target3);
		targetList.Add(target4);
		targetList.Add(enemyBase);
		
	}
	
	void addStats(string statistic)
	{
		if (statistic.Equals("enemyKilled") )
			observer.SendMessage("incrementStatistics", LayerMask.LayerToName(baseLayer) + " hasKilledEnemy");
		
		if (statistic.Equals("hasStoodOneMine") )
			observer.SendMessage("incrementStatistics", LayerMask.LayerToName(baseLayer) + " hasStoodOnMine");
				
	}
	
	// coordinate paths for each agent and give them new destinations
	void coordinatePaths(SoldierStates states)
	{
		GameObject soldier = states.gameObject;
		Vector3 oldTarget = states.destination;
		Transform newTarget;
		
		// Note: if a soldier is calling, they are unlikely to be null
		if (soldier == null)
			return;
		
		// make sure the new target is not the same as the last one
		// by cloning the target list and not adding the previous target
		List<Vector3> tempList = new List<Vector3>();
		foreach(Transform target in targetList)
		{
			if (target.position != oldTarget)
			{
				tempList.Add(target.position);
			}
		}
		
		// get a new target from the tempList
		newTarget = targetList[Random.Range(0, tempList.Count)];
		Debug.Log(soldier.name + " has a target: " + newTarget);
		soldier.SendMessage("setTarget", newTarget.position, SendMessageOptions.DontRequireReceiver);
		
	}
	
	// spawn the units at startup
	void spawnUnit(int rosterIndex)
	{
		Vector3 newPosition = transform.position;
		newPosition += new Vector3(Random.Range(-2, 2), 0, Random.Range(-2, 2) );
		string unitName = LayerMask.LayerToName(baseLayer) + " Soldier " + (rosterIndex + 1);
		
		roster.Add( (GameObject) Instantiate(soldierTemplate, newPosition, transform.rotation) );
		roster[rosterIndex].name = unitName;
	}
	
	// respawn the agents if they are dead
	void respawn(GameObject soldier)
	{
		int index = roster.IndexOf(soldier);
		spawnUnit(index);
		
	}
	
}
