using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public class EnemyListResponse
{
    public string runId;
    public List<CharacterData> enemies;
}
public class TypeChartResponse
{
    public string status;
    public string enemy;
    public string type_chart;
}

public class CurrentCharacterStatus
{
    public string charaName;
    public string charaType;
    public int currentHp;
    public int maxHp;
    public int tmpAtk;
    public int tmpDef;
    public int tmpSpAtk;
    public int tmpSpDef;
    public int tmpSpeed;
    public bool isAlive;
}
public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }
    public int currentRound = 1;
    public int selectedChara = 99;
    public List<CurrentCharacterStatus> charaStatus = null; 
    public string runId;
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
        currentRound = 1;
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
        string json = CharacterSheet.Instance.ToJsonOfCharacters("RunCreateRequest");
        Debug.Log(json);
        StartCoroutine(apiRequester.SendJsonRequest("/api/runs", "POST", json, null, (response) =>
        {
            Debug.Log("적 정보 받아옴!");
            var res = JsonConvert.DeserializeObject<EnemyListResponse>(response);
            runId = res.runId;
            var charaList = res.enemies;
            CharacterSheet.Instance.enemies = charaList;
        }, (error) =>
        {
            Debug.Log("적 정보 받아오기 실패!");
            Debug.Log(error);
        }));
    }
    public void UpdateCharacterStatus(List<CurrentCharacterStatus> newStatus = null)
    {
        if (charaStatus == null) // Initialize
        {
            charaStatus = new List<CurrentCharacterStatus>();
            List<CharacterData> characters = CharacterSheet.Instance.characters;
            for (int i = 0; i < characters.Count; i++)
            {
                CurrentCharacterStatus status = new CurrentCharacterStatus();
                status.charaName = characters[i].character_name;
                status.charaType = characters[i].character_type;
                status.maxHp = characters[i].stats.hp + 100; // 체력 수치
                status.currentHp = status.maxHp;
                status.tmpAtk = characters[i].stats.atk;
                status.tmpDef = characters[i].stats.def;
                status.tmpSpAtk = characters[i].stats.sp_atk;
                status.tmpSpDef = characters[i].stats.sp_def;
                status.tmpSpeed = characters[i].stats.speed;
                status.isAlive = true;
                charaStatus.Add(status);
            }
        }
        else if (newStatus != null)
        {
            charaStatus = newStatus;
        }
        else Debug.Log("Invalid idx");
    }

    public void GetRoundInfo()
    {
        apiRequester.SendJsonRequest($"/api/runs/{runId}/floors/{currentRound}", "GET", null, null, (response) =>
        {

            var res = JsonConvert.DeserializeObject<TypeChartResponse>(response);
            if (res.status == "completed")
            {
                var type_chart = JsonConvert.DeserializeObject<Dictionary<string, string>>(res.type_chart);
                var charaToEnemy = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, float>>>(type_chart["player_vs_enemy"]);
                var enemyToChara = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, float>>>(type_chart["enemy_vs_player"]);
                typeChart = new Dictionary<string, Dictionary<string, float>>(charaToEnemy);
                foreach (var kvp in enemyToChara)
                {
                    typeChart[kvp.Key] = kvp.Value;
                }
            }
        }, (error) =>
        {
            Debug.Log("상성표 받아오기 실패!");
            Debug.Log(error);
        });
    }

    public void SaveCharacters()
    {
        string path = Application.dataPath + "/characters.json";
        string json = CharacterSheet.Instance.ToJsonOfCharacters();
        File.WriteAllText(path, json);
    }
    public void LoadCharacters()
    {
        string path = Application.dataPath + "/characters.json";
        if (!File.Exists(path))
        {
            Debug.LogWarning("캐릭터 파일이 없습니다: " + path);
            return;
        }
        string json = File.ReadAllText(path);
        var charaList = CharacterSheet.Instance.ParseMultipleCharacterData(json);
        CharacterSheet.Instance.characters = charaList;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
