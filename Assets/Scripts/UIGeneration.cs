using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using Object = System.Object;
using Toggle = UnityEngine.UI.Toggle;

public class UIGeneration : MonoBehaviour
{
    //private RunSim script;
    public GameObject optionsTemplate;
    private GameObject options;
    public GameObject simulation;
    public GameObject genome;
    //[FormerlySerializedAs("view")] public GameObject SimView;
    //public GameObject genomView;
    private GameObject SimView;
    private GameObject genomView;
    public List<string> templateNames = new List<string>();
    public List<GameObject> templates = new List<GameObject>();
    public Camera MenuCamera;
    public Camera SimCamera;

    private ArrayList arrayList = new ArrayList();
    private GameObject arrayPanel;
    private GameObject arrayPanelContent;
    // Start is called before the first frame update
    void Start()
    {
        generateUI();
        arrayPanel.SetActive(false);
    }

    public void createMenu()
    {
        if (options == null)
        {
            GameObject canvas = GameObject.Find("Canvas");
            options = Instantiate(optionsTemplate,canvas.transform).gameObject;
            /*Debug.Log(options.transform.Find("Panel").Find("StartSim").name);
            GameObject startSim = options.transform.Find("Panel").Find("StartSim").gameObject;
            Button startSimButton = startSim.GetComponent<Button>();
            startSimButton.onClick.AddListener(delegate { createSim(); });*/
        }
    }

    public void generateUI()
    {
        //createMenu();
        SimView = GameObject.Find("FirstContent").gameObject;
        genomView = GameObject.Find("SecondContent").gameObject;
        if (arrayPanel == null)
        {
            arrayPanelContent = GameObject.Find("ArrayContent").gameObject;
            arrayPanel = GameObject.Find("ArrayPanel").gameObject;
            removeUI(arrayPanelContent);
            //arrayPanel.SetActive(false);
        }

        //script = simulation.GetComponent<RunSim>();
        removeUI(SimView);
        
        try
        {
            simulation.GetComponent<RunSim>();
            //script = simulation.GetComponent<RunSim>();
            
            foreach (FieldInfo variable in typeof(RunSim).GetFields(
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public))
            {
                generateUIElement(variable, SimView, simulation.GetComponent<RunSim>());
            }
        }
        catch
        {
            removeUI(SimView);
            foreach (FieldInfo variable in typeof(RunSimVirus).GetFields(
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public))
            {
                generateUIElement(variable, SimView, simulation.GetComponent<RunSimVirus>());
            }
        }
        //script = genome.GetComponent<Genome>();
        removeUI(genomView);
        foreach (FieldInfo variable in typeof(Genome).GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public))
        {
            generateUIElement(variable,genomView,genome.GetComponent<Genome>());
        }
        
    }

    public void removeUI(GameObject view)
    {
        foreach (Transform child in view.transform) {
            GameObject.DestroyImmediate(child.gameObject);
        }

        if (view.transform.childCount != 0)
        {
            removeUI(view);
        }
    }
    public void removeUI()
    {
        removeUI(genomView);
        removeUI(SimView);
    }
    void generateUIElement(FieldInfo variable,GameObject view,Object script)
    {
        

        //Debug.Log(variable.FieldType.ToString());
        int index = templateNames.FindIndex(x => x.Equals(variable.FieldType.ToString()));
        
        if (variable.FieldType.ToString().Contains("[]"))
        {
            GameObject gameObject = Instantiate(templates[index], view.transform);
            gameObject.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = variable.Name;
            string name = gameObject.transform.GetChild(1).gameObject.name;
            TMP_InputField inputField = gameObject.transform.GetChild(1).gameObject.GetComponent<TMP_InputField>();
            int[] array = (Int32[]) variable.GetValue(script);
            inputField.text = array.Length.ToString();
            inputField.interactable = false;
            inputField.ForceLabelUpdate();
            GameObject details = gameObject.transform.Find("Details").gameObject;
            Toggle detailsToggle = details.GetComponent<Toggle>();
            arrayList.Add(array);
            int arrayIndex = arrayList.Count - 1;
            
            detailsToggle.onValueChanged.AddListener(delegate
            {
                if (detailsToggle.isOn)
                {
                    arrayPanel.SetActive(true);
                    removeUI(arrayPanelContent);
                    for (int i = 0; i < array.Length; i++)
                    {
                        generateIntTemplate(arrayIndex,i,array[i],arrayPanelContent,variable,script);
                    }
                    //fillArrayDetails(variable,script);
                    
                }
                else
                {
                    arrayPanel.SetActive(false);
                }
            });
            
            return;
        } else if (index != -1)
        {
            GameObject gameObject = Instantiate(templates[index], view.transform);
            Debug.Log(variable.Name);
            gameObject.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = variable.Name;
            string name = gameObject.transform.GetChild(1).gameObject.name;
            if (name == "InputField (TMP)")
            {
                TMP_InputField inputField = gameObject.transform.GetChild(1).gameObject.GetComponent<TMP_InputField>();
                inputField.text = variable.GetValue(script).ToString();
                inputField.ForceLabelUpdate();
                if ("System.String" == variable.FieldType.ToString())
                {
                    inputField.onValueChanged.AddListener(delegate { ValueChangeInputFieldString(variable, inputField,script); });
                    inputField.contentType = TMP_InputField.ContentType.Alphanumeric;
                }else if ("System.Int32" == variable.FieldType.ToString())
                {
                    inputField.onValueChanged.AddListener(delegate { ValueChangeInputFieldNumber(variable, inputField,script); });
                    inputField.contentType = TMP_InputField.ContentType.IntegerNumber;
                    
                }
                else
                {
                    inputField.onValueChanged.AddListener(delegate { ValueChangeInputFieldFloat(variable, inputField,script); });
                    inputField.contentType = TMP_InputField.ContentType.DecimalNumber;
                }
            }else if (name == "Toggle"){
                Toggle toggle =  gameObject.transform.GetChild(1).gameObject.GetComponent<Toggle>();
                toggle.isOn = (bool)variable.GetValue(script);
                toggle.onValueChanged.AddListener(delegate {ValueChangeToggle(variable,toggle,script); });
            }
            
        }
    }

    private void generateIntTemplate(int arrayIndex, int i, int value, GameObject view, FieldInfo variable, object script)
    {
        int index = templateNames.FindIndex(x => x.Equals("System.Int32"));
        GameObject gameObject = Instantiate(templates[index], view.transform);
        gameObject.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = i.ToString();
        TMP_InputField inputField = gameObject.transform.GetChild(1).gameObject.GetComponent<TMP_InputField>();
        inputField.text = value.ToString();
        inputField.ForceLabelUpdate();
        inputField.onValueChanged.AddListener(delegate { ValueChangeIntArray(variable, inputField,script,i,arrayIndex); });
        inputField.contentType = TMP_InputField.ContentType.IntegerNumber;
    }
    
    public void ValueChangeIntArray(FieldInfo variable,TMP_InputField inputField,Object script, int i,int arrayIndex)
    {
        int value = Int32.Parse(inputField.text);
        int[] array = (int[]) arrayList[arrayIndex];
        array[i] = value;
        arrayList[arrayIndex] = array;
        variable.SetValue(script, array);
    }
    public void ValueChangeToggle(FieldInfo variable,Toggle toggle,Object script)
    {
        variable.SetValue(script, toggle.isOn);
    }
    public void ValueChangeInputFieldString(FieldInfo variable,TMP_InputField inputField,Object script)
    {
        variable.SetValue(script, inputField.text);
    }
    public void ValueChangeInputFieldNumber(FieldInfo variable,TMP_InputField inputField,Object script)
    {
        variable.SetValue(script, Int32.Parse(inputField.text));
    }
    public void ValueChangeInputFieldFloat(FieldInfo variable,TMP_InputField inputField,Object script)
    {
        variable.SetValue(script, float.Parse(inputField.text));
    }
    public void createSim()
    {
        simulation.gameObject.SetActive(true);
        try
        {
            MenuCamera.gameObject.SetActive(false);
            SimCamera.gameObject.SetActive(true);
        }
        catch (Exception e)
        {
            
        }
        
    }
}
