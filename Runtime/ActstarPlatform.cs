using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TriInspector;

namespace CHM.Actstar
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class ActstarPlatform : MonoBehaviour//, IMovingPlatform
    {
        private Rigidbody2D rb;
        private Vector2 previousPosition;
        public Vector2 Delta { get; private set; } = Vector2.zero;
        [ShowInInspector, ShowInPlayMode]
        public Vector2 Velocity => Delta / Time.fixedDeltaTime;
        void Awake() 
        {
            TryGetComponent<Rigidbody2D>(out rb);
        }
        void Start()
        {
            previousPosition = rb.position;
        }
        void FixedUpdate()
        {
            Delta = rb.position - previousPosition;
            previousPosition = rb.position; 
        }
    }
}
