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
    [RequireComponent(typeof(CollisionState))]
    [RequireComponent(typeof(ActstarBody))]
    public class Fastfaller : MonoBehaviour
    {
        private CollisionState collisionState;
        private ActstarBody body;
        private Jumper jumper; // Can be null!
        [ShowInPlayMode, ShowInInspector, ReadOnly]
        private bool active = false;
        [Range(-100, -0.1f), Tooltip("The fastfalling acceleration.")]
        public float acceleration = -30.0f;
        [Range(-100, -0.1f), Tooltip("The minimum speed that can be reached via fastfalling.")]
        public float terminalSpeed = -20.0f;


        void Awake() 
        {
            TryGetComponent<CollisionState>(out collisionState);
            TryGetComponent<ActstarBody>(out body);
            TryGetComponent<Jumper>(out jumper);
        }
        public void StartFastfall()
        {
            active = true;
        }
        public void CancelFastfall()
        {
            active = false;
        }
        void FixedUpdate() 
        {
            if(!active) return;
            if(collisionState.IsGrounded) return;
            if(jumper && jumper.IsJumping) return;
            float speed = body.Velocity.y + acceleration * Time.fixedDeltaTime;
            if(speed < terminalSpeed) speed = terminalSpeed;
            body.SetMoveVelocityY(speed);
        }
    }
}
