﻿
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PathedProjectile:MonoBehaviour,ITakeDamage
{
    private Transform _destination;
    private float _speed;
	public int PointsToGivePlayer=0;
	public GameObject DestroyEffect;

	public AudioClip DestroySound;
    public void Initalize(Transform destination, float speed)
    {
        _destination = destination;
        _speed = speed;
    }
    public void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, _destination.position, Time.deltaTime * _speed);

        var distanceSquared=(_destination.transform.position-transform.position).sqrMagnitude;

        if (distanceSquared > .01f * .01f)
            return;

		if (DestroyEffect != null)
						Instantiate (DestroyEffect,transform.position,transform.rotation);
        Destroy(gameObject);

		if (DestroySound != null)
						AudioSource.PlayClipAtPoint (DestroySound, transform.position);
    }
	public void TakeDamage(int damage , GameObject instigator){
		if (DestroyEffect != null)
						Instantiate (DestroyEffect,transform.position,transform.rotation);

		Destroy (gameObject);

		var projectile=instigator.GetComponent<Projectile>();

		if (projectile != null && projectile.Owner.GetComponent<Player> () != null && PointsToGivePlayer != 0) {

			GameManager.Instance.AddPoint(PointsToGivePlayer);
			//FloatingText.Show(string.Format("+{0}!",PointsToGivePlayer),"PointStartText",new FromWorldPortTextPositioner(Camera.main,transform.position,1.5f,50));
			FloatingText.Show(string.Format("+{0}!",PointsToGivePlayer),"PointStarText",new FromWorldPortTextPositioner(Camera.main,transform.position,1.5f,50)) ;

		}
	}
}

