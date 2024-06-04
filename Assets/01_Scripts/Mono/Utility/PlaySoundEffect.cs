using UnityEngine;

public class PlaySoundEffect : MonoBehaviour
{
    [SerializeField] private AudioClip clip;

    private AudioSource source;

    private void Start ()
    {
        source ??= gameObject.AddComponent<AudioSource>();
    }

    public void Play ()
    {
        if ( clip == null )
            return;

        source.clip = clip;
        source.Play();
    }
}