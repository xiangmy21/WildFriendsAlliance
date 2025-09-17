using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // 单例实例
    public static AudioManager Instance { get; private set; }

    [Header("音频源")]
    public AudioSource bgmSource; // 背景音乐音频源
    public AudioSource sfxSource; // 音效音频源

    [Header("背景音乐")]
    public AudioClip mainMenuBGM; // 主界面背景音乐
    public AudioClip quizBGM; // 答题音乐
    public AudioClip victoryBGM; // 胜利音乐
    public AudioClip defeatBGM; // 失败音乐
    public AudioClip battleBGM; // 战斗音乐
    public AudioClip preparationBGM1; // 准备音乐 1
    public AudioClip preparationBGM2; // 准备音乐 2

    [Header("音效")]
    public AudioClip buttonClickSFX; // 按钮点击音效
    public AudioClip placePieceSFX; // 放下棋子音效
    public AudioClip illegalActionSFX; // 非法操作音效
    public AudioClip coinSFX; // 花钱或卖出时的金币声
    public AudioClip meleeAttackSFX; // 近战攻击音效
    public AudioClip rangedAttackSFX; // 远程攻击音效

    private void Awake()
    {
        // 实现单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 获取音频源组件
        AudioSource[] audioSources = GetComponents<AudioSource>();
        if (audioSources.Length >= 2)
        {
            bgmSource = audioSources[0];
            sfxSource = audioSources[1];
        }

        // 配置背景音乐音频源
        if (bgmSource != null)
        {
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
        }

        // 配置音效音频源
        if (sfxSource != null)
        {
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }
    }

    private void Start()
    {
        // 游戏开始时播放主界面音乐
        PlayMainMenuBGM();
    }

    /// <summary>
    /// 播放主界面背景音乐
    /// </summary>
    public void PlayMainMenuBGM()
    {
        PlayBGM(mainMenuBGM);
    }

    /// <summary>
    /// 播放答题背景音乐
    /// </summary>
    public void PlayQuizBGM()
    {
        PlayBGM(quizBGM);
    }

    /// <summary>
    /// 播放胜利背景音乐
    /// </summary>
    public void PlayVictoryBGM()
    {
        PlayBGM(victoryBGM);
    }

    /// <summary>
    /// 播放失败背景音乐
    /// </summary>
    public void PlayDefeatBGM()
    {
        PlayBGM(defeatBGM);
    }

    /// <summary>
    /// 播放战斗背景音乐
    /// </summary>
    public void PlayBattleBGM()
    {
        PlayBGM(battleBGM);
    }

    /// <summary>
    /// 播放准备音乐 1
    /// </summary>
    public void PlayPreparationBGM1()
    {
        PlayBGM(preparationBGM1);
    }

    /// <summary>
    /// 播放准备音乐 2
    /// </summary>
    public void PlayPreparationBGM2()
    {
        PlayBGM(preparationBGM2);
    }

    /// <summary>
    /// 通用BGM播放方法
    /// </summary>
    private void PlayBGM(AudioClip clip)
    {
        if (bgmSource != null && clip != null)
        {
            bgmSource.clip = clip;
            bgmSource.Play();
        }
    }

    /// <summary>
    /// 播放按钮点击音效
    /// </summary>
    public void PlayButtonClick()
    {
        PlaySFX(buttonClickSFX);
    }

    /// <summary>
    /// 播放放下棋子音效
    /// </summary>
    public void PlayPlacePiece()
    {
        PlaySFX(placePieceSFX);
    }

    /// <summary>
    /// 播放非法操作音效
    /// </summary>
    public void PlayIllegalAction()
    {
        PlaySFX(illegalActionSFX);
    }

    /// <summary>
    /// 播放金币音效
    /// </summary>
    public void PlayCoin()
    {
        PlaySFX(coinSFX);
    }

    /// <summary>
    /// 播放近战攻击音效
    /// </summary>
    public void PlayMeleeAttack()
    {
        PlaySFX(meleeAttackSFX);
    }

    /// <summary>
    /// 播放远程攻击音效
    /// </summary>
    public void PlayRangedAttack()
    {
        PlaySFX(rangedAttackSFX);
    }

    /// <summary>
    /// 通用SFX播放方法
    /// </summary>
    private void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    /// <summary>
    /// 停止背景音乐
    /// </summary>
    public void StopBGM()
    {
        if (bgmSource != null && bgmSource.isPlaying)
        {
            bgmSource.Stop();
        }
    }

    /// <summary>
    /// 设置背景音乐音量
    /// </summary>
    public void SetBGMVolume(float volume)
    {
        if (bgmSource != null)
        {
            bgmSource.volume = Mathf.Clamp01(volume);
        }
    }

    /// <summary>
    /// 设置音效音量
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        if (sfxSource != null)
        {
            sfxSource.volume = Mathf.Clamp01(volume);
        }
    }
}