using System;
using UnityEngine;

public class ShrimpPunch : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponentInChildren<Rigidbody2D>().AddForce(Vector3.back, ForceMode2D.Impulse);
        }
    }
}
