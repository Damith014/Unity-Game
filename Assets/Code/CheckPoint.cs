using UnityEngine;
using System.Collections.Generic;
using System.Collections;

   public  class CheckPoint : MonoBehaviour
    {
      private List<IPlayerRespawnListener> _listeners;

       public void Awake()
       {
           _listeners = new List<IPlayerRespawnListener>();
       }
       public void PlayerHitCheckpoint()
       {
          StartCoroutine(PlayerHitCheckpointCo(LevelManager.Instance.CurrentTimeBouns));

       }
       private IEnumerator PlayerHitCheckpointCo(int bouns)
       {
           FloatingText.Show("Check point!","CheckpointText",new CenteredTextPositioner(.3f));
           yield return new WaitForSeconds(.5f);
           FloatingText.Show(string.Format("+{0} time bouns!",bouns),"CheckpointText",new CenteredTextPositioner(.3f));
		//yield break;
       }
       public void PlayerLeftCheckpoint()
       {
           
       }
       public void SpamPlayer(Player player)
       {
           player.RespawnAt(transform);
           foreach (var listener in _listeners)
               listener.OnPlayerRespawnInThisCheckpoint(this, player);
       }
	 public void AssignObjectToCheckpoint(IPlayerRespawnListener listener)
       {
           _listeners.Add(listener);
       } 
    }

