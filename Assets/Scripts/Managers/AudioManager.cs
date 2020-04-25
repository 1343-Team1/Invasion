/// Author: Jeremy Anderson, March 31, 2020.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invasion
{
    /// <summary>
    /// Play the correct music based on gameManager intensity.
    /// Take requests to play sounds or music.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        /********************
         * =- Variables -=
        ********************/

        // ========== STATIC ==========
        public static AudioManager player;

        // ========== PRIVATE / PROTECTED ==========
        GameManager gameManager;
        float timeToPlaySwarmlingAmbiance;

        // ========== PUBLIC ==========
        [Header("Audio Sources")]
        public AudioSource musicMild;
        public AudioSource musicIntense;
        public AudioSource sFX;

        [Header("Music Clips")]
        public AudioClip titleMusic;
        public AudioClip mildMusic;

        [Header("Swarmling Ambience")]
        public List<AudioClip> swarmlingClips = new List<AudioClip>();
        public float minFrequency = 1f;
        public float maxFrequency = 3f;


        /********************
         * =- Methods -=
        ********************/

        // Get the GameManager and AudioSource.
        void Awake()
        {
            gameManager = GetComponent<GameManager>();

            player = this;
        }

        // Fade music in and out based on intensity.
        void Update()
        {
            // Not time to play yet.
            if (GameManager.GetGameState() != GameManager.GameState.Playing)
                return;

            FadeMusic();

            if (gameManager.intensity > 0.4)
                PlaySwarmlingAmbiance();
        }

        void PlaySwarmlingAmbiance()
        {
            // Not time to play yet.
            if (Time.time < timeToPlaySwarmlingAmbiance)
                return;

            AudioClip clip = swarmlingClips[Random.Range(0, (swarmlingClips.Count - 1))];
            PlaySFX(clip, gameManager.intensity);
            timeToPlaySwarmlingAmbiance = Time.time + Random.Range(minFrequency, maxFrequency);
        }

        // Play a new clip.
        public static void PlayMusic(AudioClip clip, Source source)
        {
            AudioSource audioSource = player.musicMild;
            if (source == Source.INTENSE_MUSIC)
                audioSource = player.musicIntense;

            audioSource.Stop();
            audioSource.clip = clip;
            audioSource.Play();
        }

        public static void SetVolume(Source source, float volume)
        {
            AudioSource audioSource = player.musicMild;
            if (source == Source.INTENSE_MUSIC)
                audioSource = player.musicIntense;

            audioSource.volume = volume;
        }

        // Fade volume in or out.
        void FadeMusic()
        {
            musicMild.volume = 1 - gameManager.intensity;
            musicIntense.volume = gameManager.intensity;
        }

        // Play the specified audio clip.
        public static void PlaySFX(AudioClip clip, float volume = 0.5f)
        {
            player.sFX.PlayOneShot(clip, volume);
        }
    }

    /// <summary>
    /// The two AudioSources that fade music.
    /// </summary>
    public enum Source
    {
        MILD_MUSIC,
        INTENSE_MUSIC,
        TITLE_MUSIC
    }
}
