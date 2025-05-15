using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelManager : MonoBehaviour
{
    // Establish necessary fields
    
    public List<GameObject> panels;
    public GameObject defaultPanels; // 0
    public GameObject laptopPanel; // 1
    public GameObject newsPanel; // 2
    public GameObject stockPanel; // 3
    
    
    public int activePanel;
    
    // Start is called before the first frame update
    void Start()
    {
        panels.Add(defaultPanels);
        panels.Add(laptopPanel);
        panels.Add(newsPanel);
        panels.Add(stockPanel);
        foreach (GameObject panel in panels)
        {
            if (panels.IndexOf((panel)) != activePanel)
            {
                panel.SetActive(false);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha0))
        {
            ChangePanel(0);
        }
        
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            ChangePanel(1);
        }
        
        if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            ChangePanel(2);
        }
        
        if(Input.GetKeyDown(KeyCode.Alpha3))
        {
            ChangePanel(3);
        }
    }

    public void ChangePanel(int newPanel)
    {
        if (newPanel >= 0 && newPanel < panels.Count)
        {
            panels[activePanel].SetActive(false);
            panels[newPanel].SetActive(true);
            activePanel = newPanel;
        }
        else
        {
            panels[activePanel].SetActive(false);
            activePanel = 0;
        }
    }
}
