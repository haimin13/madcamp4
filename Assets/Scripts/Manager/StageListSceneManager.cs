using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.SceneManagement;

public class StageListSceneManager : MonoBehaviour
{
    public GameObject enemyItemPrefab;   // 프사+이름 프리팹
    public Transform contentParent;     // 부모(ScrollView Content 등)
    public List<CharacterData> enemyList;
    public Image enemyImage;
    public TextMeshProUGUI enemyName;
    public TextMeshProUGUI enemyDescription;
    public Button enemyDetailButton;
    public ToggleGroup charaToggleGroup;
    public List<Toggle> charaToggles;
    public Button startButton;
    public GameObject loadingPanel;

    void Start()
    {
        CreateEnemyList();
        ShowCurrentEnemy();
        enemyDetailButton.onClick.AddListener(ShowEnemyDetail);
        charaToggles = charaToggleGroup.GetComponentsInChildren<Toggle>().ToList();
        foreach (var tog in charaToggles)
        {
            tog.onValueChanged.AddListener((_) => OnToggleChanged());
        }
        startButton.onClick.AddListener(StartBattle);
    }

    void CreateEnemyList()
    {
        int count = Mathf.Min(9, enemyList.Count);

        for (int i = 0; i < count; i++)
        {
            // 프리팹 인스턴스 생성
            GameObject item = Instantiate(enemyItemPrefab, contentParent);

            // 구성요소 연결 (Image, Text 등)
            item.transform.Find("EnemyImagePreview").GetComponent<Image>().sprite = enemyList[i].character_sprite;
            item.transform.Find("EnemyName").GetComponent<Text>().text = enemyList[i].character_name;
            item.transform.Find("EnemyLevel").GetComponent<Text>().text = $"Round {i + 1}";

            // 필요시 다른 초기화
            // if (i+1 == GameDataManager.Instance.currentRound)
            // {
            //     // 테두리에 효과 넣기
            // }
        }
    }

    void ShowCurrentEnemy()
    {
        if (CharacterSheet.Instance != null && GameDataManager.Instance != null)
        {
            var chara = CharacterSheet.Instance.characters[GameDataManager.Instance.currentRound - 1];
            // image 추가
            enemyName.text = chara.character_name;
            enemyDescription.text = chara.description;
        }
    }
    void ShowEnemyDetail()
    {
        if (CharacterSheet.Instance != null)
        {
            var chara = CharacterSheet.Instance.characters[GameDataManager.Instance.currentRound - 1];
            // 스탯, 스킬 디테일 표시해주는 팝업 패널
        }

    }

    void OnToggleChanged()
    {
        var activeToggle = charaToggleGroup.ActiveToggles().FirstOrDefault();
        if (activeToggle != null)
        {
            int index = charaToggles.IndexOf(activeToggle);
            if (GameDataManager.Instance != null)
                GameDataManager.Instance.selectedChara = index;
        }
    }

    void StartBattle()
    {
        
        if (GameDataManager.Instance != null)
        {
            int selected = GameDataManager.Instance.selectedChara;
            if (selected == 99)
            {
                Debug.Log("character not selected");
            }
            else if (!GameDataManager.Instance.charaStatus[selected].isAlive)
            {
                Debug.Log("the character is dead!");
            }
            else
                loadingPanel.SetActive(true);
                GameDataManager.Instance.GetRoundInfo();
                loadingPanel.SetActive(true);
                SceneManager.LoadScene("BattleScene");
        }
    }
}