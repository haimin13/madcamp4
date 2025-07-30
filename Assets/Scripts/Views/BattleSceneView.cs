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
    public GameObject gameOverPanel;
    public CanvasGroup gameOverCanvasGroup; // 게임 오버 패널의 CanvasGroup
    public Button gameOverButton; // 게임 오버 패널의 버튼
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
        SetLogtextAndWait(wait: 0f);
        switchPanelButton.onClick.AddListener(OnSwitchPanelButtonClicked);
        switchButton.onClick.AddListener(OnSwitchButtonClicked);
        cancelButton.onClick.AddListener(OnCancelButtonClicked);
        charaButtons = new List<Button>();
        for (int i = 0; i < switchPanel.transform.childCount; i++)
        {
            Transform child = switchPanel.transform.GetChild(i);
            Button btn = child.GetComponent<Button>();
            if (btn != null)
                charaButtons.Add(btn);
        }

        for (int i = 0; i < charaButtons.Count; i++)
        {
            int idx = i;
            charaButtons[i].onClick.AddListener(() => OnCharaClicked(idx));
        }
        gameOverButton.onClick.AddListener(() => {restartGame();});
    }

    void OnSkillClicked(int btnIndex)
    {
        Debug.Log($"스킬 {btnIndex} 클릭!");
        SetSkillButtonsInteractable(false);
        controller.SelectSkill(btnIndex);
    }
    public void SetSkillButtonsInteractable(bool enable)
    {
        switchPanelButton.interactable = enable;
        foreach (var btn in skillButtons)
            btn.interactable = enable;
        switchPanelButton.interactable = enable;
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

    public IEnumerator SetLogtextAndWait(string sentence = " ", float wait = 1.0f)
    {
        first = second;
        second = sentence;
        logText.text = first + "\n" + second;
        yield return new WaitForSeconds(wait);
    }
    public void OnSwitchPanelButtonClicked()
    {
        ShowSwitchPanel();
    }
    public void ShowSwitchPanel()
    {
        switchPanel.SetActive(true);
        var candidateSkillButtons = new List<Button>(candidateSkillPanel.GetComponentsInChildren<Button>(true));
        foreach (var btn in candidateSkillButtons)
        {
            if (btn.gameObject.name.Contains("SkillButton"))
            {
                btn.gameObject.SetActive(false);
            }
        }
        // 캐릭터 스테이터스 반영해서 캐릭터 표시
        for (int i = 0; i < model.charaStatus.Count; i++)
        {
            var btn = charaButtons[i];
            var chara = model.charaStatus[i];
            var nameText = btn.transform.Find("CharaName").GetComponent<TextMeshProUGUI>();
            if (nameText != null)
                nameText.text = chara.charaName;
            var charaSlider = btn.transform.Find("HPSlider").GetComponent<Slider>();
            if (charaSlider != null)
            {
                charaSlider.maxValue = chara.maxHp;
                charaSlider.value = chara.currentHp;

                var fill = charaSlider.fillRect.GetComponent<Image>();
                fill.enabled = chara.currentHp > 0;
            }
            var debuff = btn.transform.Find("DebuffText").GetComponent<TextMeshProUGUI>();
            if (debuff != null)
                debuff.text = chara.debuff;

            // 이미지 추가

            // 죽은 캐릭터 표시
            if (!chara.isAlive)
            {
                var btnBg = btn.GetComponent<Image>();
                if (btnBg != null)
                    btnBg.color = Color.red;
                foreach (Transform child in btn.transform)
                {
                    var img = child.GetComponent<Image>();
                    if (img != null)
                    {
                        img.color = Color.red;
                    }
                }
            }
            else if (i == model.currentChara)
            {
                var btnBg = btn.GetComponent<Image>();
                if (btnBg != null)
                    btnBg.color = Color.gray;
                foreach (Transform child in btn.transform)
                {
                    var img = child.GetComponent<Image>();
                    if (img != null)
                    {
                        img.color = Color.gray;
                    }
                }
            }
        }

    }

    public void OnSwitchButtonClicked()
    {
        controller.SwitchCharacter();
    }

    public void OnCharaClicked(int idx)
    {
        Debug.Log("OnCharaClicked");
        controller.ChangeCandidate(idx);
    }
    public void ShowCandidate(List<Skill> skills)
    {
        var candidateSkillButtons = new List<Button>(candidateSkillPanel.GetComponentsInChildren<Button>(true));
        for (int i = 0; i < candidateSkillButtons.Count; i++)
        {
            Button btn = candidateSkillButtons[i];
            btn.gameObject.SetActive(true);
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
        {
            Debug.Log("you have to change!");
            return;
        }
        model.candidate = 99;
        switchPanel.SetActive(false);
    }
    public void ShowGameOver(bool isWin)
    {
        SetSkillButtonsInteractable(false);
        if (isWin)
        {
            Debug.Log("You Win");
            // 게임오버 화면 구현
            // 
        }
        else
        {
            Debug.Log("You Lost");
            StartCoroutine(ShowGameOverCoroutine());
        }
    }

    private IEnumerator ShowGameOverCoroutine()
    {
        // 1. 패널을 활성화하고, 처음에는 완전히 투명하게 설정합니다.
        gameOverPanel.SetActive(true);
        gameOverCanvasGroup.alpha = 0f;

        // 2. 지정된 시간(예: 1.5초) 동안 서서히 나타나게 합니다.
        float duration = 1.5f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // 경과 시간에 따라 alpha 값을 0에서 1로 부드럽게 변경합니다.
            elapsedTime += Time.deltaTime;
            gameOverCanvasGroup.alpha = Mathf.Clamp01(elapsedTime / duration);
            yield return null; // 다음 프레임까지 대기
        }

        // 3. 애니메이션이 끝나면 alpha 값을 확실하게 1로 설정합니다.
        gameOverCanvasGroup.alpha = 1f;
        gameOverCanvasGroup.interactable = true; // 이제 버튼 등을 클릭할 수 있게 설정
    }
    public void restartGame()
    {
        // TODO 케릭터 만들어진거 삭제하는 코드 만들기
        SceneManager.LoadScene("ChampCreateScene");
    }
}
