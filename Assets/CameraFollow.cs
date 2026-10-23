using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform target;                              // leave empty: found in Awake
    [SerializeField] Vector3 offset = new Vector3(0f, 6f, -8f);
    [SerializeField] float smoothTime = 0.3f;                       // SmoothDamp, in seconds
    [SerializeField] float lookAhead = 0.35f;                       // seconds of velocity added to the aim point
    [SerializeField] float lookHeight = 1f;                         // aim a little above the feet

    Rigidbody targetBody;
    Vector3 velocity;

    void Awake()
    {
        if (target == null)
        {
            PlayerController player = FindAnyObjectByType<PlayerController>();
            if (player != null) target = player.transform;
        }
        if (target != null) targetBody = target.GetComponent<Rigidbody>();
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Look-ahead: where the player will be, using flat velocity
        Vector3 flatVelocity = Vector3.zero;
        if (targetBody != null)
        {
            flatVelocity = targetBody.linearVelocity;
            flatVelocity.y = 0f;
        }
        Vector3 aim = target.position + flatVelocity * lookAhead;

        // Same curve at any frame rate (SmoothDamp)
        Vector3 wanted = aim + offset;
        transform.position = Vector3.SmoothDamp(transform.position, wanted, ref velocity, smoothTime);

        // Look at the aim point, not at the player, or the look-ahead is undone
        transform.LookAt(aim + Vector3.up * lookHeight);
    }
}
