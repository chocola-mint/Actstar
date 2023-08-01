using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TriInspector;

namespace CHM.Actstar
{
    /// <summary>
    /// Implements fastfalling. If it detects a Jumper component, it will wait until
    /// the Jumper is no longer jumping.
    /// </summary>
    [DeclareBoxGroup("PPS", Title = "Platform Probe Settings")]
    [RequireComponent(typeof(CollisionState))]
    [RequireComponent(typeof(ActstarBody))]
    public class Fastfaller : MonoBehaviour
    {
        private CollisionState collisionState;
        private ActstarBody body;
        private IJumper jumper; // Can be null!
        private Platformer platformer; // Can be null!
        [ShowInPlayMode, ShowInInspector, ReadOnly]
        public bool IsFastfalling { get; private set; } = false;
        [Range(-100, -0.1f), Tooltip("The fastfalling acceleration.")]
        public float acceleration = -30.0f;
        [Range(-100, -0.1f), Tooltip("The minimum speed that can be reached via fastfalling.")]
        public float terminalSpeed = -20.0f;
        [SerializeField]
        [Tooltip("Enable to fall through PlatformEffector2Ds. Do not change this field via the inspector when playing.")]
        [HideInPlayMode, ValidateInput(nameof(ValidateFallthroughSetup))]
        private bool toggleFallthrough = false;
        private TriValidationResult ValidateFallthroughSetup()
        {
            if(!TryGetComponent<Platformer>(out _)) 
                return TriValidationResult.Warning(
                    $"A {nameof(Platformer)} component must exist "
                    + "on this GameObject for fallthrough to work.");
            return TriValidationResult.Valid;
        }
        void Awake() 
        {
            TryGetComponent<CollisionState>(out collisionState);
            TryGetComponent<ActstarBody>(out body);
            TryGetComponent<IJumper>(out jumper);
            TryGetComponent<Platformer>(out platformer);
        }
        public void StartFastfall()
        {
            IsFastfalling = true;
            if(toggleFallthrough && platformer)
                platformer.UseFallthrough = true;
        }
        public void CancelFastfall()
        {
            IsFastfalling = false;
            if(toggleFallthrough && platformer)
                platformer.UseFallthrough = false;
        }
        public void EnableFallthrough()
        {
            if(IsFastfalling && platformer)
                platformer.UseFallthrough = true;
            toggleFallthrough = true;
        }
        public void DisableFallthrough()
        {
            toggleFallthrough = false;
            if(toggleFallthrough && platformer)
                platformer.UseFallthrough = false;
        }
        void FixedUpdate() 
        {
            if(!IsFastfalling) return;
            if(collisionState.IsTouchingBottom) return;
            if(jumper != null && jumper.IsJumping) return;
            float speed = body.Velocity.y + acceleration * Time.fixedDeltaTime;
            if(speed < terminalSpeed) speed = terminalSpeed;
            body.SetMoveVelocityY(speed);
        }
    }
}
