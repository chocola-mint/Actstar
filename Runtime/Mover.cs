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
            if(Mathf.Approximately(MoveAxis, 0)) return;
            // Prevent moving against walls to get stuck on them.
            if(collisionState.IsTouchingRight && MoveAxis > 0
            || collisionState.IsTouchingLeft && MoveAxis < 0)
                return;
            float moveSpeed;
            if(collisionState.IsGrounded)
                moveSpeed = IsDashing ? dashSpeed : speed;
            else moveSpeed = IsDashing ? airDashSpeed : airSpeed;
            body.SetMoveVelocityX(MoveAxis * moveSpeed);
        }
        public void Move(float moveAxis)
        {
            this.MoveAxis = moveAxis;
        }
        public void StartDashing()
        {
            if(collisionState.IsGrounded)
                IsDashing = true;
        }
        public void StopDashing()
        {
            IsDashing = false;
        }
    }
}
