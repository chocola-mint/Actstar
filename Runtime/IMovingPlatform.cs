using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CHM.Actstar
{
    public interface IMovingPlatform
    {
        public Vector2 Velocity { get; }
    }
}
