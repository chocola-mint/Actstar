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
        private Jumper jumper; // Can be null!
        [ShowInPlayMode, ShowInInspector, ReadOnly]
        public bool IsFastfalling { get; private set; } = false;
        [Range(-100, -0.1f), Tooltip("The fastfalling acceleration.")]
        public float acceleration = -30.0f;
        [Range(-100, -0.1f), Tooltip("The minimum speed that can be reached via fastfalling.")]
        public float terminalSpeed = -20.0f;
        [SerializeField]
        [Tooltip("Enable to fall through PlatformEffector2Ds. Do not change this field via the inspector when playing.")]
        [HideInPlayMode]
        private bool toggleFallthrough = false;
        [GroupNext("PPS")]
        [EnableIf(nameof(toggleFallthrough))]
        [Min(0), Tooltip("The width of the probe box. The wider the probe, the less likely a platform will be missed.")]
        public float probeWidth = 8.0f;
        [EnableIf(nameof(toggleFallthrough))]
        [Min(0), Tooltip("The height of the probe box. Should be a sufficiently large number.")]
        public float probeHeight = 10000.0f;
        [EnableIf(nameof(toggleFallthrough))]
        [Min(1), Tooltip("The size of the collider buffer used when fastfall starts. "
        + "Make this larger if you expect a lot of colliders to be covered by the probe.")]
        private int probeBufferSize = 32;
        #if UNITY_EDITOR
        [SerializeField, Tooltip("[EDITOR ONLY] If set to true, "
        + "the probe area will be shown in the scene viewport.")]
        [EnableIf(nameof(toggleFallthrough))]
        private bool drawProbeArea = false;
        private DrawProbe drawProbe;
        #endif
        private HashSet<Collider2D> ignoredColliders = new();
        private Collider2D[] probeBuffer;
        private Collider2D[] attachedColliders;
        void Awake() 
        {
            TryGetComponent<CollisionState>(out collisionState);
            TryGetComponent<ActstarBody>(out body);
            TryGetComponent<Jumper>(out jumper);
            attachedColliders = GetComponentsInChildren<Collider2D>(true);
            probeBuffer = new Collider2D[probeBufferSize];
            #if UNITY_EDITOR
                drawProbe = DrawProbe.Create(new(probeWidth, probeHeight));
                drawProbe.gameObject.SetActive(false);
            #endif
        }
        public void StartFastfall()
        {
            IsFastfalling = true;
            ignoredColliders.Clear();
            SetProbe();
        }
        public void CancelFastfall()
        {
            IsFastfalling = false;
            ResetProbe();
        }
        public void EnableFallthrough()
        {
            toggleFallthrough = true;
            ignoredColliders.Clear();
        }
        public void DisableFallthrough()
        {
            ResetProbe();
            toggleFallthrough = false;
        }
        void FixedUpdate() 
        {
            if(!IsFastfalling) return;
            if(collisionState.IsGrounded) return;
            if(jumper && jumper.IsJumping) return;
            float speed = body.Velocity.y + acceleration * Time.fixedDeltaTime;
            if(speed < terminalSpeed) speed = terminalSpeed;
            body.SetMoveVelocityY(speed);
            SetProbe();
        }
        private void SetProbe()
        {
            if(!toggleFallthrough) return;
            #if UNITY_EDITOR
                if(drawProbeArea)
                {
                    drawProbe.gameObject.SetActive(true);
                    drawProbe.transform.position = transform.position;
                    drawProbe.size = new(probeWidth, probeHeight);
                }
            #endif
            int overlapCount = Physics2D.OverlapBoxNonAlloc(
                transform.position, 
                new(probeWidth, probeHeight), 
                0, 
                probeBuffer);
            for(int i = 0; i < overlapCount; ++i)
            {
                Collider2D col = probeBuffer[i];
                if(col.attachedRigidbody 
                && col.attachedRigidbody.TryGetComponent<PlatformEffector2D>(out _))
                {
                    foreach(var attachedCollider in attachedColliders)
                        Physics2D.IgnoreCollision(attachedCollider, col, true);
                    ignoredColliders.Add(col);
                }
            }
        }
        private void ResetProbe()
        {
            if(!toggleFallthrough) return;
            foreach (var attachedCollider in attachedColliders)
                foreach (var ignoredCollider in ignoredColliders)
                    Physics2D.IgnoreCollision(attachedCollider, ignoredCollider, false);
            ignoredColliders.Clear();
#if UNITY_EDITOR
            drawProbe.gameObject.SetActive(false);
#endif
        }
        #if UNITY_EDITOR
        private class DrawProbe : MonoBehaviour
        {
            public Vector2 size;
            public static DrawProbe Create(Vector2 size)
            {
                var GO = new GameObject($"[DEBUG/Fastfaller] Draw Probe");
                // GO.hideFlags |= HideFlags.HideInHierarchy;
                var drawProbe = GO.AddComponent<DrawProbe>();
                drawProbe.size = size;
                return drawProbe;
            }
            void OnDrawGizmos() 
            {
                Gizmos.color = Color.cyan * new Color(1, 1, 1, 0.5f);
                Gizmos.DrawCube(transform.position, size);
            }
        }
        #endif
    }
}
