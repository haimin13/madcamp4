using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class EnemyListResponse
{
    public int runId;
    public string enemiesJson;
}

public class CurrentCharacterState
{
    public int currentHp;
    public bool isAlive;
}
public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }
    public int currentRound = 1;
    public int selectedChara = 99;
    public List<CurrentCharacterState> charaStates = null; 
    public int runId;
    public Dictionary<string, Dictionary<string, float>> typeChart;
    public APIRequester apiRequester;
    public CharacterSheet characterSheet;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        if (APIRequester.Instance != null)
            apiRequester = APIRequester.Instance;
    }

    public void SetChampion(string response)
    {
        if (CharacterSheet.Instance != null)
        {
            Debug.Log("CharacterSheet.Instance != null");
            var character = CharacterSheet.Instance.ParseSingleCharacterData(response);

            if (CharacterSheet.Instance.characters == null)
                CharacterSheet.Instance.characters = new List<CharacterData>();

            CharacterSheet.Instance.characters.Add(character);
            Debug.Log($"만든 캐릭터 수 : {CharacterSheet.Instance.characters.Count}");
        }
    }

    public void GetEnemiesData()
    {
        string json = CharacterSheet.Instance.ToJsonOfCharacters();
        apiRequester.SendJsonRequest("/api/runs", "POST", json, null, (response) =>
        {
            Debug.Log("적 정보 받아옴!");
            var res = JsonConvert.DeserializeObject<EnemyListResponse>(response);
            runId = res.runId;
            var charaList = CharacterSheet.Instance.ParseMultipleCharacterData(res.enemiesJson);
            CharacterSheet.Instance.enemies = charaList;
        }, (error) =>
        {
            Debug.Log("적 정보 받아오기 실패!");
            Debug.Log(error);
        });
    }
    public void UpdateCharacterState(int idx, CurrentCharacterState newState = null)
    {
        if (charaStates == null)
        {
            charaStates = new List<CurrentCharacterState>();
            List<CharacterData> characters = CharacterSheet.Instance.characters;
            for (int i = 0; i < characters.Count; i++)
            {
                CurrentCharacterState state = new CurrentCharacterState();
                state.currentHp = characters[i].stats.hp;
                state.isAlive = true;
                charaStates.Add(state);
            }
        }
        else if (0 <= idx && idx < charaStates.Count)
        {
            charaStates[idx] = newState;
        }
        else Debug.Log("Invalid idx");
    }

    public void GetRoundInfo()
    {
        apiRequester.SendJsonRequest($"/api/runs/{runId}/floors/{currentRound}", "GET", null, null, (response) =>
        {
            typeChart = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, float>>>(response);
        }, (error) =>
        {
            Debug.Log("상성표 받아오기 실패!");
            Debug.Log(error);
        });
    }

    // Update is called once per frame
    void Update()
    {

    }
}
