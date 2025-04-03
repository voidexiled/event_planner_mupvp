// classes/UI/Styles/MainMenuStyle.cs

using System.Numerics;
using ImGuiNET;

namespace MUPVPUI.Classes.UI.Styles;

public static class MainMenuStyle
{
    public static void ApplyStyle()
    {
        // Copied directly from original RenderClass.SetupImGuiStyle
        var style = ImGui.GetStyle();

        style.Alpha = 1.0f;
        style.DisabledAlpha = 1.0f;
        style.WindowPadding = new Vector2(12.0f, 12.0f);
        style.WindowRounding = 11.5f;
        style.WindowBorderSize = 0.0f;
        style.WindowMinSize = new Vector2(20.0f, 20.0f);
        style.WindowTitleAlign = new Vector2(0.5f, 0.5f);
        style.WindowMenuButtonPosition = ImGuiDir.Right;
        style.ChildRounding = 6.0f; // Original value
        style.ChildBorderSize = 1.0f;
        style.PopupRounding = 6.0f; // Original value
        style.PopupBorderSize = 1.0f;
        style.FramePadding = new Vector2(20.0f, 3.400000095367432f);
        style.FrameRounding = 11.89999961853027f;
        style.FrameBorderSize = 0.0f;
        style.ItemSpacing = new Vector2(4.300000190734863f, 5.5f);
        style.ItemInnerSpacing = new Vector2(7.099999904632568f, 1.799999952316284f);
        style.CellPadding = new Vector2(12.10000038146973f, 9.199999809265137f);
        style.IndentSpacing = 21.0f; // Default ImGui value, adjust if needed
        style.ColumnsMinSpacing = 6.0f; // Default ImGui value
        style.ScrollbarSize = 11.60000038146973f;
        style.ScrollbarRounding = 15.89999961853027f;
        style.GrabMinSize = 10.0f; // Adjusted from original 3.7 which is very small
        style.GrabRounding = 11.9f; // Made consistent with FrameRounding
        style.TabRounding = 6.0f; // Original Value
        style.TabBorderSize = 0.0f;
        style.TabMinWidthForCloseButton = 0.0f;
        style.ColorButtonPosition = ImGuiDir.Right;
        style.ButtonTextAlign = new Vector2(0.5f, 0.5f);
        style.SelectableTextAlign = new Vector2(0.0f, 0.0f);
        // Separator Text properties from original dark style applied later
        style.SeparatorTextBorderSize = 1.0f;
        style.SeparatorTextAlign = new Vector2(0.5f, 0.5f);
        style.SeparatorTextPadding = new Vector2(20f, 3f); // Consistent padding

        style.Colors[(int)ImGuiCol.Text] = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        style.Colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.2745098173618317f, 0.3176470696926117f, 0.4509803950786591f, 1.0f);
        style.Colors[(int)ImGuiCol.WindowBg] = new Vector4(0.0784313753247261f, 0.08627451211214066f, 0.1019607856869698f, 1.0f);
        style.Colors[(int)ImGuiCol.ChildBg] = new Vector4(0.09411764889955521f, 0.1019607856869698f, 0.1176470592617989f, 1.0f);
        style.Colors[(int)ImGuiCol.PopupBg] =
            new Vector4(0.0784313753247261f, 0.08627451211214066f, 0.1019607856869698f, 1.0f); // Adjusted alpha if needed
        style.Colors[(int)ImGuiCol.Border] = new Vector4(0.1568627506494522f, 0.168627455830574f, 0.1921568661928177f, 1.0f);
        style.Colors[(int)ImGuiCol.BorderShadow] = new Vector4(0.0f, 0.0f, 0.0f, 0.0f); // No shadow
        style.Colors[(int)ImGuiCol.FrameBg] = new Vector4(0.1137254908680916f, 0.125490203499794f, 0.1529411822557449f, 1.0f);
        style.Colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.1568627506494522f, 0.168627455830574f, 0.1921568661928177f, 1.0f);
        style.Colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.18f, 0.19f, 0.21f, 1.0f); // Slightly different active
        style.Colors[(int)ImGuiCol.TitleBg] = new Vector4(0.0470588244497776f, 0.05490196123719215f, 0.07058823853731155f, 1.0f);
        style.Colors[(int)ImGuiCol.TitleBgActive] =
            new Vector4(0.1137254908680916f, 0.125490203499794f, 0.1529411822557449f, 1.0f); // Use FrameBg color for active title
        style.Colors[(int)ImGuiCol.TitleBgCollapsed] =
            new Vector4(0.0784313753247261f, 0.08627451211214066f, 0.1019607856869698f, 0.7f); // Semi-transparent
        style.Colors[(int)ImGuiCol.MenuBarBg] = new Vector4(0.09803921729326248f, 0.105882354080677f, 0.1215686276555061f, 1.0f);
        style.Colors[(int)ImGuiCol.ScrollbarBg] =
            new Vector4(0.0470588244497776f, 0.05490196123719215f, 0.07058823853731155f, 0.5f); // Semi-transparent
        style.Colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.31f, 0.31f, 0.31f, 1.0f); // Darker Grab
        style.Colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.41f, 0.41f, 0.41f, 1.0f);
        style.Colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(0.51f, 0.51f, 0.51f, 1.0f);
        style.Colors[(int)ImGuiCol.CheckMark] = new Vector4(0.9725490212440491f, 1.0f, 0.4980392158031464f, 1.0f);
        style.Colors[(int)ImGuiCol.SliderGrab] = new Vector4(0.51f, 0.51f, 0.51f, 1.0f); // Consistent grab color
        style.Colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(0.86f, 0.86f, 0.86f, 1.0f); // Lighter active slider
        style.Colors[(int)ImGuiCol.Button] = new Vector4(0.1176470592617989f, 0.1333333402872086f, 0.1490196138620377f, 1.0f);
        style.Colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.1803921610116959f, 0.1882352977991104f, 0.196078434586525f, 1.0f);
        style.Colors[(int)ImGuiCol.ButtonActive] = new Vector4(0.20f, 0.21f, 0.23f, 1.0f); // Slightly different active
        style.Colors[(int)ImGuiCol.Header] =
            new Vector4(0.1411764770746231f, 0.1647058874368668f, 0.2078431397676468f, 1.0f); // Header for Selectable, etc.
        style.Colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.26f, 0.59f, 0.98f, 0.8f); // Use standard ImGui blue hover
        style.Colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.26f, 0.59f, 0.98f, 1.0f); // Use standard ImGui blue active
        style.Colors[(int)ImGuiCol.Separator] = style.Colors[(int)ImGuiCol.Border];
        style.Colors[(int)ImGuiCol.SeparatorHovered] = new Vector4(0.1f, 0.4f, 0.75f, 0.78f); // Blue hover
        style.Colors[(int)ImGuiCol.SeparatorActive] = new Vector4(0.1f, 0.4f, 0.75f, 1.0f);
        style.Colors[(int)ImGuiCol.ResizeGrip] = new Vector4(0.31f, 0.31f, 0.31f, 0.2f); // More transparent
        style.Colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(0.26f, 0.59f, 0.98f, 0.67f);
        style.Colors[(int)ImGuiCol.ResizeGripActive] = new Vector4(0.26f, 0.59f, 0.98f, 0.95f);
        style.Colors[(int)ImGuiCol.Tab] = style.Colors[(int)ImGuiCol.Button]; // Base tab like button
        style.Colors[(int)ImGuiCol.TabHovered] = style.Colors[(int)ImGuiCol.ButtonHovered];
        // style.Colors[(int)ImGuiCol.TabActive] = style.Colors[(int)ImGuiCol.HeaderActive]; // Active tab like active header
        // style.Colors[(int)ImGuiCol.TabUnfocused] = ImGui.ColorConvertU32ToFloat4(ImGui.ColorConvertFloat4ToU32(style.Colors[(int)ImGuiCol.Tab]) & 0x7FFFFFFF); // 50% Alpha
        // style.Colors[(int)ImGuiCol.TabUnfocusedActive] = ImGui.ColorConvertU32ToFloat4(ImGui.ColorConvertFloat4ToU32(style.Colors[(int)ImGuiCol.TabActive]) & 0xBFFFFFFF); // 75% Alpha for active unfocused

        style.Colors[(int)ImGuiCol.PlotLines] = new Vector4(0.61f, 0.61f, 0.61f, 1.0f);
        style.Colors[(int)ImGuiCol.PlotLinesHovered] = new Vector4(1.0f, 0.43f, 0.35f, 1.0f);
        style.Colors[(int)ImGuiCol.PlotHistogram] = new Vector4(0.90f, 0.70f, 0.0f, 1.0f);
        style.Colors[(int)ImGuiCol.PlotHistogramHovered] = new Vector4(1.0f, 0.60f, 0.0f, 1.0f);
        style.Colors[(int)ImGuiCol.TableHeaderBg] = new Vector4(0.19f, 0.19f, 0.20f, 1.0f);
        style.Colors[(int)ImGuiCol.TableBorderStrong] = new Vector4(0.31f, 0.31f, 0.35f, 1.0f); // thicker lines
        style.Colors[(int)ImGuiCol.TableBorderLight] = new Vector4(0.23f, 0.23f, 0.25f, 1.0f); // thinner lines
        style.Colors[(int)ImGuiCol.TableRowBg] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f); // transparent row
        style.Colors[(int)ImGuiCol.TableRowBgAlt] = new Vector4(1.00f, 1.00f, 1.00f, 0.06f); // slight grey alt row
        style.Colors[(int)ImGuiCol.TextSelectedBg] = new Vector4(0.26f, 0.59f, 0.98f, 0.35f);
        style.Colors[(int)ImGuiCol.DragDropTarget] = new Vector4(1.00f, 1.00f, 0.00f, 0.90f); // Yellow drop target
        // style.Colors[(int)ImGuiCol.NavHighlight] = new Vector4(0.26f, 0.59f, 0.98f, 1.00f); // Navigation highlight (e.g., keyboard nav)
        style.Colors[(int)ImGuiCol.NavWindowingHighlight] = new Vector4(1.00f, 1.00f, 1.00f, 0.70f); // Highlight when window is selected for docking
        style.Colors[(int)ImGuiCol.NavWindowingDimBg] =
            new Vector4(0.80f, 0.80f, 0.80f, 0.20f); // Dim background of non-selected windows during docking
        style.Colors[(int)ImGuiCol.ModalWindowDimBg] = new Vector4(0.2f, 0.2f, 0.2f, 0.6f); // Dim background for modal popups
    }

    public static void ApplyDarkStyleWithRounding() // From original RenderClass
    {
        ImGui.StyleColorsDark();
        var style = ImGui.GetStyle();
        style.ChildRounding = 6f;
        style.WindowRounding = 8f;
        style.FrameRounding = 6f;
        style.PopupRounding = 6f;
        style.ScrollbarRounding = 6f;
        style.GrabRounding = 6f;
        style.TabRounding = 10f;
        style.SeparatorTextBorderSize = 1.0f;
        style.SeparatorTextAlign = new Vector2(0.5f, 0.5f);
        style.SeparatorTextPadding = new Vector2(20f, 3f);
    }
}