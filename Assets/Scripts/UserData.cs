using Firebase.Firestore;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

[FirestoreData]
public class UserData
{
    [FirestoreProperty("spinCount")]
    public int spinCount { get; set; }

    [FirestoreProperty("cooldownEndTimestamp")]
    public long cooldownEndTimestamp { get; set; }

    [FirestoreProperty("gold")]
    public float golds { get; set; }

    [FirestoreProperty("gems")]
    public float gems { get; set; }
    
    [FirestoreProperty("lsatSpinTime")]
    public Timestamp lastSpinTime { get; set; }



    public UserData()
    {
        spinCount = 0;
        cooldownEndTimestamp = 0;
        lastSpinTime = Timestamp.GetCurrentTimestamp();
        golds = 0;
        gems = 0;
    }
}

