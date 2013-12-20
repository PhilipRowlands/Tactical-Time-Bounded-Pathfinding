using UnityEngine;
using System.Collections;

// this operates an AI-controlled gun

public class FireAI : MonoBehaviour {
	
	public int magazineSize = 30;
	public int magazineCount;
	public int totalMags = 5;				// the maximum amount of magazines
	int roundsLeft;
	
	public GameObject bullet;
	public Transform muzzle, soldier;		// muzzle is where the bullets are spawned
	
	SoldierStates statesList;
	
	bool canFire = true;
	
	public float fireDelay = 0.1f;
	public float burstDelay = 0.4f;			// if the weapon fires bursts, how long between them?
	public float reloadDelay = 1f;

	// Use this for initialization
	void Start () {
		statesList = (SoldierStates) transform.parent.GetComponent("SoldierStates");
		soldier = transform.parent;
//		Debug.Log(soldier);
		
		magazineCount = totalMags;
		roundsLeft = magazineSize;
		
	}
	
	// get more ammo
	void replenishAmmo(int magazineNumber)
	{
		magazineCount = magazineNumber;
	}
	
	void startFiring()
	{
//		Debug.Log(transform.parent.name + " is firing!");
		InvokeRepeating("fire", 0f, fireDelay);
	}
	
	void stopFiring()
	{
		CancelInvoke("fire");
	}
	
	// create a new bullet, and decrease the rounds left in the magazine
	void fire()
	{
		// if able to fire
		if (canFire == true)
		{
			
			Instantiate(bullet, muzzle.position, transform.rotation);
			roundsLeft--;
			if (roundsLeft == 0)
			{
				canFire = false;
				Invoke ("reload", reloadDelay);
			}
		}
		
	}
	
	void reload()
	{
		if (magazineCount > 1)
			magazineCount--;
		else
		{
			statesList.SendMessage("setState", "lowOnAmmo");
			stopFiring();
			Debug.Log("Low on ammo!");
		}
		canFire = true;
	}
	
}
