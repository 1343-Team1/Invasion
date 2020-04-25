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
        static AudioManager player;

        // ========== PRIVATE / PROTECTED ==========
        GameManager gameManager;

        // ========== PUBLIC ==========
        [Header("Audio Sources")]
        public AudioSource musicMild;
        public AudioSource musicIntense;
        public AudioSource sFX;


        /********************
         * =- Methods -=
        ********************/

        // Get the GameManager and AudioSource.
        void Start()
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
        }

        // Play a new clip.
        void PlayMusic(AudioClip clip, Source source)
        {
            AudioSource audioSource = (source == Source.MILD_MUSIC) ? musicMild : musicIntense;

            audioSource.Stop();
            audioSource.clip = clip;
            audioSource.Play();
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
        INTENSE_MUSIC
    }
}
