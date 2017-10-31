  using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PathedProjectilesSpawner:MonoBehaviour
{
    public Transform Destination;
    public PathedProjectile Projectile;

	public GameObject SpawnEffect;
    public float Speed;
    public float FireRate;
	public AudioClip SpawnProjectileSOund;
	public Animator Animator;

    private float _nextShotInSeconds;

    public void Start()
    {
        _nextShotInSeconds = FireRate;
    }
    public void Update()
    {
        if ((_nextShotInSeconds -= Time.deltaTime) > 0)
            return;

        _nextShotInSeconds = FireRate;
        var projectile = (PathedProjectile)Instantiate(Projectile, transform.position, transform.rotation);
        projectile.Initalize(Destination, Speed);

		if (SpawnEffect != null)
						Instantiate (SpawnEffect, transform.position, transform.rotation);

		if (SpawnProjectileSOund != null)
						AudioSource.PlayClipAtPoint (SpawnProjectileSOund, transform.position);

		if (Animator != null)
						Animator.SetTrigger ("Fire");
    }
    public void OnDrawGizmos()
    {
        if (Destination == null)
            return;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position,Destination.position);
    }
}



