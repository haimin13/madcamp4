using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;


public class BattleSceneView : MonoBehaviour
{
    public GameObject skillPanel;
    public List<Button> skillButtons;
    public GameObject charaStatus;
    public GameObject EnemyStatus;
    // Start is called before the first frame update
    void Start()
    {
        skillButtons = new List<Button>(skillPanel.GetComponentsInChildren<Button>(true));
        for (int i = 0; i < skillButtons.Count; i++)
        {
            int idx = i;
            skillButtons[i].onClick.AddListener(() => OnSkillClicked(idx));
        }
        SetStatusPanel();
    }

    void OnSkillClicked(int btnIndex)
    {
        Debug.Log($"스킬 {btnIndex} 클릭!");
    }
    void SetStatusPanel()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
