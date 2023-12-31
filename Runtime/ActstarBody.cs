using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TriInspector;

namespace CHM.Actstar
{
    /// <summary>
    /// The core of the Actstar package. A meditator for a velocity-based Rigidbody2D.
    /// </summary>
    [DefaultExecutionOrder(1000)]
    [RequireComponent(typeof(CollisionState))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class ActstarBody : MonoBehaviour
    {
        private CollisionState collisionState;
        private Rigidbody2D rb;
        private Vector2 moveVelocity = Vector2.zero;
        private float remainingKnockbackDuration = 0;
        private float moveWeight = 1;
        private bool moveXSet = false, moveYSet = false;
        [InfoBox("Damping is used to slow down the Rigidbody. Numbers close to 1 are recommended.")]
        public Vector2 groundDamping = new(0.85f, 1f);
        public Vector2 airDamping = new(0.95f, 1f);
        public Vector2 Velocity => rb.velocity;
        [Tooltip(
@"Enable to automatically parent to objects that collide with the body.
Good for making the body stick to moving platforms.")]
        public bool sticky = true;
        private enum VerticalState
        {
            Grounded, // Initial state
            Rising, // Y speed > 0
            Falling, // Y speed <= 0
        }
        [ReadOnly, ShowInInspector, ShowInPlayMode]
        private VerticalState verticalState = VerticalState.Grounded;
        public bool IsGrounded => verticalState == VerticalState.Grounded;
        public bool IsRising => verticalState == VerticalState.Rising;
        public bool IsFalling => verticalState == VerticalState.Falling;
        public event System.Action onGrounded, onTakeoff;
        private IMovingPlatform currentPlatform = null;
        public void SetMoveVelocityX(float x)
        {
            moveVelocity.x = x;
            moveXSet = true;
        }
        public void SetMoveVelocityY(float y)
        {
            moveVelocity.y = y;
            moveYSet = true;
        }
        public void SetMoveVelocity(Vector2 velocity)
        {
            moveVelocity = velocity;
            moveXSet = moveYSet = true;
        }
        public void SetRising()
        {
            verticalState = VerticalState.Rising;
        }
        public void AddKnockbackImpulse(Vector2 impulse, float duration)
        {
            rb.AddForce(impulse, ForceMode2D.Impulse);
            remainingKnockbackDuration += duration;
            moveWeight = 0;
        }
        public void AddForce(Vector2 force)
        {
            rb.AddForce(force);
        }
        void Awake() 
        {
            TryGetComponent<Rigidbody2D>(out rb);
            TryGetComponent<CollisionState>(out collisionState);
        }
        void FixedUpdate() 
        {
            if(remainingKnockbackDuration <= 0)
            {
                moveWeight = Mathf.Min(moveWeight + Time.fixedDeltaTime, 1);
            }
            if(IsGrounded) rb.AddForce(-collisionState.BottomNormal * 200);

            Vector2 damping = IsGrounded ? groundDamping : airDamping;
            Vector2 trueMoveVelocity = rb.velocity * damping;
            Vector2 platformVelocity = Vector2.zero;
            if(currentPlatform != null) 
            {
                platformVelocity = currentPlatform.Velocity;
                if(!moveXSet)
                    platformVelocity.x = 0;
                // rb.AddForce(-collisionState.BottomNormal * 100);
            }
            if(moveXSet)
                trueMoveVelocity.x = moveVelocity.x;
            if(moveYSet)
                trueMoveVelocity.y = moveVelocity.y;
            
            trueMoveVelocity = Vector3.Slerp(rb.velocity, trueMoveVelocity + 6 * platformVelocity, moveWeight);
            rb.velocity = trueMoveVelocity;

            UpdateVerticalState();
            
            moveXSet = moveYSet = false;
        }
        void OnCollisionEnter2D(Collision2D other) 
        {
            if(sticky && other.gameObject.TryGetComponent<IMovingPlatform>(out var platform))
            {
                transform.SetParent(other.transform);
                currentPlatform = platform;
            }
        }
        void OnCollisionExit2D(Collision2D other) 
        {
            if(sticky && other.gameObject.TryGetComponent<IMovingPlatform>(out var platform))
                if(platform == currentPlatform) 
                {
                    transform.SetParent(null);
                    currentPlatform = null;
                }
        }
        private void UpdateVerticalState()
        {
            bool wasGrounded = IsGrounded;
            switch(verticalState)
            {
                case VerticalState.Rising:
                    if(rb.velocity.y <= 0) verticalState = VerticalState.Falling;
                    break;
                case VerticalState.Falling:
                    if(collisionState.IsTouchingBottom) verticalState = VerticalState.Grounded;
                    else if(rb.velocity.y > 0) verticalState = VerticalState.Rising;
                    break;
                case VerticalState.Grounded:
                default:
                    if(collisionState.IsTouchingBottom) return;
                    if(rb.velocity.y <= 0) verticalState = VerticalState.Falling;
                    else verticalState = VerticalState.Rising;
                    break;
            }
            if(!wasGrounded && IsGrounded) onGrounded?.Invoke();
            else if(wasGrounded && !IsGrounded) onTakeoff?.Invoke();
        }
    }
}
