using System.Collections;
using UnityEngine;

public class JumpPad : MonoBehaviour
{
    public float launchForce = 10f;
    public float launchDelay = 3f;

    private bool canLaunch = true;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && canLaunch)
        {
            Rigidbody playerRigidbody = collision.gameObject.GetComponent<Rigidbody>();
            if (playerRigidbody != null)
            {
                playerRigidbody.AddForce(Vector3.up * launchForce, ForceMode.Impulse);
                canLaunch = false;
                StartCoroutine(ResetLaunch());
                Debug.Log("JumpPad launched player");
            }
        }
    }

    private IEnumerator ResetLaunch()
    {
        yield return new WaitForSeconds(launchDelay);
        canLaunch = true;
    }
}
