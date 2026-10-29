using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;
    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Object.FindFirstObjectByType<AudioManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("AudioManager");
                    instance = go.AddComponent<AudioManager>();
                }
            }
            return instance;
        }
    }

    [Header("BGM & Music Jingles")]
    [SerializeField] private AudioClip clipBGMMainMenu;
    [SerializeField] private AudioClip clipBGMCombat;
    [SerializeField] private AudioClip clipBGMIntense;
    [SerializeField] private AudioClip clipMEGameOver;
    [SerializeField] private AudioClip clipMELevelUp;
    [SerializeField] private AudioClip clipAMBWind;

    [Header("UI SFX")]
    [SerializeField] private AudioClip clipUIHover;
    [SerializeField] private AudioClip clipUIClick;
    [SerializeField] private AudioClip clipUICardAppear;
    [SerializeField] private AudioClip clipUICardSelect;
    [SerializeField] private AudioClip clipUIGameOver;

    [Header("Player SFX")]
    [SerializeField] private AudioClip clipPlayerHurt;
    [SerializeField] private AudioClip clipPlayerDie;
    [SerializeField] private AudioClip[] clipsPlayerFootsteps;
    [SerializeField] private AudioClip clipPlayerHeal;
    [SerializeField] private AudioClip clipPlayerLowHealth;

    [Header("Spell SFX")]
    [SerializeField] private AudioClip clipSpellBoltCast;
    [SerializeField] private AudioClip clipSpellBoltImpact;
    [SerializeField] private AudioClip clipSpellFireballLaunch;
    [SerializeField] private AudioClip clipSpellFireballExplode;
    [SerializeField] private AudioClip clipSpellIceSpearCast;
    [SerializeField] private AudioClip clipSpellIceSpearPierce;
    [SerializeField] private AudioClip clipSpellDarkOrbHit;
    [SerializeField] private AudioClip clipSpellDarkOrbLoop;
    [SerializeField] private AudioClip clipSpellShieldAbsorb;
    [SerializeField] private AudioClip clipSpellShieldActivate;
    [SerializeField] private AudioClip clipSpellShieldBreak;
    [SerializeField] private AudioClip clipSpellShieldRegen;

    [Header("Enemy SFX")]
    [SerializeField] private AudioClip clipEnemyAttack;
    [SerializeField] private AudioClip[] clipsEnemyHit;
    [SerializeField] private AudioClip[] clipsEnemyDeath;
    [SerializeField] private AudioClip clipEnemySpawn;

    [Header("Pickups SFX")]
    [SerializeField] private AudioClip clipGemPickup;
    [SerializeField] private AudioClip clipMagnetPickup;
    [SerializeField] private AudioClip clipPotionPickup;

    [Header("Volume Controls")]
    [Range(0f, 1f)] [SerializeField] private float masterVolume = 1f;
    [Range(0f, 1f)] [SerializeField] private float bgmVolume = 0.7f;
    [Range(0f, 1f)] [SerializeField] private float ambVolume = 0.35f;
    [Range(0f, 1f)] [SerializeField] private float sfxVolume = 1f;
    [Range(0f, 1f)] [SerializeField] private float uiVolume = 1f;

    // Dedicated Audio Sources
    private AudioSource bgmSource;
    private AudioSource ambSource;
    private AudioSource jingleSource;
    private AudioSource uiSource;
    private AudioSource lowHealthLoopSource;
    private AudioSource darkOrbLoopSource;

    // SFX Pool
    private const int PoolSize = 24;
    private AudioSource[] sfxSources;
    private int nextSourceIndex = 0;

    // Rate Limiting
    private Dictionary<AudioClip, float> lastPlayTime = new Dictionary<AudioClip, float>();
    private const float MinTimeBetweenSameClip = 0.04f;

    // Gem combo
    private float lastGemPickupTime;
    private int gemComboCount = 0;
    private const float GemComboResetDelay = 0.6f;

    private Coroutine bgmCrossfadeRoutine;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
#if UNITY_EDITOR
            AutoPopulateClipsIfMissing();
#endif
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        AutoPlaySceneMusic();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AutoPlaySceneMusic();
    }

    private void InitializeAudioSources()
    {
        // 1. Music (BGM) Source
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.playOnAwake = false;
        bgmSource.loop = true;
        bgmSource.spatialBlend = 0f;

        // 2. Ambience Source
        ambSource = gameObject.AddComponent<AudioSource>();
        ambSource.playOnAwake = false;
        ambSource.loop = true;
        ambSource.spatialBlend = 0f;

        // 3. Jingle Source (can play during pause)
        jingleSource = gameObject.AddComponent<AudioSource>();
        jingleSource.playOnAwake = false;
        jingleSource.spatialBlend = 0f;
        jingleSource.ignoreListenerPause = true;

        // 4. SFX Pool
        sfxSources = new AudioSource[PoolSize];
        for (int i = 0; i < PoolSize; i++)
        {
            AudioSource src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.spatialBlend = 0f;
            sfxSources[i] = src;
        }

        // 5. UI Source (can play during Time.timeScale = 0)
        uiSource = gameObject.AddComponent<AudioSource>();
        uiSource.playOnAwake = false;
        uiSource.spatialBlend = 0f;
        uiSource.ignoreListenerPause = true;

        // 6. Low Health Loop
        lowHealthLoopSource = gameObject.AddComponent<AudioSource>();
        lowHealthLoopSource.playOnAwake = false;
        lowHealthLoopSource.spatialBlend = 0f;
        lowHealthLoopSource.loop = true;
        if (clipPlayerLowHealth != null) lowHealthLoopSource.clip = clipPlayerLowHealth;

        // 7. Dark Orb Loop
        darkOrbLoopSource = gameObject.AddComponent<AudioSource>();
        darkOrbLoopSource.playOnAwake = false;
        darkOrbLoopSource.spatialBlend = 0f;
        darkOrbLoopSource.loop = true;
        if (clipSpellDarkOrbLoop != null) darkOrbLoopSource.clip = clipSpellDarkOrbLoop;
    }

    // ==========================================
    // BGM & AMBIENCE MANAGEMENT
    // ==========================================
    public void AutoPlaySceneMusic()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName.Contains("MainMenu"))
        {
            PlayMainMenuBGM();
            StopAmbience();
        }
        else if (sceneName.Contains("InGame"))
        {
            PlayInGameCombatBGM();
            PlayAmbience(clipAMBWind);
        }
    }

    public void PlayBGM(AudioClip clip, float fadeDuration = 0.8f)
    {
        if (clip == null) return;
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        if (bgmCrossfadeRoutine != null) StopCoroutine(bgmCrossfadeRoutine);
        bgmCrossfadeRoutine = StartCoroutine(CrossfadeBGMRoutine(clip, fadeDuration));
    }

    private IEnumerator CrossfadeBGMRoutine(AudioClip newClip, float duration)
    {
        float startVol = bgmSource.volume;
        float half = duration * 0.5f;

        // Fade out
        if (bgmSource.isPlaying && half > 0f)
        {
            float elapsed = 0f;
            while (elapsed < half)
            {
                elapsed += Time.unscaledDeltaTime;
                bgmSource.volume = Mathf.Lerp(startVol, 0f, elapsed / half);
                yield return null;
            }
        }

        bgmSource.Stop();
        bgmSource.clip = newClip;
        bgmSource.volume = 0f;
        bgmSource.Play();

        // Fade in
        float targetVol = bgmVolume * masterVolume;
        float elapsedIn = 0f;
        while (elapsedIn < half)
        {
            elapsedIn += Time.unscaledDeltaTime;
            bgmSource.volume = Mathf.Lerp(0f, targetVol, elapsedIn / half);
            yield return null;
        }
        bgmSource.volume = targetVol;
    }

    public void PlayMainMenuBGM() => PlayBGM(clipBGMMainMenu);
    public void PlayInGameCombatBGM() => PlayBGM(clipBGMCombat);
    public void PlayInGameIntenseBGM() => PlayBGM(clipBGMIntense);

    public void StopBGM()
    {
        if (bgmCrossfadeRoutine != null) StopCoroutine(bgmCrossfadeRoutine);
        if (bgmSource != null) bgmSource.Stop();
    }

    public void PlayAmbience(AudioClip clip)
    {
        if (clip == null || ambSource == null) return;
        ambSource.clip = clip;
        ambSource.volume = ambVolume * masterVolume;
        ambSource.Play();
    }

    public void StopAmbience()
    {
        if (ambSource != null) ambSource.Stop();
    }

    public void PlayJingle(AudioClip clip, float volume = 1f)
    {
        if (clip == null || jingleSource == null) return;
        jingleSource.volume = volume * masterVolume;
        jingleSource.PlayOneShot(clip);
    }

    public void PlayLevelUpJingle() => PlayJingle(clipMELevelUp, 0.9f);
    public void PlayGameOverJingle() => PlayJingle(clipMEGameOver, 1f);

    // ==========================================
    // CORE SFX & UI
    // ==========================================
    public void PlaySFX(AudioClip clip, float volume = 1f, float minPitch = 1f, float maxPitch = 1f, bool ignoreThrottle = false)
    {
        if (clip == null) return;

        if (!ignoreThrottle)
        {
            if (lastPlayTime.TryGetValue(clip, out float lastTime))
            {
                if (Time.unscaledTime - lastTime < MinTimeBetweenSameClip) return;
            }
            lastPlayTime[clip] = Time.unscaledTime;
        }

        if (sfxSources == null || sfxSources.Length == 0) return;

        AudioSource src = sfxSources[nextSourceIndex];
        nextSourceIndex = (nextSourceIndex + 1) % PoolSize;

        src.pitch = Random.Range(minPitch, maxPitch);
        src.volume = volume * sfxVolume * masterVolume;
        src.PlayOneShot(clip);
    }

    public void PlayUI(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null || uiSource == null) return;
        uiSource.pitch = pitch;
        uiSource.volume = volume * uiVolume * masterVolume;
        uiSource.PlayOneShot(clip);
    }

    // UI
    public void PlayUIHover() => PlayUI(clipUIHover, 0.7f, Random.Range(0.98f, 1.02f));
    public void PlayUIClick() => PlayUI(clipUIClick, 0.9f);
    public void PlayCardAppear() => PlayUI(clipUICardAppear, 0.85f);
    public void PlayCardSelect() => PlayUI(clipUICardSelect, 0.95f);
    public void PlayGameOverPopup() => PlayUI(clipUIGameOver, 1f);

    // Player
    public void PlayPlayerHurt() => PlaySFX(clipPlayerHurt, 0.9f, 0.92f, 1.08f);
    public void PlayPlayerDie() => PlaySFX(clipPlayerDie, 1f, 1f, 1f, true);
    public void PlayPlayerHeal() => PlaySFX(clipPlayerHeal, 0.85f, 0.98f, 1.02f);

    public void PlayPlayerFootstep()
    {
        if (clipsPlayerFootsteps != null && clipsPlayerFootsteps.Length > 0)
        {
            AudioClip clip = clipsPlayerFootsteps[Random.Range(0, clipsPlayerFootsteps.Length)];
            PlaySFX(clip, 0.35f, 0.9f, 1.1f);
        }
    }

    public void SetLowHealthLoop(bool active)
    {
        if (lowHealthLoopSource == null) return;
        if (active)
        {
            if (!lowHealthLoopSource.isPlaying)
            {
                if (lowHealthLoopSource.clip == null) lowHealthLoopSource.clip = clipPlayerLowHealth;
                lowHealthLoopSource.volume = 0.8f * sfxVolume * masterVolume;
                lowHealthLoopSource.Play();
            }
        }
        else
        {
            if (lowHealthLoopSource.isPlaying)
            {
                lowHealthLoopSource.Stop();
            }
        }
    }

    // Spells
    public void PlayBoltCast() => PlaySFX(clipSpellBoltCast, 0.65f, 0.92f, 1.08f);
    public void PlayBoltImpact() => PlaySFX(clipSpellBoltImpact, 0.6f, 0.9f, 1.1f);

    public void PlayFireballLaunch() => PlaySFX(clipSpellFireballLaunch, 0.8f, 0.95f, 1.05f);
    public void PlayFireballExplode() => PlaySFX(clipSpellFireballExplode, 1f, 0.9f, 1.1f);

    public void PlayIceSpearCast() => PlaySFX(clipSpellIceSpearCast, 0.75f, 0.95f, 1.05f);
    public void PlayIceSpearPierce() => PlaySFX(clipSpellIceSpearPierce, 0.7f, 0.92f, 1.08f);

    public void PlayDarkOrbHit() => PlaySFX(clipSpellDarkOrbHit, 0.65f, 0.95f, 1.05f);

    public void SetDarkOrbLoop(bool active)
    {
        if (darkOrbLoopSource == null) return;
        if (active)
        {
            if (!darkOrbLoopSource.isPlaying)
            {
                if (darkOrbLoopSource.clip == null) darkOrbLoopSource.clip = clipSpellDarkOrbLoop;
                darkOrbLoopSource.volume = 0.5f * sfxVolume * masterVolume;
                darkOrbLoopSource.Play();
            }
        }
        else
        {
            if (darkOrbLoopSource.isPlaying)
            {
                darkOrbLoopSource.Stop();
            }
        }
    }

    public void PlayShieldActivate() => PlaySFX(clipSpellShieldActivate, 0.85f);
    public void PlayShieldAbsorb() => PlaySFX(clipSpellShieldAbsorb, 0.8f, 0.95f, 1.05f);
    public void PlayShieldBreak() => PlaySFX(clipSpellShieldBreak, 0.9f);
    public void PlayShieldRegen() => PlaySFX(clipSpellShieldRegen, 0.75f);

    // Enemies
    public void PlayEnemyAttack() => PlaySFX(clipEnemyAttack, 0.75f, 0.92f, 1.08f);

    public void PlayEnemyHit()
    {
        if (clipsEnemyHit != null && clipsEnemyHit.Length > 0)
        {
            AudioClip clip = clipsEnemyHit[Random.Range(0, clipsEnemyHit.Length)];
            PlaySFX(clip, 0.7f, 0.9f, 1.1f);
        }
    }

    public void PlayEnemyDeath()
    {
        if (clipsEnemyDeath != null && clipsEnemyDeath.Length > 0)
        {
            AudioClip clip = clipsEnemyDeath[Random.Range(0, clipsEnemyDeath.Length)];
            PlaySFX(clip, 0.8f, 0.92f, 1.08f);
        }
    }

    public void PlayEnemySpawn() => PlaySFX(clipEnemySpawn, 0.5f, 0.95f, 1.05f);

    // Pickups
    public void PlayGemPickup()
    {
        if (Time.unscaledTime - lastGemPickupTime < GemComboResetDelay)
        {
            gemComboCount = Mathf.Min(gemComboCount + 1, 12);
        }
        else
        {
            gemComboCount = 0;
        }
        lastGemPickupTime = Time.unscaledTime;

        float pitch = 1.0f + (gemComboCount * 0.035f);
        PlaySFX(clipGemPickup, 0.65f, pitch, pitch, true);
    }

    public void PlayMagnetPickup() => PlaySFX(clipMagnetPickup, 0.9f);
    public void PlayPotionPickup() => PlaySFX(clipPotionPickup, 0.85f);

#if UNITY_EDITOR
    [ContextMenu("Auto Populate Clips from Assets/Audio")]
    public void AutoPopulateClipsIfMissing()
    {
        string p = "Assets/Audio/";
        clipBGMMainMenu = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(p + "BGM_MainMenu.mp3");
        clipBGMCombat = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(p + "BGM_InGame_Combat.wav");
        clipBGMIntense = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(p + "BGM_InGame_Intense.wav");
        clipMEGameOver = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(p + "ME_GameOver.wav");
        clipMELevelUp = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(p + "ME_LevelUp.wav");
        clipAMBWind = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(p + "AMB_PostApocalyptic_Wind.ogg");

        clipUIHover = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(p + "UI_Hover.wav");
        clipUIClick = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(p + "UI_Click.wav");
        clipUICardAppear = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(p + "UI_Card_Appear.wav");
        clipUICardSelect = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(p + "UI_Card_Select.wav");
        clipUIGameOver = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(p + "UI_GameOver_Popup.wav");

        clipPlayerHurt = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(p + "Player_Hurt.wav");
        clipPlayerDie = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(p + "Player_Die.wav");
        clipsPlayerFootsteps = new AudioClip[]
        {
            UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(p + "Player_Footstep_01.wav"),
            UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(p + "Player_Footstep_02.wav"),
            UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(p + "Player_Footstep_03.wav")
        };
        clipPlayerHeal = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(p + "Player_Heal.wav");
        clipPlayerLowHealth = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(p + "Player_LowHealth_Loop.wav");

        clipSpellBoltCast = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(p + "Spell_Bolt_Cast.wav");
        clipSpellBoltImpact = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(p + "Spell_Bolt_Impact.wav");
        clipSpellFireballLaunch = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(p + "Spell_Fireball_Launch.wav");
        clipSpellFireballExplode = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(p + "Spell_Fireball_Explode.wav");
        clipSpellIceSpearCast = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(p + "Spell_IceSpear_Cast.wav");
        clipSpellIceSpearPierce = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(p + "Spell_IceSpear_Pierce.wav");
        clipSpellDarkOrbHit = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(p + "Spell_DarkOrb_Hit.wav");
        clipSpellDarkOrbLoop = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(p + "Spell_DarkOrb_Loop.wav");
        clipSpellShieldAbsorb = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(p + "Spell_Shield_Absorb.wav");
        clipSpellShieldActivate = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(p + "Spell_Shield_Activate.wav");
        clipSpellShieldBreak = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(p + "Spell_Shield_Break.wav");
        clipSpellShieldRegen = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(p + "Spell_Shield_Regen.wav");

        clipEnemyAttack = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(p + "Enemy_Attack.wav");
        clipsEnemyHit = new AudioClip[]
        {
            UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(p + "Enemy_Hit_01.wav"),
            UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(p + "Enemy_Hit_02.wav"),
            UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(p + "Enemy_Hit_03.wav")
        };
        clipsEnemyDeath = new AudioClip[]
        {
            UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(p + "Enemy_Death_01.wav"),
            UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(p + "Enemy_Death_02.wav"),
            UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(p + "Enemy_Death_03.wav")
        };
        clipEnemySpawn = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(p + "Enemy_Spawn.wav");

        clipGemPickup = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(p + "SFX_Gem_Pickup.wav");
        clipMagnetPickup = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(p + "SFX_Magnet_Pickup.wav");
        clipPotionPickup = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(p + "SFX_Potion_Pickup.wav");
    }
#endif
}
