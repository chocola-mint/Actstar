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
        public float speed = 6;
        [Range(0, 50)]
        public float airSpeed = 5;
        [Range(0, 200)]
        public float acceleration = 100;
        [Range(0, 200)]
        public float airAcceleration = 35;
        [ShowInPlayMode, ShowInInspector]
        public float MoveAxis { get; private set; }
        [ShowInPlayMode, ShowInInspector]
        public bool IsMoving => Mathf.Abs(MoveAxis) > 0;
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
            if(collisionState.IsTouchingRight && MoveAxis > 0
            || collisionState.IsTouchingLeft && MoveAxis < 0)
                return;
            body.SetMoveVelocityX(MoveAxis * (collisionState.IsGrounded ? speed : airSpeed));
        }
        public void Move(float moveAxis)
        {
            this.MoveAxis = moveAxis;
        }
    }
}
