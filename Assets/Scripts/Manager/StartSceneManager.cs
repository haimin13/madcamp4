using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

public class StartSceneManager : MonoBehaviour
{
    public Button loginButton;
    // Start is called before the first frame update
    void Start()
    {
        loginButton.onClick.AddListener(Login);
    }

    // Update is called once per frame
    void Update()
    {

    }
    void Login()
    {
        // api 요청
        StartCoroutine(APIRequester.Instance.SendJsonRequest("/login", "POST", null, null, (response) =>
        {
            SceneManager.LoadScene("ChampCreateScene");
        }, (error) =>
        {
            
        }));
    }
}
