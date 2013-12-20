using UnityEngine;
using System.Collections;

public class MyBulletScript : MonoBehaviour {
	
	public int speed = 5;
	public int bulletDamage = 10;
	public int range = 20;
	
	int noBulletsLayer, blue, orange;
	
	Vector3 start;
	
	void Start()
	{
		noBulletsLayer = LayerMask.NameToLayer("Ignore Attacks");
		blue = LayerMask.NameToLayer("Blue");
		orange = LayerMask.NameToLayer("Orange");
		start = transform.position;
		
		// randomly change the angle to account for spread
		transform.Rotate(transform.up, Random.Range(-0.5f, 0.51f) );
		
		Invoke("destroyNow", 2f);		// backup to destroy it
	}
	
	// Update is called once per frame
	void Update () {
		
		transform.Translate(Vector3.forward * speed * Time.deltaTime);
		
		if (Vector3.Distance(start, transform.position) > range )
			Destroy(gameObject);		// destroy it if it goes too far
		

	}
	
	void OnCollisionEnter(Collision coll)
	{
		GameObject collObject = coll.gameObject;
//		Debug.Log(collObject);
		int objectLayer = collObject.layer;
		
		if (objectLayer != noBulletsLayer)
		{
			if ( (objectLayer == blue || objectLayer == orange) && coll.collider.isTrigger == false )
			{
				collObject.SendMessage("damage", bulletDamage);
				collObject.SendMessage("setState", "isUnder Fire");
				
			}
			Destroy(gameObject);
		}
	}
	
	void destroyNow()
	{
		Destroy(gameObject);
	}
	
}
