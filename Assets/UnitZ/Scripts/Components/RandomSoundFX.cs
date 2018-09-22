using UnityEngine;

public class RandomSoundFX : MonoBehaviour {

    public AudioClip[] Sounds;
    private AudioSource audioSource;

	void Start () {
        audioSource = this.GetComponent<AudioSource>();
        if (audioSource != null)
        {
            if (Sounds.Length > 0)
                audioSource.PlayOneShot(Sounds[Random.Range(0, Sounds.Length)]);
        }

    }
}
