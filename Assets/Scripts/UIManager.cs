using System.Collections;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] GameObject NewObjective;

    void Start()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        Instance = this;
    }

    public IEnumerator ShowNewObjective()
    {
        NewObjective.SetActive(true);
        yield return new WaitForSecondsRealtime(2f);
        NewObjective.SetActive(false);
    }
}
