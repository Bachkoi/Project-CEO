using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the UI elements related to the cash-out feature that appears periodically in the game.
/// </summary>
/// <remarks>
/// This class is responsible for displaying cash-out UI elements at specific intervals 
/// based on the in-game day cycle. It subscribes to day change events from the TimeManager
/// and shows the cash-out container when appropriate time intervals are reached.
/// </remarks>
public class CashOutUI : MonoBehaviour
{
    /// <summary>
    /// Reference to the GameObject that contains all cash-out related UI elements.
    /// This container is shown/hidden based on the game's day cycle.
    /// </summary>
    [SerializeField] protected GameObject cashOutContainer;
    [SerializeField] protected Button cashOutBtn;
    [SerializeField] protected Button continueBtn;
    
    /// <summary>
    /// Called when the script instance is being enabled.
    /// Subscribes to the TimeManager's day change event to receive notifications when the day changes.
    /// </summary>
    private void OnEnable()
    {
        TimeManager.onWeekChange += OnWeekChange;
        continueBtn.onClick.AddListener(ContinuePlaying);
    }

    /// <summary>
    /// Called when the script instance is being disabled.
    /// Unsubscribes from the TimeManager's day change event to prevent memory leaks.
    /// </summary>
    private void OnDisable()
    {
        TimeManager.onWeekChange -= OnWeekChange;
        continueBtn.onClick.RemoveListener(ContinuePlaying);
    }

    /// <summary>
    /// Event handler that is called when the game day changes.
    /// Shows the cash-out UI container every 5 days, starting from day 5.
    /// </summary>
    /// <param name="day">The current game day</param>
    /// <remarks>
    /// The cash-out UI is displayed when:
    /// - It's not day 0 (initial day)
    /// - The current day minus 1, modulo 5 equals 0
    /// This creates a pattern where the UI appears on days 5, 10, 15, etc.
    /// </remarks>
    private void OnWeekChange(int day)
    {
        cashOutContainer.SetActive(true);
    }

    public void ContinuePlaying()
    {
        TimeManager.Instance.EndDayAnimation();
    }
}
