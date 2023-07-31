using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TriInspector;

namespace CHM.Actstar
{
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
        void FixedUpdate() 
        {
            if(Mathf.Approximately(MoveAxis, 0)) 
                return;
            // Prevent moving against walls to get stuck on them.
            if(collisionState.IsTouchingRight && MoveAxis > 0
            || collisionState.IsTouchingLeft && MoveAxis < 0)
                return;
            if(body.IsGrounded)
                GroundMoveUpdate();
            else AirMoveUpdate();
            DashLockYUpdate();
        }
        private void GroundMoveUpdate()
        {
            float moveSpeed = IsDashing ? dashSpeed : speed;
            var alongSurface = Vector2.Perpendicular(collisionState.BottomNormal);
            // Align with move axis. Perpendicular rotates the vector CCW.
            // Since the bottom normal faces upwards, this means it will be facing -X.
            alongSurface *= -MoveAxis;
            Vector2 vel = alongSurface.normalized * moveSpeed;
            body.SetMoveVelocityX(vel.x);
            if(!Mathf.Approximately(vel.y, 0))
            { 
                // vel.y -= 0.1f; // bias
                body.SetMoveVelocityY(vel.y);
            }
        }
        private void AirMoveUpdate()
        {
            float moveSpeed = IsDashing ? airDashSpeed : airSpeed;
            body.SetMoveVelocityX(MoveAxis * moveSpeed);
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
    }
}
