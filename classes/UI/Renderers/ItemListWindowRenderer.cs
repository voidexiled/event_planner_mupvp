using System.Numerics;
using event_planner_mupvp.classes.core;
using event_planner_mupvp.classes.utils;
using event_planner_mupvp.enums;
using event_planner_mupvp.lib;
using ImGuiNET;

namespace event_planner_mupvp.classes.UI.Renderers;

// classes/UI/Renderers/ItemListWindowRenderer.cs

/// <summary>
///     Renders the window displaying the filterable list of all base items.
///     Allows adding items to the Reward List.
/// </summary>
public class ItemListWindowRenderer : IWindowRenderer
{
    #region State

    private List<Item> _filteredItems = new();
    private string _nameFilter = "";
    private readonly List<ClassTypes> _classFilters = new();
    private int _typeFilter = -1; // -1 indicates no type filter
    private ItemGroups _groupFilter = ItemGroups.ALL_ITEMS;
    private bool _sortByTypeAscending = true;
    private bool _needsUpdate = true; // Flag to trigger initial filtering

    #endregion

    #region Dependencies

    private readonly RewardListWindowRenderer _rewardListRenderer; // To add selected items

    #endregion

    #region Constructor

    public ItemListWindowRenderer(RewardListWindowRenderer rewardListRenderer)
    {
        _rewardListRenderer = rewardListRenderer ?? throw new ArgumentNullException(nameof(rewardListRenderer));
        // ItemManager is assumed static
    }

    #endregion

    #region IWindowRenderer Implementation

    public void Render()
    {
        if (_needsUpdate) UpdateFilteredItems(); // Update filter if needed

        ImGui.SetNextWindowSize(new Vector2(400, 500), ImGuiCond.FirstUseEver);
        if (ImGui.Begin("Lista de Items", ref WindowManager.ShowItemListWindow))
        {
            var filterChanged = RenderFilters();
            ImGui.Separator();
            RenderItemList();

            if (filterChanged) _needsUpdate = true; // Mark for update next frame if filters changed
        }

        // Always call End, even if Begin returned false
        ImGui.End();

        // Handle window close via 'X' button
        if (!WindowManager.ShowItemListWindow) OnClose();
    }

    public void OnOpen()
    {
        _needsUpdate = true; // Force filter update when window opens
        Console.WriteLine("Item List Window opened.");
    }

    public void OnClose()
    {
        // Reset filters to default when closed
        _nameFilter = "";
        _classFilters.Clear();
        _typeFilter = -1;
        _groupFilter = ItemGroups.ALL_ITEMS;
        _sortByTypeAscending = true;
        _filteredItems.Clear(); // Clear the list to save memory
        _needsUpdate = true; // Ensure update on next open
        Console.WriteLine("Item List Window closed and filters reset.");
    }

    #endregion

    #region UI Rendering

    /// <summary>
    ///     Renders the filter controls for the item list.
    /// </summary>
    /// <returns>True if any filter value was changed in this frame.</returns>
    private bool RenderFilters()
    {
        var changed = false;
        ImGui.PushID("ItemFilters"); // Unique ID scope for filters

        // --- Row 1: Name & Type ---
        ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X * 0.5f - ImGui.GetStyle().ItemSpacing.X * 0.5f);
        changed |= ImGui.InputTextWithHint("##NameFilter", "Filtrar Nombre", ref _nameFilter, 100);
        ImGui.PopItemWidth();
        ImGui.SameLine();
        ImGui.PushItemWidth(-1); // Rest of the width
        // Use a temporary variable because InputInt doesn't handle nullable well for the placeholder text logic
        var currentType = _typeFilter;
        if (ImGui.InputInt("Type", ref currentType, 0, 0)) // Step 0 allows negative input
            // Only update if the value actually changed
            if (currentType != _typeFilter)
            {
                _typeFilter = currentType;
                changed = true;
            }

        if (ImGui.IsItemHovered()) ImGui.SetTooltip("Filtrar por Type exacto (negativo o vacío para desactivar)");
        ImGui.PopItemWidth();


        // --- Row 2: Group & Sort ---
        ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X * 0.5f - ImGui.GetStyle().ItemSpacing.X * 0.5f);
        if (ImGui.BeginCombo("Grupo", _groupFilter.ToString()))
        {
            foreach (ItemGroups group in Enum.GetValues(typeof(ItemGroups)))
            {
                var isSelected = _groupFilter == group;
                if (ImGui.Selectable(group.ToString(), isSelected))
                    if (_groupFilter != group) // Check if actually changed
                    {
                        _groupFilter = group;
                        changed = true;
                    }

                if (isSelected) ImGui.SetItemDefaultFocus();
            }

            ImGui.EndCombo();
        }

        ImGui.PopItemWidth();
        ImGui.SameLine();
        ImGui.PushItemWidth(-1);
        var sortLabel = _sortByTypeAscending ? "Orden: Tipo Asc" : "Orden: Tipo Desc";
        if (ImGui.Button(sortLabel, new Vector2(-1, 0)))
        {
            _sortByTypeAscending = !_sortByTypeAscending;
            changed = true; // Sorting changed, need update
        }

        ImGui.PopItemWidth();

        // --- Row 3: Class Checkboxes ---
        ImGui.SeparatorText("Filtrar por Clase");
        const int checkboxesPerRow = 4;
        var currentCheckbox = 0;
        foreach (ClassTypes classType in Enum.GetValues(typeof(ClassTypes)))
        {
            var isSelected = _classFilters.Contains(classType);
            // Use a temporary variable for the checkbox state
            var tempIsSelected = isSelected;
            if (ImGui.Checkbox(classType.ToString(), ref tempIsSelected))
            {
                // Update the list only if the state truly changed
                if (tempIsSelected && !isSelected)
                {
                    _classFilters.Add(classType);
                    changed = true;
                }
                else if (!tempIsSelected && isSelected)
                {
                    _classFilters.Remove(classType);
                    changed = true;
                }
            }

            if (ImGui.IsItemHovered()) ImGui.SetTooltip($"Filtrar por {classType}");

            currentCheckbox++;
            // Add SameLine only if not the last item in the row and not the last item overall
            if (currentCheckbox % checkboxesPerRow != 0 && currentCheckbox < Enum.GetValues(typeof(ClassTypes)).Length) ImGui.SameLine();
        }

        ImGui.NewLine(); // Ensure layout integrity

        ImGui.PopID(); // Pop ItemFilters ID scope
        return changed;
    }

    /// <summary>
    ///     Renders the scrollable list of filtered items.
    /// </summary>
    private void RenderItemList()
    {
        // Calculate height for the list, leaving space for footer text
        float footerHeight = ImGui.GetStyle().ItemSpacing.Y + ImGui.GetFrameHeightWithSpacing();
        ImGui.BeginChild("ItemListChild", new Vector2(0, -footerHeight), ImGuiChildFlags.Borders, ImGuiWindowFlags.HorizontalScrollbar);

        if (_filteredItems.Count == 0 && !_needsUpdate)
        {
            ImGui.Text("No se encontraron items con esos filtros.");
        }
        else if (_needsUpdate)
        {
            ImGui.Text("Actualizando..."); // Show loading state
        }
        else
        {
            // --- Simple foreach loop ---
            for (int i = 0; i < _filteredItems.Count; i++)
            {
                var item = _filteredItems[i];
                // Format: Group-Type: Name (e.g., SWORD-0: Short Sword)
                string label = $"{item.Group}-{item.Type}: {item.Name}";
                ImGui.PushID($"item_{item.Group}_{item.Type}_{i}"); // Unique ID still recommended
                if (ImGui.Selectable(label))
                {
                    HandleItemSelection(item);
                }
                ImGui.PopID();

                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip($"Click para añadir '{item.Name}' a Recompensas");
                }
            }
            // --- End simple foreach loop ---
        }

        ImGui.EndChild();

        // Footer Information
        ImGui.TextColored(new Vector4(0.8f, 0.8f, 0f, 1f), $"{_filteredItems.Count} items encontrados.");
        ImGui.SameLine(ImGui.GetWindowWidth() - 220);
        ImGui.TextColored(new Vector4(0f, 1f, 0f, 1f), "Click Izquierdo para Añadir");
    }

    #endregion

    #region Logic

    /// <summary>
    ///     Applies the current filters and sorting to ItemManager.AllItems and updates _filteredItems.
    /// </summary>
    private void UpdateFilteredItems()
    {
        Console.WriteLine("Updating filtered items...");
        var itemsToFilter = ItemManager.AllItems; // Assuming static access
        var query = itemsToFilter.AsEnumerable();

        // Apply Name Filter (Case-Insensitive, Diacritic-Insensitive)
        if (!string.IsNullOrWhiteSpace(_nameFilter))
        {
            string normalizedFilter = StringUtils.RemoveDiacritics(_nameFilter.ToLowerInvariant());
            query = query.Where(item => StringUtils.RemoveDiacritics(item.Name.ToLowerInvariant()).Contains(normalizedFilter));
        }

        // Apply Class Filter
        if (_classFilters.Count > 0)
            query = query.Where(item =>
                    item is Equipable equipableItem && // Check if the item is Equipable
                    _classFilters.Any(filterClass => equipableItem.Classes.Contains(filterClass)) // Check if any selected class matches
            );

        // Apply Type Filter
        if (_typeFilter >= 0) query = query.Where(item => item.Type == _typeFilter);

        // Apply Group Filter
        if (_groupFilter != ItemGroups.ALL_ITEMS) query = query.Where(item => item.Group == _groupFilter);

        // Apply Sorting
        query = _sortByTypeAscending
            ? query.OrderBy(item => item.Group).ThenBy(item => item.Type).ThenBy(item => item.Name)
            : query.OrderByDescending(item => item.Group).ThenByDescending(item => item.Type).ThenBy(item => item.Name);

        _filteredItems = query.ToList();
        _needsUpdate = false; // Mark as updated
        Console.WriteLine($"Filtering complete: {_filteredItems.Count} items matched.");
    }

    /// <summary>
    ///     Handles the action when an item is selected from the list.
    /// </summary>
    /// <param name="selectedItem">The base Item object selected.</param>
    private void HandleItemSelection(Item selectedItem)
    {
        if (selectedItem == null) return;

        Console.WriteLine($"Item selected: {selectedItem.Name}");
        // Create a new ProcessedItem based on the selected base Item
        var newItem = new ProcessedItem(selectedItem.Group, selectedItem.Type, selectedItem.Name, selectedItem.Slot);

        // Add the new ProcessedItem to the Reward List via its renderer
        _rewardListRenderer.AddItem(newItem);

        // Optionally, automatically open the Reward List window if it's closed
        if (!WindowManager.ShowRewardListWindow) WindowManager.ShowRewardListWindow = true;
        // Optionally call OnOpen for reward list if needed
        // _rewardListRenderer.OnOpen();
    }

    #endregion
}