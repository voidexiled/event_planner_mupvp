using System.Numerics;
using event_planner_mupvp.classes.core;
using event_planner_mupvp.classes.utils;
using event_planner_mupvp.lib;
using ImGuiNET;

namespace event_planner_mupvp.classes.UI.Renderers;

// classes/UI/Renderers/GuideEditorWindowRenderer.cs
// For StringUtils Join/Split

public class GuideEditorWindowRenderer : IWindowRenderer
{
    #region State

    private Guide? _guideToEdit; // Null if creating new guide
    private bool _isCreatingNew;

    // --- Edit Buffer Fields ---
    private string _editTitle = "";
    private string _editAuthor = "";
    private string _editImage = "";
    private string _editVideo = "";
    private string _editBody = "";
    private string _editTags = ""; // Comma-separated string for input
    private bool _editIsFeatured;
    private List<Guide> _editSelectedRelatedGuides = new(); // List of full Guide objects

    // --- Related Guides Popup State ---
    private const string RelatedGuidesPopupId = "SelectRelatedGuidesPopup";
    private bool _openRelatedGuidesPopup;
    private string _relatedSearchInput = "";
    private List<Guide> _allGuidesForPopup = new(); // Cache for popup
    private List<Guide> _filteredAvailableGuides = new();

    #endregion

    #region Constructor

    #endregion

    #region IWindowRenderer Implementation

    public void Render()
    {
        var windowTitle = _isCreatingNew ? "Agregar Nueva Guía" : $"Editando Guía: [{_guideToEdit?.Id}] {_guideToEdit?.Title}";
        ImGui.SetNextWindowSize(new Vector2(600, 700), ImGuiCond.FirstUseEver); // Larger window for editor

        if (ImGui.Begin(windowTitle, ref WindowManager.ShowGuideEditorWindow, ImGuiWindowFlags.MenuBar)) // Add MenuBar flag
        {
            RenderMenuBar(); // Add Save/Cancel actions to menu bar

            RenderEditorFields();
            ImGui.Separator();
            RenderActionButtons(); // Keep Save/Cancel buttons at bottom too? Optional.
        }

        if (!WindowManager.ShowGuideEditorWindow) OnClose(); // Handle 'X' button close
        ImGui.End();

        // Render the popup if flag is set
        if (_openRelatedGuidesPopup) RenderRelatedGuidesPopup();
    }

    public void OnOpen()
    {
        // State is set by OpenForNewGuide or OpenForEdit
        Console.WriteLine($"Guide Editor opened. Mode: {(_isCreatingNew ? "New" : "Edit")}");
    }

    public void OnClose()
    {
        // Reset all state when window closes
        _guideToEdit = null;
        _isCreatingNew = false;
        _editTitle = "";
        _editAuthor = "";
        _editImage = "";
        _editVideo = "";
        _editBody = "";
        _editTags = "";
        _editIsFeatured = false;
        _editSelectedRelatedGuides.Clear();
        _openRelatedGuidesPopup = false; // Ensure popup closes
        _relatedSearchInput = "";
        _allGuidesForPopup.Clear();
        _filteredAvailableGuides.Clear();
        Console.WriteLine("Guide Editor closed and state cleared.");
    }

    // Note: UpdateDynamicColors might not be needed here unless displaying something special

    #endregion

    #region Public Control Methods

    /// <summary>
    ///     Configures the editor to create a new guide.
    /// </summary>
    public void OpenForNewGuide()
    {
        OnClose(); // Clear previous state first
        _isCreatingNew = true;
        _guideToEdit = null; // Ensure no guide is being edited
        // Default values can be set here if needed
        _editAuthor = SessionManager.currentUser?.Key ?? "Unknown"; // Default author to current user?

        WindowManager.ShowGuideEditorWindow = true; // Set flag to show
    }

    /// <summary>
    ///     Configures the editor to edit an existing guide.
    /// </summary>
    /// <param name="guide">The guide to edit.</param>
    public void OpenForEdit(Guide guide)
    {
        OnClose(); // Clear previous state
        if (guide == null) return;

        _isCreatingNew = false;
        _guideToEdit = guide;

        // Load guide data into edit buffers
        _editTitle = guide.Title;
        _editAuthor = guide.Author;
        _editImage = guide.Image;
        _editVideo = guide.Video;
        _editBody = string.Join("\n", guide.Body); // Join list into multiline string
        _editTags = string.Join(",", guide.Tags); // Join list into CSV string
        _editIsFeatured = guide.IsFeatured;

        // Load related guides (requires fetching full objects)
        _editSelectedRelatedGuides = ApiManager.GetGuidesFromDB() // Inefficient: Fetch only needed IDs ideally
            .Where(g => guide.RelatedGuides.Contains(g.Id))
            .ToList();

        WindowManager.ShowGuideEditorWindow = true; // Set flag to show
    }

    #endregion

    #region UI Rendering Sections

    private void RenderMenuBar()
    {
        if (ImGui.BeginMenuBar())
        {
            if (ImGui.MenuItem("Guardar")) SaveGuide();
            if (ImGui.MenuItem("Cancelar")) WindowManager.ShowGuideEditorWindow = false; // Close without saving
            // Add other menu items if needed (e.g., Reset Fields)
            ImGui.EndMenuBar();
        }
    }

    private void RenderEditorFields()
    {
        // Input fields using the _edit* variables
        ImGui.InputText("Título", ref _editTitle, 100);
        ImGui.InputText("Autor", ref _editAuthor, 100);
        ImGui.InputText("URL Imagen", ref _editImage, 200);
        ImGui.InputText("URL Video", ref _editVideo, 200);
        ImGui.Text("Cuerpo (Contenido):");
        ImGui.InputTextMultiline("##Body", ref _editBody, 10000, new Vector2(-1, ImGui.GetTextLineHeight() * 15)); // Large input area
        ImGui.InputText("Tags (separados por coma)", ref _editTags, 200);
        ImGui.Checkbox("Destacada", ref _editIsFeatured);

        ImGui.SeparatorText("Guías Relacionadas");
        RenderSelectedRelatedGuidesList(); // Show currently selected related guides
        if (ImGui.Button("Seleccionar Guías Relacionadas...")) OpenRelatedGuidesPopup(); // Opens the modal popup
    }

    private void RenderSelectedRelatedGuidesList()
    {
        // Display currently selected related guides in a small, non-interactive list perhaps
        if (_editSelectedRelatedGuides.Any())
        {
            ImGui.BeginChild("CurrentRelated", new Vector2(-1, ImGui.GetTextLineHeightWithSpacing() * 3), ImGuiChildFlags.Borders);
            ImGui.Text("Seleccionadas:");
            foreach (var related in _editSelectedRelatedGuides)
            {
                ImGui.TextDisabled($"- [{related.Id}] {related.Title}");
                ImGui.SameLine();
                // Add a small 'x' button to remove?
                if (ImGui.SmallButton($"X##{related.Id}"))
                {
                    _editSelectedRelatedGuides.Remove(related);
                    break; // Avoid modifying list while iterating fully
                }
            }

            ImGui.EndChild();
        }
        else
        {
            ImGui.TextDisabled("Ninguna guía relacionada seleccionada.");
        }
    }

    private void RenderActionButtons()
    {
        // Optional: Keep buttons at the bottom as well as menu bar
        if (ImGui.Button("Guardar Cambios", new Vector2(120, 30))) SaveGuide();
        ImGui.SameLine();
        if (ImGui.Button("Cancelar", new Vector2(120, 30))) WindowManager.ShowGuideEditorWindow = false;
    }

    #endregion

    #region Related Guides Popup

    private void OpenRelatedGuidesPopup()
    {
        // Load all guides for the popup selection
        _allGuidesForPopup = ApiManager.GetGuidesFromDB() ?? new List<Guide>();
        // Filter out the guide being edited itself, if applicable
        if (!_isCreatingNew && _guideToEdit != null) _allGuidesForPopup.RemoveAll(g => g.Id == _guideToEdit.Id);
        UpdateFilteredAvailableGuides(); // Initial filter for popup
        _openRelatedGuidesPopup = true;
        ImGui.OpenPopup(RelatedGuidesPopupId); // Use ImGui's popup system
    }

    private void RenderRelatedGuidesPopup()
    {
        // Center the popup
        var center = ImGui.GetMainViewport().GetCenter();
        ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
        ImGui.SetNextWindowSize(new Vector2(700, 450), ImGuiCond.Appearing);

        if (ImGui.BeginPopupModal(RelatedGuidesPopupId, ref _openRelatedGuidesPopup, ImGuiWindowFlags.NoResize))
        {
            ImGui.Text("Seleccionar Guías para Relacionar");
            ImGui.Separator();

            // Search bar within popup
            ImGui.InputTextWithHint("##RelatedSearch", "Buscar disponibles...", ref _relatedSearchInput, 100);
            if (ImGui.IsItemEdited()) UpdateFilteredAvailableGuides(); // Update filter as user types

            // Two columns: Available | Selected
            var columnWidth = (ImGui.GetContentRegionAvail().X - ImGui.GetStyle().ItemSpacing.X) * 0.5f;
            var listHeight = ImGui.GetContentRegionAvail().Y - ImGui.GetFrameHeightWithSpacing() * 2.5f; // Leave space for buttons

            // --- Available Guides ---
            ImGui.BeginChild("AvailableGuides", new Vector2(columnWidth, listHeight), ImGuiChildFlags.Borders);
            ImGui.Text("Disponibles:");
            ImGui.Separator();
            var guidesToAdd = new List<Guide>();
            foreach (var guide in _filteredAvailableGuides)
                // Only show guides *not* already selected
                if (!_editSelectedRelatedGuides.Any(g => g.Id == guide.Id))
                {
                    if (ImGui.Selectable($"[{guide.Id}] {guide.Title}")) guidesToAdd.Add(guide);
                    if (ImGui.IsItemHovered()) ImGui.SetTooltip("Click para añadir a Relacionadas");
                }

            ImGui.EndChild();

            // Add selected guides
            foreach (var g in guidesToAdd) _editSelectedRelatedGuides.Add(g);

            ImGui.SameLine();

            // --- Currently Selected Guides ---
            ImGui.BeginChild("SelectedGuidesInPopup", new Vector2(0, listHeight), ImGuiChildFlags.Borders);
            ImGui.Text("Relacionadas (Seleccionadas):");
            ImGui.Separator();
            var guidesToRemove = new List<Guide>();
            foreach (var guide in _editSelectedRelatedGuides)
            {
                if (ImGui.Selectable($"[{guide.Id}] {guide.Title}")) guidesToRemove.Add(guide);
                if (ImGui.IsItemHovered()) ImGui.SetTooltip("Click para quitar de Relacionadas");
            }

            ImGui.EndChild();

            // Remove deselected guides
            foreach (var g in guidesToRemove) _editSelectedRelatedGuides.RemoveAll(x => x.Id == g.Id);

            ImGui.Separator();
            if (ImGui.Button("Aceptar", new Vector2(120, 0)))
            {
                _openRelatedGuidesPopup = false; // Close the popup
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }
        else // If popup was closed by clicking outside or Esc
        {
            _openRelatedGuidesPopup = false; // Ensure flag is reset
        }
    }

    private void UpdateFilteredAvailableGuides()
    {
        var query = _allGuidesForPopup.AsEnumerable();
        // Filter out the guide being edited itself
        if (!_isCreatingNew && _guideToEdit != null) query = query.Where(g => g.Id != _guideToEdit.Id);

        // Apply search term
        if (!string.IsNullOrWhiteSpace(_relatedSearchInput))
        {
            var searchTerm = StringUtils.RemoveDiacritics(_relatedSearchInput.ToLowerInvariant());
            // Simple filter by ID, Title, Author
            query = query.Where(g => g.Id.ToString().Contains(searchTerm) ||
                                     StringUtils.RemoveDiacritics(g.Title.ToLowerInvariant()).Contains(searchTerm) ||
                                     StringUtils.RemoveDiacritics(g.Author.ToLowerInvariant()).Contains(searchTerm));
        }

        _filteredAvailableGuides = query.OrderBy(g => g.Title).ToList();
    }

    #endregion

    #region Save Logic

    private void SaveGuide()
    {
        Console.WriteLine("Intentando guardar guía...");
        // Basic Validation
        if (string.IsNullOrWhiteSpace(_editTitle) || string.IsNullOrWhiteSpace(_editAuthor))
        {
            Console.WriteLine("Error: Título y Autor no pueden estar vacíos.");
            // TODO: Show error message to user via ImGui
            ApiManager.adminMessage = "Error: Título y Autor no pueden estar vacíos."; // Use existing message system
            ApiManager.messageColor = new Vector4(1, 0, 0, 1);
            return;
        }

        // Create or Update Guide Object
        Guide guideToSave;
        if (_isCreatingNew)
        {
            guideToSave = new Guide(); // Assuming Guide class has parameterless constructor or factory
            Console.WriteLine("Creando nueva guía...");
        }
        else if (_guideToEdit != null)
        {
            guideToSave = _guideToEdit; // Modify existing object
            Console.WriteLine($"Actualizando guía ID: {guideToSave.Id}...");
        }
        else
        {
            Console.WriteLine("Error: Intento de guardar sin guía válida seleccionada.");
            ApiManager.adminMessage = "Error interno al guardar.";
            ApiManager.messageColor = new Vector4(1, 0, 0, 1);
            return;
        }

        // Populate guideToSave with data from edit buffers
        guideToSave.Title = _editTitle;
        guideToSave.Author = _editAuthor;
        guideToSave.Image = _editImage;
        guideToSave.Video = _editVideo;
        guideToSave.Body = _editBody.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList(); // Split multiline text
        guideToSave.Tags = _editTags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList(); // Split CSV text
        guideToSave.IsFeatured = _editIsFeatured;
        guideToSave.RelatedGuides = _editSelectedRelatedGuides.Select(g => g.Id).ToList(); // Get IDs from selected guides

        // Call ApiManager
        try
        {
            if (_isCreatingNew)
                ApiManager.AddGuide(guideToSave); // Assumes this method exists
            // Optionally clear fields after successful add?
            // OnClose(); // This would close the window too
            // ResetFieldsForNew(); // Better: just clear fields
            else
                ApiManager.UpdateGuide(guideToSave); // Assumes this method exists

            // Close editor window after successful save
            WindowManager.ShowGuideEditorWindow = false;
            Console.WriteLine("Guía guardada exitosamente.");
            // Optionally trigger refresh in AdminGuidesWindow if it's open
            // _adminGuidesWindowRenderer?.MarkForRefresh();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error guardando guía vía API: {ex.Message}");
            ApiManager.adminMessage = $"Error al guardar: {ex.Message}";
            ApiManager.messageColor = new Vector4(1, 0, 0, 1);
            // Don't close window on error
        }
    }

    // Helper to reset fields without closing, after adding a new guide
    private void ResetFieldsForNew()
    {
        _editTitle = "";
        // Keep author maybe? _editAuthor = "";
        _editImage = "";
        _editVideo = "";
        _editBody = "";
        _editTags = "";
        _editIsFeatured = false;
        _editSelectedRelatedGuides.Clear();
    }

    #endregion
}