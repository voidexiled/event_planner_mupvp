// classes/UI/Renderers/SetListWindowRenderer.cs

using System.Numerics;
using event_planner_mupvp.classes.core;
using event_planner_mupvp.classes.utils;
using event_planner_mupvp.lib;
using ImGuiNET;
// ArmorSet, ProcessedItem

namespace event_planner_mupvp.classes.UI.Renderers;

public class SetListWindowRenderer : IWindowRenderer
{
    #region State

    private List<ArmorSet> _filteredSets = new();
    private string _setNameFilter = "";
    private readonly List<ClassTypes> _classFilters = new();
    private int _typeFilter = -1; // Original code didn't seem to use type filter for sets, but keep if needed

    // Dependency
    private readonly RewardListWindowRenderer _rewardListRenderer;

    #endregion

    #region Constructor

    public SetListWindowRenderer(RewardListWindowRenderer rewardListRenderer)
    {
        _rewardListRenderer = rewardListRenderer;
    }

    #endregion

    #region IWindowRenderer Implementation

    public void Render()
    {
        UpdateFilteredSets(); // Update on each frame

        ImGui.SetNextWindowSize(new Vector2(400, 400), ImGuiCond.FirstUseEver);
        if (ImGui.Begin("Lista de Sets", ref WindowManager.ShowSetListWindow))
        {
            RenderFilters();
            ImGui.Separator();
            RenderSetList();
        }

        if (!WindowManager.ShowSetListWindow) OnClose();
        ImGui.End();
    }

    public void OnClose()
    {
        // Clear filters on close
        _setNameFilter = "";
        _classFilters.Clear();
        _typeFilter = -1;
        _filteredSets.Clear(); // Clear the filtered list
        Console.WriteLine("Set List Window closed and filters cleared.");
    }

    #endregion

    #region UI Rendering

    private void RenderFilters()
    {
        ImGui.InputTextWithHint("##SetNameFilter", "Filtrar por Nombre", ref _setNameFilter, 40);

        ImGui.SeparatorText("Filtrar por Clase");
        const int checkboxesPerRow = 4;
        var currentCheckbox = 0;
        foreach (ClassTypes classType in Enum.GetValues(typeof(ClassTypes)))
        {
            // Optional: Check if classType is valid if needed
            var isSelected = _classFilters.Contains(classType);
            if (ImGui.Checkbox(classType.ToString(), ref isSelected))
            {
                if (isSelected) _classFilters.Add(classType);
                else _classFilters.Remove(classType);
                // Filter update happens in UpdateFilteredSets()
            }

            currentCheckbox++;
            if (currentCheckbox % checkboxesPerRow != 0 && currentCheckbox < Enum.GetValues(typeof(ClassTypes)).Length) ImGui.SameLine();
        }

        ImGui.NewLine(); // Ensure proper layout after checkboxes
    }

    private void RenderSetList()
    {
        float footerHeight = ImGui.GetStyle().ItemSpacing.Y + ImGui.GetFrameHeightWithSpacing();
        ImGui.BeginChild("SetListChild", new Vector2(0, -footerHeight), ImGuiChildFlags.Borders, ImGuiWindowFlags.HorizontalScrollbar);

        if (_filteredSets.Count == 0)
        {
            ImGui.Text("No se encontraron sets con esos filtros.");
        }
        else
        {
            // --- Simple foreach loop ---
            for (int i = 0; i < _filteredSets.Count; i++)
            {
                var set = _filteredSets[i];
                string label = $"{set.Name} (Type: {set.Type})"; // Include Type if relevant

                ImGui.PushID($"set_{set.Type}_{i}"); // Unique ID
                if (ImGui.Selectable(label))
                {
                    HandleAddSetToRewardList(set);
                    if (!WindowManager.ShowRewardListWindow)
                    {
                        WindowManager.ShowRewardListWindow = true;
                    }
                }
                ImGui.PopID();

                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip($"Click para añadir '{set.Name}' a Recompensas");
                }
            }
            // --- End simple foreach loop ---
        }
        ImGui.EndChild();
        ImGui.TextColored(new Vector4(0.8f, 0.8f, 0f, 1f), $"{_filteredSets.Count} sets encontrados.");
        ImGui.SameLine(ImGui.GetWindowWidth() - 250);
        ImGui.TextColored(new Vector4(0f, 1f, 0f, 1f), "Click Izquierdo para Añadir a Recompensas");
    }

    #endregion

    #region Logic

    private void UpdateFilteredSets()
    {
        // Start with all sets from ItemManager (assuming it's populated)
        var query = ItemManager.ArmorSets.AsEnumerable();

        // Filter by name (case-insensitive, ignoring diacritics)
        if (!string.IsNullOrWhiteSpace(_setNameFilter))
        {
            var normalizedFilter = StringUtils.RemoveDiacritics(_setNameFilter.ToLowerInvariant());
            query = query.Where(set => StringUtils.RemoveDiacritics(set.Name.ToLowerInvariant()).Contains(normalizedFilter));
        }

        // Filter by class (if any class selected)
        if (_classFilters.Count > 0) query = query.Where(set => _classFilters.Any(filterClass => set.Classes.Contains(filterClass)));

        // Filter by type (if needed and implemented)
        if (_typeFilter >= 0) query = query.Where(set => set.Type == _typeFilter);

        // Apply ordering if needed (e.g., by name)
        query = query.OrderBy(set => set.Name);

        _filteredSets = query.ToList();
    }

    private void HandleAddSetToRewardList(ArmorSet set)
    {
        // Create a ProcessedItem representing the set
        // Use the specific constructor for sets
        var newItem = new ProcessedItem(set.Type, set.Name);
        _rewardListRenderer.AddItem(newItem); // Call the RewardList renderer to add it
        Console.WriteLine($"Set '{set.Name}' añadido a la lista de recompensas.");
    }

    #endregion
}