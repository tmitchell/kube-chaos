using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class StartScene : MonoBehaviour
{
    public TMP_InputField namespaceInput;
    public TMP_InputField contextInput;
    public TMP_InputField kubectlExecutableInput;

    private void Start()
    {
        namespaceInput.text = "structsure";
        kubectlExecutableInput.text = "/usr/bin/kubectl";
        contextInput.text = "default";
    }

    public void StartGame()
    {
        if (string.IsNullOrEmpty(namespaceInput.text) 
            || string.IsNullOrEmpty(contextInput.text)
            || string.IsNullOrEmpty(kubectlExecutableInput.text))
        {
            return;
        }

        KubeManager.Instance.kubeContextName = contextInput.text;
        KubeManager.Instance.kubectlExecutableName = kubectlExecutableInput.text;
        KubeManager.Instance.kubeNamespace = namespaceInput.text;
        KubeManager.Instance.FetchNodeList();
        KubeManager.Instance.gameStarted = true;
        
        SceneManager.LoadScene("KubeMainScene");
    }

    public void Update()
    {
        if (Input.GetButton("Submit"))
        {
            StartGame();
        }
    }
}
