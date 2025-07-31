using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioClip click;
    public AudioClip showPanel;
    public AudioClip closePanel;
    public AudioClip BGM1;
    public AudioClip BGM4;
    public AudioClip BGM7;
    public AudioClip rankUp;
    public AudioClip rankDown;
    public AudioClip hit;
    public AudioClip heal;
    public AudioClip shake;
    public AudioClip projectile;
    public AudioClip laser;

    private static AudioManager instance;
    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AudioManager>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("AudioManager");
                    instance = obj.AddComponent<AudioManager>();
                }
            }
            return instance;
        }
    }

    private AudioSource audioSource;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject);
        }
        click = Resources.Load<AudioClip>("ClickButton");
        showPanel = Resources.Load<AudioClip>("ShowPanel");
        closePanel = Resources.Load<AudioClip>("ClosePanel");;
        BGM1 = Resources.Load<AudioClip>("round1");
        BGM4 = Resources.Load<AudioClip>("round4");
        BGM7 = Resources.Load<AudioClip>("round7");
        rankUp = Resources.Load<AudioClip>("RankUp");
        rankDown = Resources.Load<AudioClip>("RankDown");
        hit = Resources.Load<AudioClip>("Hit");
        heal = Resources.Load<AudioClip>("Heal");
        shake = Resources.Load<AudioClip>("Shake");
        projectile = Resources.Load<AudioClip>("Projectile");
        laser = Resources.Load<AudioClip>("Laser");

    }

    // 효과음 재생용 예시 메서드
    public void PlaySound(AudioClip clip)
    {
        if (clip == null) return;
        audioSource.PlayOneShot(clip);
    }
    public void PlayClickSound()
    {
        audioSource.PlayOneShot(click);
    }
    public void PlayOpenSound()
    {
        audioSource.PlayOneShot(showPanel);
    }
    public void PlayCloseSound()
    {
        audioSource.PlayOneShot(closePanel);
    }
    public void PlayBGM(int currentRound)
    {
        if (currentRound != 1 && currentRound != 4 && currentRound != 7) return;
        if (audioSource.isPlaying) audioSource.Stop();
        switch (currentRound)
        {
            case 1:
                audioSource.clip = BGM1;
                break;
            case 4:
                audioSource.clip = BGM4;
                break;
            case 7:
                audioSource.clip = BGM7;
                break;
            default:
                break;
        }
        audioSource.loop = true;
        audioSource.Play();
    }
    public void StopBGM()
    {
        if (audioSource.isPlaying) audioSource.Stop();
        audioSource.loop = false;
    }
    public void PlayRankUp()
    {
        audioSource.PlayOneShot(rankUp);
    }
    public void PlayRankDown()
    {
        audioSource.PlayOneShot(rankDown);
    }
    public void PlayHit()
    {
        audioSource.PlayOneShot(hit);
    }
    public void PlayProjectile()
    {
        audioSource.PlayOneShot(projectile);
    }
    public void PlayLaser()
    {
        audioSource.PlayOneShot(laser);
    }
    public void PlayHeal()
    {
        audioSource.PlayOneShot(heal);
    }
    public void PlayShake()
    {
        audioSource.PlayOneShot(shake);
    }
}
