using UnityEngine;

public class OrbitingSword : MonoBehaviour
{
    [Tooltip("Transform of the player to orbit around.")]
    public Transform player;

    [Tooltip("Distance from the player.")]
    public float radius = 1.5f;

    [Tooltip("Rotation speed in degrees per second.")]
    public float angularSpeed = 180f;

    [Tooltip("Axis to orbit around (use Z for 2D).")]
    public Vector3 axis = Vector3.forward;

    private float _angle;

    private void Start()
    {
        if (player == null)
        {
            return;
        }

        // Initialize the angle based on the current offset, if any.
        Vector3 offset = transform.position - player.position;
        if (offset.sqrMagnitude > Mathf.Epsilon)
        {
            _angle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
        }
    }

    private void Update()
    {
        if (player == null)
        {
            return;
        }

        if (axis == Vector3.zero)
        {
            axis = Vector3.forward;
        }

        _angle += angularSpeed * Time.deltaTime;
        float rad = _angle * Mathf.Deg2Rad;

        Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * radius;
        transform.position = player.position + offset;

        // Face along the motion direction for a natural swing.
        Vector3 tangent = new Vector3(-Mathf.Sin(rad), Mathf.Cos(rad), 0f);
        transform.up = tangent;
        transform.forward = axis.normalized;
    }

    private void OnDrawGizmosSelected()
    {
        if (player == null)
        {
            return;
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(player.position, radius);
    }
}

