using System;
using UnityEngine;

public class CheckGround : MonoBehaviour
{
    [NonSerialized] public bool IsGrounded = false;
    private void OnTriggerStay(Collider other) {
        IsGrounded = true;
    }
    private void OnTriggerExit(Collider other) {
        IsGrounded = false;
    }
}
