using UnityEngine;
using UnityEngine.SceneManagement;

namespace WeGotOne
{
    public class AudioManager : SceneSingleton<AudioManager>
{
    /*
     * 예전에 적어둔건데 돌려쓰고 있어서 긁어왓습니다.
     * 싱글톤만 선생님껄로 바꿧고 코드 더 궁금하시면,
     * 유튜브에 (골드메탈)치시면 거기가 원본이에요.
     */
    
    // SFX랑 BGM 전체 관리하는 부분, 여기서 필요한 SFX나 BGM 넣은 다음 인스펙터 번호 지정해주면 됨.
    // 24번줄 주석까지만 보면 딱히 뭐 알필요 없긴한데 코드 궁금하실까봐 필요한 부분은 주석처리 해놨습니다.
    // EX) 인스펙터 상에서 아이템줍는 효과음을 3번 인덱스에 넣었으면
    public enum Sfx
    {
        /*
         * 예시 브금1 = 0,
         * 예시 브금2 = 1,
         * 예시 브금3 = 2,
         * 예시 브금4(여기에 아이템 줍는 효과음) = 3 이러고 나중에 PlaySfx(Sfx.예시브금4) 하면 됩니다.
         */
        Pulse = 0,
        PopGrab,
        PopRelease,
        Confused,
        GetItem,
        BridgeBuild,
        FishTerrainSmashed,
    }

    public enum Bgm
    {
        MainMenu,
        GameMenu,
    }

    public AudioClip[] bgmClips;
    public float bgmVolume;
    private AudioSource bgmPlayer;

    public AudioClip[] sfxClips;
    public float sfxVolume;
    private AudioSource[] sfxPlayer;

    public int channels;
    private int channelIndex;

    // 씬 변경될때마다 자동으로 bgm 바꿔줄려고 세팅했습니다.
    // 밑에 OnSceneLoaded 함수 보시면 string으로 씬 이름 가져와서 브금 틀어주는거라
    // 그대로 쓸꺼면 OnSceneLoaded 함수에 string 부분 저희 씬 이름으로 바꿔야돼요.(InGame이라든가 그런걸루)
    protected override void OnInitialize()
    {
        base.OnInitialize();
        Init();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    protected override void OnDispose()
    {
        base.OnDispose();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 플레이어 생성 부분인데 BGM은 한 씬당 하나만 나오면 되니까 채널 나눌 필요가 없는데,
    // SFX는 여러개가 나올 수 있기 때문에 채널로 분리해서 다중 실행을 가능하게 합니다.
    // EX) 아이템을 인벤토리에 넣으면서 체력이 위기 상황이 되면 "아이템 획득 SFX, 체력 알림이 동시"에 떠야함 -> 소리 재생이 2개가 동시에 실행되어야함
    private void Init()
    {
        GameObject bgmObject = new GameObject("BgmPlayer");
        bgmObject.transform.parent = transform;
        bgmPlayer = bgmObject.AddComponent<AudioSource>();
        bgmPlayer.playOnAwake = false;
        bgmPlayer.loop = true;
        bgmPlayer.volume = bgmVolume;

        GameObject sfxObject = new GameObject("SfxPlayer");
        sfxObject.transform.parent = transform;
        sfxPlayer = new AudioSource[channels];

        for (int i = 0; i < sfxPlayer.Length; i++)
        {
            sfxPlayer[i] = sfxObject.AddComponent<AudioSource>();
            sfxPlayer[i].playOnAwake = false;
            sfxPlayer[i].volume = sfxVolume;
        }
    }

    // 그냥 브금 틀기
    public void PlayBgm(Bgm bgm)
    {
        bgmPlayer.clip = bgmClips[(int)bgm];
        bgmPlayer.Play();
    }

    // 브금 멈추기
    public void StopBgm()
    {
        bgmPlayer.Stop();
    }

    // SFX 실행시키기. 인스펙터 Clips에 원하는거 넣고 enum 번호랑 매칭시킨다음에 play하면 됩니다. 
    public void PlaySfx(Sfx sfx, float pitch = 1f)
    {
        for (int i = 0; i < sfxPlayer.Length; i++)
        {
            int loopIndex = (i + channelIndex) % sfxPlayer.Length;

            if (sfxPlayer[loopIndex].isPlaying)
                continue;

            channelIndex = loopIndex;
            sfxPlayer[loopIndex].clip = sfxClips[(int)sfx];
            sfxPlayer[loopIndex].pitch = pitch;
            sfxPlayer[loopIndex].Play();
            break;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case "MainMenu":
                PlayBgm(Bgm.MainMenu);
                break;

            case "GameMenu":
                PlayBgm(Bgm.GameMenu);
                break;

            case "LoadingScene":
                StopBgm();
                break;
        }
    }

    // 여긴 그냥 볼륨 조절
    public void SetBgmVolume(float volume)
    {
        bgmVolume = volume;
        bgmPlayer.volume = volume;
    }

    public void SetSfxVolume(float volume)
    {
        sfxVolume = volume;

        for (int i = 0; i < sfxPlayer.Length; i++)
        {
            sfxPlayer[i].volume = volume;
        }
    }
}
}