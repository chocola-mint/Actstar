using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CHM.Actstar.Samples.SmashBrosLike
{
    public class Player : MonoBehaviour
    {
        public Rect boundary;
        private Vector3 spawnPosition;
        // Start is called before the first frame update
        void Start()
        {
            spawnPosition = transform.position;
        }

        void FixedUpdate()
        {
            if(!boundary.Contains(
                transform.position 
                + new Vector3(boundary.width, boundary.height, 0) / 2))
                transform.position = spawnPosition;
        }
        void OnDrawGizmosSelected() 
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(boundary.position, new(boundary.width, boundary.height, 0));
        }
    }
}