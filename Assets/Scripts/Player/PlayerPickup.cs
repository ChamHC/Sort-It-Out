using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPickup : MonoBehaviour
{
    public Transform holdPosition; // Position where the stone will be held
    private GameObject heldObject = null;
    private GameObject nearbyPickup = null; // To track the nearby pickup object
    public float throwForce;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldObject == null && nearbyPickup != null)
            {
                PickupObject(nearbyPickup);
            }
            else if (heldObject != null)
            {
                DropObject();
            }
        }

        if (Input.GetMouseButtonDown(0) && heldObject != null)
        {
            ThrowObject();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Interactable"))
        {
            nearbyPickup = other.gameObject;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Interactable"))
        {
            if (nearbyPickup == other.gameObject)
            {
                nearbyPickup = null;
            }
        }
    }

    void PickupObject(GameObject pickObject)
    {
        Rigidbody rb = pickObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            pickObject.transform.position = holdPosition.position;
            pickObject.transform.rotation = holdPosition.rotation; // Optional: match rotation
            pickObject.transform.parent = holdPosition;
            heldObject = pickObject;
            nearbyPickup = null; // Clear the nearby pickup reference
        }
    }

    void DropObject()
    {
        if (heldObject != null)
        {
            Rigidbody rb = heldObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                //rb.isKinematic = false;
                heldObject.transform.parent = null;
                heldObject = null;
            }
        }
    }

    void ThrowObject()
    {
        if (heldObject != null)
        {
            Rigidbody rb = heldObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                heldObject.transform.parent = null;

                // Apply a force to throw the object
                rb.AddForce(transform.forward * throwForce, ForceMode.VelocityChange);

                heldObject = null;
            }
        }
    }
}
