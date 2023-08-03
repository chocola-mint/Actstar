using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TriInspector;
using CHM.Actstar;

[RequireComponent(typeof(Rigidbody2D))]
public class PlatformPathCircle : MonoBehaviour, IMovingPlatform
{
    [Min(0)]
    public float radius = 3.0f;
    [ValidateInput(nameof(ValidatePeriod))]
    public float period = 2.0f;
    private TriValidationResult ValidatePeriod()
    {
        if(Mathf.Approximately(period, 0)) return TriValidationResult.Warning("A period close to zero will stop the platform from moving!");
        if(period > 0) return TriValidationResult.Info("The platform will move counter-clockwise.");
        else return TriValidationResult.Info("The platform will move clockwise.");
    }
    [Range(0, 1)]
    public float phase = 0;
    private Rigidbody2D rb;
    private Vector3 source;
    [ShowInInspector, ShowInPlayMode]
    public Vector2 Velocity {
        get {
            float theta = Theta;
            return new Vector2(-Mathf.Sin(theta), Mathf.Cos(theta)) * radius / period;
        }
    }
    private Vector2 Position {
        get {
            float theta = Theta;
            return new Vector2(Mathf.Cos(theta), Mathf.Sin(theta)) * radius;
        }
    }
    private float Theta => Mathf.Lerp(0, 2 * Mathf.PI, Mathf.Repeat(phase + Time.fixedTime / period, 1));
    void Awake() 
    {
        TryGetComponent<Rigidbody2D>(out rb);
    }
    // Start is called before the first frame update
    void Start()
    {
        source = transform.position;
    }
    void FixedUpdate() 
    {
        if(Mathf.Approximately(period, 0)) return; // Avoid divide by zero.
        float theta = Theta;
        rb.MovePosition((Vector2) source + Position);
    }
    void OnDrawGizmos() 
    {
        Vector3 center = Application.isPlaying ? source : transform.position;
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(center, radius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(
            center, 
            center 
            + Quaternion.Euler(0, 0, Mathf.Lerp(0, 360, phase)) * Vector3.right * radius);
    }
}
