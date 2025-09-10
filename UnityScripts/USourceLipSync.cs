using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using uSource;

public class USourceLipSync : MonoBehaviour
{
    [Tooltip("Renderer z blendshape’ami twarzy")]
    public SkinnedMeshRenderer faceMesh;
    [Tooltip("Źródło dźwięku dla phoneme events")]
    public AudioSource audioSource;
    [Tooltip("Czas trwania pojedynczego phoneme (w sekundach)")]
    public float phonemeDuration = 0.1f;

    // Mapa fonem → indeks blendshape’u
    private Dictionary<string, int> phonemeMap = new Dictionary<string, int>();
    // Lista zdarzeń z SoundScriptLoader
    public List<SoundScriptLoader.PhonemeEvent> PhonemeEvents { get; set; }

    // Wskaźnik na następną niewykonaną komendę
    private int nextEventIndex = 0;

    void Awake()
    {
        // Autopodpinanie komponentów
        if (faceMesh == null) faceMesh = GetComponent<SkinnedMeshRenderer>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        // Budujemy mapę blendshape’ów
        for (int i = 0; i < faceMesh.sharedMesh.blendShapeCount; i++)
        {
            string name = faceMesh.sharedMesh.GetBlendShapeName(i).ToLower();
            if (name.Contains("aa")) phonemeMap["AA"] = i;
            else if (name.Contains("ee")) phonemeMap["E"] = i;
            else if (name.Contains("ih")) phonemeMap["I"] = i;
            else if (name.Contains("oh")) phonemeMap["O"] = i;
            else if (name.Contains("ou")) phonemeMap["U"] = i;
            else if (name.Contains("mb")) phonemeMap["MBP"] = i;
            else if (name.Contains("fv")) phonemeMap["FV"] = i;
            else if (name.Contains("l"))  phonemeMap["L"] = i;
            else if (name.Contains("w"))  phonemeMap["W"] = i;
        }
    }

    void Update()
    {
        // Jeśli nie ma zdarzeń lub audio nie gra, nic nie robimy
        if (PhonemeEvents == null || nextEventIndex >= PhonemeEvents.Count || !audioSource.isPlaying)
            return;

        // Przetwarzamy wszystkie zdarzenia, których time <= currentTime
        float currentTime = audioSource.time;
        while (nextEventIndex < PhonemeEvents.Count && PhonemeEvents[nextEventIndex].time <= currentTime)
        {
            var evt = PhonemeEvents[nextEventIndex];
            StartCoroutine(PlaySinglePhoneme(evt.phoneme));
            nextEventIndex++;
        }
    }

    /// <summary>
    /// Uruchamia blendshape dla pojedynczego phonemu, potem cofa po upływie phonemeDuration.
    /// </summary>
    private IEnumerator PlaySinglePhoneme(string phoneme)
    {
        if (phonemeMap.TryGetValue(phoneme, out int idx))
        {
            faceMesh.SetBlendShapeWeight(idx, 100f);
            yield return new WaitForSeconds(phonemeDuration);
            faceMesh.SetBlendShapeWeight(idx, 0f);
        }
        yield break;
    }

    /// <summary>
    /// Wywołaj to po załadowaniu PhonemeEvents i przygotowaniu audioSource.clip.
    /// Od razu zaczyna odtwarzanie i resetuje licznik zdarzeń.
    /// </summary>
    public void Initialize()
    {
        nextEventIndex = 0;
        if (audioSource.clip != null)
            audioSource.Play();
    }
}
