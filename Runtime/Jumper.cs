using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TriInspector;

namespace CHM.Actstar
{
    [RequireComponent(typeof(CollisionState))]
    [RequireComponent(typeof(ActstarBody))]
    public abstract class Jumper : MonoBehaviour
    {
        [Range(0, 1f), Tooltip("The amount of extra grounded time after leaving the ground.")]
        public float coyoteTime = 0.2f;
        #if UNITY_EDITOR
        [ShowInPlayMode, ShowInInspector, LabelText("Can Jump")]
        private bool DebugCanJump => collisionState != null ? CanJump : false;
        [ShowInPlayMode, ShowInInspector, LabelText("Is Jumping")]
        private bool DebugIsJumping => collisionState != null ? IsJumping : false;
        #endif
        public abstract bool CanJump { get; }
        public abstract bool IsJumping { get; }
        protected CollisionState collisionState;
        protected ActstarBody body;
        private bool wasCollisionStateGrounded = true;
        private float extendGroundTimeUntil = float.MinValue;
        protected bool Grounded => collisionState.IsGrounded 
        || Time.fixedTime < extendGroundTimeUntil;
        protected bool wasGrounded { get; private set; } = true;
        protected float previousJumpTime { get; private set; } = float.MinValue;
        protected bool jumpCancelled { get; private set; } = false;
        public void Jump()
        {
            if(CanJump)
            {
                previousJumpTime = Time.fixedTime;
                jumpCancelled = false;
                OnJump();
            }
        }
        public void CancelJump()
        {
            jumpCancelled = true;
        }
        protected virtual void OnAwake() {}
        protected abstract void OnFixedUpdate();
        protected abstract void OnJump();
        void Awake() 
        {
            TryGetComponent<CollisionState>(out collisionState);
            TryGetComponent<ActstarBody>(out body);
            OnAwake();
        }
        // Update is called once per frame
        void FixedUpdate()
        {
            UpdateCoyoteTime();
            OnFixedUpdate();
            wasCollisionStateGrounded = collisionState.IsGrounded;
            wasGrounded = Grounded;
        }
        private void UpdateCoyoteTime()
        {
            if(!collisionState.IsGrounded && wasCollisionStateGrounded)
            {
                extendGroundTimeUntil = Time.fixedTime + coyoteTime;
            }
        }
    }
}
