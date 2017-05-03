using UnityEngine;
using System.Collections;

//attach this script to an object, then click on the object to activate explosion, to use with particle effect explosion,
//put the explosion pre fab in the spot in the spot that says "explosion" in the inspector where the script is, then adjust
//the force and radius to make the explosion the size you want.

public class ExplosionES : MonoBehaviour {

	public float force;
	public float radius;

	public GameObject explosion;

	void OnMouseDown ()
	{
		Collider[] colliders = Physics.OverlapSphere (transform.position, radius);

		foreach (Collider c in colliders) 
		{
			if (c.GetComponent<Rigidbody>() == null)
				continue;

			c.GetComponent<Rigidbody>().AddExplosionForce (force, transform.position, radius, .5f, ForceMode.Impulse);
			
		}

		Instantiate (explosion, transform.position, Quaternion.identity);

		Destroy (this.gameObject);
	}
		
}
