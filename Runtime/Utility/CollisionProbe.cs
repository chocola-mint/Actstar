using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace CHM.Actstar.Utility
{
    [System.Serializable]
    public class CollisionProbeSettings
    {
        [Tooltip("The minimum normal angle on the XY plane to be registered as a collision.")]
        public float minNormalAngle;
        [Tooltip("The maximum normal angle on the XY plane to be registered as a collision.")]
        public float maxNormalAngle;
    }
    internal class CollisionProbe : MonoBehaviour
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
        internal Vector2 Normal {
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
        internal static CollisionProbe Create(
            GameObject attachTo, 
            CollisionProbeSettings probeSettings, 
            Vector2 direction, 
            Rigidbody2D rb, 
            float minProbeDistance = 0.1f)
        {
            CollisionProbe probe = attachTo.AddComponent<CollisionProbe>();
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
        internal void DoFixedUpdate() 
        {
            float nextStepLength = Vector2.Dot(rb.velocity * Time.fixedDeltaTime, direction);
            float probeDistance = Mathf.Max(minProbeDistance, nextStepLength);
            numHits = col.Cast(direction, filter, hits, probeDistance);
            hits = hits.Where(x => !Physics2D.GetIgnoreCollision(x.collider, col)).ToList();
            numHits = hits.Count;
            normalCache = null;
        }
    }
    internal static class Extensions
    {
        // Note: These extension methods are so that the inspector doesn't throw
        // while in edit mode.
        public static bool GetHasHits(this CollisionProbe probe)
        {
            #if UNITY_EDITOR
            if(!Application.isPlaying || probe == null) return false;
            #endif
            return probe.HasHits;
        }
        public static Vector2 GetNormal(this CollisionProbe probe)
        {
            #if UNITY_EDITOR
            if(!Application.isPlaying || probe == null) return Vector2.zero;
            #endif
            return probe.Normal;
        }
    }
}