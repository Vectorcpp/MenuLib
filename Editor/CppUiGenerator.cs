// Created by Vector_cpp Have fun!
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class CppUiCodeGenerator
{
    private const string version = "v1.1";

    [MenuItem("Tools/C++ Converter for RootObjects", true)]
    private static bool ValidateGenerateCode() => Selection.gameObjects.Length > 0;

    [MenuItem("Tools/C++ Converter for RootObjects", false, 0)]
    private static void GenerateCode()
    {
        GameObject[] selected = Selection.gameObjects;
        if (selected.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", "Select at least one GameObject.", "OK");
            return;
        }

        var sb = new StringBuilder();
        var buttons = new List<string>();
        var buttonTexts = new List<string>();
        string titleTextVar = "";

        sb.AppendLine("void BaseMenu::Initialize(Transform* parent) {");

        foreach (var root in selected)
        {
            if (root != null)
                ProcessTransform(root.transform, "parent", 1, sb, buttons, buttonTexts, ref titleTextVar);
        }

        sb.AppendLine();

        if (buttons.Count > 0)
            sb.AppendLine("    buttonObjects = { " + string.Join(", ", buttons) + " };");

        if (buttonTexts.Count > 0)
            sb.AppendLine("    buttonTexts = { " + string.Join(", ", buttonTexts) + " };");

        if (!string.IsNullOrEmpty(titleTextVar))
            sb.AppendLine($"    titleText = {titleTextVar};");

        sb.AppendLine("    pages = GetMenuDefinition();");
        sb.AppendLine("    CreateHandCollider();");
        sb.AppendLine();
        sb.AppendLine("    staticButtons = {");
        sb.AppendLine("        { GameObject::Find(\"PageLeft\"),  [this] { PreviousPage(); } },");
        sb.AppendLine("        { GameObject::Find(\"PageRight\"), [this] { NextPage(); } },");
        sb.AppendLine("        { GameObject::Find(\"Disconnect\"), MenuActions::Disconnect },");
        sb.AppendLine("        { GameObject::Find(\"Discord\"),    MenuActions::OpenDiscord }");
        sb.AppendLine("    };");
        sb.AppendLine();
        sb.AppendLine("    if (!pages.empty())");
        sb.AppendLine("        LoadPage(currentPage);");
        sb.AppendLine("}");

        EditorGUIUtility.systemCopyBuffer = sb.ToString();
        EditorUtility.DisplayDialog("Success!", "C++ code copied to clipboard with button + text references.", "Nice!");
    }

    private static void ProcessTransform(Transform target, string parent, int indentLevel, StringBuilder sb, List<string> buttons, List<string> buttonTexts, ref string titleTextVar)
    {
        string indent = new string(' ', indentLevel * 4);
        GameObject go = target.gameObject;
        string cppVar = Sanitize(go.name) + "_" + Mathf.Abs(go.GetInstanceID());

        sb.AppendLine($"{indent}// --- {go.name} ---");
        sb.AppendLine($"{indent}GameObject* {cppVar} = GameObject::CreatePrimitive(PrimitiveType::Cube);");
        sb.AppendLine($"{indent}{cppVar}->SetName(\"{go.name}\");");

        if (go.layer != 0)
            sb.AppendLine($"{indent}{cppVar}->SetLayer({go.layer});");

        sb.AppendLine($"{indent}auto {cppVar}_transform = {cppVar}->GetTransform();");
        sb.AppendLine($"{indent}{cppVar}_transform->SetParent({parent}, false);");
        sb.AppendLine($"{indent}{cppVar}_transform->SetLocalPosition({ToVec3(target.localPosition)});");
        sb.AppendLine($"{indent}{cppVar}_transform->SetLocalRotation({ToQuat(target.localRotation)});");
        sb.AppendLine($"{indent}{cppVar}_transform->SetLocalScale({ToVec3(target.localScale)});");

        RectTransform rt = go.GetComponent<RectTransform>();
        if (rt)
            sb.AppendLine($"{indent}((RectTransform*){cppVar}_transform)->SetSizeDelta({ToVec2(rt.sizeDelta)});");

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

        sb.AppendLine($"{indent}if (auto col = {cppVar}->GetComponent(Collider::GetType())) {{");
        sb.AppendLine($"{indent}    auto triggerField = col->GetClass().GetField(\"IsTrigger\");");
        sb.AppendLine($"{indent}    if (triggerField.IsValid()) {{");
        sb.AppendLine($"{indent}        triggerField.SetInstance(col);");
        sb.AppendLine($"{indent}        *triggerField.cast<bool*>() = false;");
        sb.AppendLine($"{indent}    }}");
        sb.AppendLine($"{indent}}}");

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
