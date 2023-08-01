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
        public bool CanJump => (maximumJumpCount < 0 
        || jumpPointer < maximumJumpCount - 1) 
        && Time.fixedTime - previousJumpTime > jumpCooldown;
        public bool IsJumping => jumpPointer >= 0 
        && Time.fixedTime - previousJumpTime <= jumpCooldown;
        private int jumpPointer = -1;
        [Range(0, 1f), Tooltip("The amount of extra grounded time after leaving the ground.")]
        public float coyoteTime = 0.2f;
        private ActstarBody body;
        private float extendGroundTimeUntil = float.MinValue;
        private float previousJumpTime = float.MinValue;
        private bool jumpCancelled  = false;
        #if UNITY_EDITOR
        [ShowInPlayMode, ShowInInspector, LabelText("Can Jump")]
        protected bool DebugCanJump => Application.isPlaying ? CanJump : false;
        [ShowInPlayMode, ShowInInspector, LabelText("Is Jumping")]
        protected bool DebugIsJumping => Application.isPlaying ? IsJumping : false;
        #endif
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
            jumpCancelled = true;
        }
        void Awake() 
        {
            TryGetComponent<ActstarBody>(out body);
        }
        void Start() 
        {
            body.onTakeoff += UpdateCoyoteTime;
            body.onGrounded += ResetJumpPointer;
            body.onTakeoff += AdvanceJumpPointer;
        }
        // Update is called once per frame
        void FixedUpdate()
        {
            UpdateCoyoteTime();
            UpdateJump();
        }
        private void UpdateCoyoteTime()
        {
            extendGroundTimeUntil = Time.fixedTime + coyoteTime;
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
