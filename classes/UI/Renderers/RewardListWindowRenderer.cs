// classes/UI/Renderers/RewardListWindowRenderer.cs

using System.Numerics;
using event_planner_mupvp.classes.core;
using event_planner_mupvp.lib;
using ImGuiNET;

namespace event_planner_mupvp.classes.UI.Renderers;

/// <summary>
///     Renders the window for managing and editing the list of reward items.
/// </summary>
public class RewardListWindowRenderer : IWindowRenderer
{
    #region State

    private readonly List<ProcessedItem> _processedItems = new(); // The list of items being managed
    private ProcessedItem? _selectedItem; // Item currently selected for editing

    // --- Edit Buffer State (mirrors ProcessedItem properties) ---
    private int _editPlusLevel;
    private bool _editSkill;
    private bool _editLuck;
    private bool _editFullOption; // Specific to Set/Shield/Ring
    private int _editExcellentOption; // Original 'Option' field
    private List<Option> _editSelectedExcOptions = new();

    // --- UI State ---
    private ImGuiStylePtr _style; // To access style colors/properties
    private float _dynamicRed = 1.0f; // For featured/exc item text color
    private float _dynamicGreen = 1.0f;
    private float _dynamicBlue;
    
    // --- Confirmation Popup ---
    private bool _deleteConfirmationPopupOpen = false;

    #endregion

    #region Constructor

    #endregion

    #region IWindowRenderer Implementation

    public void UpdateDynamicColors(float r, float g, float b)
    {
        _dynamicRed = r;
        _dynamicGreen = g;
        _dynamicBlue = b;
    }

    public void Render()
    {
        _style = ImGui.GetStyle(); // Get current style

        ImGui.SetNextWindowSize(new Vector2(580, 650), ImGuiCond.FirstUseEver); // Slightly larger
        if (ImGui.Begin("Lista de Recompensas / Editor", ref WindowManager.ShowRewardListWindow))
        {
            // Split into two main sections
            RenderEditorSection(); // Top section for editing selected item
            ImGui.Separator();
            RenderProcessedItemListSection(); // Bottom section listing items
        }

        // Always call End()
        ImGui.End();

        // Handle window close via 'X'
        if (!WindowManager.ShowRewardListWindow) OnClose();
    }

    public void OnOpen()
    {
        // Optional: Load state or refresh something when opened
        Console.WriteLine("Reward List Window opened.");
    }

    public void OnClose()
    {
        // Optional: Clear list or reset state on close
        // _processedItems.Clear(); // Uncomment to clear list on close
        DeselectItem(); // Ensure editor is cleared when window closes
        Console.WriteLine("Reward List Window closed.");
    }

    #endregion

    #region Public Methods (Called by other Renderers)

    /// <summary>
    ///     Adds a new item to the processed items list. Called by ItemList/SetList Renderers.
    /// </summary>
    /// <param name="newItem">The ProcessedItem to add.</param>
    public void AddItem(ProcessedItem newItem)
    {
        if (newItem == null) return;

        // Optional: Prevent adding exact duplicates (based on Group/Type/Name?)
        // bool exists = _processedItems.Any(p => p.Group == newItem.Group && p.Type == newItem.Type && p.Name == newItem.Name);
        // if (exists) { Console.WriteLine($"Item '{newItem.Name}' already in list."); return; }

        _processedItems.Add(newItem);
        Console.WriteLine($"Item '{newItem.Name}' added to reward list.");

        // Optional: Automatically select the newly added item for editing
        SelectItemForEditing(newItem);
    }

    #endregion

    #region UI Rendering Sections

    /// <summary>
    ///     Renders the top section containing controls to edit the _selectedItem.
    /// </summary>
    private void RenderEditorSection()
    {
        ImGui.BeginChild("EditorSection", new Vector2(0, 280), ImGuiChildFlags.Borders); // Fixed height for editor
        ImGui.PushID("ItemEditor"); // Scope IDs

        if (_selectedItem == null)
        {
            ImGui.TextWrapped("Selecciona un item de la lista de abajo (Click Izquierdo) para editar sus opciones aquí.");
        }
        else
        {
            ImGui.SeparatorText($"Editando: {_selectedItem.GetDecoratedName()}"); // Show decorated name

            // --- Basic Info (Read Only) ---
            RenderEditorInfoFields();

            // --- Core Modifiers ---
            RenderEditorCoreModifiers();

            // --- Excellent Options ---
            RenderEditorExcellentOptions();

            // --- Action Buttons ---
            RenderEditorActionButtons();
        }

        ImGui.PopID(); // Pop ItemEditor ID scope
        ImGui.EndChild();
    }

    /// <summary>
    ///     Renders read-only info fields for the selected item.
    /// </summary>
    private void RenderEditorInfoFields()
    {
        if (_selectedItem == null) return;
        // Use InputText with ReadOnly flag for consistent alignment
        var name = _selectedItem.Name;
        var group = _selectedItem.Group.ToString();
        var type = _selectedItem.Type.ToString();
        var slot = _selectedItem.Slot.ToString();
        ImGui.InputText("Nombre", ref name, (uint)name.Length, ImGuiInputTextFlags.ReadOnly);
        ImGui.InputText("Grupo", ref group, (uint)group.Length, ImGuiInputTextFlags.ReadOnly);
        ImGui.InputText("Tipo", ref type, (uint)type.Length, ImGuiInputTextFlags.ReadOnly);
        ImGui.InputText("Slot", ref slot, (uint)slot.Length, ImGuiInputTextFlags.ReadOnly);
    }

    /// <summary>
    ///     Renders controls for Level, Skill, Luck.
    /// </summary>
    private void RenderEditorCoreModifiers()
    {
        ImGui.SeparatorText("Modificadores Base");
        // Ensure SliderInt uses the edit buffer variable
        ImGui.SliderInt("+ Nivel", ref _editPlusLevel, 0, 15); // Max level 15 typical
        ImGui.Checkbox("Skill", ref _editSkill);
        ImGui.SameLine(ImGui.GetWindowWidth() * 0.5f); // Align Luck checkbox roughly in middle
        ImGui.Checkbox("Luck", ref _editLuck);
    }

    /// <summary>
    ///     Renders the controls for selecting excellent options.
    /// </summary>
    private void RenderEditorExcellentOptions()
    {
        if (_selectedItem == null) return;
        ImGui.SeparatorText("Opciones Excelentes");

        List<Option>? availableOptions = ExcManager.GetApplicableOptions(_selectedItem.Slot);

        if (availableOptions == null || availableOptions.Count == 0)
        {
            ImGui.TextDisabled("Este item no soporta opciones excelentes.");
            return;
        }

        // --- Controls (e.g., Full Option Checkbox, Num Options Slider) ---
        // Checkbox for "Full Option" only if applicable (Set/Shield/Ring)
        if (ExcManager.IsSetShieldRing(_selectedItem.Slot))
        {
            var prevFullOption = _editFullOption;
            ImGui.Checkbox("Full Option (Set/Shield/Ring)", ref _editFullOption);
            if (_editFullOption && !prevFullOption) // If just checked
            {
                // Select all available options
                _editSelectedExcOptions = new List<Option>(availableOptions);
            }
            else if (!_editFullOption && prevFullOption) // If just unchecked
            {
                // Optional: Clear all options when unchecked? Or let user remove manually?
                // _editSelectedExcOptions.Clear(); // Uncomment to clear when unchecked
            }
        }

        // Slider for original 'Option' value (ExcellentAncientOption)
        // Label needs clarification based on what this value represents
        ImGui.SliderInt("Excelent", ref _editExcellentOption, 0, 7); // Example range 0-7
        if (ImGui.IsItemHovered()) ImGui.SetTooltip("Valor original 'Option'. Representa Opción Antigua o número de opciones?");


        // --- Option Selection Lists ---
        ImGui.Text("Selecciona opciones:");
        var halfWidth = ImGui.GetContentRegionAvail().X * 0.5f - _style.ItemSpacing.X * 0.5f;
        float listHeight = 100; // Adjust height as needed

        // Available Options Column
        ImGui.BeginChild("AvailableExcOptions", new Vector2(halfWidth, listHeight), ImGuiChildFlags.Borders);
        ImGui.TextColored(new Vector4(0.6f, 0.6f, 1f, 1f), "Disponibles");
        ImGui.Separator();
        var optionsToAdd = new List<Option>();
        foreach (var option in availableOptions)
            if (!_editSelectedExcOptions.Contains(option)) // Show only if not already selected
            {
                if (ImGui.Selectable($"{option.Name}##avail")) // Unique ID per list
                    optionsToAdd.Add(option);
                if (ImGui.IsItemHovered() && option.Description != null) ImGui.SetTooltip(option.Description);
            }

        ImGui.EndChild();

        // Add options marked for adding
        foreach (var opt in optionsToAdd)
        {
            _editSelectedExcOptions.Add(opt);
            _editFullOption = false; // Uncheck "Full" if adding manually
        }

        ImGui.SameLine();

        // Selected Options Column
        ImGui.BeginChild("SelectedExcOptions", new Vector2(0, listHeight), ImGuiChildFlags.Borders); // Use remaining width
        ImGui.TextColored(new Vector4(0.6f, 1f, 0.6f, 1f), "Seleccionadas");
        ImGui.Separator();
        var optionsToRemove = new List<Option>();
        foreach (var option in _editSelectedExcOptions)
        {
            if (ImGui.Selectable($"{option.Name}##sel")) // Unique ID per list
                optionsToRemove.Add(option);
            if (ImGui.IsItemHovered() && option.Description != null) ImGui.SetTooltip(option.Description);
        }

        ImGui.EndChild();

        // Remove options marked for removal
        foreach (var opt in optionsToRemove)
        {
            _editSelectedExcOptions.Remove(opt);
            _editFullOption = false; // Uncheck "Full" if removing manually
        }
    }


    /// <summary>
    ///     Renders the action buttons (Save, Copy, Cancel) for the editor.
    /// </summary>
    private void RenderEditorActionButtons()
    {
        if (_selectedItem == null) return;
        ImGui.Separator();
        ImGui.Spacing();

        // Save Changes Button
        if (ImGui.Button("Guardar Cambios", new Vector2(140, 25)))
        {
            SaveChangesToSelectedItem();
            // DeselectItem(); // Deselect after saving to clear editor
        }

        ImGui.SameLine();

        // Copy Command Button (uses current editor state for preview)
        var previewCommand = _selectedItem.GetCommand();
        if (ImGui.Button("Copiar Comando", new Vector2(140, 25))) ImGui.SetClipboardText(previewCommand);
        if (ImGui.IsItemHovered()) ImGui.SetTooltip($"Comando a copiar:\n{previewCommand}");
        ImGui.SameLine();

        // Cancel Edit Button
        if (ImGui.Button("Cancelar Edición", new Vector2(140, 25))) DeselectItem(); // Deselect without saving changes
    }

    /// <summary>
    ///     Renders the bottom section containing the list of ProcessedItems.
    /// </summary>
    private void RenderProcessedItemListSection()
    {
        ImGui.SeparatorText("Items en Lista");

        // Calculate dynamic height or use fixed? Let's make it dynamic.
        var listHeight = ImGui.GetContentRegionAvail().Y - ImGui.GetFrameHeightWithSpacing(); // Leave space for buttons below maybe
        if (listHeight < 100) listHeight = 100; // Minimum height

        ImGui.BeginChild("ProcessedItemsList", new Vector2(0, listHeight), ImGuiChildFlags.Borders, ImGuiWindowFlags.HorizontalScrollbar);
        ImGui.PushID("ProcessedItemList"); // Scope IDs

        ProcessedItem? itemToSelectOnClick = null; // Item to select after loop finishes
        var itemsToRemove = new List<ProcessedItem>();
        var itemsToDuplicate = new List<ProcessedItem>();
        var itemToMoveUp = -1;
        var itemToMoveDown = -1;


        var defaultColor = _style.Colors[(int)ImGuiCol.Text];
        var selectedColor = new Vector4(1f, 1f, 0.4f, 1f); // Yellowish for selected
        var selectedExcColor = new Vector4(_dynamicRed, _dynamicGreen, _dynamicBlue, 1.0f);
        var excColor = new Vector4(_dynamicRed, _dynamicGreen, _dynamicBlue, 1.0f);

        for (var i = 0; i < _processedItems.Count; i++)
        {
            var item = _processedItems[i];
            var decoratedName = item.GetDecoratedName(); // Use the method from ProcessedItem
            var isSelected = _selectedItem == item;

            // Determine text color
            var currentItemColor = defaultColor;
            if (item.OptionsList.Any()) currentItemColor = excColor; // Use dynamic color if has options
            if (isSelected)
                if (item.OptionsList.Any())
                    currentItemColor = selectedExcColor;
                else
                    currentItemColor = selectedColor;

            ImGui.PushStyleColor(ImGuiCol.Text, currentItemColor);

            // --- Selectable Item ---
            var label = $"[{i + 1}] {decoratedName}";
            if (ImGui.Selectable(label, isSelected, ImGuiSelectableFlags.AllowDoubleClick))
            {
                // Single click selects for editing
                itemToSelectOnClick = item;
                // Double click could potentially copy command? (Optional)
                if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                {
                    ImGui.SetClipboardText(item.GetCommand());
                    Console.WriteLine($"Comando copiado (doble click): {item.Name}");
                }
            }

            ImGui.PopStyleColor(); // Pop text color

            // --- Context Menu (Right Click) ---
            var popupId = $"ItemContext_{i}";
            if (ImGui.BeginPopupContextItem(popupId)) // Use default right-click trigger
            {
                ImGui.Text(decoratedName); // Show item name in menu
                ImGui.Separator();
                if (ImGui.MenuItem("Editar")) itemToSelectOnClick = item; // Mark for selection
                if (ImGui.MenuItem("Copiar Comando")) ImGui.SetClipboardText(item.GetCommand());
                if (ImGui.MenuItem("Duplicar")) itemsToDuplicate.Add(item);
                ImGui.Separator();
                if (ImGui.MenuItem("Eliminar", null, false, true)) itemsToRemove.Add(item); // Ensure enabled
                ImGui.Separator();
                if (ImGui.MenuItem("Mover Arriba", null, false, i > 0)) itemToMoveUp = i; // Enabled if not first
                if (ImGui.MenuItem("Mover Abajo", null, false, i < _processedItems.Count - 1)) itemToMoveDown = i; // Enabled if not last

                ImGui.EndPopup();
            }

            if (ImGui.IsItemHovered()) ImGui.SetTooltip("Click Izq: Editar\nDoble Click Izq: Copiar Comando\nClick Der: Opciones");
        }

        ImGui.PopID(); // Pop ProcessedItemList ID scope
        ImGui.EndChild(); // End ProcessedItemsList Child

        // --- Process Actions After Loop ---
        // Select item clicked
        if (itemToSelectOnClick != null && itemToSelectOnClick != _selectedItem)
        {
            // Save changes to previously selected item *before* switching
            SaveChangesToSelectedItem();
            SelectItemForEditing(itemToSelectOnClick);
        }

        // Remove items marked for removal
        foreach (var item in itemsToRemove)
        {
            if (_selectedItem == item) DeselectItem(); // Deselect if removing the edited item
            _processedItems.Remove(item);
        }

        // Duplicate items marked for duplication
        // Insert clones after the original index to avoid messing up iteration
        var insertionOffset = 0;
        foreach (var item in itemsToDuplicate)
        {
            var originalIndex = _processedItems.IndexOf(item);
            if (originalIndex != -1)
            {
                _processedItems.Insert(originalIndex + 1 + insertionOffset, item.Clone());
                insertionOffset++; // Offset subsequent insertions
            }
        }

        // Move items
        if (itemToMoveUp != -1) SwapItems(itemToMoveUp, itemToMoveUp - 1);
        if (itemToMoveDown != -1) SwapItems(itemToMoveDown, itemToMoveDown + 1);


        // --- Footer Buttons ---
        RenderListActionButtons();
    }

    /// <summary>
    ///     Renders buttons acting on the whole list (Copy All, Clear All).
    /// </summary>
    private void RenderListActionButtons()
    {
        ImGui.Separator();
        if (ImGui.Button("Copiar Todos los Comandos", new Vector2(200, 25))) CopyAllCommands();
        ImGui.SameLine();
        // Add confirmation for clearing list
        var clearPopupId = "ConfirmClearListPopup";
        if (ImGui.Button("Limpiar Lista", new Vector2(120, 25)))
            if (_processedItems.Any()) // Only open popup if list is not empty
                ImGui.OpenPopup(clearPopupId);

        // Confirmation Popup for Clear List
        var center = ImGui.GetMainViewport().GetCenter();
        ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
        if (ImGui.BeginPopupModal(clearPopupId, ref _deleteConfirmationPopupOpen, ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.Text("¿Estás seguro de que quieres borrar TODOS los items de la lista?");
            ImGui.Text("Esta acción no se puede deshacer.");
            ImGui.Separator();
            if (ImGui.Button("Sí, Borrar Todo", new Vector2(120, 0)))
            {
                ClearAllItems();
                ImGui.CloseCurrentPopup();
            }

            ImGui.SameLine();
            if (ImGui.Button("Cancelar", new Vector2(120, 0))) ImGui.CloseCurrentPopup();
            ImGui.EndPopup();
        }
    }

    #endregion

    #region Logic Methods

    /// <summary>
    ///     Sets the item to be edited and loads its data into the editor buffers.
    /// </summary>
    private void SelectItemForEditing(ProcessedItem item)
    {
        if (item == null) return;
        _selectedItem = item;
        // Load data from selected item into edit buffers
        _editPlusLevel = item.PlusLevel;
        _editSkill = item.Skill;
        _editLuck = item.Luck;
        _editExcellentOption = item.ExcellentOption;
        _editSelectedExcOptions = new List<Option>(item.OptionsList); // Create a copy for editing
        _editFullOption = item.IsFullExcellent(); // Check if it meets "Full" criteria
        Console.WriteLine($"Selected '{item.Name}' for editing.");
    }

    /// <summary>
    ///     Clears the item selection and resets the editor buffers.
    /// </summary>
    private void DeselectItem()
    {
        if (_selectedItem == null) return; // No need to deselect if nothing is selected

        _selectedItem = null;
        // Reset edit buffers to default values
        _editPlusLevel = 0;
        _editSkill = false;
        _editLuck = false;
        _editExcellentOption = 0;
        _editSelectedExcOptions.Clear();
        _editFullOption = false;
        Console.WriteLine("Item deselected, editor cleared.");
    }

    /// <summary>
    ///     Saves the data from the editor buffers back to the currently selected ProcessedItem.
    /// </summary>
    private void SaveChangesToSelectedItem()
    {
        if (_selectedItem == null)
        {
            Console.WriteLine("Save attempted but no item selected.");
            return; // Nothing to save if no item is selected
        }

        // Apply buffer values back to the ProcessedItem object in the list
        _selectedItem.PlusLevel = _editPlusLevel;
        _selectedItem.Skill = _editSkill;
        _selectedItem.Luck = _editLuck;
        _selectedItem.ExcellentOption = _editExcellentOption;
        _selectedItem.OptionsList = new List<Option>(_editSelectedExcOptions); // Assign the edited list back

        Console.WriteLine($"Changes saved for item: {_selectedItem.Name}");
    }

    /// <summary>
    ///     Copies the generated commands for all items in the list to the clipboard.
    /// </summary>
    private void CopyAllCommands()
    {
        if (!_processedItems.Any()) return;
        // Join commands with newline separator
        var allCommands = string.Join(Environment.NewLine, _processedItems.Select(item => item.GetCommand()));
        ImGui.SetClipboardText(allCommands);
        Console.WriteLine($"Copiados {_processedItems.Count} comandos al portapapeles.");
        // Optionally show a temporary success message in UI
    }

    /// <summary>
    ///     Clears all items from the list and resets the editor.
    /// </summary>
    private void ClearAllItems()
    {
        _processedItems.Clear();
        DeselectItem(); // Ensure editor is also cleared
        Console.WriteLine("Lista de recompensas borrada.");
    }


    /// <summary>
    ///     Swaps two items in the processed items list by their indices.
    /// </summary>
    private void SwapItems(int indexA, int indexB)
    {
        if (indexA >= 0 && indexA < _processedItems.Count &&
            indexB >= 0 && indexB < _processedItems.Count &&
            indexA != indexB)
        {
            // Simple tuple swap
            (_processedItems[indexA], _processedItems[indexB]) = (_processedItems[indexB], _processedItems[indexA]);
            Console.WriteLine($"Swapped items at index {indexA} and {indexB}.");
        }
    }

    #endregion
}