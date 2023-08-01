using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TriInspector;

namespace CHM.Actstar
{
    /// <summary>
    /// Interface for components that implement some kind of jumping behavior.
    /// </summary>
    public interface IJumper
    {
        /// <summary>
        /// Can the object jump at the moment?
        /// </summary>
        public bool CanJump { get; }
        /// <summary>
        /// Is the object currently jumping?
        /// </summary>
        public bool IsJumping { get; }

        /// <summary>
        /// Makes the object start jumping upwards.
        /// </summary>
        public void Jump();
        /// <summary>
        /// Aborts the ongoing jump. This is usually used to indicate short-hops.
        /// </summary>
        public void CancelJump();
    }
}
