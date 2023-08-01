using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TriInspector;

namespace CHM.Actstar
{
    /// <summary>
    /// Base class for jump implementations.
    /// </summary>
    [RequireComponent(typeof(ActstarBody))]
    public abstract class Jumper : MonoBehaviour
    {
        [Range(0, 1f), Tooltip("The amount of extra grounded time after leaving the ground.")]
        public float coyoteTime = 0.2f;
        #if UNITY_EDITOR
        [ShowInPlayMode, ShowInInspector, LabelText("Can Jump")]
        private bool DebugCanJump => Application.isPlaying ? CanJump : false;
        [ShowInPlayMode, ShowInInspector, LabelText("Is Jumping")]
        private bool DebugIsJumping => Application.isPlaying ? IsJumping : false;
        #endif
        public abstract bool CanJump { get; }
        public abstract bool IsJumping { get; }
        protected ActstarBody body;
        private float extendGroundTimeUntil = float.MinValue;
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
        protected virtual void OnStart() {}
        protected abstract void OnFixedUpdate();
        protected abstract void OnJump();
        void Awake() 
        {
            TryGetComponent<ActstarBody>(out body);
            OnAwake();
        }
        void Start() 
        {
            body.onTakeoff += UpdateCoyoteTime;
            OnStart();
        }
        // Update is called once per frame
        void FixedUpdate()
        {
            UpdateCoyoteTime();
            OnFixedUpdate();
        }
        private void UpdateCoyoteTime()
        {
            extendGroundTimeUntil = Time.fixedTime + coyoteTime;
        }
    }
}
