using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;

public class AutoDestroyParticleSystem : MonoBehaviour
{
        private ParticleSystem _particleSystem;

        public void Start()
        {
            _particleSystem = GetComponent<ParticleSystem>();
        }
        public void Update()
        {
            if (_particleSystem.isPlaying)
                return;

            Destroy(gameObject);
        }
}

