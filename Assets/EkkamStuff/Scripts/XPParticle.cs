using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ekkam {
    public class XPParticle : MonoBehaviour
    {
        [SerializeField] ParticleSystem xpParticles;
        List<ParticleSystem.Particle> particles = new List<ParticleSystem.Particle>();
        bool started = false;

        public float xpAmount = 1f;

        private void OnParticleTrigger()
        {
            int triggeredParticles = xpParticles.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, particles);
            
            for (int i = 0; i < triggeredParticles; i++)
            {
                ParticleSystem.Particle p = particles[i];
                p.remainingLifetime = 0;
                particles[i] = p;
                print("Got a particle!");
                GameManager.instance.playersXP += xpAmount;
                GameManager.instance.playersTotalXP += xpAmount;
            }
            xpParticles.SetTriggerParticles(ParticleSystemTriggerEventType.Enter, particles);
        }

        void Update()
        {
            if (xpParticles.particleCount > 0 && !started) started = true;

            if (xpParticles.particleCount == 0 && started)
            {
                Destroy(gameObject);
            }
        }
    }
}