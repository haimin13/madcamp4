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
        loginButton.onClick.AddListener(StartButtonClicked);
    }

    // Update is called once per frame
    void Update()
    {

    }
    void StartButtonClicked()
    {
        SceneManager.LoadScene("ChampCreateScene");
    }
}
