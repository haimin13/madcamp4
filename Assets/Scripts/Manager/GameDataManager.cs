using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public class EnemyListResponse
{
    public string run_id;
    public List<CharacterData> enemies;
}
public class TypeChartResponse
{
    public string status;
    public CharacterData enemy;
    public TypeChartData type_chart;
}
public class TypeChartData
{
    public Dictionary<string, Dictionary<string, float>> player_vs_enemy { get; set; }
    public Dictionary<string, Dictionary<string, float>> enemy_vs_player { get; set; }
}

public class CurrentCharacterStatus
{
    public string charaName;
    public string charaType;
    public string debuff;
    public int duration;
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
    public int currentRound = 0;
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
        bool debugMode = false;
        if (APIRequester.Instance != null)
            apiRequester = APIRequester.Instance;
        if (debugMode)
        {
            currentRound = 1;
            selectedChara = 0;
        }
        else
        {
            currentRound = 0;
        }
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
            Debug.Log(res.run_id);
            runId = res.run_id;
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
                status.debuff = "정상";
                status.duration = 0;
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

    public void GetRoundInfo(System.Action onComplete = null, System.Action onFail = null)
    {
        Debug.Log("GetRoundInfo");
        StartCoroutine(apiRequester.SendJsonRequest($"/api/runs/{runId}/floors/{currentRound}", "GET", null, null, (response) =>
        {
            var res = JsonConvert.DeserializeObject<TypeChartResponse>(response);
            Debug.Log(res.status);
            if (res.status == "completed")
            {
                var typeChart = new Dictionary<string, Dictionary<string, float>>();

                // player_vs_enemy
                foreach (var kvp in res.type_chart.player_vs_enemy)
                    typeChart[kvp.Key] = kvp.Value;

                // enemy_vs_player
                foreach (var kvp in res.type_chart.enemy_vs_player)
                    typeChart[kvp.Key] = kvp.Value;

                this.typeChart = typeChart; // 원하는 곳에 저장
                onComplete?.Invoke();
            }
            else
            {
                StartCoroutine(RetryGetRoundInfo(onComplete, onFail));
                //onFail?.Invoke();
            }
        }, (error) =>
        {
            Debug.Log("상성표 받아오기 실패!");
            Debug.Log(error);
            StartCoroutine(RetryGetRoundInfo(onComplete, onFail));
            //onFail?.Invoke();
        }));
    }

    private IEnumerator RetryGetRoundInfo(System.Action onComplete, System.Action onFail)
    {
        yield return new WaitForSeconds(1f);
        GetRoundInfo(onComplete, onFail);
    }

    public void SaveCharacters()
    {
        Debug.Log("SaveCharacters");
        string path = Application.dataPath + "/characters.json";
        string json = CharacterSheet.Instance.ToJsonOfCharacters();
        File.WriteAllText(path, json);

        path = Application.dataPath + "/enemies.json";
        json = CharacterSheet.Instance.ToJsonOfEnemies();
        File.WriteAllText(path, json);
    }
    public void LoadCharacters()
    {
        Debug.Log("LoadCharacters");
        string path = Application.dataPath + "/characters.json";
        if (!File.Exists(path))
        {
            Debug.LogWarning("캐릭터 파일이 없습니다: " + path);
            return;
        }
        string json = File.ReadAllText(path);
        var charaList = CharacterSheet.Instance.ParseMultipleCharacterData(json);
        CharacterSheet.Instance.characters = charaList;

        path = Application.dataPath + "/enemies.json";
        if (!File.Exists(path))
        {
            Debug.LogWarning("적 파일이 없습니다: " + path);
            return;
        }
        json = File.ReadAllText(path);
        charaList = CharacterSheet.Instance.ParseEnemiesData(json);
        CharacterSheet.Instance.enemies = charaList;
    }
    public void SaveTypeChart()
    {
        Debug.Log("SaveTypeChart");
        string path = Application.dataPath + "/typeChart.json";
        string json = JsonConvert.SerializeObject(typeChart);
        File.WriteAllText(path, json);
    }
    public void LoadTypeChart()
    {
        Debug.Log("LoadTypeChart");
        string path = Application.dataPath + "/typeChart.json";
        if (!File.Exists(path))
        {
            Debug.LogWarning("타입차트 파일이 없습니다: " + path);
            return;
        }
        string json = File.ReadAllText(path);
        typeChart = JsonConvert.DeserializeObject <Dictionary<string, Dictionary<string, float>>>(json);
    }
    public string GetCharaDescription(CharacterData chara)
    {
        string skillsText = "";
        string space = "    ";
        foreach (var skill in chara.skills)
        {
            skillsText += $"\n{space}{skill.skill_name}({skill.skill_type}):\n{space}{space}{skill.description}";
        }
        string desc = (
            $"이름: {chara.character_name}\n" +
            $"설명: {chara.description}\n" +
            $"타입: {chara.character_type}\n" +
            $"스탯:\n{space}체력: {chara.stats.hp}\n{space}공격: {chara.stats.atk}\n{space}방어: {chara.stats.def}\n{space}" +
            $"특공: {chara.stats.sp_atk}\n{space}특방: {chara.stats.sp_def}\n{space}스피드: {chara.stats.speed}\n" +
            "스킬:" + skillsText + "\n");

        return desc;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
