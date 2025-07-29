// 💻 1. C# 데이터 구조 스크립트
// 파일 이름: CharacterData.cs
// 백엔드 JSON과 1:1로 매칭되는 데이터 구조입니다.

using System;
using System.Collections.Generic;
using UnityEngine;

// --- 메인 캐릭터 데이터 클래스 ---
[System.Serializable]
public class CharacterData
{
    public string character_id;
    public string character_name;
    public string description;
    public string image_url; // 백엔드에서 이미지 주소를 받아올 필드
    public Stats stats;
    public string character_type; // 캐릭터의 고유 타입 (1개)
    public List<Skill> skills;

    [NonSerialized]
    public Sprite character_sprite; // 런타임에 다운로드하여 채워질 이미지 데이터
}

// --- 능력치 클래스 ---
[System.Serializable]
public class Stats // total 400, 50~150 per one
{
    public int hp;
    public int atk;
    public int def;
    public int sp_atk;
    public int sp_def;
    public int speed;
}

// --- 스킬 관련 클래스 및 열거형 ---

// 각 스킬의 데이터를 담는 메인 클래스
[System.Serializable]
public class Skill
{
    // 기본 정보
    public string skill_name;
    public string description;
    public int base_power;
    public string damage_type; // "물리" 또는 "특수"
    public string skill_type; // 스킬의 고유 타입 (1개)

    // 연출 정보
    public string visual_effect_type; // 이 스킬이 어떤 연출을 사용할지 결정

    // 3가지 연출 타입별 데이터. 이 중 하나만 채워집니다.
    public ShakeEffect shake_effect;
    public ProjectileEffect projectile_effect;
    public LaserEffect laser_effect;
}

// --- 각 연출 타입별 세부 파라미터 클래스 ---

// 1. '흔들림' 연출 데이터
[System.Serializable]
public class ShakeEffect
{
    // 적에게 표시될 파티클의 색상 (예: "#FF0000")
    public string particle_color;
}

// 2. '투사체' 연출 데이터
[System.Serializable]
public class ProjectileEffect
{
    public string shape; // 예: "구체", "화살"
    public int count;    // 예: 1, 3, 5
    public string color; // 예: "#00BFFF"
}

// 3. '레이저' 연출 데이터
[System.Serializable]
public class LaserEffect
{
    // 발사 위치 예: "Player", "TopToBottom", "BottomToTop"
    public string origin;
    public int thickness; // 예: "얇음", "두꺼움"
    public string color;
}