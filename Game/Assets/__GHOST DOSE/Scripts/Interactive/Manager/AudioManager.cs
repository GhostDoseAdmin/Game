using UnityEngine.Audio;
using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace InteractionSystem
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager instance;
        public AudioMixerGroup mixerGroup;
        public Sound[] sounds;

        void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                instance = this;
            }

            foreach (Sound s in sounds)
            {
                s.source = gameObject.AddComponent<AudioSource>();
                s.source.clip = s.clip;
                s.source.loop = s.loop;

                s.source.outputAudioMixerGroup = mixerGroup;
            }
        }

        public void Play(string sound, AudioSource audioSource)
        {

            Sound s = sounds.FirstOrDefault(item => item.name == sound);

            if (s == null)
            {
                Debug.LogWarning("Sound: " + sound + " not found!");
                return;
            }

            //specified audio source
            if (audioSource != null)
            {
                audioSource.clip = s.clip;
                audioSource.loop = s.loop;
                audioSource.volume = s.volume;
                audioSource.pitch = s.pitch;
                s.source = audioSource;
             }

            s.source.volume = s.volume * (1f + UnityEngine.Random.Range(-s.volumeVariance / 2f, s.volumeVariance / 2f));
            s.source.pitch = s.pitch * (1f + UnityEngine.Random.Range(-s.pitchVariance / 2f, s.pitchVariance / 2f));
            //Debug.Log("PLAYING SOUND" + s.source.clip.name);
            s.source.Play();
        }

        public void StopPlaying(string sound, AudioSource audioSource)
        {
            Sound s = sounds.FirstOrDefault(item => item.name == sound);
            if (s == null)
            {
                Debug.LogWarning("Sound: " + name + " not found!");
                return;
            }

            if (audioSource != null)
            {
                s.source = audioSource;
            }

            s.source.volume = s.volume * (1f + UnityEngine.Random.Range(-s.volumeVariance / 2f, s.volumeVariance / 2f));
            s.source.pitch = s.pitch * (1f + UnityEngine.Random.Range(-s.pitchVariance / 2f, s.pitchVariance / 2f));
            //Debug.Log("STOPPING SOUND" + s.source.clip.name);
            s.source.Stop();
        }


        public void UpdateVolume(string sound, AudioSource audioSource, float newVolume)
        {

            Sound s = sounds.FirstOrDefault(item => item.name == sound);

            if (s == null)
            {
                Debug.LogWarning("Sound: " + sound + " not found!");
                return;
            }

            //specified audio source
            if (audioSource != null)
            {
                s.source = audioSource;
            }

            s.source.volume = newVolume * (1f + UnityEngine.Random.Range(-s.volumeVariance / 2f, s.volumeVariance / 2f));
            //s.source.Play();
        }


        public AudioClip GetSound(string soundName)
        {
            foreach (Sound s in sounds)
            {
                if (s.name == soundName)
                {
                    return s.clip;
                }
            }
            return null;
        }






    }
    }