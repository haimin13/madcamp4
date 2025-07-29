using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using Newtonsoft.Json;


public class APIRequester : MonoBehaviour
{
    public static APIRequester Instance { get; private set; }
    public string baseUrl;
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
        baseUrl = "https://airouge.yyacht.camp";
        //baseUrl = "http://localhost:8000";
    }

    // Update is called once per frame
    void Update()
    {

    }
    public IEnumerator SendJsonRequest(
        string api,
        string method, // "GET", "POST", "PUT" 등
        string json = null,
        Dictionary<string, string> queryParams = null, // 쿼리 파라미터 추가
        System.Action<string> onSuccess = null,
        System.Action<string> onError = null)
    {
        Debug.Log("요청보낸다");
        // 쿼리 문자열 빌드
        string queryString = "";
        if (queryParams != null && queryParams.Count > 0)
        {
            StringBuilder sb = new StringBuilder("?");
            foreach (var pair in queryParams)
            {
                sb.Append(UnityWebRequest.EscapeURL(pair.Key));
                sb.Append("=");
                sb.Append(UnityWebRequest.EscapeURL(pair.Value));
                sb.Append("&");
            }
            sb.Length--; // 마지막 & 제거
            queryString = sb.ToString();
        }
        string url = baseUrl + api + queryString;
        Debug.Log($"Request: [{method}] {url}");
        if (!string.IsNullOrEmpty(json))
            Debug.Log("RequestBody: " + json);

        UnityWebRequest request;

        if (method.ToUpper() == "GET")
        {
            request = UnityWebRequest.Get(url);
        }
        else
        {
            request = new UnityWebRequest(url, method.ToUpper());
            if (!string.IsNullOrEmpty(json))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            }
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
        }

        yield return request.SendWebRequest();

        Debug.Log("ResponseBody: " + request.downloadHandler.text);

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Response: " + request.downloadHandler.text);
            onSuccess?.Invoke(request.downloadHandler.text);
        }
        else
        {
            Debug.Log($"{method} Failed: {request.error}");
            onError?.Invoke(request.error);
        }
    }


    #region 이미지 다운로드 및 캐싱

    /// <summary>
    /// 이미지 URL을 받아 스프라이트를 반환합니다. 캐시된 이미지를 우선 사용합니다.
    /// </summary>
    /// <param name="url">이미지의 전체 웹 URL</param>
    /// <param name="onComplete">스프라이트 로딩이 완료되었을 때 호출될 콜백</param>
    public void GetSprite(string url, Action<Sprite> onComplete)
    {
        // URL이 비어있으면 아무 작업도 하지 않음
        if (string.IsNullOrEmpty(url))
        {
            onComplete?.Invoke(null);
            return;
        }

        // 상대 경로인 경우 (예: /static/images/...), baseUrl을 붙여줌
        if (url.StartsWith("/"))
        {
            url = baseUrl + url;
        }

        StartCoroutine(LoadSpriteCoroutine(url, onComplete));
    }

    private IEnumerator LoadSpriteCoroutine(string url, Action<Sprite> onComplete)
    {
        string localPath = GetLocalPathFromUrl(url);

        if (File.Exists(localPath))
        {
            // 캐시에서 이미지 불러오기
            byte[] fileData = File.ReadAllBytes(localPath);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(fileData);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            onComplete?.Invoke(sprite);
        }
        else
        {
            // 웹에서 이미지 다운로드
            using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(www);
                    byte[] pngData = texture.EncodeToPNG();
                    File.WriteAllBytes(localPath, pngData); // 로컬에 저장 (캐싱)

                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    onComplete?.Invoke(sprite);
                }
                else
                {
                    Debug.LogError($"이미지 다운로드 실패: {url}\n에러: {www.error}");
                    onComplete?.Invoke(null);
                }
            }
        }
    }

    private string GetLocalPathFromUrl(string url)
    {
        string fileName = url.GetHashCode() + ".png";
        return Path.Combine(Application.persistentDataPath, fileName);
    }

    #endregion
}

