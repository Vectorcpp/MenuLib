# MenuLib

**MenuLib** allows you to build a UI menu inside Unity and convert it into customizable C++ code for use in your application or game.

## ⚠️ WARNING
- Im currently reworking most of the design for the CppUiGenerator, so if it doesnt work now just wait until i make it SO much more better.
- This is currently also in a beta phase, so expect bugs

## ✨ Features
- Build menus visually using Unity
- Export menu layouts to C++ with one click
- Customize everything: layout, styles, button actions, and more
- Unity-style C++ menu system using GameObject, Transform, and UI concepts
- Currently Working on materials, (colors only)

---

## 🛠️ How to Use

1. **Install the Editor Script**
   - Place the provided C# editor script (`CppUiCodeGenerator.cs`) into your Unity project's `Editor` folder.

2. **Create Your Menu in Unity**
   - Design your menu in the Unity Editor.
   - Name your buttons: `ButtonTemp1`, `ButtonTemp2`, etc.
   - Name the corresponding text objects: `ButtonText1`, `ButtonText2`, etc.

3. **Select Your Menu Root Object**
   - In the Unity Hierarchy, select the **root GameObject** of your menu.
   - Also, a quick note your going to have do to the poistion and parent yourself, recommened as the RightPalm

4. **Convert to C++**
   - Go to the **Unity top menu bar**.
   - Click on `Tools > C++ Converter for RootObjects` (menu name may vary depending on version).
   - The C++ menu code will be generated and automatically copied to your clipboard.

5. **Paste the Code**
   - Open `MenuMaker3000.cpp` (or your designated C++ menu file).
   - Paste the generated code inside the appropriate method (typically `BaseMenu::Initialize`).

6. **Customize as Needed**
   - You're ready to tweak the layout, button styles, logic, etc.
   - Add logic for button pages, toggles, static buttons, and more.

---

## ⚠️ Notes

- **Button Logic Must Be Hardcoded:**
  - The Unity-to-C++ converter only handles layout and naming.
  - Button actions (e.g., `Fly`, `Disconnect`, `OpenDiscord`) must be hardcoded in C++ using `ButtonInfo`.

- **Reference Code Provided:**
  - Use the included C# and C++ code as a guide.
  - Helpful classes like `BaseMenu`, `MenuButton`, and `StaticButton` are provided for flexibility.
  - You can also look through my code for better readability.

- **Pages & Button Actions**
  - Modify the `GetMenuDefinition()` function to define your custom pages and button behaviors.
  - Example:
    ```cpp
    {
      "Base",
      {
        { "Fly", MenuActions::ToggleFly },
        { "Speed Boost", nullptr },
        { "Platforms", nullptr },
      }
    }
    ```
---

## 🧠 Tips
- You can use any GameObject layout you want—directory structure doesn't matter.
- Font names like "Arial" are supported and default to built-in fonts.
- Collider behavior (for button interaction) is also handled via Unity naming conventions.

---

## ✅ Output Example

The generator produces code like this in C++:

```cpp
GameObject* ButtonTemp1_123456 = GameObject::CreatePrimitive(PrimitiveType::Cube);
ButtonTemp1_123456->SetName("ButtonTemp1");

buttonObjects = { ButtonTemp1_123456, ButtonTemp2_654321 };
buttonTexts = { ButtonText1_123456, ButtonText2_654321 };
```

