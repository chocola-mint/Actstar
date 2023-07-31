using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TriInspector;

namespace CHM.Actstar
{
    /// <summary>
    /// A meditator for a velocity-based Rigidbody2D.
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
            Vector2 damping = collisionState.IsTouchingBottom ? groundDamping : airDamping;
            Vector2 trueMoveVelocity = rb.velocity * damping;
            if(moveXSet)
                trueMoveVelocity.x = moveVelocity.x;
            if(moveYSet)
                trueMoveVelocity.y = moveVelocity.y;

            UpdateVerticalState();
            
            rb.velocity = Vector3.Slerp(rb.velocity, trueMoveVelocity, moveWeight); 
            
            moveXSet = moveYSet = false;
        }
        void OnCollisionStay2D(Collision2D other) 
        {
            if(sticky && other.rigidbody)
                transform.SetParent(other.rigidbody.transform);
        }
        void OnCollisionExit2D(Collision2D other) 
        {
            if(sticky && other.rigidbody)
                if(other.rigidbody.transform == transform.parent) 
                    transform.SetParent(null);
        }
        private void UpdateVerticalState()
        {
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
                    if(collisionState.IsTouchingBottom) return; // todo: this doesn't cover every case
                    if(rb.velocity.y <= 0) verticalState = VerticalState.Falling;
                    else verticalState = VerticalState.Rising;
                    break;
            }
        }
    }
}
