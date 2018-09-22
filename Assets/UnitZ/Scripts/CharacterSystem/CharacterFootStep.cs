//----------------------------------------------
//      UnitZ Battleground : Online PVP starter kit
//    Copyright © Hardworker studio 2018 
// by Rachan Neamprasert www.hardworkerstudio.com

using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class CharacterFootStep : MonoBehaviour
{
    private AudioSource audios;
    private CharacterSystem character;
    private float delay = 0;
    public AudioClip[] FoodSteps;
    public float Delay = 3;

    void Start()
    {
        character = this.gameObject.GetComponent<CharacterSystem>();
        audios = this.gameObject.GetComponent<AudioSource>();
    }

    void PlaySound()
    {
        if (character && (character.Motor != null && character.Motor.grounded || character.Motor == null))
        {
            // play sound only on ground
            if (FoodSteps.Length > 0)
            {
                // play foot step sound randomly
                audios.PlayOneShot(FoodSteps[Random.Range(0, FoodSteps.Length)]);
            }
        }
    }

    void Update()
    {
        if (!character)
            return;

        if (character.enabled && character.IsAlive && (character.Motor != null && character.Motor.grounded || character.Motor == null))
        {
            if (delay >= Delay)
            {
                PlaySound();
                delay = 0;
            }
        }
        if (delay < Delay)
        {
            // calculate foot step frequency by move velocity
            delay += character.MoveVelocity.magnitude * Time.deltaTime;
        }
    }
}
