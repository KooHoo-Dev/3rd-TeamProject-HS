using System;
using System.Collections;
using UnityEngine;
using WeGotOne.Fish_Interface;

public class ShrimpPunch : MonoBehaviour
{
    [SerializeField] private float knockbackForce;
    [SerializeField] private Vector3 direction;
    [SerializeField] private ParticleSystem punchParticle;
    [SerializeField] private float punchTime;
    
    private bool canPunch;
    private FishStateManager manager;

    private void Start()
    {
        manager = GetComponentInParent<FishStateManager>();
    }

    private void Update()
    {
        if (manager.CurrentState is FishFaintState or FishExhaustedState)
        {
            canPunch = false;
            return;
        }

        canPunch = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && canPunch)
        {
            Rigidbody2D bodyRb = other.transform.GetComponent<Rigidbody2D>();
            bool isRight = transform.GetComponentInParent<Transform>().position.x - other.transform.position.x < 0;
            knockbackForce = isRight ? knockbackForce : -knockbackForce;
            StartCoroutine(PlayConfusedParticle());
            bodyRb.AddForce(new Vector2(direction.x, 0) * knockbackForce, ForceMode2D.Impulse);
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