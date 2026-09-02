using System;
using System.Collections;
using UnityEngine;

public class ShrimpPunch : MonoBehaviour
{
    [SerializeField] private float knockbackForce;
    [SerializeField] private Vector3 direction;
    [SerializeField] private ParticleSystem punchParticle;
    [SerializeField] private float punchTime;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody2D bodyRb= other.transform.GetComponent<Rigidbody2D>();
            StartCoroutine(PlayConfusedParticle()); 
           bodyRb.AddForce(new Vector2(direction.x, direction.y) * knockbackForce,  ForceMode2D.Impulse);
        }
    }
    
    private IEnumerator PlayConfusedParticle()
    {
        if (punchParticle == null)
            yield return null;
        if (punchParticle.isPlaying)
            yield return null;

        punchParticle.gameObject.SetActive(true);
        yield return new WaitForSeconds(punchTime);
        punchParticle.gameObject.SetActive(false);
    }
}
