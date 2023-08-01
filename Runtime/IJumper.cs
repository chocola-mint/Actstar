using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TriInspector;

namespace CHM.Actstar
{
    public interface IJumper
    {
        public bool CanJump { get; }
        public bool IsJumping { get; }

        public void CancelJump();
        public void Jump();
    }
}
