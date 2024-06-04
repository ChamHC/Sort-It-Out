using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalController : MonoBehaviour
{
    [SerializeField] public GameObject Block;
    [NonSerialized] public bool IsFulfilled = false;
    [NonSerialized] private string blockTag;
    void Start()
    {
        blockTag = Block.tag;
    }

    void Update()
    {

    }

    void OnTriggerStay(Collider other) {
        if (other.tag == blockTag) {
            IsFulfilled = true;
        }
    }

    void OnTriggerExit(Collider other) {
        if (other.tag == blockTag) {
            IsFulfilled = false;
        }
    }
}
