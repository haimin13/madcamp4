using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using TMPro;
using UnityEngine.SceneManagement;

public class ChampionCreateSceneManager : MonoBehaviour
{
    public Button OKButton;
    public Button sendButton;
    public Button nextButton;
    public Button startButton;
    public GameObject storyPanel;
    public GameObject loadingPanel;
    public GameObject champInfoPanel;
    public List<TextMeshProUGUI> countText;
    public TextMeshProUGUI champInfoText;
    public TMP_InputField champDescription;
    int createdCount = 0;

    // Start is called before the first frame update
    void Start()
    {
        
        GameDataManager.Instance.ResetChampions();
        OKButton.onClick.AddListener(OnOKButtonClicked);
        sendButton.onClick.AddListener(OnSendButtonClicked);
        nextButton.onClick.AddListener(OnNextButtonClicked);
        startButton.onClick.AddListener(OnStartButtonClicked);

        OnNextButtonClicked();
        storyPanel.SetActive(true);
        loadingPanel.SetActive(false);

        bool debug = false;

        //Debug Mode
        if (debug)
        {
            GameDataManager.Instance.LoadCharacters();
            OnStartButtonClicked();
        }
        
    }

    void OnOKButtonClicked()
    {
        AudioManager.Instance.PlayClickSound();
        storyPanel.SetActive(false);  // 비활성화
    }

    void OnSendButtonClicked()
    {
        AudioManager.Instance.PlayClickSound();
        // 입력 처리 json화
        string desc = champDescription.text;
        
        Debug.Log(desc);

        loadingPanel.SetActive(true);
        var req = new Dictionary<string, object>();
        req["user_prompt"] = desc;

        string json = JsonConvert.SerializeObject(req);
        Debug.Log(json);
        if (APIRequester.Instance != null)
        {
            StartCoroutine(APIRequester.Instance.SendJsonRequest("/api/v1/characters", "POST", json, null, (response) =>
            {
                Debug.Log("POST successful!");
                loadingPanel.SetActive(false);
                if (GameDataManager.Instance != null)
                {
                    Debug.Log("GameDataManager.Instance != null");
                    GameDataManager.Instance.SetChampion(response);
                }
                champInfoPanel.SetActive(true);
                if (createdCount > 2)
                {
                    nextButton.gameObject.SetActive(false);
                    startButton.gameObject.SetActive(true);
                }
                ShowChampDescription(CharacterSheet.Instance.characters.Count - 1);
            }, (error) =>
            {
                Debug.Log("POST unsuccessful! Try Again");
                loadingPanel.SetActive(false);
            }));
        }
    }

    void ShowChampDescription(int champIdx)
    {
        CharacterData chara = CharacterSheet.Instance.characters[champIdx];
        string skillsText = "";
        string space = "    ";
        foreach (var skill in chara.skills)
        {
            skillsText += $"\n{space}{skill.skill_name}({skill.skill_type}):\n{space}{space}{skill.description}";
        }
        champInfoText.text = (
            $"이름: {chara.character_name}\n" +
            $"설명: {chara.description}\n" +
            $"타입: {chara.character_type}\n" +
            $"스탯:\n{space}체력: {chara.stats.hp}\n{space}공격: {chara.stats.atk}\n{space}방어: {chara.stats.def}\n{space}" + 
            $"특공: {chara.stats.sp_atk}\n{space}특방: {chara.stats.sp_def}\n{space}스피드: {chara.stats.speed}\n" +
            "스킬:" + skillsText +"\n");
    }
    void OnNextButtonClicked()
    {
        AudioManager.Instance.PlayClickSound();
        createdCount += 1;
        foreach (var text in countText)
        {
            text.text = $"({createdCount}/3)";
        }
        champDescription.text = "";
        champInfoPanel.SetActive(false);
    }
    void OnStartButtonClicked()
    {
        AudioManager.Instance.PlayClickSound();
        Debug.Log("OnStartButtonClicked");
        if (GameDataManager.Instance != null)
        {
            Debug.Log("GameDataManager.Instance != null");
            GameDataManager.Instance.GetEnemiesData();
            // 내부에서 씬 전환함
        }
    }
    void Update()
    {

    }
}
