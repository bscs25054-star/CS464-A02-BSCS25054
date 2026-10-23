using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float speed = 5f;          // metres per second
    [SerializeField] float jumpHeight = 1.8f;   // metres

    [Header("Fair jump")]
    [SerializeField] float coyoteTime = 0.1f;   // seconds after leaving a ledge
    [SerializeField] float jumpBuffer = 0.12f;  // seconds before landing
    [SerializeField] float fallMultiplier = 2.2f;

    [Header("Ground check (CheckSphere)")]
    [SerializeField] Transform feet;            // empty child at local (0, -0.95, 0)
    [SerializeField] float checkRadius = 0.2f;
    [SerializeField] LayerMask groundMask;      // ONLY the Ground layer

    [Header("Juice: squash on landing")]
    [SerializeField] Transform body;            // the child with the mesh
    [SerializeField] float squashDuration = 0.25f;
    [SerializeField] float squashY = 0.7f;
    [SerializeField] float minAirTime = 0.1f;   // seconds in the air before a landing counts

    Rigidbody rb;
    Transform cam;
    Vector3 move;                               // built in Update
    bool isGrounded, jumpQueued;
    float lastGroundedTime = -1f, lastPressedTime = -1f;
    float airTime;
    Coroutine squashRoutine;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        cam = Camera.main.transform;
    }

    // ---------- Update: read keys, build move, queue the jump, start juice ----------
    void Update()
    {
        // Camera-relative movement: flatten and normalize the camera's axes
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 forward = cam.forward;
        forward.y = 0f;
        forward.Normalize();
        Vector3 right = cam.right;
        right.y = 0f;
        right.Normalize();

        move = Vector3.ClampMagnitude(forward * v + right * h, 1f); // diagonals clamped

        // Ground check
        isGrounded = Physics.CheckSphere(feet.position, checkRadius, groundMask);

        // Stamp the ground only when no jump is queued and we are not rising
        if (isGrounded && !jumpQueued && rb.linearVelocity.y <= 0f)
            lastGroundedTime = Time.time;

        if (Input.GetKeyDown(KeyCode.Space))
            lastPressedTime = Time.time;

        bool canJump = Time.time - lastGroundedTime <= coyoteTime;
        bool wantJump = Time.time - lastPressedTime <= jumpBuffer;

        if (canJump && wantJump && !jumpQueued)
        {
            jumpQueued = true;
            lastGroundedTime = -1f; // the jump uses up both windows
            lastPressedTime = -1f;
        }

        // Juice: once per real landing
        if (isGrounded)
        {
            if (airTime > minAirTime)
            {
                if (squashRoutine != null) StopCoroutine(squashRoutine);
                squashRoutine = StartCoroutine(Squash());
            }
            airTime = 0f;
        }
        else
        {
            airTime += Time.deltaTime;
        }
    }

    // ---------- FixedUpdate: move, apply the queued jump once, fall gravity ----------
    void FixedUpdate()
    {
        Vector3 vel = rb.linearVelocity;
        rb.linearVelocity = new Vector3(move.x * speed, vel.y, move.z * speed);

        if (jumpQueued)
        {
            jumpQueued = false;

            float g = -Physics.gravity.y;                     // 9.81
            float v0 = Mathf.Sqrt(2f * g * jumpHeight);       // launch speed

            // zero vertical velocity first, or a jump taken while falling comes out short
            Vector3 flat = rb.linearVelocity;
            rb.linearVelocity = new Vector3(flat.x, 0f, flat.z);

            rb.AddForce(Vector3.up * rb.mass * v0, ForceMode.Impulse);
        }

        // Extra gravity only while falling
        if (rb.linearVelocity.y < 0f)
            rb.AddForce(Physics.gravity * (fallMultiplier - 1f), ForceMode.Acceleration);
    }

    // ---------- Juice: squash Body, eased with OutBack, volume kept ----------
    IEnumerator Squash()
    {
        float y = squashY;
        float xz = 1f / Mathf.Sqrt(y); // keeps the volume: y=0.7 -> x,z ~ 1.2
        Vector3 squashed = new Vector3(xz, y, xz);

        for (float elapsed = 0f; elapsed < squashDuration; elapsed += Time.deltaTime)
        {
            float p = Ease.OutBack(elapsed / squashDuration);
            body.localScale = Vector3.LerpUnclamped(squashed, Vector3.one, p);
            yield return null;
        }

        body.localScale = Vector3.one; // exactly at rest
        squashRoutine = null;
    }

    // ---------- Gizmo for the ground check ----------
    void OnDrawGizmosSelected()
    {
        if (feet == null) return;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(feet.position, checkRadius);
    }
}