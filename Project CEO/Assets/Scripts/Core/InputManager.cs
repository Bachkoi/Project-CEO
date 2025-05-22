using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static Action OnEnterKeyDown = delegate { };
    public static Action OnEnterKeyDownLate = delegate { };

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            OnEnterKeyDown.Invoke();
            OnEnterKeyDownLate.Invoke();
        }
    }
}
