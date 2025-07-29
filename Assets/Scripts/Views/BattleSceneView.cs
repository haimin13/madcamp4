using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;


public class BattleSceneView : MonoBehaviour
{
    public BattleSceneModel model;
    public GameObject skillPanel;
    public List<Button> skillButtons;
    public GameObject charaStatus;
    public GameObject enemyStatus;
    public BattleSceneController controller;
    public TextMeshProUGUI logText;
    public Button switchPanelButton;
    public GameObject switchPanel;
    public List<Button> charaButtons;
    public GameObject candidateSkillPanel;
    public Button switchButton;
    public Button cancelButton;
    public string first = " ";
    public string second = " ";
    // Start is called before the first frame update
    void Start()
    {
        skillButtons = new List<Button>(skillPanel.GetComponentsInChildren<Button>(true));
        for (int i = 0; i < skillButtons.Count; i++)
        {
            int idx = i;
            skillButtons[i].onClick.AddListener(() => OnSkillClicked(idx));
        }
        SetLogtext();
        switchPanelButton.onClick.AddListener(OnSwitchPanelButtonClicked);
        switchButton.onClick.AddListener(OnSwitchButtonClicked);
        cancelButton.onClick.AddListener(OnCancelButtonClicked);
        charaButtons = new List<Button>(switchPanel.GetComponentsInChildren<Button>(true));
        for (int i = 0; i < charaButtons.Count; i++)
        {
            int idx = i;
            charaButtons[i].onClick.AddListener(() => OnCharaClicked(idx));
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

    public void SetLogtext(string sentence = " ")
    {
        first = second;
        second = sentence;
        logText.text = first + "\n" + second;
    }
    public void OnSwitchPanelButtonClicked()
    {
        ShowSwitchPanel();
    }
    public void ShowSwitchPanel()
    {
        switchPanel.SetActive(true);
        // 캐릭터 스테이터스 반영해서 캐릭터 표시
        
    }

    public void OnSwitchButtonClicked()
    {
        controller.SwitchCharacter();
    }

    public void OnCharaClicked(int idx)
    {
        controller.ChangeCandidate(idx);
    }
    public void ShowCandidate(List<Skill> skills)
    {
        var candidateSkillButtons = new List<Button>(candidateSkillPanel.GetComponentsInChildren<Button>(true));
        for (int i = 0; i < candidateSkillButtons.Count; i++)
        {
            Button btn = candidateSkillButtons[i];
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
    public void OnCancelButtonClicked()
    {
        if (model.isDead)
            return;
        switchPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
