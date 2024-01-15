using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class UITemplateData
{
    public string templateName;
    public List<UITemplateElementData> elements;
}

[System.Serializable]
public class UITemplateElementData
{
    public string elementName = "Element Name";
    public Vector3 position = Vector3.zero;
    public Vector3 rotation = Vector3.zero;
    public Vector3 scale = Vector3.one;
    public Vector2 width_height = new Vector2(100,100);

    public int parentIndex;
    public Sprite sprite;
    public string text;

    public UITemplateElementData()
    {
        parentIndex = -1;
    }
}

public class UITemplateGenerator : EditorWindow
{
    private TextAsset jsonTemplate;
    private UITemplateData templateData;
    private Vector2 scrollPosition;
    private string newTemplateName = "NewTemplate";

    [MenuItem("Window/UITemplateGenerator")]
    public static void ShowWindow()
    {
        GetWindow<UITemplateGenerator>("UITemplateGenerator");
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        GUILayout.Label("UI Template Generator", EditorStyles.boldLabel);

        jsonTemplate = EditorGUILayout.ObjectField("JSON Template", jsonTemplate, typeof(TextAsset), false) as TextAsset;

        GUILayout.Space(10);

        if (GUILayout.Button("Load Template"))
        {
            LoadTemplate();
        }

        GUILayout.Space(20);

        if (templateData != null)
        {
            GUILayout.Label("Template Properties", EditorStyles.boldLabel);

            templateData.templateName = EditorGUILayout.TextField("Template Name", templateData.templateName);

            if (templateData.elements == null)
            {
                templateData.elements = new List<UITemplateElementData>();
            }

            for (int i = 0; i < templateData.elements.Count; i++)
            {
                DisplayElementProperties(templateData.elements[i], i);
            }

            GUILayout.Space(20);
            if (GUILayout.Button("Add Element"))
            {
                templateData.elements.Add(new UITemplateElementData());
            }

            GUILayout.Space(20);
            if (GUILayout.Button("Save Template"))
            {
                SaveTemplate();
            }

            GUILayout.Space(20);
            if (GUILayout.Button("Save As New Template"))
            {
                SaveAsNewTemplate();
            }

            GUILayout.Space(20);
            if (GUILayout.Button("Instantiate Template"))
            {
                InstantiateUITemplate();
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private void DisplayElementProperties(UITemplateElementData elementData, int index)
    {
        EditorGUILayout.BeginVertical("box");

        GUILayout.Label($"Element {index + 1} Properties", EditorStyles.boldLabel);
        elementData.elementName = EditorGUILayout.TextField("Element Name", elementData.elementName);
        elementData.position = EditorGUILayout.Vector3Field("Position", elementData.position);
        elementData.rotation = EditorGUILayout.Vector3Field("Rotation", elementData.rotation);
        elementData.scale = EditorGUILayout.Vector3Field("Scale", elementData.scale);
        elementData.width_height = EditorGUILayout.Vector2Field("Width-Height", elementData.width_height);
        // Can add more UI attributes as needed

        int parentIndex = EditorGUILayout.Popup("Parent Element", elementData.parentIndex, GetElementNames());
        elementData.parentIndex = parentIndex;

        //elementData.sprite = (Sprite)EditorGUILayout.ObjectField("Sprite", elementData.sprite, typeof(Sprite), false);
        if (elementData.elementName.ToUpper().Equals("IMAGE") || elementData.elementName.ToUpper().Equals("BUTTON"))
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Image Options", EditorStyles.boldLabel);
            elementData.sprite = EditorGUILayout.ObjectField("Sprite", elementData.sprite, typeof(Sprite), false) as Sprite;
        }

        if (elementData.elementName.ToUpper().Equals("TEXT"))
        {
            GUILayout.Space(10);
            elementData.text = EditorGUILayout.TextField("Enter text here", elementData.text);
        }

        GUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Remove Element"))
        {
            templateData.elements.RemoveAt(index);
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        //EditorGUILayout.EndScrollView();
    }

    private string[] GetElementNames()
    {
        string[] elementNames = new string[templateData.elements.Count + 1];
        elementNames[0] = "None"; // No parent

        for (int i = 0; i < templateData.elements.Count; i++)
        {
            elementNames[i + 1] = templateData.elements[i].elementName;
        }

        return elementNames;
    }

    private void LoadTemplate()
    {
        if (jsonTemplate != null)
        {
            templateData = JsonUtility.FromJson<UITemplateData>(jsonTemplate.text);
        }
        else
        {
            Debug.LogError("Please assign a JSON template file.");
        }
    }

    private void SaveTemplate()
    {
        if (templateData != null)
        {
            string jsonText = JsonUtility.ToJson(templateData, true);
            string filePath = AssetDatabase.GetAssetPath(jsonTemplate);

            try
            {
                File.WriteAllText(filePath, jsonText);
                Debug.Log("Template saved successfully to: " + filePath);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error saving template: {e.Message}");
            }
        }
        else
        {
            Debug.LogError("No template loaded to save.");
        }
    }

    private void SaveAsNewTemplate()
    {
        if (templateData != null)
        {
            string newFilePath = EditorUtility.SaveFilePanel("Save Template As", "Assets", newTemplateName, "json");

            if (!string.IsNullOrEmpty(newFilePath))
            {
                string jsonText = JsonUtility.ToJson(templateData, true);

                try
                {
                    File.WriteAllText(newFilePath, jsonText);
                    Debug.Log("Template saved as a new file successfully.");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error saving template as a new file: {e.Message}");
                }
            }
        }
        else
        {
            Debug.LogError("No template loaded to save as a new file.");
        }
    }

    private void InstantiateUITemplate()
    {
        if (templateData != null)
        {
            // Create a single Canvas for all UI elements
            GameObject canvasObject = CreateCanvas();

            Dictionary<int, GameObject> instantiatedElements = new Dictionary<int, GameObject>();

            foreach (UITemplateElementData elementData in templateData.elements)
            {
                GameObject uiElement = new GameObject(elementData.elementName);
                uiElement.transform.SetParent(canvasObject.transform);

                RectTransform rectTransform = uiElement.AddComponent<RectTransform>();
                rectTransform.localPosition = elementData.position;
                rectTransform.localRotation = Quaternion.Euler(elementData.rotation);
                rectTransform.localScale = elementData.scale;
                rectTransform.sizeDelta = elementData.width_height;

                switch(elementData.elementName.ToUpper())
                {
                    case "IMAGE":
                        AddImageComponents(uiElement, elementData);
                        break;

                    case "BUTTON":
                        AddButtonComponents(uiElement);
                        break;

                    case "TEXT":
                        AddTextComponent(uiElement, elementData);
                        break;

                    case "PANEL":
                        AddPanelComponents(uiElement);
                        break;

                }
                instantiatedElements.Add(templateData.elements.IndexOf(elementData), uiElement);

                // Set the parent if there is one
                if (elementData.parentIndex >= 0 && elementData.parentIndex < templateData.elements.Count)
                {
                    int parentIndex = elementData.parentIndex - 1;
                    if (instantiatedElements.ContainsKey(parentIndex))
                    {
                        uiElement.transform.SetParent(instantiatedElements[parentIndex].transform);
                    }
                }

            }

            Debug.Log("Template instantiated successfully.");
        }
        else
        {
            Debug.LogError("No template loaded to instantiate.");
        }
    }

    private GameObject CreateCanvas()
    {
        GameObject canvasObject;

        if (FindObjectOfType<Canvas>())
        {
            canvasObject = FindObjectOfType<Canvas>().gameObject;
        }
        else
        {
            canvasObject = new GameObject("Canvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        if (!FindObjectOfType<EventSystem>())
        {
            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        return canvasObject;
    }

    private void AddImageComponents(GameObject imageObject, UITemplateElementData elementData = null)
    {
        imageObject.AddComponent<CanvasRenderer>();
        Image imageComponent = imageObject.AddComponent<Image>();
        imageComponent.sprite = elementData.sprite;
        imageComponent.SetNativeSize();

        RectTransform panelRect = imageObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
    }

    private void AddButtonComponents(GameObject buttonObject, UITemplateElementData elementData = null)
    {
        buttonObject.AddComponent<CanvasRenderer>();
        buttonObject.AddComponent<Image>();
        Button button = buttonObject.AddComponent<Button>();

        GameObject textElement = new GameObject("Text");
        RectTransform rectTransform = textElement.AddComponent<RectTransform>();

        GameObject textObj = AddTextComponent(textElement);
        textObj.transform.parent = buttonObject.transform;
        textObj.transform.localPosition = buttonObject.transform.localPosition;

        TextMeshProUGUI textTM = textObj.GetComponent<TextMeshProUGUI>();
        textTM.text = "Button";
        
        RectTransform textRect = textTM.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.pivot = new Vector2(0.5f, 0.5f);
    }

    private GameObject AddTextComponent(GameObject textObject, UITemplateElementData elementData = null)
    {
        textObject.AddComponent<CanvasRenderer>();
        TextMeshProUGUI tm = textObject.AddComponent<TextMeshProUGUI>();
        if(elementData != null)
        {
            tm.text = elementData.text;
        }

        tm.alignment = TextAlignmentOptions.Center;
        tm.color = Color.white;

        return textObject;
    }

    private void AddPanelComponents(GameObject panelObject, UITemplateElementData elementData = null)
    {
        panelObject.AddComponent<CanvasRenderer>();
        panelObject.AddComponent<Image>();

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0);
        panelRect.anchorMax = new Vector2(1, 1);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
    }
}
