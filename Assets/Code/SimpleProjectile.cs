using UnityEngine;
using System.Collections;

public class SimpleProjectile : Projectile,ITakeDamage {

	public int Damage;
	public GameObject DestroyedEffect;
	public int PointsToGivePlayer;
	public float TimeToLive;
	public AudioClip DestroySound;

	public void Update(){
		if ((TimeToLive -= Time.deltaTime) <= 0) {
				
			DestroyProjectile();
			return;
		}
		//transform.Translate (Direction+new Vector2(InitialVelocity.x,0)*Speed*Time.deltaTime,Space.World);
		transform.Translate(Direction*((Mathf.Abs(InitialVelocity.x)+Speed)*Time.deltaTime),Space.World);
	}
	public void TakeDamage(int damage , GameObject instigator){
		if(PointsToGivePlayer!=0){

			var projetile=instigator.GetComponent<Projectile>();
			if(projetile!=null&&projetile.Owner.GetComponent<Player>()!=null){
				GameManager.Instance.AddPoint(PointsToGivePlayer);
				//FloatingText.Show(string.Format("+{0}!",PointsToGivePlayer),"PointStartText",new FromWorldPortTextPositioner(Camera.main,transform.position,1.5f,50));
				FloatingText.Show(string.Format("+{0}!",PointsToGivePlayer),"PointStarText",new FromWorldPortTextPositioner(Camera.main,transform.position,1.5f,50)) ;

			
			}
		}
		DestroyProjectile ();
		}
	protected override void OnColliderOther(Collider2D other){

		DestroyProjectile ();
	}
	protected override void OnColliderTakeDamage(Collider2D other,ITakeDamage takeDamage){

		takeDamage.TakeDamage (Damage, gameObject);
		DestroyProjectile ();
	}
	private void DestroyProjectile (){

		if (DestroyedEffect != null)
						Instantiate (DestroyedEffect, transform.position, transform.rotation);

		if (DestroySound != null)
			AudioSource.PlayClipAtPoint (DestroySound,transform.position);
		Destroy (gameObject);
	}
}
