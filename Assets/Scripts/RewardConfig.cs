using System;
using UnityEngine;

public enum RewardType
{
    Gold,
    Gem
}

[Serializable]
public struct Reward
{
    public RewardType currency;
    public float amount;
    public GameObject display;
}

[CreateAssetMenu(menuName = "Spin/RewardConfig")]
public class RewardConfig : ScriptableObject
{
    public Reward[] rewards = new Reward[8];
}
