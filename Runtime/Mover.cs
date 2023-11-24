using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TriInspector;

namespace CHM.Actstar
{
    /// <summary>
    /// Implements horizontal movement. Notably lets the player stick to the ground
    /// while moving.
    /// </summary>
    [RequireComponent(typeof(CollisionState))]
    [RequireComponent(typeof(ActstarBody))]
    public class Mover : MonoBehaviour
    {
        [Range(0, 50)]
        public float speed = 4;
        [Range(0, 50)]
        public float airSpeed = 3;
        [Range(0, 50)]
        public float dashSpeed = 6;
        [Range(0, 50)]
        public float airDashSpeed = 5;
        public bool lockYWhenDashing = false;
        #if UNITY_EDITOR
        [DisableIf(nameof(IsRuntime))]
        #endif
        [SerializeField, Min(0), Tooltip(
            "A force exerted against the bottom normal for one frame "
            + "when the body lands. Can help with landing and snapping "
            + "to moving platforms. If this field is set too large, "
            + "you may experience numerical instability. So it's best to leave it as is.")]
        private float stabilizingImpulse = 100;
        [ShowInPlayMode, ShowInInspector]
        public float MoveAxis { get; private set; }
        [ShowInPlayMode, ShowInInspector]
        public bool IsMoving => Mathf.Abs(MoveAxis) > 0;
        [ShowInPlayMode, ShowInInspector, ReadOnly]
        public bool IsDashing { get; private set; } = false;
        private CollisionState collisionState;
        private ActstarBody body;
        void Awake() 
        {
            TryGetComponent<CollisionState>(out collisionState);
            TryGetComponent<ActstarBody>(out body);
        }
        void Start() 
        {
            body.onGrounded += () => {
                body.AddForce(collisionState.BottomNormal * -stabilizingImpulse);
            };
        }
        void FixedUpdate() 
        {
            if(Mathf.Approximately(MoveAxis, 0)) 
                return;
            // Prevent moving against walls to get stuck on them.
            if(collisionState.IsTouchingRight && MoveAxis > 0
            || collisionState.IsTouchingLeft && MoveAxis < 0)
                return;
            float moveSpeed;
            if(body.IsGrounded)
                moveSpeed = IsDashing ? dashSpeed : speed;
            else moveSpeed = IsDashing ? airDashSpeed : airSpeed;
            if(TryGetSurfaceVector(out Vector2 surfaceVector))
            {
                Debug.DrawRay(transform.position, surfaceVector, Color.yellow);
                Vector2 vel = surfaceVector.normalized * moveSpeed;
                body.SetMoveVelocity(vel);
            }
            else
            {
                body.SetMoveVelocityX(MoveAxis * moveSpeed);
            }
        }
        private bool TryGetSurfaceVector(out Vector2 surfaceVector)
        {
            if(body.IsGrounded)
            {
                surfaceVector = Vector2.Perpendicular(collisionState.BottomNormal);
                surfaceVector *= -MoveAxis;
                return true;
            }
            else if(collisionState.IsTouchingTop)
            {
                surfaceVector = Vector2.Perpendicular(collisionState.TopNormal);
                surfaceVector *= MoveAxis;
                if(surfaceVector.y >= 0)
                    return false;
                return true;
            }
            // else if(collisionState.IsTouchingSide)
            // {
            //     surfaceVector = Vector2.Perpendicular(collisionState.SideNormal);
            //     return true;
            // }
            surfaceVector = Vector2.zero;
            return false;
        }
        private void DashLockYUpdate()
        {
            if(lockYWhenDashing && IsDashing)
                body.SetMoveVelocityY(0);
        }
        public void Move(float moveAxis)
        {
            this.MoveAxis = moveAxis;
        }
        public void StartDashing()
        {
            if(body.IsGrounded)
                IsDashing = true;
        }
        public void StopDashing()
        {
            IsDashing = false;
        }
        #if UNITY_EDITOR
        private bool IsRuntime() => Application.isPlaying;
        #endif
    }
}
