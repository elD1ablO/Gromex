using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Sounds Collection")]
public class SoundsCollectionSO : ScriptableObject
{
    [Header("Menu (1.ogg)")]
    public SoundSO[] Menu;

    [Header("Game mode start")]
    public SoundSO[] TimeMode;   
    public SoundSO[] LivesMode;  

    [Header("Coins")]
    public SoundSO[] GoodCoin;   
    public SoundSO[] BadCoin;    

    [Header("Countdown 3-2-1")]
    public SoundSO[] Countdown321; 
}
