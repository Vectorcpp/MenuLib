#pragma once
#include <string>
#include <vector>
#include <functional>
#include "NotifcationLib.h"
#include "MenuMaker3000.hpp"
#include "BNMResolve.hpp"

using namespace BNM;
using namespace BNM::Structures;
using namespace BNM::UnityEngine;
using namespace Mono;


using namespace VectorWorks::Notifications;

namespace MenuActions {
    void ToggleFly(bool isToggled) {
        if (isToggled) {
            // This is when the button is on, you can imed turn it off by doing isToggled = false;
            // This MUST be in a update method, or somewhere else
        } else {
            //This is when the button is NOT. on
        }

        //Used for notications or some sorts.
        bool isT = isToggled ? "On" : "False";
    }

    //These are the base buttons of the menu
    void Disconnect() {
    }

    void OpenDiscord() {
        // You can you use your own discord link here!
        Application::OpenUrl("");

    }
    void OpenSettings() {
    }
}

// Button info
struct ButtonInfo {
    std::string text;
    std::function<void(bool)> action = nullptr;
};

// A Pages info.
struct PageInfo {
    std::string name;
    std::vector<ButtonInfo> buttons;
};

inline std::vector<PageInfo> GetMenuDefinition() {
    std::vector<PageInfo> pages = {
            // These are just for you to see how to structure this code.
            {
                    "Base",
                    {
                        // Nullptr means they wont have any functionality.

                            { "Fly", MenuActions::ToggleFly },
                            { "Speed Boost", nullptr },
                            { "Platforms", nullptr },
                            { "Long Arms", nullptr },
                            { "Long Arms", nullptr }
                    }
            }
    };
    return pages;
}