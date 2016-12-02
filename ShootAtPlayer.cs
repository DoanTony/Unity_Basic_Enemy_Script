using UnityEngine;
using AnimatedPixelPack;
using System.Collections;

public class ShootAtPlayer : MonoBehaviour {

	public float playerRange;
	public GameObject Fireball;
	public Character player;
	public Transform launchPoint;
	public float waitbetweenshots; // Cooldown before next shots
	private float shotCounter;
	Object[] listFireball = new Object[3];
	
	void Start () {
		player = FindObjectOfType<Character> ();
		shotCounter = waitbetweenshots;
	}

	
	// Update is called once per frame
	void Update () {
		Debug.DrawLine(new Vector3(transform.position.x - playerRange - 5, transform.position.y, transform.position.z),new Vector3(transform.position.x + playerRange + 5, transform.position.y, transform.position.z));
		shotCounter -= Time.deltaTime;
		if (player!= null) {
			float distance = Vector2.Distance (transform.position, player.transform.position);
			if (distance < playerRange && shotCounter < 0 && player.transform.position.y - transform.position.y < Mathf.Abs (5)) {
				//Apply for tower shots
				if (this.tag == "Tower") {
					if (player.transform.position.x < transform.position.x) {
						this.transform.localScale = new Vector2 (1f, this.transform.localScale.y);
					} else if (player.transform.position.x > transform.position.x) {
						this.transform.localScale = new Vector2 (-1f, this.transform.localScale.y);
					}
					for (int i = 0; i < 2; i++) {
						Quaternion target = Quaternion.AngleAxis ((500 * (i - (10 / 2))), transform.up);
						Instantiate (Fireball, launchPoint.position, target * launchPoint.rotation);
					}
				} else {
					Instantiate (Fireball, launchPoint.position, launchPoint.rotation);
				}
			
				shotCounter = waitbetweenshots;
			}
		}

	}
}
