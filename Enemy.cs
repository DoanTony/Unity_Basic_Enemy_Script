using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AnimatedPixelPack
{
	[RequireComponent(typeof(Animator))]
	[RequireComponent(typeof(Rigidbody2D))]
	public class Enemy : MonoBehaviour
	{
		[Header("Enemy")]
		[Tooltip("Damage to dealt to player")]
		public int AttackPower = 0;
		[Tooltip("Transform used to check if the Enemy is touching the ground")]
		public Transform GroundChecker;
		[Tooltip("Layer that contains all the 'ground' colliders")]
		public LayerMask GroundLayer;
		[Tooltip("Speed of the Enemy when running")]
		public float RunSpeed = 1000;
		[Tooltip("Factor of run speed to lose(-)/gain(+) when pressing the modifier. Use for run boost or sneak.")]
		[Range(-1, 1)]
		public float RunModifierFactor = -0.75f;
		[Tooltip("Factor of velocity to lose(-)/gain(+) when blocking (if enabled)")]
		[Range(-1, 1)]
		public float BlockingMoveFactor = -0.75f;
		[Tooltip("Should the Enemy be allowed to control direction while in the air")]
		public LayerMask WallLayer;
		[Tooltip("Factor of velocity to lose(-)/gain(+) when sliding down a wall")]
		[Range(-1, 1)]
		public bool IgnoreAnimationStates = false;
		[Tooltip("Health of the Enemy")]
		public int MaxHealth = 100;	


		// Script Properties
		public int CurrentHealth { get; private set; }
		public bool IsDead { get { return this.CurrentHealth <= 0; } }
		public Direction CurrentDirection { get; private set; }
		public float ModifiedSpeed
		{
			get
			{
				return this.RunSpeed * this.GetMultiplier(this.RunModifierFactor);
			}
		}
		public bool IsAttacking
		{
			get
			{
				AnimatorStateInfo state = this.animatorObject.GetCurrentAnimatorStateInfo(3);
				return state.IsName("Attack") || state.IsName("Quick Attack");
			}
		}

		// Members
		private Animator animatorObject;
		private Rigidbody2D body2D;
		private bool isGrounded = true;
		private bool isOnWall = false;
		private bool isOnWallFront = false;
		private bool isRunningNormal = false;
		private float groundRadius = 0.1f;
		private Direction startDirection = Direction.Right;

		public static Enemy Create(Enemy instance, Direction startDirection, Vector3 position)
		{
			Enemy c = GameObject.Instantiate<Enemy>(instance);
			c.transform.position = position;
			c.startDirection = startDirection;
			return c;
		}

		void Start()
		{
			this.body2D = this.GetComponent<Rigidbody2D>();
			this.animatorObject = this.GetComponent<Animator>();


			// Setup the Enemy
			this.CurrentHealth = this.MaxHealth;
			this.ApplyDamage(0);
			this.body2D.centerOfMass = new Vector2(0f, 0.4f);
			if (this.startDirection != Direction.Right)
			{
				this.ChangeDirection(this.startDirection);
			}
			else
			{
				this.CurrentDirection = this.startDirection;
			}


			// Warn the user if they have forgotten to setup the layers correctly
			if ((this.GroundLayer & (1 << this.gameObject.layer)) != 0)
			{
				Debug.LogWarningFormat(this, "The Enemy has its GroundLayer set incorrectly.\r\nThe GroundLayer matches the Enemy's main Layer, so it will not jump/fall correctly\r\nPlease update either the GroundLayer or the Layer of the Enemy.");
			}

			// Perform an initial ground check
			this.isGrounded = this.CheckGround();
		}

		void FixedUpdate()
		{
			// Check if we are touching the ground using the rigidbody
			this.isGrounded = this.CheckGround();

		}

		public void Move(Vector2 axis, bool isHorizontalStillPressed)
		{
			// Quit early if dead
			if (this.IsDead)
			{
				this.body2D.velocity = new Vector2(0, this.body2D.velocity.y);
				return;
			}

			if (this.IgnoreAnimationStates)
			{
				this.isGrounded = true;
				this.animatorObject.SetBool("IsGrounded", this.isGrounded);
				return;
			}

			// Get the input and speed
			if (this.isGrounded)
			{
				float horizontal = axis.x;

				

				// Set the new velocity for the Enemy based on the run modifier
				float speed = (this.isRunningNormal ? this.RunSpeed : this.ModifiedSpeed);
				Vector2 newVelocity = new Vector2(horizontal * speed * Time.deltaTime, this.body2D.velocity.y);


				this.body2D.velocity = newVelocity;
			}
				
		
			// Update the animator
			this.animatorObject.SetBool("IsGrounded", this.isGrounded);
			this.animatorObject.SetBool("IsOnWall", this.isOnWall);
			this.animatorObject.SetInteger("WeaponType", (int)this.EquippedWeaponType);
			this.animatorObject.SetFloat("AbsY", Mathf.Abs(this.body2D.velocity.y));
			this.animatorObject.SetFloat("VelocityY", this.body2D.velocity.y);
			this.animatorObject.SetFloat("VelocityX", Mathf.Abs(this.body2D.velocity.x));
			this.animatorObject.SetBool("HasMoveInput", isHorizontalStillPressed);

			// Flip the sprites if necessary
			if (this.isOnWall)
			{
				if (this.isOnWallFront)
				{
					this.ChangeDirection(this.CurrentDirection == Direction.Left ? Direction.Right : Direction.Left);
				}
			}
			else if (this.body2D.velocity.x != 0)
			{
				this.ChangeDirection(this.body2D.velocity.x < 0 ? Direction.Left : Direction.Right);
			}
		}

		public void setAnimation(String actions, bool verification)
		{
			this.animatorObject.SetBool (actions, verification);
		}

		public void Perform(Action action)
		{
			// Quit early if dead
			if (this.IsDead)
			{
				return;
			}
			// Check for the running modifier key
			this.isRunningNormal = !IsAction(action, Action.RunModified);
		

			// Now check the rest of the keys for actions
			 if (IsAction(action, Action.QuickAttack))
			{
				this.TriggerAction("TriggerQuickAttack");
			}
			else if (IsAction(action, Action.Attack))
			{
				this.TriggerAction("TriggerAttack");
			}
			else if (IsAction(action, Action.Cast))
			{
				this.TriggerAction("TriggerCast");
			}
			else if (IsAction(action, Action.ThrowOff))
			{
				this.TriggerAction("TriggerThrowOff");
			}
			else if (IsAction(action, Action.ThromMain))
			{
				this.TriggerAction("TriggerThrowMain");
			}
			else if (IsAction(action, Action.Consume))
			{
				this.TriggerAction("TriggerConsume");
			}
			else if (IsAction(action, Action.Hurt))
			{
				// Apply some damage to test the animation
				this.ApplyDamage(10);
			}
				
		}

		public bool ApplyDamage(int damage, float direction = 0)
		{
			if (!this.IsDead)
			{
				this.animatorObject.SetFloat("LastHitDirection", direction * (int)this.CurrentDirection);

				// Update the health
				this.CurrentHealth = Mathf.Clamp(this.CurrentHealth - damage, 0, this.MaxHealth);
				this.animatorObject.SetInteger("Health", this.CurrentHealth);

				if (damage != 0)
				{
					// Show the hurt animation
					this.TriggerAction("TriggerHurt", false);
				}

				if (this.CurrentHealth <= 0)
				{
					// Since the player is dead, remove the corpse
					StartCoroutine(this.DestroyAfter(1, this.gameObject));
				}
			}

			return this.IsDead;
		}

		private void TriggerAction(string action, bool isCombatAction = true)
		{
			// Update the animator object
			this.animatorObject.SetTrigger(action);

			if (isCombatAction)
			{
				// Combat actions also trigger an additional parameter to move correctly through states
				this.animatorObject.SetTrigger("TriggerCombatAction");
			}
		}

		private void ChangeDirection(Direction newDirection)
		{
			if (this.CurrentDirection == newDirection)
			{
				return;
			}

			// Swap the direction of the sprites
			Vector3 rotation = this.transform.localRotation.eulerAngles;
			rotation.y -= 180;
			this.transform.localEulerAngles = rotation;
			this.CurrentDirection = newDirection;

			SpriteRenderer[] sprites = this.GetComponentsInChildren<SpriteRenderer>();
			for (int i = 0; i < sprites.Length; i++)
			{
				Vector3 position = sprites[i].transform.localPosition;
				position.z *= -1;
				sprites[i].transform.localPosition = position;
			}
		}

		private void OnCastEffect()
		{
			// If we have an effect start it now
			if (this.Effect != null)
			{
				this.activeEffect = WeaponEffect.Create(this.Effect, this.EffectPoint);
			}
		}
		private void OnCastEffectStop()
		{
			// If we have an effect stop it now
			if (this.activeEffect != null)
			{
				this.activeEffect.Stop();
				this.activeEffect = null;
			}
		}


		private Collider2D CheckGround()
		{
			// Check if we are touching the ground using the rigidbody

			return Physics2D.OverlapCircle(GroundChecker.position, this.groundRadius, this.GroundLayer);
		}
			
		private bool IsTrigger(GameObject other, string name)
		{
			name = name.ToLower();

			if ((other.tag != null && other.tag.ToLower() == name) ||
				(other.name != null && other.name.ToLower() == name))
			{
				return true;
			}

			return false;
		}

		private bool IsAction(Action value, Action flag)
		{
			return (value & flag) != 0;
		}

		private float GetMultiplier(float factor)
		{
			if (Mathf.Sign(factor) < 0)
			{
				return 1 + factor;
			}
			else
			{
				return factor;
			}
		}

		private IEnumerator DestroyAfter(float seconds, GameObject gameObject)
		{
			yield return new WaitForSeconds(seconds);

			GameObject.Destroy(gameObject);
		}

		private IEnumerator EnableAfter(float seconds, Behaviour obj)
		{
			yield return new WaitForSeconds(seconds);

			obj.enabled = true;
		}
	}
}