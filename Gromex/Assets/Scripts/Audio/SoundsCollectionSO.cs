using UnityEngine;

[CreateAssetMenu]
public class SoundsCollectionSO : ScriptableObject
{
    [Header("Music")]
    public SoundSO[] DiscoBall;
    public SoundSO[] Music;

    [Header("SFX")]
    public SoundSO[] GunShot;
    public SoundSO[] Jump;
    public SoundSO[] Splat;
    public SoundSO[] Jetpack;
    public SoundSO[] GrenadeShoot;
    public SoundSO[] GrenadeExplode;
    public SoundSO[] GrenadeBeep;
    public SoundSO[] PlayerHit;
    public SoundSO[] PlayerDeath;
}
