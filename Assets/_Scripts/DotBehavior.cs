using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DotBehavior : MonoBehaviour
{
    private GameObject floor; // Made private
    private GameObject player; // Added reference to the Player GameObject
    private Collider floorCollider;
    private Bounds floorBounds;
    private Collider sphereCollider;

    // Start is called before the first frame update
    void Start()
    {
        // Find the floor object by tag "GroundPlane"
        // Ensure only one object in the scene has this tag
        floor = GameObject.FindGameObjectWithTag("GroundPlane");
        if (floor == null)
        {
            Debug.LogError("No GameObject with tag 'GroundPlane' found in the scene.");
            return;
        }

        floorCollider = floor.GetComponent<Collider>();
        if (floorCollider == null)
        {
            Debug.LogError("The GameObject with tag 'GroundPlane' does not have a Collider component.");
            return;
        }

        floorBounds = floorCollider.bounds;
        sphereCollider = gameObject.GetComponent<Collider>();

        // Find the player object by tag "Player"
        // Ensure only one object in the scene has this tag
        player = GameObject.Find("CenterEyeAnchor"); // Changed to find the center eye anchor directly
        if (player == null)
        {
            Debug.LogError("No GameObject called CenterEyeAnchor found in the scene.");
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            MoveSphere();
        }
    }

    public void MoveSphere()
    {
        Vector3 playerPosition = player.transform.position;
        Vector3 targetPosition;
        int maxAttempts = 3; // Limit the number of attempts to avoid infinite loops
        int attempts = 0;

        // Store the current y position of the sphere
        float currentY = gameObject.transform.position.y;

        // Try to find a random valid position
        do
        {
            Vector3 randomDirection = Random.onUnitSphere; // Random direction
            randomDirection.y = 0; // Keep movement on the XZ plane
            randomDirection.Normalize();

            float randomDistance = Random.Range(6f, 12f); // Random distance between 6 and 12 units
            targetPosition = playerPosition + randomDirection * randomDistance;

            // Clamp the target position within the floor bounds
            targetPosition.x = Mathf.Clamp(targetPosition.x, floorBounds.min.x, floorBounds.max.x);
            targetPosition.z = Mathf.Clamp(targetPosition.z, floorBounds.min.z, floorBounds.max.z);

            // Retain the current y position
            targetPosition.y = currentY;

            attempts++;
        } while (!IsPositionValid(targetPosition) && attempts < maxAttempts);

        // If random position fails, move toward the center of the floor with noise
        if (attempts >= maxAttempts)
        {
            Debug.LogWarning("Random position failed, selecting position toward the center of the floor.");

            Vector3 floorCenter = floorBounds.center;
            Vector3 directionToCenter = (floorCenter - playerPosition).normalized;

            // Add random angular variation of ±15 degrees
            float angleVariation = Random.Range(-15f, 15f);
            Quaternion rotation = Quaternion.Euler(0, angleVariation, 0);
            directionToCenter = rotation * directionToCenter;

            float fallbackDistance = 8f; // Fixed fallback distance
            targetPosition = playerPosition + directionToCenter * fallbackDistance;

            // Clamp the fallback position within the floor bounds
            targetPosition.x = Mathf.Clamp(targetPosition.x, floorBounds.min.x, floorBounds.max.x);
            targetPosition.z = Mathf.Clamp(targetPosition.z, floorBounds.min.z, floorBounds.max.z);

            // Retain the current y position
            targetPosition.y = currentY;
        }

        Debug.Log(targetPosition);
        gameObject.transform.position = targetPosition;
    }

    private bool IsPositionValid(Vector3 position)
    {
        // Check if the position is within the floor bounds
        return position.x >= floorBounds.min.x && position.x <= floorBounds.max.x &&
               position.z >= floorBounds.min.z && position.z <= floorBounds.max.z;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MainCamera"))
        {
            MoveSphere();
            Debug.Log($"Sphere moved to a new position: {gameObject.transform.position}");
        }
    }
}