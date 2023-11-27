using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ekkam {
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager instance;

        [SerializeField] AudioClip dashSound;

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(this);
            }
        }

        public void PlayDashSound(AudioSource source)
        {
            source.PlayOneShot(dashSound);
        }
    }
}
