using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TriInspector;

/// <summary>
/// An orthographic camera controller that supports smooth follow and confinement.
/// Exists so that the samples don't have a dependency to Cinemachine.
/// </summary>
[RequireComponent(typeof(Camera))]
public class BasicCameraController : MonoBehaviour
{
    [Required]
    public Transform followTarget;
    public Vector2 followOffset = Vector2.zero;
    public float smoothTime = 0.3f;
    public Rect confineBounds = new Rect(0, 0, 1, 1);
    private Camera cameraComponent;
    [ReadOnly, ShowInInspector, HideInEditMode]
    private Vector2 velocity = Vector2.zero;
    void Awake() 
    {
        TryGetComponent<Camera>(out cameraComponent);
        Debug.Assert(cameraComponent.orthographic);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 currentPosition = transform.position;
        // We save the z here because we won't touch z at all here.
        float z = currentPosition.z;
        currentPosition.z = 0;
        // Compute and apply damping.
        Vector2 dampedPosition = Vector2.SmoothDamp(
            currentPosition, 
            followTarget.position + (Vector3) followOffset, 
            ref velocity, 
            smoothTime);
        currentPosition = dampedPosition;
        // Compute and apply confine bounds.
        Bounds computedBounds = new Bounds(confineBounds.position, confineBounds.size);
        float camHeight = cameraComponent.orthographicSize * 2;
        float camWidth = camHeight * cameraComponent.aspect;
        computedBounds.Expand(-new Vector3(camWidth, camHeight));
        currentPosition = computedBounds.ClosestPoint(currentPosition);
        // Write final value back.
        currentPosition.z = z;
        transform.position = currentPosition;
    }
    void OnDrawGizmos() 
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(confineBounds.position, confineBounds.size);
    }
}
