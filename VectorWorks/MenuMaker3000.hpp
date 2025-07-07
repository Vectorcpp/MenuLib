#pragma once

#include <vector>
#include <string>
#include <functional>
#include <unordered_map>
#include "../Include/BNMResolve.hpp"
#include "MenuDef.h"

using namespace BNM;
using namespace Structures::Unity;
using namespace Structures::Mono;

namespace VectorWorks::MenuMaker {


    // A Nice Helper function if you want to find a font byitself, you can but is not neeeded
    inline Font* FindFontByName(const std::string& name) {
        auto fonts = (Array<Font*>*)Resources::FindObjectsOfTypeAll(Font::GetType());
        if (!fonts) return nullptr;

        for (int i = 0; i < fonts->GetSize(); i++) {
            Font* font = fonts->operator[](i);
            if (font && font->GetName() == name) return font;
        }

        // Fallback to Arial
        return (Font*)Resources::GetBuiltinResource(Font::GetType(), "Arial.ttf");
    }

    struct MenuButton {
        GameObject* buttonObject = nullptr;
        Text* textComponent = nullptr;
        ButtonInfo config;
        bool isToggled = false;
        bool wasPressedLastFrame = false;
    };

    struct StaticButton {
        GameObject* buttonObject = nullptr;
        std::function<void()> callback = nullptr;
        bool wasPressedLastFrame = false;
    };

    class BaseMenu {
    public:
        //Declaring Voids here

        void Initialize(Transform* parent);
        void Update();
        void NextPage();
        void PreviousPage();

    private:
        //Just some built in fields

        void SetColor(GameObject* obj, const Color& color);
        void CreateHandCollider();
        void LoadPage(int index);

        GameObject* baseMenuObject = nullptr;
        GameObject* leftHandCollider = nullptr;

        Text* titleText = nullptr;

        std::vector<MenuButton> currentButtons;
        std::vector<StaticButton> staticButtons;
        std::vector<PageInfo> pages;
        std::unordered_map<std::string, bool> toggleStates;

        std::vector<GameObject*> buttonObjects;
        std::vector<Text*> buttonTexts;

        int currentPage = 0;

        //Base Colors
        // You can set these however you need!

        const Color backgroundColor = Color(0.1f, 0.1f, 0.1f, 0.7f);
        const Color surroundColor = Color(0.8f, 0.8f, 0.0f, 1.0f);
        const Color buttonDefault = Color(0.2f, 0.2f, 0.2f, 1.0f);
        const Color buttonToggled = Color(0.0f, 0.5f, 0.0f, 1.0f);
        const Color buttonPressed = Color(0.4f, 0.4f, 0.4f, 1.0f);
    };
    // ---- Implementation ----

    void BaseMenu::SetColor(GameObject* obj, const Color& color) {
        if (!obj) return;
        auto renderer = (MeshRenderer*)obj->GetComponent(MeshRenderer::GetType());
        if (renderer && renderer->GetMaterial())
            renderer->GetMaterial()->SetColor(color);
    }
    // This creates a collider on the Left hand of the player.
    void BaseMenu::CreateHandCollider() {
        auto controller = GameObject::Find("LeftHand Controller");
        if (!controller) return;

        leftHandCollider = GameObject::CreatePrimitive(PrimitiveType::Sphere);
        leftHandCollider->SetName("VectorMenuCollider");

        auto collider = (SphereCollider*)leftHandCollider->GetComponent(SphereCollider::GetType());
        if (collider)
        {
            auto field = collider->GetClass().GetField("IsTrigger");
            if (field.IsValid())
            {
                field.SetInstance(collider);
                *field.cast<bool*>() = true;
            }
        }

        leftHandCollider->GetTransform()->SetParent(controller->GetTransform(), false);

        GameObject::Destroy(leftHandCollider->GetComponent(MeshRenderer::GetType()));
    }
    void BaseMenu::LoadPage(int index) {
        if (pages.empty() || index < 0 || index >= pages.size()) return;

        currentPage = index;
        currentButtons.clear();

        const auto& page = pages[index];
        if (titleText)
            titleText->SetText("<color=yellow>" + page.name + "</color>\nVersion <color=red>V1.0</color>");

        for (size_t i = 0; i < buttonObjects.size(); ++i) {
            if (i < page.buttons.size()) {
                const auto& btn = page.buttons[i];
                bool isToggled = toggleStates[btn.text];

                buttonTexts[i]->SetText(btn.text);
                SetColor(buttonObjects[i], isToggled ? buttonToggled : buttonDefault);

                currentButtons.push_back({ buttonObjects[i], buttonTexts[i], btn, isToggled, false });
                buttonObjects[i]->SetActive(true);
            } else {
                buttonObjects[i]->SetActive(false);
            }
        }
    }
    void BaseMenu::NextPage() {
        if (pages.empty()) return;
        currentPage = (currentPage + 1) % pages.size();
        LoadPage(currentPage);
    }
    void BaseMenu::PreviousPage() {
        if (pages.empty()) return;
        currentPage = (currentPage - 1 + pages.size()) % pages.size();
        LoadPage(currentPage);
    }
    // IMPORTANT: This MUST be called apon a LateUpdate or a Regular Update
    void BaseMenu::Update() {
        if (!leftHandCollider) return;

        Vector3 handPos = leftHandCollider->GetTransform()->GetPosition();

        for (auto& btn : currentButtons) {
            if (!btn.buttonObject || !btn.buttonObject->GetActiveSelf()) continue;

            float dist = (handPos - btn.buttonObject->GetTransform()->GetPosition()).magnitude();
            bool pressed = dist < 0.05f;

            if (pressed && !btn.wasPressedLastFrame) {
                btn.isToggled = !btn.isToggled;
                toggleStates[btn.config.text] = btn.isToggled;
                if (btn.config.action) btn.config.action(btn.isToggled);
            }

            SetColor(btn.buttonObject, btn.isToggled ? buttonToggled : buttonDefault);
            btn.wasPressedLastFrame = pressed;
        }

        for (auto& btn : staticButtons) {
            if (!btn.buttonObject) continue;

            float dist = (handPos - btn.buttonObject->GetTransform()->GetPosition()).magnitude();
            bool pressed = dist < 0.05f;

            SetColor(btn.buttonObject, pressed ? buttonPressed : buttonDefault);
            if (pressed && !btn.wasPressedLastFrame && btn.callback)
                btn.callback();

            btn.wasPressedLastFrame = pressed;
        }
    }
    void BaseMenu::Initialize(Transform* parent) {
        pages = GetMenuDefinition();
        CreateHandCollider();

        // Paste in your method that you copied here, just replace the whole method.

        staticButtons = {
                { GameObject::Find("PageLeft"),  [this] { PreviousPage(); } },
                { GameObject::Find("PageRight"), [this] { NextPage(); } },
                { GameObject::Find("Disconnect"), MenuActions::Disconnect },
                { GameObject::Find("Discord"),    MenuActions::OpenDiscord }
        };

        if (!pages.empty())
            LoadPage(currentPage);
    }
}
