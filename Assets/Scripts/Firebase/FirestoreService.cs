using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;

public class FirestoreService : MonoBehaviour
{
    public FirestoreService Instance { get; private set; }
    public FirebaseFirestore db;
    ListenerRegistration listenerRegistration;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
    }

    public void SetData()
    {
        if (db == null)
        {
            Debug.Log("DB NULL");
            db = FirebaseFirestore.DefaultInstance;

        }
        var userData = new UserData();

        db.Collection("users").Document(FirebaseInitializer.Instance.User.UserId).SetAsync(userData);
    }
}
