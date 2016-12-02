using UnityEngine;
using AnimatedPixelPack;
using System.Collections;

//Enemy caster projectile

public class Fireball : MonoBehaviour {

	public float speed;
	public Character player;

	public float rotationspeed;

	private Rigidbody2D myrigidbody2D;

	// Use this for initialization
	void Start () {
		player = FindObjectOfType<Character> ();
		myrigidbody2D = GetComponent<Rigidbody2D> ();

		if (player.transform.position.x < transform.position.x) {
			speed = -speed;

			rotationspeed = -rotationspeed;
		}
		else if (player.transform.position.x > transform.position.x) {
			speed = speed;
			rotationspeed = rotationspeed;
		}
	}


	void Update () {
	
		myrigidbody2D.velocity = new Vector2 (speed,transform.rotation.y);
		myrigidbody2D.angularDrag = rotationspeed;
}


	void OnTriggerEnter2D(Collider2D c)
	{
		
			if (c.tag == "Player") {
				Character.Action actionflag;
				actionflag = Character.Action.Hurt;
				player.ApplyDamage (10);
				player.Perform (actionflag);	
				Debug.Log (player.CurrentHealth);
				Destroy (gameObject);
		
			}
		if (c.tag == "Wall" && c.tag != "untagged" ) {
				Destroy (gameObject);
			}

		}

	}
	