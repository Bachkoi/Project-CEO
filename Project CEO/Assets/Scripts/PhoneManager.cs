using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class PhoneManager : MonoBehaviour
{
    [SerializeField] protected AudioClip clip_vibrating;
    protected AudioSource audioSource;

    [SerializeField] protected Image emojiImage;
    [SerializeField] protected List<Sprite> emojiSprites;

    private void OnEnable()
    {
        GameplayManager.onPublicReact += UpdateEmoji;
    }

    private void OnDisable()
    {
        GameplayManager.onPublicReact -= UpdateEmoji;
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        emojiImage.gameObject.SetActive(false);
    }

    private void UpdateEmoji(string reaction, int score)
    {
        emojiImage.sprite = emojiSprites[score + 2];
        StartCoroutine(ShowEmoji(1f));
    }

    IEnumerator ShowEmoji(float duration)
    {
        float t = 0.0f;
        emojiImage.gameObject.SetActive(true);
        audioSource.PlayOneShot(clip_vibrating);
        emojiImage.transform.localScale = Vector3.zero;
        while (t < duration)
        {
            t += Time.deltaTime;
            emojiImage.transform.localScale = Vector3.Lerp(emojiImage.transform.localScale, Vector3.one, t/duration);
            yield return null;
        }
        
        yield return new WaitForSeconds(2f);

        t = 0.0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            emojiImage.transform.localScale = Vector3.Lerp(Vector3.one, emojiImage.transform.localScale, t/duration);
            yield return null;
        }
        emojiImage.gameObject.SetActive(false);
    }
    
    
}
