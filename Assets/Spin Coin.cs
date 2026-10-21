using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Spin & Float")]
    [SerializeField] private float rotationSpeed = 100f;   // degrees per second
    [SerializeField] private float floatAmplitude = 0.25f; // how high it bobs
    [SerializeField] private float floatSpeed = 2f;        // how fast it bobs

    [Header("Pickup")]
    [SerializeField] private int value = 1;
    [SerializeField] private AudioClip pickupSound;        // optional
    [SerializeField] private GameObject pickupEffect;      // optional particle prefab

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        // Spin around the world Y axis
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

        // Bob up and down
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Collect();
    }

    private void Collect()
    {
        // TODO: hook into your score system here, e.g.
        // GameManager.Instance.AddCoins(value);
        Debug.Log("Coin collected! +" + value);

        if (pickupSound != null)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);

        if (pickupEffect != null)
            Instantiate(pickupEffect, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}