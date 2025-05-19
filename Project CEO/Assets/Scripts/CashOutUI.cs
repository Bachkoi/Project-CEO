using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CashOutUI : MonoBehaviour
{
    [SerializeField] protected GameObject cashOutContainer;

    private void OnEnable()
    {
        TimeManager.onDayChange += OnDayChange;
    }

    private void OnDisable()
    {
        TimeManager.onDayChange -= OnDayChange;
    }

    private void OnDayChange(int day)
    {
        if (day != 0 && (day - 1) % 5 == 0)
        {
            cashOutContainer.SetActive(true);
        }
    }
}
