using GetBricked.Gameplay.Data;
using UnityEngine;

namespace GetBricked.Gameplay
{
    internal enum BreakoutMusicTrack
    {
        Menu,
        Gameplay,
        Intense,
    }

    internal sealed class BreakoutAudioService
    {
        private const string SoundResourceRoot = "Sounds/";
        private const float MusicVolume = 0.32f;
        private const float MusicDuckedVolume = 0.22f;
        private const float MusicDuckDurationSeconds = 1.15f;
        private const int SfxSourceCount = 8;

        private readonly AudioSource musicSource;
        private readonly AudioSource[] sfxSources;
        private readonly AudioClip menuMusic;
        private readonly AudioClip gameplayMusic;
        private readonly AudioClip intenseMusic;
        private readonly AudioClip ballHitPaddle;
        private readonly AudioClip ballHitWall;
        private readonly AudioClip ballHitBrick;
        private readonly AudioClip ballLostMain;
        private readonly AudioClip ballLostShort;
        private readonly AudioClip brickBreak;
        private readonly AudioClip brickExplosive;
        private readonly AudioClip brickIndestructible;
        private readonly AudioClip brickSpinning;
        private readonly AudioClip brickSplit;
        private readonly AudioClip pickupCollectPositive;
        private readonly AudioClip pickupDrop;
        private readonly AudioClip pickupCollectNegative;
        private readonly AudioClip pickupCollectNegativeShort;
        private readonly AudioClip bonusScore;
        private readonly AudioClip levelComplete;
        private int nextSfxSourceIndex;
        private BreakoutMusicTrack currentMusicTrack = (BreakoutMusicTrack)(-1);
        private float musicDuckTimer;

        private BreakoutAudioService(GameObject audioRoot)
        {
            musicSource = CreateAudioSource(audioRoot, "Music Source");
            musicSource.loop = true;
            musicSource.volume = MusicVolume;

            sfxSources = new AudioSource[SfxSourceCount];

            for (var index = 0; index < sfxSources.Length; index++)
            {
                sfxSources[index] = CreateAudioSource(audioRoot, $"SFX Source {index + 1}");
            }

            menuMusic = LoadClip("01_neon_grid_cruise_loop_96bpm");
            gameplayMusic = LoadClip("02_brick_blaster_drive_loop_112bpm");
            intenseMusic = LoadClip("03_chrome_sunset_overdrive_loop_124bpm");
            ballHitPaddle = LoadClip("ball_hit_paddle_neon_boop");
            ballHitWall = LoadClip("ball_hit_wall_laser_ping");
            ballHitBrick = LoadClip("ball_hit_brick_pixel_thunk");
            ballLostMain = LoadClip("ball_lost_to_ether_vhs_drop");
            ballLostShort = LoadClip("ball_lost_to_ether_short_drop");
            brickBreak = LoadClip("brick_break_crystal_burst");
            brickExplosive = LoadClip("brick_explosive_neon_boom");
            brickIndestructible = LoadClip("brick_indestructible_vhs_clang");
            brickSpinning = LoadClip("brick_spinning_orbit_zap");
            brickSplit = LoadClip("brick_split_shard_scatter");
            pickupCollectPositive = LoadClip("drop_pickup_powerup_rise");
            pickupDrop = pickupCollectPositive;
            pickupCollectNegative = LoadClip("power_down_cursed_pickup_vhs_drop");
            pickupCollectNegativeShort = LoadClip("power_down_short_glitch_drop");
            bonusScore = LoadClip("bonus_point_score_jingle");
            levelComplete = LoadClip("level_complete_stage_clear_neon_fanfare");
        }

        public static BreakoutAudioService Create(Transform parent)
        {
            var audioRoot = new GameObject("Audio");
            audioRoot.transform.SetParent(parent, false);
            return new BreakoutAudioService(audioRoot);
        }

        public void Update(float deltaTimeSeconds)
        {
            if (musicSource == null)
            {
                return;
            }

            if (musicDuckTimer > 0f)
            {
                musicDuckTimer = Mathf.Max(0f, musicDuckTimer - Mathf.Max(0f, deltaTimeSeconds));
            }

            musicSource.volume = musicDuckTimer > 0f ? MusicDuckedVolume : MusicVolume;
        }

        public void PlayMusic(BreakoutMusicTrack track)
        {
            var clip = track switch
            {
                BreakoutMusicTrack.Gameplay => gameplayMusic,
                BreakoutMusicTrack.Intense => intenseMusic,
                _ => menuMusic,
            };

            if (clip == null || musicSource == null)
            {
                return;
            }

            if (currentMusicTrack == track && musicSource.isPlaying)
            {
                return;
            }

            currentMusicTrack = track;
            musicSource.clip = clip;
            musicSource.pitch = 1f;
            musicSource.volume = MusicVolume;
            musicSource.Play();
        }

        public void PlayBallHitPaddle()
        {
            PlaySfx(ballHitPaddle, 0.75f, 0.96f, 1.04f);
        }

        public void PlayBallHitWall()
        {
            PlaySfx(ballHitWall, 0.55f, 0.94f, 1.08f);
        }

        public void PlayBrickHit(BrickDefinition definition)
        {
            if (definition != null && !definition.IsBreakable)
            {
                PlaySfx(brickIndestructible, 0.72f, 0.94f, 1.02f);
                return;
            }

            if (definition != null && definition.SpinsOnHit)
            {
                PlaySfx(brickSpinning, 0.64f, 0.92f, 1.1f);
                return;
            }

            PlaySfx(ballHitBrick, 0.6f, 0.92f, 1.05f);
        }

        public void PlayBrickDestroyed(BrickDefinition definition)
        {
            if (definition != null && definition.IsExplosive)
            {
                PlaySfx(brickExplosive, 0.78f, 0.95f, 1.03f);
                PlaySfx(brickBreak, 0.5f, 0.96f, 1.04f);
                return;
            }

            if (definition != null && definition.SplitsOnBreak)
            {
                PlaySfx(brickSplit, 0.78f, 0.9f, 1.12f);
                PlaySfx(brickBreak, 0.55f, 0.96f, 1.04f);
                return;
            }

            if (definition != null && definition.SpinsOnHit)
            {
                PlaySfx(brickSpinning, 0.48f, 0.92f, 1.1f);
            }

            PlaySfx(brickBreak, 0.85f, 0.96f, 1.04f);
        }

        public void PlayPickupDropped()
        {
            PlaySfx(pickupDrop, 0.42f, 0.98f, 1.04f);
        }

        public void PlayPickupCollected(PowerUpDefinition definition)
        {
            if (definition != null && !definition.IsBeneficial)
            {
                var negativeClip = definition.EffectType == PowerUpEffectType.ActiveDropMultiplier
                    ? pickupCollectNegativeShort
                    : pickupCollectNegative;
                PlaySfx(negativeClip, 0.75f, 0.95f, 1.05f);
                return;
            }

            PlaySfx(pickupCollectPositive, 0.9f, 0.98f, 1.05f);
        }

        public void PlayBallLost(bool hasOtherActiveBalls)
        {
            PlaySfx(hasOtherActiveBalls ? ballLostShort : ballLostMain, hasOtherActiveBalls ? 0.55f : 0.84f, 0.96f, 1.03f);
        }

        public void PlayBonusScore()
        {
            PlaySfx(bonusScore, 0.95f, 0.98f, 1.02f);
        }

        public void PlayLevelComplete()
        {
            PlaySfx(levelComplete, 0.86f, 1f, 1f);
            musicDuckTimer = MusicDuckDurationSeconds;
        }

        private void PlaySfx(AudioClip clip, float volume, float minPitch, float maxPitch)
        {
            if (clip == null || sfxSources == null || sfxSources.Length == 0)
            {
                return;
            }

            var source = sfxSources[nextSfxSourceIndex];
            nextSfxSourceIndex = (nextSfxSourceIndex + 1) % sfxSources.Length;

            if (source == null)
            {
                return;
            }

            source.pitch = Mathf.Approximately(minPitch, maxPitch)
                ? minPitch
                : Random.Range(Mathf.Min(minPitch, maxPitch), Mathf.Max(minPitch, maxPitch));
            source.PlayOneShot(clip, Mathf.Clamp01(volume));
        }

        private static AudioSource CreateAudioSource(GameObject root, string sourceName)
        {
            var sourceObject = new GameObject(sourceName);
            sourceObject.transform.SetParent(root.transform, false);
            var source = sourceObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.dopplerLevel = 0f;
            source.rolloffMode = AudioRolloffMode.Linear;
            return source;
        }

        private static AudioClip LoadClip(string fileName)
        {
            return Resources.Load<AudioClip>(SoundResourceRoot + fileName);
        }
    }
}
