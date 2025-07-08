using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class CppUiCodeGeneratorWindow : EditorWindow
{
    private Vector2 scroll;
    private GameObject[] selectedObjects;

    private bool isOption1On = false;

    [MenuItem("Window/Cpp UI Code Generator")]
    public static void ShowWindow()
    {
        GetWindow<CppUiCodeGeneratorWindow>("C++ UI Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("C++ UI Code Generator", EditorStyles.boldLabel);
        GUILayout.Space(5);

        if (GUILayout.Button("Refresh Selection"))
        {
            selectedObjects = Selection.gameObjects;
        }

        GUILayout.Space(15);

        GUILayout.Label("---- Options (WIP) ----", EditorStyles.boldLabel);

        GUIContent toggleOptions = new GUIContent(text:"Is this for the menu lib?");

        isOption1On = GUILayout.Toggle(isOption1On, toggleOptions);

        GUILayout.Space(5);

        if (selectedObjects == null || selectedObjects.Length == 0)
        {
            EditorGUILayout.HelpBox("No GameObjects selected. Select root objects in the Hierarchy, then click 'Refresh Selection'.", MessageType.Warning);
        }
        else
        {
            GUILayout.Label($"Selected GameObjects ({selectedObjects.Length}):");
            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(100));
            foreach (var go in selectedObjects)
                EditorGUILayout.LabelField(go.name);
            EditorGUILayout.EndScrollView();
        }

        GUI.enabled = selectedObjects != null && selectedObjects.Length > 0;

        if (GUILayout.Button("Generate C++ Code"))
        {
            GenerateCode(selectedObjects);
        }

        GUI.enabled = true;
    }

    private void GenerateCode(GameObject[] selected)
    {
        var sb = new StringBuilder();
        var buttons = new List<string>();
        var buttonTexts = new List<string>();
        string titleTextVar = "";

        if (isOption1On)
        {
            sb.AppendLine("void BaseMenu::Initialize(Transform* parent) {");

            foreach (var root in selected)
            {
                if (root != null)
                    ProcessTransform(root.transform, "parent", 1, sb, buttons, buttonTexts, ref titleTextVar);
            }

            sb.AppendLine();

            if (buttons.Count > 0)
                sb.AppendLine("    buttonObjects = { " + string.Join(", ", buttons) + " };\n");

            if (buttonTexts.Count > 0)
                sb.AppendLine("    buttonTexts = { " + string.Join(", ", buttonTexts) + " };\n");

            if (!string.IsNullOrEmpty(titleTextVar))
                sb.AppendLine($"    titleText = {titleTextVar};\n");

            sb.AppendLine("    pages = GetMenuDefinition();");
            sb.AppendLine("    CreateHandCollider();\n");
            sb.AppendLine("    // ADD YOUR OWN PAGE NAMES!");
            sb.AppendLine("    staticButtons = {");
            sb.AppendLine("        { GameObject::Find(\"PageLeft\"),  [this] { PreviousPage(); } },");
            sb.AppendLine("        { GameObject::Find(\"PageRight\"), [this] { NextPage(); } },");
            sb.AppendLine("        { GameObject::Find(\"Disconnect\"), MenuActions::Disconnect },");
            sb.AppendLine("        { GameObject::Find(\"Discord\"),    MenuActions::OpenDiscord }");
            sb.AppendLine("    };\n");

            sb.AppendLine("    if (!pages.empty())");
            sb.AppendLine("        LoadPage(currentPage);");
            sb.AppendLine("}");

            EditorGUIUtility.systemCopyBuffer = sb.ToString();
            EditorUtility.DisplayDialog("Success!", "C++ code copied to clipboard.", "Nice!");
        }
        else
        {
            foreach (var root in selected)
            {
                if (root != null)
                    ProcessTransform(root.transform, "parent", 1, sb, buttons, buttonTexts, ref titleTextVar);
            }

            EditorUtility.DisplayDialog("Success!", "This only generated properties NOT the menu lib.", "Nice!");
        }
    }

    private static void ProcessTransform(Transform target, string parent, int indentLevel, StringBuilder sb, List<string> buttons, List<string> buttonTexts, ref string titleTextVar)
    {
        string indent = new string(' ', indentLevel * 4);
        GameObject go = target.gameObject;
        string cppVar = Sanitize(go.name) + "_" + Mathf.Abs(go.GetInstanceID());

        sb.AppendLine($"{indent}// --- {go.name} ---");

        if (go.GetComponent<Canvas>())
        {
            sb.AppendLine($"{indent}GameObject* {cppVar} = (GameObject*)GameObject::GetClass().CreateNewObjectParameters();");
            sb.AppendLine($"{indent}{cppVar}->SetName(\"{go.name}\");");
            sb.AppendLine($"{indent}{cppVar}->AddComponent(Canvas::GetType());");
            sb.AppendLine($"{indent}((Canvas*){cppVar}->GetComponent(Canvas::GetType()))->SetRenderMode(RenderMode::WorldSpace);");
            sb.AppendLine($"{indent}((Canvas*){cppVar}->GetComponent(Canvas::GetType()))->SetWorldCamera(Camera::GetMain());");
        }
        else if (go.GetComponent<Text>() || go.GetComponent<Image>())
        {
            sb.AppendLine($"{indent}GameObject* {cppVar} = (GameObject*)GameObject::GetClass().CreateNewObjectParameters();");
            sb.AppendLine($"{indent}{cppVar}->SetName(\"{go.name}\");");
            sb.AppendLine($"{indent}{cppVar}->AddComponent(Text::GetType());");
        }
        else
        {
            if (go.GetComponent<Renderer>())
            {
                sb.AppendLine($"{indent}GameObject* {cppVar} = GameObject::CreatePrimitive(PrimitiveType::Cube);");
                sb.AppendLine($"{indent}{cppVar}->SetName(\"{go.name}\");");
            }
            else
            {
                sb.AppendLine($"{indent}GameObject* {cppVar} = (GameObject*)GameObject::GetClass().CreateNewObjectParameters();");
                sb.AppendLine($"{indent}{cppVar}->SetName(\"{go.name}\");");
            }
        }

        sb.AppendLine($"{indent}auto {cppVar}_transform = {cppVar}->GetTransform();");
        sb.AppendLine($"{indent}{cppVar}_transform->SetParent({parent}, false);");
        sb.AppendLine($"{indent}{cppVar}_transform->SetLocalPosition({ToVec3(target.localPosition)});");
        sb.AppendLine($"{indent}{cppVar}_transform->SetLocalRotation({ToQuat(target.localRotation)});");
        sb.AppendLine($"{indent}{cppVar}_transform->SetLocalScale({ToVec3(target.localScale)});");

        RectTransform rt = go.GetComponent<RectTransform>();
        if (rt)
            sb.AppendLine($"{indent}((RectTransform*){cppVar}_transform)->SetSizeDelta({ToVec2(rt.sizeDelta)});");

        var renderer = go.GetComponent<Renderer>();
        if (renderer && renderer.sharedMaterial != null)
        {
            sb.AppendLine($"");
            sb.AppendLine($"{indent}Renderer* {cppVar}_renderer = reinterpret_cast<Renderer*>({cppVar}_renderer->GetComponent(Renderer::GetType()));");
            sb.AppendLine($"{indent}{cppVar}_renderer->SetMaterial({cppVar}_renderer->GetMaterial());");
        }


        Text text = go.GetComponent<Text>();
        if (text)
        {
            string textVar = cppVar + "_text";
            sb.AppendLine($"{indent}auto {textVar} = (Text*){cppVar}->AddComponent(Text::GetType());");
            sb.AppendLine($"{indent}{textVar}->SetText(\"{Escape(text.text)}\");");
            sb.AppendLine($"{indent}{textVar}->SetFontSize({text.fontSize});");
            sb.AppendLine($"{indent}{textVar}->SetAlignment(TextAnchor::{text.alignment});");
            sb.AppendLine($"{indent}{textVar}->SetFontStyle(FontStyle::{text.fontStyle});");
            sb.AppendLine($"{indent}{textVar}->SetColor({ToColor(text.color)});");

            if (text.font != null)
            {
                string fontName = text.font.name;
                if (fontName.ToLower() == "arial")
                    sb.AppendLine($"{indent}{textVar}->SetFont((Font*)Resources::GetBuiltinResource(Font::GetType(), \"Arial.ttf\"));");
                else
                    sb.AppendLine($"{indent}{textVar}->SetFont(FindFontByName(\"{fontName}\"));");
            }

            if (go.name.ToLower().Contains("text"))
                buttonTexts.Add(textVar);

            if (go.name.ToLower().Contains("title") || go.name.ToLower().Contains("version"))
                titleTextVar = textVar;
        }

        Image image = go.GetComponent<Image>();
        if (image)
        {
            sb.AppendLine($"// Hey! quick heads up there is NO image's cause idk how to load assets it will just be a base gameobject for now");
            sb.AppendLine($"{indent}GameObject* {cppVar}_image = (GameObject*)GameObject::GetClass().CreateNewObjectParameters();");
        }

        if (go.GetComponent<BoxCollider>() || go.GetComponent<SphereCollider>() || go.GetComponent<MeshCollider>())
        {
            sb.AppendLine();
            sb.AppendLine($"{indent}if (auto col = {cppVar}->GetComponent(Collider::GetType())) {{");
            sb.AppendLine($"{indent}    auto triggerField = col->GetClass().GetField(\"IsTrigger\");");
            sb.AppendLine($"{indent}    if (triggerField.IsValid()) {{");
            sb.AppendLine($"{indent}        triggerField.SetInstance(col);");
            sb.AppendLine($"{indent}        *triggerField.cast<bool*>() = false;");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine($"{indent}}}");
        }

        if (go.name.ToLower().Contains("button"))
            buttons.Add(cppVar);

        foreach (Transform child in target)
            ProcessTransform(child, cppVar + "_transform", indentLevel + 1, sb, buttons, buttonTexts, ref titleTextVar);
    }

    private static string Sanitize(string name)
    {
        string clean = Regex.Replace(name, @"[^a-zA-Z0-9_]", "_");
        return string.IsNullOrEmpty(clean) || char.IsDigit(clean[0]) ? "_" + clean : clean;
    }

    private static string Escape(string s) => s.Replace("\"", "\\\"").Replace("\n", "\\n");
    private static string ToVec2(Vector2 v) => $"Vector2({v.x}, {v.y})";
    private static string ToVec3(Vector3 v) => $"Vector3({v.x}, {v.y}, {v.z})";
    private static string ToQuat(Quaternion q) => $"Quaternion({q.x}, {q.y}, {q.z}, {q.w})";
    private static string ToColor(Color c) => $"Color({c.r}, {c.g}, {c.b}, {c.a})";
}
