using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;


public class TimeManager : SerializedMonoBehaviour
{
    public List<(int, List<string>)> days = new List<(int time, List<string> playerResponses)>();
    
    //getters & setters
    public int CurrentDay
    {
        get => days.Count > 0 ? days[days.Count - 1].Item1 : 0;
    }
    
    /// <summary>
    /// Static instance of the TimeManager that can be accessed from anywhere.
    /// </summary>
    public static TimeManager Instance { get; private set; }
    
    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// Used to initialize the singleton instance.
    /// </summary>
    private void Awake()
    {
        // Singleton pattern implementation
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // If an instance already exists, destroy this duplicate
            Destroy(gameObject);
        }
    }

    public void OnPlayerRespond(string response)
    {
        if (days.Count <= 0)
        {
            days.Add((CurrentDay, new List<string>()));
        }

        if (days[CurrentDay].Item2.Count >= 3)
        {
            days.Add((CurrentDay+1, new List<string>(){response}));
        }
        else
        {
            UnityEngine.Debug.Log("Addingf");
            days[CurrentDay].Item2.Add(response);
        }
    }
}
