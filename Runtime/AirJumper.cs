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
    [RequireComponent(typeof(ActstarBody))]
    public class AirJumper : MonoBehaviour, IJumper
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
        [Min(0), Tooltip("How fast the jump speed decays exponentially on jump cancel."
        + "The higher this number is, the more abrupt short-hops will look.")]
        public float jumpCancelDecayRate = 10;
        [Range(0, 1f), Tooltip("The amount of extra grounded time after leaving the ground.")]
        public float coyoteTime = 0.2f;
        [ShowInPlayMode, ShowInInspector]
        public bool CanJump => (maximumJumpCount < 0 
        || jumpPointer < maximumJumpCount - 1) 
        && Time.fixedTime - previousJumpTime > jumpCooldown;
        [ShowInPlayMode, ShowInInspector]
        public bool IsJumping => jumpPointer >= 0 
        && Time.fixedTime - previousJumpTime <= jumpCooldown;
        private ActstarBody body;
        // The jump pointer points to the current jump curve index.
        // It starts from -1, which indicates that no jump curve is active at the moment.
        private int jumpPointer = -1;
        private float coyoteTimeUntil = float.MinValue;
        private float previousJumpTime = float.MinValue;
        private float previousJumpCancelTime = float.MinValue;
        private bool jumpCancelled = false;
        public void Jump()
        {
            if(CanJump)
            {
                previousJumpTime = Time.fixedTime;
                jumpCancelled = false;
                AdvanceJumpPointer();
            }
        }
        public void CancelJump()
        {
            // Only the first jump is cancellable (short hop)
            if(jumpPointer == 0)
            {
                jumpCancelled = true;
                previousJumpCancelTime = Time.fixedTime;
            }
        }
        void Awake() 
        {
            TryGetComponent<ActstarBody>(out body);
        }
        void Start() 
        {
            body.onTakeoff += UpdateCoyoteTime;
            body.onTakeoff += AdvanceJumpPointer;
            body.onGrounded += ResetJumpPointer;
        }
        // Update is called once per frame
        void FixedUpdate()
        {
            UpdateCoyoteTime();
            UpdateJump();
        }
        private void UpdateCoyoteTime()
        {
            coyoteTimeUntil = Time.fixedTime + coyoteTime;
        }
        private void UpdateJump()
        {
            if(IsJumping)
            {
                float jumpSpeed = GetJumpSpeed();
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
        private float GetJumpSpeed()
        {
            float t = (Time.fixedTime - previousJumpTime) / jumpCooldown;
            if(jumpCancelled) 
            {
                float t2 = (Time.fixedTime - previousJumpCancelTime) * jumpCancelDecayRate;
                float decay = 1.0f / Mathf.Exp(t2);
                return decay * GetCurrentJumpCurve().Evaluate(t);
            }
            else return GetCurrentJumpCurve().Evaluate(t);
        }
    }
}
