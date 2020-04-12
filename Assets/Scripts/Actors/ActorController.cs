/// Author: Jeremy Anderson, March 10, 2020.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invasion
{
    /// <summary>
    /// Translates input received from an ActorInput object into actual movement,
    /// whether from the player or AI, and coordinates the animations.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))][RequireComponent(typeof(Collider2D))]
    public class ActorController : MonoBehaviour
    {
        /********************
         * =- Variables -=
         ********************/

        // ========== PRIVATE / PROTECTED ==========
        GameManager GM;                                         // a reference to the GameManager.
        Collider2D collider2d;                                  // a reference to the collider.
        Rigidbody2D rigidbody2d;                                // a reference to the rigidbody component.
        SpriteRenderer spriteRenderer;                          // a reference to the spriterenderer.
        Animator animator;                                      // a reference to the animator.
        Damager damager;                                        // a reference to the damager (if present).
        Vector2 input = new Vector2();                          // regularly updated by the ActorInput component.
        public Vector2 Input { get { return input; } }          // used to allow input to be seen publicly.
        bool accelerating = false;                              // used to determine whether to slow down.
        bool waitingToJump = false;                             // used to determine whether the actor is jumping.
        bool isRunning = false;                                 // used to determine whether the actor is running.
        bool isJumpBursting = false;                            // used to determine whether the actor is using a jetpack.
        bool isFalling = false;                                 // used to determine whether the actor is falling.
        bool isDead = false;                                    // used to determine whether the actor is dead.
        public bool IsDead { get { return isDead; } }           // used to allow isDead to be seen publicly.
        int numberOfJumps = 0;                                  // used to determine whether the actor can jump yet.
        float editorGravity;                                    // used to store the editor gravity for the actor.
        bool facingRight = true;                                // used to coordinate projectile origin when still.
        public bool FacingRight { get { return facingRight; } } // used to allow facingRight to be seen publicly.
        bool meleeRebounding = false;                           // used to control jumpback after melee attacks.
        bool playedExplosion = false;                           // whether the turret played it's explosion.
        bool isUnlocked = false;                                // whether the swarmling has been unlocked.
        bool isShooting = false;                                // whether the actor is shooting.

        // ========== PUBLIC ==========
        [Header("Automated")]
        public Stats stats;                                     // a reference to the actors stats for speed. If this component isn't present, maximumSpeed will be used.

        [Header("! IF NO STATS COMPONENT !")]
        public float runMultiplier;                             // the running speed of the actor.
        public float walkSpeed;                                 // the walking speed of the actor.
        public float jumpStrength;                              // the strength of the character's jumps.
        public int extraJumps;                                  // the maximum jumps this actor can make before falling.

        [Header("Raycasting Settings")]
        public GameObject targetingPoint;                       // a reference to the point from which targeting raycasting will be done.

        [Header("General Movement Settings")]
        public bool isStationary = false;                       // whether this actor is stationary.
        public bool hasWalkAnimation = false;                   // whether this actor walks or just runs.
        public bool stopsToShoot = true;                        // whether this actor stops to shoot or continues moving while shooting.
        public bool isWallCrawler = false;                      // whether this actor is restricted to grounded horizontal movement.
        public bool rotateTowardMovement = false;               // whether this actor will rotate to point in whichever direction it is moving.

        [Header("Force Based Movement Settings")]
        public bool moveByForce = false;                        // make the actor move by applying force instead of setting velocity.
        public Vector2 accelerationRate = new Vector2();        // allow precise control of acceleration when using force mode.
        public Vector2 decelerateAt = new Vector2();            // a drag amplifier to force a quick deceleration below a specified velocity.
        public Vector2 decelerationMultiplier = new Vector2();  // modify the rate of deceleration.

        [Header("General Jump Settings")]
        public ParticleBurster jetPackParticleBurster;          // a reference to the particle burster (if there is one).
        public bool jetPackOnlyOnForceJump;                     // is the jetpack only used on the force jump?
        public float fallingControlMultiplier;                  // how much the player can influence the fall trajectory of the actor.

        [Header("Force Based Jump Settings")]
        public bool jumpByForce = false;                        // make the actor jump by applying force instead of setting velocity.
        public float jumpForceMultiplier;                       // allow precise control of jump strength when using force jump mode.
        public float jumpVelocityMaximum;                       // stops the force-based jump from compounding too strongly.
        public float jumpGravity;                               // adjust the gravity effect for force jumping.

        [Header("Turret Sprites")]
        public GameObject explosionObject;                      // the explosion object to activate to play an explosion.
        public Sprite activeTurret;                             // the regular turret sprite.
        public Sprite destroyedTurret;                          // the destroyed turret sprite.

        // Exposed private/protected variables.
        [Header("Debug Data")]
        [SerializeField][DisplayWithoutEdit()] Vector2 velocity = new Vector2();    // display the velocity.


        /********************
         * =- Functions -=
         ********************/

        // Give the ActorController component access to the GameManager and certain other components.
        void Start()
        {
            GM = FindObjectOfType<GameManager>();
            collider2d = GetComponent<Collider2D>();
            rigidbody2d = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            stats = GetComponent<Stats>();
            damager = GetComponent<Damager>();
            editorGravity = rigidbody2d.gravityScale;

            if (animator)
                animator.SetBool("Has Walk Animation", hasWalkAnimation);
        }

        // ========== MOVEMENT ANIMATION ==========
        // Animate the actor in the Update so that there is immediate response.
        // This is the loop for animation logic.
        void Update()
        {
            if (animator)
                animator.SetBool("Dead", isDead);               // inform the animator whether the actor is alive or dead.

            if (isDead)
            {
                // Swarmlings need to fall.
                if (isUnlocked)
                    return;

                if (isWallCrawler)
                {
                    UnlockSwarmling();
                    return;
                }

                // Turrets don't have animators.
                if (playedExplosion || animator)
                    return;

                spriteRenderer.sprite = destroyedTurret;
                explosionObject.SetActive(true);
                playedExplosion = true;
                return;                                         // don't worry about the rest of Update if the actor is dead.
            }

            if (!isDead && !animator && spriteRenderer.sprite != activeTurret)
                spriteRenderer.sprite = activeTurret;

            if (isStationary)                                   // turrets just flip to face the target.
            {
                FlipByInput();
                return;                                         // Don't worry about other animations for turrets.
            }

            UpdateGravity();

            // ---- Movement ----
            if (rotateTowardMovement)
                RotateByInput();                         // rotate in direction of movement.
            else
                FlipByVelocity();                      // flip in direction of movement.
            UpdateAnimator();
            AnimateToIdle();

            // ---- Jumping ----
            AnimateJetpack();
            AnimateFalling();
        }

        // Use the horizontal transform scale to flip the actor in the direction of their INPUT.
        void FlipByInput()
        {
            if (input.x > 0)
            {
                transform.localScale = new Vector3(1, 1, 1);
                facingRight = true;
            }
            else {
                transform.localScale = new Vector3(-1, 1, 1);
                facingRight = false;
            }
        }

        // Use the horizontal transform scale to flip the actor in the direction of their VELOCITY.
        void FlipByVelocity()
        {
            if (rigidbody2d.velocity.x < 0 && transform.localScale.x != -1)
            {
                transform.localScale = new Vector3(-1, 1, 1);
                facingRight = false;
            }
            else if (rigidbody2d.velocity.x > 0 && transform.localScale.x != 1)
            {
                transform.localScale = new Vector3(1, 1, 1);
                facingRight = true;
            }
        }

        // Rotate to face movement.
        void RotateByInput()
        {
            // If there is no input, don't worry about it.
            if (input == Vector2.zero)
                return;

            Quaternion rotation = Quaternion.LookRotation(input);
            rotation.x = transform.rotation.x;
            rotation.y = transform.rotation.y;

            transform.rotation = rotation;

            FlipByInput();
        }

        // Animate the jetpack.
        void AnimateJetpack()
        {
            if (jetPackParticleBurster && isJumpBursting)
                jetPackParticleBurster.BurstParticles();
        }

        // ========== OUTPUT TO ANIMATOR ==========
        // Update the animator with the current rigidbody data of the actor.
        void UpdateAnimator()
        {
            animator.SetFloat("Horizontal Velocity", Mathf.Abs(rigidbody2d.velocity.x));
            animator.SetFloat("Vertical Velocity", rigidbody2d.velocity.y);
            animator.SetFloat("Vertical Input", input.y);
            animator.SetBool("Running", isRunning);
            if (Mathf.Abs(rigidbody2d.velocity.x) > 0)
                animator.SetBool("Walking", true);
        }

        // Let the animator know the actor is falling.
        void AnimateFalling()
        {
            if (rigidbody2d.velocity.y >= 0)        // grounded or jumping, but not falling.
            {
                animator.SetBool("Falling", false); // let the animator know the actor isn't falling.
                isFalling = false;                  // record that the actor is no longer falling.

                if (rigidbody2d.velocity.y == 0)    // grounded!
                    numberOfJumps = 0;              // reset the jumps.

                return;                             // early out.
            }

            // Velocity.y is less than 0, the actor is falling.
            if (!waitingToJump)
                animator.SetBool("Jumping", false);     // let the animator know the actor is not jumping.

            isJumpBursting = false;                     // turn off the jetPack particles.
            animator.SetBool("Falling", true);          // let the animator know the actor is falling.
            isFalling = true;                           // record that the actor is falling.
        }

        // Animate back to idle only if not moving.
        void AnimateToIdle()
        {
            if (!waitingToJump && rigidbody2d.velocity.x > -0.01 && rigidbody2d.velocity.x < 0.01)
                animator.SetBool("Walking", false);
        }

        // ========== PHYSICS ==========
        // Move the actor according to input and state in the FixedUpdate, so it synchs with physics.
        // This is the loop for physics.
        void FixedUpdate()
        {
            // ---- Unique ----
            if (isDead)
                return;                                 // don't worry about the rest of Update if the actor is dead.

            if (meleeRebounding)                        // the melee attack was successful, waiting to regain control.
                return;

            // ---- Movement ----
            if (stopsToShoot && isShooting)             // don't move if shooting and stops to shoot.
            {
                rigidbody2d.velocity = new Vector2(0, rigidbody2d.velocity.y);
                return;
            }

            if (!moveByForce)       // the default movement method sets the velocity directly.
                MoveByVelocity();
            else                    // optional movement by adding force to the rigidbody2d.
                MoveByForce();

            if (ShouldDecelerate()) // slowing down only affects moving in force mode.
                Decelerate();

            // ---- Jumping ----
            if (waitingToJump)
            {
                if (!jumpByForce)       // the default jumping method sets the velocity directly.
                    JumpByVelocity();
                else                    // optional jumping by adding force to the rigidbody2d.
                    JumpByForce();
            }
        }

        // Keep track of the actor's gravity based on force jump settings.
        void UpdateGravity()
        {
            if (jumpByForce && rigidbody2d.gravityScale != jumpGravity)
                rigidbody2d.gravityScale = jumpGravity;

            else if (!jumpByForce && rigidbody2d.gravityScale != editorGravity)
                rigidbody2d.gravityScale = editorGravity;
        }

        // Set the gravity for this rigidbody to the amount specified.
        void UnlockSwarmling()
        {
            rigidbody2d.gravityScale = 1f;
            rigidbody2d.constraints = RigidbodyConstraints2D.None;
            isUnlocked = true;
        }

        // Jump by setting the velocity of the rigidbody2d directly from stats.jumpStrength if present, or jumpStrength.
        void JumpByVelocity()
        {
            rigidbody2d.velocity = new Vector2(rigidbody2d.velocity.x, ((stats) ? stats.jumpStrength : jumpStrength));
            waitingToJump = false;

            if (jetPackParticleBurster && !jetPackOnlyOnForceJump)
                isJumpBursting = true;              // turn on the jetpack particles.
        }

        // Jump by adding force to the sprite's collider.
        void JumpByForce()
        {
            float jumpForce = ((stats) ? stats.jumpStrength : jumpStrength) * jumpForceMultiplier;
            jumpForce = ((jumpForce + rigidbody2d.velocity.y > jumpVelocityMaximum) ? 0 : jumpForce);
            rigidbody2d.AddForce(new Vector2(0.0f, jumpForce), ForceMode2D.Impulse);
            waitingToJump = false;

            if (jetPackParticleBurster)
                isJumpBursting = true;              // turn on the jetpack particles.

            velocity = rigidbody2d.velocity;
        }

        // Hit the ground after falling when dead.
        void OnCollisionEnter2D(Collision2D collision)
        {
            if (!isDead)
                return;

            // Only stop when hitting a non-actor.
            int layer = collision.gameObject.layer;
            if (layer == 12 || layer == 11 || layer == 13)
            {
                DeactivateRigidBody2D();
            } 
        }

        // ========== INPUT ==========
        // Take axis input from an ActorInput component.
        public void Move(Vector2 input)
        {
            float oldX = Mathf.Abs(this.input.x);
            float newX = Mathf.Abs(input.x);

            accelerating = (newX != 0) || (newX > oldX);
            this.input = input;
        }

        // Take run input from an ActorInput component.
        public void Run(bool isRunning, bool isToggle = false)
        {
            if (!hasWalkAnimation)                  // walk animation off, always true.
            {
                this.isRunning = true;
                return;
            }

            if (!isToggle)                          // if it's not a toggle, just track the data.
            {
                this.isRunning = isRunning;
                return;
            }

            if (isRunning)                          // if running is a toggle, toggle it.
                this.isRunning = !this.isRunning;
        }

        // Take jump input from an ActorInput component.
        public void Jump(bool isJumping)
        {
            if (!waitingToJump && isJumping && numberOfJumps < ((stats) ? stats.extraJumps : extraJumps)) // can this actor jump right now?
            {
                waitingToJump = true;                // this will be looked at to inform physics.
                animator.SetBool("Jumping", true);   // this will be looked at to inform animation.
                numberOfJumps++;                     // one less jump available.
            }
        }

        // Take shooting input from a Damager component.
        public void Shoot(bool isShooting)
        {
            if (isShooting == false)
            {
                this.isShooting = false;
                if (animator)
                    animator.SetBool("Shooting", false);
                return;
            }

            // No Damager, so no ranged attack possible.
            if (!damager)
                return;

            // Dead.
            if (isDead)
                return;

            // Failed to fire.
            if (!damager.FireProjectile())
                return;

            this.isShooting = true;
            if (animator)
                animator.SetBool("Shooting", true);                 // make sure the shooting animation plays.

        }

        // Move by setting the velocity of the rigidbody2d directly from stats.speed if present, or maximumVelocity.
        void MoveByVelocity()
        {
            Vector2 moveBy = input * ((stats) ? stats.walkSpeed : walkSpeed);

            if (!hasWalkAnimation || isRunning)     // always running or run key active.
                moveBy.x *= runMultiplier;

            if (!isWallCrawler && isFalling)
                moveBy.x *= fallingControlMultiplier;

            rigidbody2d.velocity = new Vector2(moveBy.x, ((isWallCrawler) ? moveBy.y : rigidbody2d.velocity.y));
        }

        // Move by adding accelerationRate in force to the rigidbody, limited to maximumVelocity.
        void MoveByForce()
        {
            if (!hasWalkAnimation || isRunning)     // always running or run key active.
                input *= runMultiplier;

            if (!isWallCrawler && isFalling)
                input *= fallingControlMultiplier;

            rigidbody2d.AddForce(input);
            LimitRunVelocity();
        }

        // Limit maximum velocity of the rigidbody to the Stats.speed if present, or the maximumVelocity.
        void LimitRunVelocity()
        {
            float maxVelocity = ((stats) ? stats.walkSpeed : walkSpeed);

            if (!hasWalkAnimation || isRunning)     // always running or run key active.
                maxVelocity *= runMultiplier;

            if (Mathf.Abs(rigidbody2d.velocity.x) > maxVelocity)
                rigidbody2d.velocity = new Vector2(
                    ((rigidbody2d.velocity.x > 0) ? maxVelocity : -maxVelocity),
                    rigidbody2d.velocity.y);

            // Record for debugging.
            velocity = rigidbody2d.velocity;
        }

        // If there is no input, slow down immediately.
        bool ShouldDecelerate()
        {
            return !accelerating && Mathf.Abs(rigidbody2d.velocity.x) > 0 && Mathf.Abs(rigidbody2d.velocity.x) < decelerateAt.x;
        }

        // Decelerate at an increased rate.
        void Decelerate()
        {
            rigidbody2d.AddForce(-rigidbody2d.velocity * decelerationMultiplier);
        }

        // ========== DEATH ==========
        // Inform the ActorController that it is dead.
        public void Kill() {
            isDead = true;
            rigidbody2d.AddForce(new Vector2(0, 200)); // bounce to all collision with floor.
            AIBrain brain = GetComponent<AIBrain>();
            if (brain.isSwarmling)
            {
                brain.IsAlive = false;
                
            }
        }

        void DeactivateRigidBody2D()
        {
            collider2d.isTrigger = true;
            rigidbody2d.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        // ========== MISC ==========
        public void MeleeRebounding(bool meleeSuccessful)
        {
            this.meleeRebounding = meleeSuccessful;
        }
    }
}
