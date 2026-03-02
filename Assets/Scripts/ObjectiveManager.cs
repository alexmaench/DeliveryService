using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance { get; private set; }

    public Action ObjectiveReached, ObjectiveSelected;


    [SerializeField] GameObject objectiveParent;

    List<Objective> objectiveList;

    GameObject currentObjective;

    [Header("Referencias UI (TMP)")]
    [SerializeField] private TMP_Text textoTiempo; // El que tiene formato 00:00
    [SerializeField] private TMP_Text textoScore;  // El contador 0000000

    [Header("Configuración Animación")]
    [SerializeField] private float duracionMovimiento = 1.5f;
    [SerializeField] private float escalaFinalMultiplier = 2.0f; // Cuánto se agrandan (2x)
    [SerializeField] private float separacionVerticalFinal = 50f; // Espacio entre ellos en el centro

    [Header("Configuración Conteo")]
    [SerializeField] private int puntosPorSegundo = 100;
    [SerializeField] private float tiempoEntrePuntos = 0.05f; // Velocidad del "tick" del contador

    // Variables para guardar el estado original
    private Vector2 posOriginalTiempo, posOriginalScore;
    private Vector3 escalaOriginalTiempo, escalaOriginalScore;
    private bool estaAnimando = false;

    // Puntuación lógica interna (para no depender solo del texto)
    private long puntuacionActual = 0;

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

        if (long.TryParse(textoScore.text, out long scoreInicial))
        {
            puntuacionActual = scoreInicial;
        }

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
        currentObjective = null;
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
        TimeManager.Instance.StopTimer();
        EjecutarSecuenciaFinal();
        StartCoroutine(StartNewRound());
    }

    IEnumerator StartNewRound()
    {
        foreach(var obj in objectiveList)
        {
            obj.gameObject.SetActive(false);
        }
        if(currentObjective == null) yield return new WaitForSecondsRealtime(2f);
        else yield return new WaitForSecondsRealtime(10f);
        GetRandomObjective();
    }

    public void EjecutarSecuenciaFinal()
    {
        if (estaAnimando) return;
        StartCoroutine(CorrutinaSecuenciaCompleta());
    }

    private IEnumerator CorrutinaSecuenciaCompleta()
    {
        estaAnimando = true;

        posOriginalTiempo = textoTiempo.rectTransform.anchoredPosition;
        posOriginalScore = textoScore.rectTransform.anchoredPosition;
        escalaOriginalTiempo = textoTiempo.transform.localScale;
        escalaOriginalScore = textoScore.transform.localScale;

        Vector2 centroPantalla = Vector2.zero;
        Vector2 destinoTiempo = centroPantalla + new Vector2(0, separacionVerticalFinal / 2f);
        Vector2 destinoScore = centroPantalla + new Vector2(0, -separacionVerticalFinal);
        Vector3 escalaObjetivo = escalaOriginalTiempo * escalaFinalMultiplier;

        yield return StartCoroutine(CorrutinaTraslacionYEscala(destinoTiempo, destinoScore, escalaObjetivo, true));

        yield return StartCoroutine(CorrutinaConvertirTiempoAScore());

        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(CorrutinaTraslacionYEscala(posOriginalTiempo, posOriginalScore, escalaOriginalTiempo, false));

        estaAnimando = false;
    }

    private IEnumerator CorrutinaTraslacionYEscala(Vector2 descTiempo, Vector2 descScore, Vector3 escObjetivo, bool haciaElCentro)
    {
        float tiempoTranscurrido = 0;

        Vector2 iniPosTiempo = textoTiempo.rectTransform.anchoredPosition;
        Vector2 iniPosScore = textoScore.rectTransform.anchoredPosition;
        Vector3 iniEscTiempo = textoTiempo.transform.localScale;
        Vector3 iniEscScore = textoScore.transform.localScale;

        while (tiempoTranscurrido < duracionMovimiento)
        {
            tiempoTranscurrido += Time.deltaTime;
            float t = tiempoTranscurrido / duracionMovimiento;

            t = Mathf.SmoothStep(0, 1, t);

            textoTiempo.rectTransform.anchoredPosition = Vector2.Lerp(iniPosTiempo, descTiempo, t);
            textoScore.rectTransform.anchoredPosition = Vector2.Lerp(iniPosScore, descScore, t);

            textoTiempo.transform.localScale = Vector3.Lerp(iniEscTiempo, escObjetivo, t);
            textoScore.transform.localScale = Vector3.Lerp(iniEscScore, escObjetivo, t);

            yield return null;
        }

        textoTiempo.rectTransform.anchoredPosition = descTiempo;
        textoScore.rectTransform.anchoredPosition = descScore;
        textoTiempo.transform.localScale = escObjetivo;
        textoScore.transform.localScale = escObjetivo;
    }

    private IEnumerator CorrutinaConvertirTiempoAScore()
    {
        string stringTiempo = textoTiempo.text;
        int segundosTotales = 0;

        string[] partes = stringTiempo.Split(':');
        if (partes.Length == 2)
        {
            if (int.TryParse(partes[0], out int minutos) && int.TryParse(partes[1], out int segundos))
            {
                segundosTotales = (minutos * 60) + segundos;
            }
        }
        else
        {
            Debug.LogError($"Formato de tiempo inválido en textoTiempo: {stringTiempo}. Use MM:SS");
            yield break;
        }

        Debug.Log($"Segundos a convertir: {segundosTotales}. Puntos a añadir: {segundosTotales * puntosPorSegundo}");

        int segundosRestantes = segundosTotales;

        while (segundosRestantes > 0)
        {
            segundosRestantes--;
            puntuacionActual += puntosPorSegundo;

            textoTiempo.text = FormatearTiempo(segundosRestantes);
            ActualizarTextoScore(puntuacionActual);


            yield return new WaitForSeconds(tiempoEntrePuntos);
        }

        textoTiempo.text = "00:00";
    }

    private string FormatearTiempo(int segundosTotales)
    {
        int minutos = segundosTotales / 60;
        int segundos = segundosTotales % 60;
        return string.Format("{0:00}:{1:00}", minutos, segundos);
    }

    private void ActualizarTextoScore(long score)
    {
        textoScore.text = score.ToString("D7", CultureInfo.InvariantCulture);
    }
}
