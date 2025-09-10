using System;
using System.Collections.Generic;
using UnityEngine;
using uSource;  // przestrzeń nazw z MDLLoader i CommandLineArgs

[RequireComponent(typeof(Transform))]
public class MDLImporter : MonoBehaviour
{
    [Tooltip("Ścieżka do pliku .mdl, np. \"models/player.mdl\"")]
    public string mdlPath;

    [Tooltip("Czy parsować flexy (blendshape'y)?")]
    public bool parseFlexes = true;

    [Tooltip("Automatycznie ładuje model przy starcie")]
    public bool loadOnStart = true;

    // Referencja do aktualnie wczytanego GameObjecta
    private GameObject importedModel;

    void Start()
    {
        if (loadOnStart)
            LoadModel();
    }

    /// <summary>
    /// Ładuje plik .mdl przy użyciu MDLLoader, ustawia parseFlexes przez CommandLineArgs,
    /// niszczy ewentualny poprzedni import i parentuje nowy pod ten GameObject.
    /// </summary>
    public void LoadModel()
    {
        if (string.IsNullOrEmpty(mdlPath))
        {
            Debug.LogError("[MDLImporter] Nie podano ścieżki do modelu!", this);
            return;
        }

        // Ustawiamy convar do parsowania flexów
        CommandLineArgs.Args["parseflexes"] = parseFlexes.ToString().ToLower();

        // Czyszczenie starego modelu
        if (importedModel != null)
            Destroy(importedModel);

        // Właściwe ładowanie
        importedModel = MDLLoader.LoadModel(mdlPath);
        if (importedModel != null)
        {
            importedModel.transform.SetParent(transform, false);
            Debug.Log($"[MDLImporter] Zaimportowano: {mdlPath}", this);
        }
        else
        {
            Debug.LogError($"[MDLImporter] Błąd ładowania: {mdlPath}", this);
        }
    }

    /// <summary>
    /// Importuje model i zwraca instancję GameObjecta.
    /// </summary>
    public GameObject ImportToUnity()
    {
        LoadModel();
        return importedModel;
    }

    /// <summary>
    /// Zapisuje przekazane klucz=wartość do CommandLineArgs i aktualizuje właściwości skryptu.
    /// </summary>
    /// <param name="args">Słownik konsolek, np. { "parseflexes": "false", "someFlag": "value" }</param>
    public void ApplyConVars(Dictionary<string, string> args)
    {
        foreach (var kv in args)
        {
            // Ustaw konsole globalnie
            CommandLineArgs.Args[kv.Key] = kv.Value;

            // Jeśli dotyczy parseFlexes, zaktualizuj też pole
            if (kv.Key.Equals("parseflexes", StringComparison.OrdinalIgnoreCase)
                && bool.TryParse(kv.Value, out bool pf))
            {
                parseFlexes = pf;
            }
        }
    }
}
