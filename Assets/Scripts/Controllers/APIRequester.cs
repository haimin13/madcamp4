using System.Collections;
using System.Collections.Generic;
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
        baseUrl = "http://localhost:8000";
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
}
