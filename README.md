# menulib

yo this is menulib. its a tool that lets u build a ui in unity and then spits it out as c++ code for your mod or whatever.

---

### ⚠️ warning

*   this is still super beta. expect bugs and shit to be broken.
*   im currently redoing a lot of the generator code to make it way better, so if it doesnt work right now just chill and wait for an update.

---

### ✨ features

*   build your menu visually in unity, no coding needed for the layout.
*   one click exports the whole thing to c++ and copies it.
*   the generated c++ code looks like unity's api (gameobject, transform etc) so it should feel familiar.
*   converts components like text, images, and layout groups.
*   it even handles basic materials, but just colors for now.
*   if wanted, you can toggle the box and you can just only do components, if you need it for another script or something like that.

---

### 🛠️ how to use

1.  **install:**
    *   toss the `CppUiCodeGenerator.cs` file into an `Editor` folder in your unity project.

2.  **design:**
    *   make your menu in the unity scene. name your buttons and text stuff that makes sense like `ButtonFly` and `ButtonFlyText`.

3.  **select:**
    *   in the hierarchy, click the main root gameobject for your menu.
    *   quick note: you gotta set the position and parent yourself in the code. sticking it to the right hand palm is usually a good bet.

4.  **convert:**
    *   go up to the menu bar `Tools > C++ Converter for RootObjects`.
    *   it'll generate the code and copy it to your clipboard automatically.

5.  **paste:**
    *   open your c++ file, probably `MenuMaker3000.cpp` or whatever u called it.
    *   paste the code into the `BaseMenu::Initialize` function.

6.  **customize:**
    *   the tool only does the layout. you still have to code what the buttons actually do.

---

### ⚠️ notes

*   **button logic is on you:**
    *   the converter only handles the visual setup. you have to hardcode the actions (like `MenuActions::ToggleFly`) in c++ yourself.

*   **use the examples:**
    *   look at the c++ files i included. `BaseMenu` and the other classes show you how to set everything up.

*   **pages & actions:**
    *   edit the `GetMenuDefinition()` function to make your pages and hook up your button functions. looks like this:
    ```cpp
    {
      "Base", // Page name
      {
        { "Fly", MenuActions::ToggleFly }, // Button text and its C++ function
        { "Speed Boost", nullptr }, // Button with no action yet
        { "Platforms", nullptr },
      }
    }
    ```
    *    edit the 'staticButtons', this is just a preview you can add your own gameobject defs, as the buttons!

---

### 🧠 tips
*   ur gameobject hierarchy can be a total mess, the script doesnt care about folders.
*   it knows what "arial" is and will use the builtin font for it.
*   it sets up colliders on stuff so your buttons can be clicked.

---

### ✅ output example

it'll spit out code that looks something like this:

```cpp
GameObject* ButtonTemp1_123456 = GameObject::CreatePrimitive(PrimitiveType::Cube);
ButtonTemp1_123456->SetName("ButtonTemp1");

buttonObjects = { ButtonTemp1_123456, ButtonTemp2_654321 };
buttonTexts = { ButtonText1_123456_text, ButtonText2_654321_text };
