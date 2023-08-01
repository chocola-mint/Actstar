using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TriInspector;

namespace CHM.Actstar
{
    /// <summary>
    /// Tracks the collision state of a Rigidbody2D component,
    /// such as if it's touching a wall.
    /// </summary>
    [DeclareBoxGroup("Runtime", Title = "Runtime Info")]
    [DeclareBoxGroup("Filters", Title = "Contact Filters")]
    [DefaultExecutionOrder(-1000)]
    [RequireComponent(typeof(Rigidbody2D))]
    public class CollisionState : MonoBehaviour
    {
        
        [GroupNext("Filters")]
        public ProbeSettings bottom = new(){
            minNormalAngle = 15,
            maxNormalAngle = 165,
        };
        public ProbeSettings left = new(){
            minNormalAngle = 0 - 15,
            maxNormalAngle = 0 + 15,
        };
        public ProbeSettings right = new(){
            minNormalAngle = 180 - 15,
            maxNormalAngle = 180 + 15,
        };
        public ProbeSettings top = new(){
            minNormalAngle = -165,
            maxNormalAngle = -15,
        };
        [Button("Mirror Filters (Left -> Right)")]
        public void MirrorFiltersLR()
        {
            right.minNormalAngle = left.minNormalAngle + 180;
            right.maxNormalAngle = left.maxNormalAngle + 180;
        }
        [Button("Mirror Filters (Ground -> Ceiling)")]
        public void MirrorFiltersGC()
        {
            top.minNormalAngle = bottom.minNormalAngle - 180;
            top.maxNormalAngle = bottom.maxNormalAngle - 180;
        }
        [GroupNext("Runtime")]
        [HideInEditMode, ShowInInspector]
        public bool IsTouchingBottom => bottomProbe.GetHasHits();
        // * idea: keep track if last frame was grounded.
        // *       then, if was not grounded, also require velocity to be approx zero.
        [HideInEditMode, ShowInInspector]
        public bool IsTouchingTop => topProbe.GetHasHits();
        [HideInEditMode, ShowInInspector]
        public bool IsTouchingLeft => leftProbe.GetHasHits();
        [HideInEditMode, ShowInInspector]
        public bool IsTouchingRight => rightProbe.GetHasHits();
        public bool IsTouchingSide => IsTouchingLeft || IsTouchingRight;
        [HideInEditMode, ShowInInspector]
        public Vector2 BottomNormal => bottomProbe.GetNormal();
        [HideInEditMode, ShowInInspector]
        public Vector2 SideNormal => (leftProbe.GetNormal() + rightProbe.GetNormal()).normalized;
        [HideInEditMode, ShowInInspector]
        public Vector2 TopNormal => topProbe.GetNormal();
        private Rigidbody2D rb;
        private Probe bottomProbe, leftProbe, rightProbe, topProbe;
        #region Unity Events
        void Awake() 
        {
            TryGetComponent<Rigidbody2D>(out rb);
        }
        void Start() 
        {
            bottomProbe = Probe.Create(gameObject, bottom, Vector2.down, rb);
            leftProbe = Probe.Create(gameObject, left, Vector2.left, rb);
            rightProbe = Probe.Create(gameObject, right, Vector2.right, rb);
            topProbe = Probe.Create(gameObject, top, Vector2.up, rb);
        }
        void FixedUpdate() 
        {
            bottomProbe.DoFixedUpdate();
            leftProbe.DoFixedUpdate();
            rightProbe.DoFixedUpdate();
            topProbe.DoFixedUpdate();
        }
        #endregion
        [System.Serializable]
        public class ProbeSettings
        {
            public float minNormalAngle, maxNormalAngle;
        }
        internal class Probe : MonoBehaviour
        {
            private ContactFilter2D filter;
            private Vector2 direction;
            private Rigidbody2D rb;
            private Collider2D col;
            private float minProbeDistance = 0.1f;
            private int numHits;
            public bool HasHits => numHits > 0;
            private List<RaycastHit2D> hits = new();
            private Vector2? normalCache = null;
            public Vector2 Normal {
                get {
                    if(!normalCache.HasValue)  
                    {
                        Vector2 result = Vector2.zero;
                        for(int i = 0; i < numHits; ++i)
                            result += hits[i].normal;
                        normalCache = result.normalized;
                    }
                    return normalCache.Value;
                }
            }
            public static Probe Create(
                GameObject attachTo, 
                ProbeSettings probeSettings, 
                Vector2 direction, 
                Rigidbody2D rb, 
                float minProbeDistance = 0.1f)
            {
                Probe probe = attachTo.AddComponent<Probe>();
                probe.filter = new(){
                    useNormalAngle = true,
                    minNormalAngle = probeSettings.minNormalAngle,
                    maxNormalAngle = probeSettings.maxNormalAngle,
                };
                probe.direction = direction;
                probe.minProbeDistance = minProbeDistance;
                probe.hideFlags |= HideFlags.HideInInspector;
                probe.rb = rb;
                // Get first attached collider.
                Collider2D[] temp = new Collider2D[1];
                rb.GetAttachedColliders(temp);
                probe.col = temp[0];
                return probe;
            }
            public void DoFixedUpdate() 
            {
                float nextStepLength = Vector2.Dot(rb.velocity * Time.fixedDeltaTime, direction);
                float probeDistance = Mathf.Max(minProbeDistance, nextStepLength);
                numHits = col.Cast(direction, filter, hits, probeDistance);
                hits = hits.Where(x => !Physics2D.GetIgnoreCollision(x.collider, col)).ToList();
                numHits = hits.Count;
                normalCache = null;
            }
        }
    }
    internal static class Extensions
    {
        public static bool GetHasHits(this CollisionState.Probe probe)
        {
            #if UNITY_EDITOR
            if(!Application.isPlaying) return false;
            #endif
            return probe.HasHits;
        }
        public static Vector2 GetNormal(this CollisionState.Probe probe)
        {
            #if UNITY_EDITOR
            if(!Application.isPlaying) return Vector2.zero;
            #endif
            return probe.Normal;
        }
    }
}
