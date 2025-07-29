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
    public GameObject enemyStatus;
    public BattleSceneController controller;
    // Start is called before the first frame update
    void Start()
    {
        skillButtons = new List<Button>(skillPanel.GetComponentsInChildren<Button>(true));
        for (int i = 0; i < skillButtons.Count; i++)
        {
            int idx = i;
            skillButtons[i].onClick.AddListener(() => OnSkillClicked(idx));
        }
    }

    void OnSkillClicked(int btnIndex)
    {
        Debug.Log($"스킬 {btnIndex} 클릭!");
        SetSkillButtonsInteractable(false);
        controller.SelectSkill(btnIndex);
    }
    public void SetSkillButtonsInteractable(bool enable)
    {
        foreach (var btn in skillButtons)
            btn.interactable = enable;
    }
    public void ShowSkills(List<Skill> skills)
    {
        for (int i = 0; i < skills.Count; i++)
        {
            Button btn = skillButtons[i];
            var nameObj = btn.transform.Find("SkillNameText");
            if (nameObj != null)
            {
                var nameText = nameObj.GetComponent<TextMeshProUGUI>();
                if (nameText != null)
                    nameText.text = skills[i].skill_name;
            }
            var typeObj = btn.transform.Find("SkillTypeText");
            if (typeObj != null)
            {
                var typeText = typeObj.GetComponent<TextMeshProUGUI>();
                if (typeText != null)
                    typeText.text = skills[i].skill_type;
            }
        }
    }

    public void UpdateStatusPanel(CurrentCharacterStatus status, bool isEnemy)
    {
        var currentStatus = isEnemy ? enemyStatus : charaStatus;

        var charaSlider = currentStatus.transform.Find("HPSlider").GetComponent<Slider>();
        var charaName = currentStatus.transform.Find("Name").GetComponent<TextMeshProUGUI>();

        charaSlider.maxValue = status.maxHp;
        charaSlider.value = status.currentHp;
        charaName.text = status.charaName;

        var fill = charaSlider.fillRect.GetComponent<Image>();
        fill.enabled = status.currentHp > 0;
    }

    public void SetStatusPanel(CurrentCharacterStatus charaStatus, CurrentCharacterStatus enemyStatus)
    {
        UpdateStatusPanel(charaStatus, false);
        UpdateStatusPanel(enemyStatus, true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
