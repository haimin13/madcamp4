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
    public ToggleGroup charaToggleGroup;
    public List<Toggle> charaToggles;
    public Button startButton;
    public GameObject loadingPanel;
    public GameObject detailPanel;
    public TextMeshProUGUI detailText;
    public Button detailButton;
    public Button closeButton;


    void Start()
    {
        Debug.Log($"before {GameDataManager.Instance.currentRound}");
        GameDataManager.Instance.currentRound += 1;
        Debug.Log($"after {GameDataManager.Instance.currentRound}");
        CreateEnemyList();
        ShowCurrentEnemy();
        SetToggles();
        detailPanel.SetActive(false);
        detailButton.onClick.AddListener(ShowDetail);
        startButton.onClick.AddListener(StartBattle);
        closeButton.onClick.AddListener(ClosePanel);
    }

    void CreateEnemyList()
    {
        enemyList = CharacterSheet.Instance.enemies;
        int count = Mathf.Min(9, enemyList.Count);

        for (int i = 0; i < count; i++)
        {
            // 프리팹 인스턴스 생성
            GameObject item = Instantiate(enemyItemPrefab, contentParent);

            // 구성요소 연결 (Image, Text 등)
            Debug.Log($"Loading sprite for enemy {i}: {enemyList[i].image_url}");
            APIRequester.Instance.GetSprite(enemyList[i].image_url, (sprite) =>
            {
                if (sprite != null)
                {
                    item.transform.Find("EnemyImagePreview").GetComponent<Image>().sprite = sprite;
                }
            });
            item.transform.Find("EnemyName").GetComponent<TextMeshProUGUI>().text = enemyList[i].character_name;
            item.transform.Find("EnemyRound").GetComponent<TextMeshProUGUI>().text = $"Round {i + 1}";

            var le = item.GetComponent<LayoutElement>();
            if (le == null)
                le = item.AddComponent<LayoutElement>();
            le.flexibleHeight = 1;
            // 필요시 다른 초기화
            // if (i+1 == GameDataManager.Instance.currentRound)
            // {
            //     // 테두리에 효과 넣기
            // }
        }
    }
    void SetToggles()
    {
        charaToggles = charaToggleGroup.GetComponentsInChildren<Toggle>().ToList();
        Debug.Log($"toggle count = {charaToggles.Count}");
        for (int i = 0; i < charaToggles.Count; i++)
        {
            Toggle tog = charaToggles[i];
            int capturedIndex = i;
            tog.onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                {
                    GameDataManager.Instance.selectedChara = capturedIndex;
                    Debug.Log($"selectedChara = {capturedIndex}");
                }
            });
            var charaName = tog.GetComponentInChildren<TextMeshProUGUI>();
            charaName.text = CharacterSheet.Instance.characters[i].character_name;
            if (!GameDataManager.Instance.charaStatus[i].isAlive)
            {
                tog.interactable = false;
                tog.targetGraphic.color = Color.red;
            }
        }
    }

    void ShowCurrentEnemy()
    {
        if (CharacterSheet.Instance != null && GameDataManager.Instance != null)
        {
            Debug.Log(GameDataManager.Instance.currentRound);
            Debug.Log(CharacterSheet.Instance.enemies.Count);
            var chara = CharacterSheet.Instance.enemies[GameDataManager.Instance.currentRound - 1];
            // image 추가
            APIRequester.Instance.GetSprite(chara.image_url, (sprite) =>
            {
                if (sprite != null)
                {
                    enemyImage.sprite = sprite;
                }
            });

            enemyName.text = chara.character_name;
            enemyDescription.text = chara.description;
        }
    }
    void ShowDetail()
    {
        detailPanel.SetActive(true);
        if (CharacterSheet.Instance != null)
        {
            var chara = CharacterSheet.Instance.enemies[GameDataManager.Instance.currentRound - 1];
            string desc = GameDataManager.Instance.GetCharaDescription(chara);
            detailText.text = desc;
        }

    }

    void ClosePanel()
    {
        detailPanel.SetActive(false);
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
            {
                loadingPanel.SetActive(true);
                GameDataManager.Instance.GetRoundInfo(() =>
                {
                    // 이 시점에서만 데이터 로딩 끝났음!
                    loadingPanel.SetActive(false);
                    SceneManager.LoadScene("BattleScene");
                }, () =>
                {
                    loadingPanel.SetActive(false);
                });
            }
        }
    }
    void Update()
    {
        
    }
}