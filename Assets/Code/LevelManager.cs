using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    public Player Player { get; private set; }
    public CameraController Camera { get; private set; }
    
	public TimeSpan RunningTime { get { return DateTime.UtcNow - _stared; } }

    public int CurrentTimeBouns
    {
        get
        {
            var secondDifference = (int)(BounsCutoffSecond - RunningTime.TotalSeconds);
            return Mathf.Max(0, secondDifference) * BounsSecondMultiplier;
        }

    }
    private List<CheckPoint> _checkpoints;
    private int _currentCheckpointIndex;
    private DateTime _stared;
    private int _savePoint;

    public CheckPoint DebugSpawm;
   	public int BounsCutoffSecond;
    public int BounsSecondMultiplier;

    public void Awake()
    {
		_savePoint = GameManager.Instance.Points;
        Instance = this;
    }

    public void Start()
    {
        _checkpoints = FindObjectsOfType<CheckPoint>().OrderBy(t => t.transform.position.x).ToList();
        _currentCheckpointIndex = _checkpoints.Count > 0 ? 0 : -1;


        Player = FindObjectOfType<Player>();
        Camera = FindObjectOfType<CameraController>();

        _stared = DateTime.UtcNow;
		var listeners = FindObjectsOfType<MonoBehaviour>().OfType<IPlayerRespawnListener>();
		foreach (var listener in listeners)
		{
			for (var i = _checkpoints.Count - 1; i >= 0; i--)
			{
				
				var distance = ((MonoBehaviour)listener).transform.position.x - _checkpoints[i].transform.position.x;
				if (distance < 0)
					continue;
				
				_checkpoints[i].AssignObjectToCheckpoint(listener);
				break;
				
			}
		}

	
#if UNITY_EDITOR
        if (DebugSpawm != null)
            DebugSpawm.SpamPlayer(Player);
        else if (_currentCheckpointIndex != -1)
            _checkpoints[_currentCheckpointIndex].SpamPlayer(Player);
#else
           if (_currentCheckpointIndex != -1)
                _checkpoints[_currentCheckpointIndex].SpamPlayer(Player);
#endif
    }
    public void Update()
        {
            var isAtLastCheckpoint = _currentCheckpointIndex + 1 >= _checkpoints.Count;
            if (isAtLastCheckpoint)
                return;

            var distanceToNextCheckpoint = _checkpoints[_currentCheckpointIndex + 1].transform.position.x - Player.transform.position.x;
            if (distanceToNextCheckpoint >= 0)
                return;

            _checkpoints[_currentCheckpointIndex].PlayerLeftCheckpoint();
            _currentCheckpointIndex++;
            _checkpoints[_currentCheckpointIndex].PlayerHitCheckpoint();
            
            GameManager.Instance.AddPoint(CurrentTimeBouns);
            _savePoint = GameManager.Instance.Points;
            _stared = DateTime.UtcNow;



        }
	public void GotoNextLevel(string levelName){

		StartCoroutine (GotoNextLevelCo(levelName));
		}

	private IEnumerator GotoNextLevelCo(string levelName){
		Player.FinishLevel ();
		GameManager.Instance.AddPoint (CurrentTimeBouns);

		FloatingText.Show ("Level Complete !","CheckpointText",new CenteredTextPositioner(.2f));
		yield return new WaitForSeconds (1);
		FloatingText.Show(string.Format("+{0} points!",GameManager.Instance.Points),"CheckpointText",new CenteredTextPositioner(.1f)) ;
		yield return new WaitForSeconds (5f);


		if (string.IsNullOrEmpty (levelName)) 
						Application.LoadLevel ("StartScreen");
				else
						Application.LoadLevel (levelName);
	}
    public void KillPlayer()
    {
        StartCoroutine(KillPlayerCo());
    }
    private IEnumerator KillPlayerCo()
    {
        Player.Kill();
        Camera.isFollowing = false;
        yield return new WaitForSeconds(2f);

        Camera.isFollowing = true;

        if (_currentCheckpointIndex != -1)
            _checkpoints[_currentCheckpointIndex].SpamPlayer(Player);

        
        _stared = DateTime.UtcNow;
        GameManager.Instance.ResetPoints(_savePoint);
		yield break;
    }
}
 
