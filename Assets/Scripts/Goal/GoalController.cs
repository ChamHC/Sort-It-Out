using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalController : MonoBehaviour
{
    [SerializeField] public GameObject Block;
    [NonSerialized] public bool IsFulfilled = false;
    [NonSerialized] private string blockTag;
    [SerializeField] public AudioClip goalSound;
    public AudioSource audioSource;
    [NonSerialized] private bool hasPlayedSound = false;

    void Start()
    {
        blockTag = Block.tag;
        audioSource.clip = goalSound;
    }

    void Update()
    {

    }

    void OnTriggerStay(Collider other)
    {
        if (other.tag == blockTag)
        {
            if (!hasPlayedSound)
            {
                audioSource.Play();
                hasPlayedSound = true;
            }
            IsFulfilled = true;
            Debug.Log("IN");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == blockTag)
        {
            IsFulfilled = false;
            hasPlayedSound = false; // Reset the flag when the object leaves the trigger
        }
    }
}

