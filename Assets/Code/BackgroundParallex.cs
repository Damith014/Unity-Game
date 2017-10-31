using UnityEngine;
using System.Collections;

public class BackgroundParallex: MonoBehaviour 
{
    public Transform[] Backgrounds;
    public float ParallaxScale;
    public float ParallaxReductionFactor;
    public float Smoothing;
    float io = 1.0f;
    private Vector3 _lastPositon;


    public void Start()
    {
        _lastPositon = transform.position;

    }
    public void Update()
    {
        var parallax = (_lastPositon.x - transform.position.x) * ParallaxScale;

        for (var i = 0; i < Backgrounds.Length; i++)
        {
            var backgroundTargetPosition = Backgrounds[i].position.x + parallax*(i * ParallaxReductionFactor + 1);
            Backgrounds[i].position = Vector3.Lerp(Backgrounds[i].position,new Vector3(backgroundTargetPosition,Backgrounds[i].position.y,Backgrounds[i].position.z),//to 
			 Smoothing*Time.deltaTime);

        }
        _lastPositon = transform.position;
    }
       
   }

