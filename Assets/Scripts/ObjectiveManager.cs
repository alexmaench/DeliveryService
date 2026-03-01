using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance { get; private set; }

    public Action ObjectiveReached, ObjectiveSelected;


    [SerializeField] GameObject objectiveParent;

    List<Objective> objectiveList;

    GameObject currentObjective;

    void Start()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        Instance = this;

        objectiveList = objectiveParent
        .GetComponentsInChildren<Objective>()
        .ToList();

        TimeManager.Instance.OnTimerFinished += OnTimeEnd;
        StartCoroutine(StartNewRound());
    }

    private void OnDisable()
    {
        TimeManager.Instance.OnTimerFinished -= OnTimeEnd;
    }

    private void OnTimeEnd()
    {
        TimeManager.Instance.StopTimer();
        StartCoroutine(StartNewRound());
    }

    public void GetRandomObjective()
    {
        int i = UnityEngine.Random.Range(0, objectiveList.Count - 1);
        ActivateObjective(objectiveList[i].gameObject);
        StartCoroutine(UIManager.Instance.ShowNewObjective());
    }

    private void ActivateObjective(GameObject gameObject)
    {
        currentObjective = gameObject;
        currentObjective.SetActive(true);
        Debug.Log(currentObjective);
        currentObjective.GetComponent<Objective>().OnEnter += OnEnterExecuted;
        TimeManager.Instance.StartTimer();
    }
    private void OnEnterExecuted()
    {
        currentObjective.GetComponent<Objective>().OnEnter -= OnEnterExecuted;
        currentObjective.SetActive(false);
        currentObjective = null;
        TimeManager.Instance.StopTimer();
        StartCoroutine(StartNewRound());
    }

    IEnumerator StartNewRound()
    {
        foreach(var obj in objectiveList)
        {
            obj.gameObject.SetActive(false);
        }
        yield return new WaitForSecondsRealtime(3f);
        GetRandomObjective();
    }
}
