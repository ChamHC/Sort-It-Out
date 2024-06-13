using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Transform waypoint1;
    public Transform waypoint2;
    public float speed = 2f;

    private Transform targetWaypoint;
    private bool movingToWaypoint1 = true;

    void Start()
    {
        // Set initial target to waypoint1
        targetWaypoint = waypoint1;
    }

    void Update()
    {
        // Move the platform towards the target waypoint
        transform.parent.position = Vector3.MoveTowards(transform.parent.position, targetWaypoint.position, speed * Time.deltaTime);

        // Check if the platform has reached the target waypoint
        if (Vector3.Distance(transform.parent.position, targetWaypoint.position) < 0.1f)
        {
            // Toggle the target waypoint
            movingToWaypoint1 = !movingToWaypoint1;
            targetWaypoint = movingToWaypoint1 ? waypoint1 : waypoint2;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null);
        }
    }
}
