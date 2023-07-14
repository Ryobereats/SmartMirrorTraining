using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AudioManager : SingletonMonoBehaviour<AudioManager>
{
  [SerializeField] private float _globalAudioVolume;

  [SerializeField] private AudioClip normalSe;
  [SerializeField] private AudioClip specialSe;

  private AudioSource _seAudioSource;

  public enum SE
  {
    NormalSe,
    SpecialSe
  }
  protected override void Awake()
  {
    base.Awake();
    _seAudioSource = gameObject.AddComponent<AudioSource>();

    //全体の音量を設定
    AudioListener.volume = _globalAudioVolume;
  }

  /// <summary>
  /// 効果音を鳴らす
  /// </summary>
  /// <param name="audio"></param>
  /// <param name="volume"></param>
  /// <param name="pitch"></param>
  /// <param name="delay"></param>
  public void PlaySE(SE audio, float volume = 1, float pitch = 1, float delay = 0, bool flagGenerateAudioSource = false)
  {
    AudioClip audioClip = null;
    switch (audio)
    {
      case SE.NormalSe:
        audioClip = normalSe;
        break;
      case SE.SpecialSe:
        audioClip = specialSe;
        break;
      default:
        break;
    }

    AudioSource audioSource = _seAudioSource;

    if (flagGenerateAudioSource)
    {
      GameObject audioSouceObject = new GameObject();
      Destroy(audioSouceObject, 5 + delay);
      audioSource = audioSouceObject.AddComponent<AudioSource>();
    }


    audioSource.pitch = pitch;

    if (delay == 0)
    {
      audioSource.PlayOneShot(audioClip, volume);
    }
    else
    {
      StartCoroutine(Delay(audioSource));
    }

    IEnumerator Delay(AudioSource audioSource)
    {
      yield return new WaitForSeconds(delay);
      audioSource.PlayOneShot(audioClip, volume);
    }
  }
}
