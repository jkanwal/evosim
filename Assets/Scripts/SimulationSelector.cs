using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SimulationSelector : MonoBehaviour
{
    public List<string> simulationNames;
    public List<string> scenes;
    public GameObject template;

    public GameObject view;

    // Start is called before the first frame update
    void Start()
    {
        generateUI();
    }

    public void generateUI()
    {
        removeUI();
        for (int i = 0; i < simulationNames.Count; i++)
        {
            generateUIElement(simulationNames[i],scenes[i]);
        }
    }

    private void generateUIElement(string simulationName, string scene)
    {
       GameObject gameObject = Instantiate(template, view.transform);
       gameObject.GetComponent<Button>().onClick.AddListener(delegate{ changeScene(scene); });
       gameObject.transform.GetChild(0).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = simulationName;
    }

    private void changeScene(string scene){
        Debug.Log(scene);
        SceneManager.LoadScene(scene, LoadSceneMode.Single);
    }
    public void removeUI()
    {
        foreach (Transform child in view.transform)
        {
            GameObject.DestroyImmediate(child.gameObject);
        }

        if (view.transform.childCount != 0)
        {
            removeUI();
        }
    }
}