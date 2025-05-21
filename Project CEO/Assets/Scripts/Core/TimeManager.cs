using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using System;
using System.Text;
using TMPro;

public class TimeManager : SerializedMonoBehaviour
{
    public List<(int, List<string>)> days = new List<(int time, List<string> playerResponses)>();
    public int currentWeek = 0;

    [SerializeField] private GameObject timePopupContainer;
    [SerializeField] private TextMeshProUGUI timePopupText;
    
    
    //getters & setters
    public int CurrentDay
    {
        get => days.Count > 0 ? days[days.Count - 1].Item1 : 0;
    }

    public static event Action<int> onDayChange;
    public static event Action<int> onWeekChange;
    
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

    /// <summary>
    /// Records a player's response and manages the game's day progression system.
    /// </summary>
    /// <param name="response">The player's response text to be recorded</param>
    /// <remarks>
    /// This method has two main functions:
    /// 1. It tracks player responses by storing them in a data structure organized by game days
    /// 2. It handles day progression logic when the maximum responses per day is reached
    /// 
    /// The method follows these steps:
    /// - If no days exist yet, it creates the first day entry with an empty response list
    /// - If the current day already has 3 or more responses, it creates a new day and adds the response there
    /// - Otherwise, it adds the response to the current day's list
    /// 
    /// This system allows the game to maintain a history of player interactions and
    /// automatically progress time when enough interactions have occurred.
    /// </remarks>
    public void OnPlayerRespond(string response)
    {
        // If no days exist yet, initialize the first day
        if (days.Count <= 0)
        {
            days.Add((CurrentDay, new List<string>()));
        }
    
        // If the current day has reached the maximum responses (3), 
        // advance to the next day and add the response there
        if (days[CurrentDay].Item2.Count >= 3)
        {
            days.Add((CurrentDay+1, new List<string>(){response}));
            onDayChange?.Invoke(CurrentDay+1);
            UpdateTimeText();
            timePopupContainer.SetActive(true);
            
            if (days.Count != 0 && days.Count % 5 == 0)
            {
                currentWeek++;
                onWeekChange?.Invoke(currentWeek);
            }
        }
        else
        {
            // Otherwise, add the response to the current day's list
            days[CurrentDay].Item2.Add(response);
        }
    }

    public void UpdateTimeText()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append("Today is week ");
        builder.Append(currentWeek + 1);
        switch (CurrentDay % 5)
        {
            case 1:
                builder.Append(" Monday");
                break;
            case 2:
                builder.Append(" Tuesday");
                break;
            case 3:
                builder.Append(" Wednesday");
                break;
            case 4:
                builder.Append(" Thursday");
                break;
            case 0:
                builder.Append(" Friday");
                break;
        }

        if (CurrentDay % 5 > 0)
        {
            builder.Append(", \nyou still need to keep the company running for ");
            builder.Append(CurrentDay % 5);
            builder.Append(" days. ");
        }
        else
        {
            builder.Append("You can cash out at the end of the day!");
        }
        timePopupText.text = builder.ToString();
    }
}
