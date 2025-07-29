// ImageManager.cs
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.IO; // 파일 입출력을 위해 필요합니다.

public class ImageManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static ImageManager Instance { get; private set; }

    private void Awake()
    {
        // 씬에 이미 인스턴스가 있는지 확인
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            // 씬이 바뀌어도 이 오브젝트가 사라지지 않도록 설정
            DontDestroyOnLoad(gameObject);
        }
    }

    /// <summary>
    /// 이미지 URL을 받아 스프라이트를 반환하는 메인 함수.
    /// 캐시된 이미지가 있으면 로컬에서, 없으면 웹에서 불러옵니다.
    /// </summary>
    /// <param name="url">이미지의 웹 URL</param>
    /// <param name="onComplete">스프라이트 로딩이 완료되었을 때 호출될 콜백 함수</param>
    public void GetSprite(string url, Action<Sprite> onComplete)
    {
        StartCoroutine(LoadSpriteCoroutine(url, onComplete));
    }

    private IEnumerator LoadSpriteCoroutine(string url, Action<Sprite> onComplete)
    {
        // 1. URL을 기반으로 로컬에 저장될 고유한 파일 경로를 생성합니다.
        string localPath = GetLocalPathFromUrl(url);

        // 2. 로컬에 이미 파일이 저장되어 있는지(캐시되었는지) 확인합니다.
        if (File.Exists(localPath))
        {
            // --- 캐시 히트(Cache Hit): 로컬에서 이미지 불러오기 ---
            // Debug.Log("이미지를 캐시에서 불러옵니다: " + localPath);
            
            // 파일 데이터를 바이트 배열로 읽어옵니다.
            byte[] fileData = File.ReadAllBytes(localPath);
            
            // 텍스처를 생성하고 바이트 데이터를 로드합니다.
            // 크기는 중요하지 않으며, LoadImage가 알아서 크기를 조절합니다.
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(fileData); // 이 함수가 이미지 데이터를 텍스처로 변환합니다.

            // 텍스처로부터 스프라이트를 생성하여 콜백 함수로 전달합니다.
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            onComplete?.Invoke(sprite);
        }
        else
        {
            // --- 캐시 미스(Cache Miss): 웹에서 이미지 다운로드 ---
            // Debug.Log("이미지를 웹에서 다운로드합니다: " + url);
            
            using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    // 다운로드 성공 시
                    Texture2D texture = DownloadHandlerTexture.GetContent(www);

                    // 3. 다운로드한 이미지를 로컬에 저장(캐싱)합니다.
                    byte[] pngData = texture.EncodeToPNG();
                    File.WriteAllBytes(localPath, pngData);
                    // Debug.Log("이미지를 캐시에 저장했습니다: " + localPath);

                    // 텍스처로부터 스프라이트를 생성하여 콜백 함수로 전달합니다.
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    onComplete?.Invoke(sprite);
                }
                else
                {
                    // 다운로드 실패 시
                    Debug.LogError("이미지 다운로드 실패: " + url + "\n에러: " + www.error);
                    onComplete?.Invoke(null); // 실패했음을 알리기 위해 null 전달
                }
            }
        }
    }

    /// <summary>
    /// 이미지 URL로부터 로컬 저장 경로를 생성합니다.
    /// </summary>
    private string GetLocalPathFromUrl(string url)
    {
        // URL의 해시코드를 파일 이름으로 사용하여 고유성을 보장합니다.
        string fileName = url.GetHashCode() + ".png";
        
        // Application.persistentDataPath는 각 플랫폼(PC, 모바일 등)에서
        // 데이터를 안전하게 저장할 수 있는 고유한 경로를 반환합니다.
        return Path.Combine(Application.persistentDataPath, fileName);
    }
}
