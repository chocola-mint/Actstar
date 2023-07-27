using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TriInspector;

namespace CHM.Actstar
{
    /// <summary>
    /// Tracks the collision state of a Rigidbody2D component,
    /// such as whether it's grounded or not. Very useful in platformers.
    /// <br>
    /// The implementation does not use raycasts, making it work with any
    /// collision shape, though a convex shape like a capsule or a box is
    /// recommended.
    /// </br>
    /// </summary>
    [DeclareBoxGroup("Runtime", Title = "Runtime Info")]
    [DeclareBoxGroup("Filters", Title = "Contact Filters")]
    [DefaultExecutionOrder(-1000)]
    [RequireComponent(typeof(Rigidbody2D))]
    public class CollisionState : MonoBehaviour
    {
        
        [GroupNext("Filters")]
        public ContactFilter2D ground = new(){
            useNormalAngle = true,
            minNormalAngle = 15,
            maxNormalAngle = 165,
        };
        public ContactFilter2D left = new(){
            useNormalAngle = true,
            minNormalAngle = 0 - 15,
            maxNormalAngle = 0 + 15,
        };
        public ContactFilter2D right = new(){
            useNormalAngle = true,
            minNormalAngle = 180 - 15,
            maxNormalAngle = 180 + 15,
        };
        public ContactFilter2D ceiling = new(){
            useNormalAngle = true,
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
            ceiling.minNormalAngle = ground.minNormalAngle - 180;
            ceiling.maxNormalAngle = ground.maxNormalAngle - 180;
        }
        [GroupNext("Runtime")]
        [ShowInPlayMode, ShowInInspector]
        public bool IsGrounded => numGroundContacts > 0;
        [ShowInPlayMode, ShowInInspector]
        public bool IsTouchingCeiling => numCeilingContacts > 0;
        [ShowInPlayMode, ShowInInspector]
        public bool IsTouchingLeft => numLeftContacts > 0;
        [ShowInPlayMode, ShowInInspector]
        public bool IsTouchingRight => numRightContacts > 0;
        public bool IsTouchingSide => IsTouchingLeft || IsTouchingRight;
        [ShowInPlayMode, ShowInInspector]
        public Vector2 GroundNormal => ComputeNormal(groundBuffer, numGroundContacts).normalized;
        [ShowInPlayMode, ShowInInspector]
        public Vector2 SideNormal => (ComputeNormal(leftBuffer, numLeftContacts) 
        + ComputeNormal(rightBuffer, numRightContacts)).normalized;
        [ShowInPlayMode, ShowInInspector]
        public Vector2 CeilingNormal => ComputeNormal(ceilingBuffer, numCeilingContacts).normalized;
        public IEnumerable<ContactPoint2D> GetGroundContacts() 
        => EnumerateContacts(groundBuffer, numGroundContacts);
        public IEnumerable<ContactPoint2D> GetLeftContacts() 
        => EnumerateContacts(leftBuffer, numLeftContacts);
        public IEnumerable<ContactPoint2D> GetRightContacts() 
        => EnumerateContacts(rightBuffer, numRightContacts);
        public IEnumerable<ContactPoint2D> GetCeilingContacts() 
        => EnumerateContacts(ceilingBuffer, numCeilingContacts);
        private Rigidbody2D rb;
        private int numGroundContacts, numLeftContacts, numRightContacts, numCeilingContacts;
        private List<ContactPoint2D> groundBuffer = new();
        private List<ContactPoint2D> leftBuffer = new();
        private List<ContactPoint2D> rightBuffer = new();
        private List<ContactPoint2D> ceilingBuffer = new();
        private IEnumerable<ContactPoint2D> EnumerateContacts(List<ContactPoint2D> contacts, int numContacts)
        {
            for(int i = 0; i < numContacts; ++i)
                yield return contacts[i];
        }
        private Vector2 ComputeNormal(List<ContactPoint2D> contacts, int numContacts)
        {
            Vector2 result = Vector2.zero;
            for(int i = 0; i < numContacts; ++i)
                result += contacts[i].normal * contacts[i].normalImpulse;
            return result;
        }
        #region Unity Events
        void OnValidate() 
        {
            ground.useNormalAngle 
            = left.useNormalAngle 
            = right.useNormalAngle 
            = ceiling.useNormalAngle = true;
        }
        void Awake() 
        {
            TryGetComponent<Rigidbody2D>(out rb);
        }
        void FixedUpdate() 
        {
            numGroundContacts = rb.GetContacts(ground, groundBuffer);
            numLeftContacts = rb.GetContacts(left, leftBuffer);
            numRightContacts = rb.GetContacts(right, rightBuffer);
            numCeilingContacts = rb.GetContacts(ceiling, ceilingBuffer);
        }
        #endregion
    }
}
