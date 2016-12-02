	using UnityEngine;
using System.Collections;
using AnimatedPixelPack;
// Basic Enemy Ai Patrol script

[RequireComponent(typeof(Enemy))]

public class EnemyPatrolAi : MonoBehaviour {

	private Enemy enemy;
	public bool moveRight;

	private Character player;

	public Transform edgeChecker;
	public float EdgeCheckRadius;
	public bool notAtEdge;

	float distance;
	public bool targetfound;


	[Tooltip("Distance for Enemy to chase player")]
	public float DistanceChase = 5;

	[Tooltip("Distance for Enemy to stop chasing player")]
	public float DistanceStopChasing = 9;


	private Rigidbody2D body;

	// Use this for initialization
	void Start () {
		this.enemy = GetComponent<Enemy> ();
		body = GetComponent<Rigidbody2D> ();
		player = FindObjectOfType<Character> ();
		StartCoroutine (UpdatePath ());
	
	}
	
	// Update is called once per frame
	void Update () {
		notAtEdge = Physics2D.OverlapCircle (edgeChecker.position, EdgeCheckRadius,enemy.GroundLayer); // Detection de non-platform
		if (!notAtEdge) {
			moveRight = !moveRight;
			body.velocity = new Vector2 (0, body.velocity.y);
		}
		if (player != null) {
			
			distance = Vector2.Distance (transform.position, player.transform.position);

			if (distance < DistanceChase && Mathf.Abs(player.transform.position.y - transform.position.y) < 2) {

				targetfound = true;
			}
			if (distance > DistanceStopChasing || Mathf.Abs(player.transform.position.y - transform.position.y) > 2) {

				targetfound = false;
			}

		} else {
			targetfound = false;
		}
		if (targetfound == false) {
			if (moveRight) {
				transform.localScale = new Vector3 (1f, 1f, 1f);
				this.enemy.Move (new Vector2 (1f, 1f), true);
				body.velocity = new Vector2 (enemy.RunSpeed, body.velocity.y);

			} else {
				transform.localScale = new Vector3 (-1f, 1f, 1f);
				this.enemy.Move (new Vector2 (1f, 1f), true);
				body.velocity = new Vector2 (-enemy.RunSpeed, body.velocity.y);
			}
		}
	}


	IEnumerator UpdatePath()
	{
		Enemy.Action actionflag;
		if (targetfound == true) {
			// move right
			if (player.transform.position.x > enemy.transform.position.x) { 
				transform.localScale = new Vector3 (1f, 1f, 1f);
				this.enemy.Move (new Vector2 (1f, 1f), true);
				if (notAtEdge) {
					body.velocity = new Vector2 (enemy.RunSpeed, body.velocity.y);
				} else {
					body.velocity = new Vector2 (0, body.velocity.y);
				}

			}
		// move left
		else if (player.transform.position.x < enemy.transform.position.x) {
				//new WaitForSeconds (2f);
				transform.localScale = new Vector3 (-1f, 1f, 1f);

				if (notAtEdge) {
					body.velocity = new Vector2 (-enemy.RunSpeed, body.velocity.y);
				} else {
					body.velocity = new Vector2 (0, body.velocity.y);
				}
			}
		}
		if (player != null) {
			
			if (Mathf.Abs (player.transform.position.x - enemy.transform.position.x) < 1 && Mathf.Abs (player.transform.position.y - enemy.transform.position.y) < 2) {
					body.velocity = new Vector2 (0, body.velocity.y);
				enemy.setAnimation ("IsGrounded", true);
				actionflag = Enemy.Action.Attack;
					this.enemy.Perform (actionflag);
				enemy.setAnimation ("IsGrounded", false);
				}
			}
		yield return new WaitForSeconds (1f / 2);// delay update
		StartCoroutine (UpdatePath ());

	}
	void OnTriggerEnter2D(Collider2D c)
	{
		// Check if we collided with a main item weapon
		if ((c.tag != null && c.tag == "MainItemP") ||
			(c.name != null && c.name == "MainItemP") )
		{
			// Take some damage if we are attacked
			Character hurtBy = c.GetComponentInParent<Character>();
			if (hurtBy != null && hurtBy.IsAttacking)
			{
				// Apply damage to this character
				float direction = c.transform.position.x - this.transform.position.x;
				if (player.transform.position.x > enemy.transform.position.x) {
					this.transform.Translate (Vector2.left * 4);

				} else if (player.transform.position.x < enemy.transform.position.x) {
					this.transform.Translate (Vector2.right * 4);
				}

				this.enemy.ApplyDamage(25, direction);
			
			}
		}

	}
}
