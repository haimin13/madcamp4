using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json;

public class CompleteResponse
{
    public string message;
}
public class ClearSceneManager : MonoBehaviour
{
    public GameObject charaPanel;
    public Button sendButton;
    public List<CharacterData> characters;
    bool debugMode;
    // Start is called before the first frame update
    void Start()
    {
        debugMode = true;

        AudioManager.Instance.StopBGM();
        ShowCharacters();
        sendButton.onClick.AddListener(SendCharacters);
    }

    void ShowCharacters()
    {
        if (debugMode)
        {
            GameDataManager.Instance.LoadCharacters();
            GameDataManager.Instance.runId = "run_a85f192c-c763-4f1c-a0ce-234c6a430f3d";
        }
        characters = CharacterSheet.Instance.characters;
        int idx = 0;
        foreach (Transform child in charaPanel.transform)
        {
            if (idx >= characters.Count) break;
            Transform image = child.Find("CharaImage");
            if (image != null)
            {
                var img = image.GetComponent<Image>();
                if (img != null)
                {
                    APIRequester.Instance.GetSprite(characters[idx].image_url, (sprite) =>
                    {
                        if (sprite != null)
                            img.sprite = sprite;
                    });
                }
            }
            Transform name = child.Find("CharaName");
            if (name != null)
            {
                var txt = name.GetComponent<TextMeshProUGUI>();
                if (txt != null)
                {
                    txt.text = characters[idx].character_name;
                }
            }
            idx++;
        }
    }
    void SendCharacters()
    {
        string json = CharacterSheet.Instance.ToJsonOfCharacters(req: "GameCompleteRequest");
        string runId = GameDataManager.Instance.runId;
        StartCoroutine(APIRequester.Instance.SendJsonRequest($"/api/runs/{runId}/complete", "POST", json, null, (response) =>
        {
            var res = JsonConvert.DeserializeObject<CompleteResponse>(response);
            Debug.Log(res.message);
            sendButton.gameObject.SetActive(false);
        }, (_) =>
        {
            Debug.Log("Upload Failed! Try Again");
        }));
    }
    // Update is called once per frame
    void Update()
    {

    }
}
