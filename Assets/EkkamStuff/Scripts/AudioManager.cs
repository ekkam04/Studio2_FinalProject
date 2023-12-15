using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ekkam {
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager instance;
        [SerializeField] AudioSource audioSource;

        [SerializeField] AudioSource gameAudioSource;
        [SerializeField] AudioClip gameMusic;
        [SerializeField] AudioClip bossMusic;

        [Header("Audio Clips")]
        [SerializeField] AudioClip dashSound;

        [Header("Weapon Sounds")]
        [SerializeField] AudioClip[] electroRifleSounds;
        [SerializeField] AudioClip[] electromagneticRifleSounds;
        [SerializeField] AudioClip[] laserPistolSounds;
        [SerializeField] AudioClip[] laserRifleSounds;
        [SerializeField] AudioClip[] pulseShotSounds;

        [Header("Game Sounds")]
        [SerializeField] AudioClip xpSound;
        [SerializeField] AudioClip hitSound;
        [SerializeField] AudioClip levelUpSound;

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

        public void PlayElectroRifleSound(AudioSource source)
        {
            source.PlayOneShot(electroRifleSounds[Random.Range(0, electroRifleSounds.Length)]);
        }

        public void PlayElectromagneticRifleSound(AudioSource source)
        {
            source.PlayOneShot(electromagneticRifleSounds[Random.Range(0, electromagneticRifleSounds.Length)]);
        }

        public void PlayLaserPistolSound(AudioSource source)
        {
            source.PlayOneShot(laserPistolSounds[Random.Range(0, laserPistolSounds.Length)]);
        }

        public void PlayLaserRifleSound(AudioSource source)
        {
            source.PlayOneShot(laserRifleSounds[Random.Range(0, laserRifleSounds.Length)]);
        }

        public void PlayPulseShotSound(AudioSource source)
        {
            source.PlayOneShot(pulseShotSounds[Random.Range(0, pulseShotSounds.Length)]);
        }

        public void PlayLevelUpSound(AudioSource source)
        {
            source.PlayOneShot(levelUpSound);
        }

        public void PlayXPSound(AudioSource source)
        {
            source.pitch = Random.Range(0.9f, 1.1f);
            source.PlayOneShot(xpSound);
        }

        public void PlayHitSound()
        {
            audioSource.PlayOneShot(hitSound);
        }

        public void PlayGameMusic()
        {
            if (gameAudioSource.clip != gameMusic)
            {
                gameAudioSource.clip = gameMusic;
                gameAudioSource.volume = 0.03f;
                gameAudioSource.Play();
            }
        }

        public void PlayBossMusic()
        {
            gameAudioSource.clip = bossMusic;
            gameAudioSource.volume = 0.1f;
            gameAudioSource.Play();
        }
    }
}
