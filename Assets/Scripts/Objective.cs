using System;
using UnityEngine;

public class Objective : MonoBehaviour
{
    public Action OnEnter;

    private void OnTriggerEnter(Collider other)
    {
        OnEnter?.Invoke();
    }
}
