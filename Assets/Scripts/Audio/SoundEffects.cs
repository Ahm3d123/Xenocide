using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffects : MonoBehaviour
{
    public static SoundEffects Instance;
    [SerializeField] private AudioSource MovementSound;
    [SerializeField] private AudioSource HitSound;
    [SerializeField] private AudioSource ButtonSound;

    private void Awake()
    {
        Instance = this;
    }

    public void PlayMovement()
    {
        MovementSound.Play();
    }

    public void PlayHit()
    {
        HitSound.Play(); 
    }
    
    public void PlayButton()
    {
        ButtonSound.Play();
    }
}
