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
    public GameObject synopsisPanel;
    public GameObject loadingPanel;
    public GameObject champInfoPanel;
    public TMP_InputField champDescription;

    // Start is called before the first frame update
    void Start()
    {
        OKButton.onClick.AddListener(OnOKButtonClicked);
        sendButton.onClick.AddListener(OnSendButtonClicked);
        loadingPanel.SetActive(false);
    }

    void OnOKButtonClicked()
    {
        synopsisPanel.SetActive(false);  // 비활성화
    }

    void OnSendButtonClicked()
    {
        // 입력 처리 json화
        string desc = champDescription.text;

        loadingPanel.SetActive(true);
        if (APIRequester.Instance != null)
        {
            // 수정하셈
            StartCoroutine(APIRequester.Instance.SendJsonRequest("/", "GET", null, null, (response) =>
            {
                Debug.Log("successful!");
                loadingPanel.SetActive(false);
                if (GameDataManager.Instance != null)
                {
                    GameDataManager.Instance.SetChampion(response);
                }
                champInfoPanel.SetActive(true);
            }, (error) =>
            {
                Debug.Log("unsuccessful!");
                loadingPanel.SetActive(false);
            }));
        }
    }
    void OnStartButtonClicked()
    {
        SceneManager.LoadScene("StageListScene");
    }
    void Update()
    {

    }
}
