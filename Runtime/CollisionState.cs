using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TriInspector;
using CHM.Actstar.Utility;
using ProbeSettings = CHM.Actstar.Utility.CollisionProbeSettings;
using Probe = CHM.Actstar.Utility.CollisionProbe;

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
        [SerializeField]
        private ProbeSettings bottom = new(){
            minNormalAngle = 15,
            maxNormalAngle = 165,
        },
        left = new(){
            minNormalAngle = 0 - 15,
            maxNormalAngle = 0 + 15,
        },
        right = new(){
            minNormalAngle = 180 - 15,
            maxNormalAngle = 180 + 15,
        },
        top = new(){
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
    }
}
