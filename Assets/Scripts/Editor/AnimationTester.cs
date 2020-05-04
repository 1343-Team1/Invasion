/// Author: Jeremy Anderson, March 13, 2020.

using System.Collections.Generic;
using UnityEngine;

namespace Invasion
{
    /// <summary>
    /// Allows all animations that exist to be played in the editor for testing.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class AnimationTester : MonoBehaviour
    {
        /********************
         * =- Variables -=
         ********************/

        // Private/protected variables.
        Animator animator;                              // the animator to update.
        Dictionary<Animation, string> getName = new Dictionary<Animation, string>(); // a way to look up strings based on the enumerator.

        // Public variables.
        public enum Animation                           // an enumerator to make a dropdown in the editor.
        {
            Idle,
            Walk,
            Crawl,
            Run,
            Jump,
            Attack,
            Die
        }
        public Animation animationToPlay;               // which animation is currently playing?
        [EditorButton("Change Facing")]                 // a button that will execute the following method.
        public void ChangeFacing()                      // make the animation face the other direction.
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, 0f);
        }


        /********************
         * =- Functions -=
         ********************/

        // Get the animator and initialize the dictionary.
        void Start()
        {
            animator = GetComponent<Animator>();
            getName.Add(Animation.Idle, "Idle");
            getName.Add(Animation.Walk, "Walk");
            getName.Add(Animation.Crawl, "Crawl");
            getName.Add(Animation.Run, "Run");
            getName.Add(Animation.Jump, "Jump");
            getName.Add(Animation.Attack, "Attack");
            getName.Add(Animation.Die, "Die");
        }

        // Switch between animations.
        void Update()
        {
            PlayAnimation(animationToPlay);
        }

        // Make sure only the specified animation is playing.
        void PlayAnimation(Animation animation)
        {
            TurnAnimationsOff();
            animator.SetBool(getName[animationToPlay], true);
        }

        // Turn off all animations.
        void TurnAnimationsOff()
        {
            animator.SetBool("Idle", false);
            animator.SetBool("Walk", false);
            animator.SetBool("Crawl", false);
            animator.SetBool("Run", false);
            animator.SetBool("Jump", false);
            animator.SetBool("Attack", false);
            animator.SetBool("Die", false);
        }
    }
}
