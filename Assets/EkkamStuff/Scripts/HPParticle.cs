using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ekkam {
    public class HPParticle : MonoBehaviour
    {
        [SerializeField] ParticleSystem hpParticles;
        List<ParticleSystem.Particle> particles = new List<ParticleSystem.Particle>();
        public Player playerToHeal;
        bool started = false;

        public float hpAmount = 1f;

        private void OnParticleTrigger()
        {
            int triggeredParticles = hpParticles.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, particles);
            
            for (int i = 0; i < triggeredParticles; i++)
            {
                ParticleSystem.Particle p = particles[i];
                p.remainingLifetime = 0;
                particles[i] = p;
                print("Got a particle!");
                GameManager.instance.CollectHP(hpAmount, playerToHeal);
            }
            hpParticles.SetTriggerParticles(ParticleSystemTriggerEventType.Enter, particles);
        }

        void Update()
        {
            if (hpParticles.particleCount > 0 && !started) started = true;

            if (hpParticles.particleCount == 0 && started)
            {
                Destroy(gameObject);
            }
        }
    }
}