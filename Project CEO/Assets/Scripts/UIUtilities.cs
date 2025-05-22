using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIUtilities : MonoBehaviour
{
    public static void SetActiveStatusAll(GameObject go ,bool active)
    {
        go.SetActive(active);

        Stack<Transform> stack = new Stack<Transform>();
        stack.Push(go.transform);

        while (stack.Count > 0)
        {
            Transform current = stack.Pop();
            foreach (Transform child in current)
            {
                child.gameObject.SetActive(active);
                stack.Push(child);
            }
        }
    }
}
