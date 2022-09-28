using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioSource slash;
    public AudioSource damage;
    public AudioSource menu;
    public void playSlash() {
        slash.Play();
    }
    public void playDamage() {
        damage.Play();
    }
    public void playMenu() {
        menu.Play();
    }
}
