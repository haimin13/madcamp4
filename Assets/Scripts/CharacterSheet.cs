using UnityEngine;
using System.Collections.Generic;

// ScriptableObject 애셋 생성용
[CreateAssetMenu(fileName = "New Character Sheet", menuName = "AI-Rouge/Character Sheet")]
public class CharacterSheet : ScriptableObject
{
    // 한 명이 아니라 여러 명 정보 저장 (배열, 혹은 List)
    public List<CharacterData> characters;    // 또는 public List<CharacterData> characters;
    public List<CharacterData> enemies;

    private static CharacterSheet _instance;
    public static CharacterSheet Instance
    {
        get
        {
            if (_instance == null)
            {
                // "CharacterSheet"는 Resources 폴더에 위치한 에셋명(확장자X)이어야 함
                _instance = Resources.Load<CharacterSheet>("CharacterSheet");
                if (_instance == null)
                {
                    Debug.LogError("Resources/CharacterSheet.asset 파일이 없습니다!");
                }
            }
            return _instance;
        }
    }

    // JSON에서 여러 명의 데이터를 읽어오는 함수
    public List<CharacterData> ParseMultipleCharacterData(string jsonText)
    {
        var charaList = new List<CharacterData>();
        CharacterDataListWrapper wrapper = JsonUtility.FromJson<CharacterDataListWrapper>(jsonText);
        if (wrapper != null && wrapper.characters != null)
            charaList = new List<CharacterData>(wrapper.characters); // 배열→리스트

        return charaList;
    }
    public CharacterData ParseSingleCharacterData(string jsonText)
    {
        CharacterData character = JsonUtility.FromJson<CharacterData>(jsonText);
        return character;
    }
    public string ToJsonOfCharacters()
    {
        CharacterDataListWrapper wrapper = new CharacterDataListWrapper();
        wrapper.characters = this.characters;
        return JsonUtility.ToJson(wrapper, true);
    }
}

// 배열을 JsonUtility로 읽으려면 중간 래퍼 클래스가 필요해요
[System.Serializable]
public class CharacterDataListWrapper
{
    public List<CharacterData> characters;
}