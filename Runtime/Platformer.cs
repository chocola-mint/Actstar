using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TriInspector;

namespace CHM.Actstar
{
    /// <summary>
    /// Component that handles interactions with PlatformEffectors (one-way platforms).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class Platformer : MonoBehaviour
    {
        [Min(0), Tooltip("The width of the probe box. The wider the probe, the less likely a platform will be missed.")]
        public float probeWidth = 8.0f;
        [Min(0), Tooltip("The height of the probe box. Should be a sufficiently large number.")]
        public float probeHeight = 10000.0f;
        [Min(1), Tooltip("The size of the collider buffer used when fastfall starts. "
        + "Make this larger if you expect a lot of colliders to be covered by the probe.")]
        private int probeBufferSize = 32;
        #if UNITY_EDITOR
        [SerializeField, Tooltip("[EDITOR ONLY] If set to true, "
        + "the probe area will be shown in the scene viewport.")]
        private bool drawProbeArea = false;
        #endif
        private Rigidbody2D rb;
        private Collider2D[] probeBuffer;
        private Collider2D[] attachedColliders;
        [field: ShowInInspector, ShowInPlayMode]
        public bool UseFallthrough { get; set; } = false;
        void Awake() 
        {
            TryGetComponent<Rigidbody2D>(out rb);
            attachedColliders = GetComponentsInChildren<Collider2D>(true);
            probeBuffer = new Collider2D[probeBufferSize];
        }
        void FixedUpdate() 
        {
            int overlapCount = Physics2D.OverlapBoxNonAlloc(
                transform.position, 
                new(probeWidth, probeHeight), 
                0, 
                probeBuffer);
            for(int i = 0; i < overlapCount; ++i)
            {
                Collider2D col = probeBuffer[i];
                if(col.attachedRigidbody 
                && col.TryGetComponent<PlatformEffector2D>(out _))
                {
                    // This avoids falling early through a one way platform's edge.
                    // It enforces the requirement that once on a platform the player
                    // needs to be no longer touching to ignore it.
                    if(rb.IsTouching(col))
                    {
                        if(!UseFallthrough) 
                            continue;
                    }
                    bool shouldIgnore = ComputeShouldIgnore(col);
                    foreach(var attachedCollider in attachedColliders)
                        Physics2D.IgnoreCollision(attachedCollider, col, shouldIgnore);
                    // if(shouldIgnore) Debug.Log($"Ignore {col.attachedRigidbody}");
                    // else Debug.Log($"No ignore {col.attachedRigidbody}");
                }
            }
        }
        private bool ComputeShouldIgnore(Collider2D collider)
        {
            if(UseFallthrough) return true;
            Vector2 vecToBody = (Vector2)transform.position - collider.ClosestPoint(transform.position);
            bool isBehindPlatform = Vector2.Dot(
                vecToBody.normalized,
                (Vector2)collider.transform.up) < 0f;
            return isBehindPlatform;
        }
        #if UNITY_EDITOR
        void OnDrawGizmos() 
        {
            if(!drawProbeArea) return;
            Gizmos.color = Color.cyan * new Color(1, 1, 1, 0.5f);
            Gizmos.DrawCube(transform.position, new(probeWidth, probeHeight));
        }
        #endif
    }
}
