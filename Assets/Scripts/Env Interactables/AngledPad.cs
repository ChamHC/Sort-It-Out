using System.Collections;
using UnityEngine;

public class AngledPad : MonoBehaviour
{
    public float forceMultiplier = 10f;
    public float cooldownTime = 3f;
    private bool canJump = true;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && canJump)
        {
            Rigidbody playerRigidbody = collision.gameObject.GetComponent<Rigidbody>();
            if (playerRigidbody != null)
            {

                // Calculate jump direction and force
                Vector3 jumpDirection = transform.up;
                float jumpForce = forceMultiplier * Vector3.Dot(jumpDirection, playerRigidbody.velocity.normalized);

                // Apply force to player
                playerRigidbody.AddForce(jumpDirection * forceMultiplier, ForceMode.Impulse);

                canJump = false;
                StartCoroutine(ResetJump());
            }
        }
    }

    private IEnumerator ResetJump()
    {
        yield return new WaitForSeconds(cooldownTime);
        canJump = true;
    }
}
