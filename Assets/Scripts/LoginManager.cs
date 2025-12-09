using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private string nextSceneName = "Humanoid";
    [Header("UI")]
    [SerializeField]
    private TextMeshProUGUI status_txt;
    [SerializeField]
    private GameObject loginPanel;
    [Header("Fake Login Results")]
    [Tooltip("toggle for sucsess/ failure")]
    [SerializeField]
    private bool isSimulateSucsess = true;


    public void SignedWithAppleClick()
    {

        Debug.Log(isSimulateSucsess);
        if (isSimulateSucsess)
        {
            if (status_txt != null)
                status_txt.text = "";

            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            if(status_txt!=null)
            status_txt.text = "Invaild login";
        }
    }
}
