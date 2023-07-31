using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TriInspector;

namespace CHM.Actstar
{
    /// <summary>
    /// Jump implementation that lets the player jump multiple times in midair.
    /// This is the most common type of "multi-jump" in 2D platformers.
    /// </summary>
    public class AirJumper : Jumper
    {
        
        [SerializeField,
        InfoBox("The speed curve of each jump along the Y axis, time-normalized.")]
        private AnimationCurve[] jumpCurves = {
            AnimationCurve.EaseInOut(0, 7, 1, 0),
        };
        [Tooltip("Set to a negative number for infinite jumps.")]
        public int maximumJumpCount = 2;
        [Range(0, 2), Tooltip("The time length of each jump.")]
        public float jumpCooldown = 0.25f;
        public override bool CanJump => (maximumJumpCount < 0 
        || jumpPointer < maximumJumpCount - 1) 
        && Time.fixedTime - previousJumpTime > jumpCooldown;
        public override bool IsJumping => jumpPointer >= 0 
        && Time.fixedTime - previousJumpTime <= jumpCooldown;
        private int jumpPointer = -1;
        protected override void OnJump()
        {
            AdvanceJumpPointer();
        }
        protected override void OnFixedUpdate()
        {
            UpdateJumpPointer();
            UpdateJump();
        }
        private void UpdateJumpPointer()
        {
            // Reset jump pointer on ground.
            if (Grounded && !wasGrounded)
                ResetJumpPointer();
            // Take away one extra jump on leave ground.
            else if (!Grounded && !wasGrounded && jumpPointer == -1)
                AdvanceJumpPointer();
        }
        private void UpdateJump()
        {
            if(IsJumping)
            {
                float t = (Time.fixedTime - previousJumpTime) / jumpCooldown;
                if(jumpCancelled && jumpPointer == 0) t = 1;
                float jumpSpeed = GetCurrentJumpCurve()
                .Evaluate(t);
                body.SetMoveVelocityY(jumpSpeed);
                body.SetRising();
            }
        }
        private void AdvanceJumpPointer()
        {
            jumpPointer++;
        }
        private void ResetJumpPointer()
        {
            jumpPointer = -1;
        }
        private AnimationCurve GetCurrentJumpCurve()
        {
            if(maximumJumpCount < 0)
            {
                return jumpCurves[Mathf.Min(jumpPointer, jumpCurves.Length - 1)];
            }
            else
            {
                return jumpCurves[Mathf.Min(
                    jumpPointer, 
                    maximumJumpCount - 1, 
                    jumpCurves.Length - 1)];
            }
        }
    }
}
