using System.Numerics;
using event_planner_mupvp.classes.core;
using event_planner_mupvp.classes.utils;
using event_planner_mupvp.lib;
using ImGuiNET;

namespace event_planner_mupvp.classes.UI.Renderers;

// classes/UI/Renderers/AdminGuidesWindowRenderer.cs
// Guide

public class AdminGuidesWindowRenderer : IWindowRenderer
{
    #region State

    private List<Guide> _allGuidesAdmin = new(); // Cache for admin view
    private List<Guide> _filteredAdminGuides = new();
    private string _searchInputAdminGuides = "";
    private bool _needsAdminRefresh = true;

    // Dynamic colors
    private float _dynamicRed = 1.0f;
    private float _dynamicGreen = 1.0f;
    private float _dynamicBlue;

    // Dependency
    private readonly GuideEditorWindowRenderer _guideEditorRenderer; // To open guides for editing

    #endregion

    #region Constructor

    public AdminGuidesWindowRenderer(GuideEditorWindowRenderer guideEditorRenderer)
    {
        _guideEditorRenderer = guideEditorRenderer;
    }

    #endregion

    #region IWindowRenderer Implementation

    public void Render()
    {
        if (_needsAdminRefresh) RefreshAdminGuides();
        UpdateFilteredAdminGuides();

        ImGui.SetNextWindowSize(new Vector2(500, 350), ImGuiCond.FirstUseEver);
        if (ImGui.Begin("Administración de Guías", ref WindowManager.ShowAdminGuidesWindow))
        {
            RenderControls();
            ImGui.Separator();
            RenderAdminGuideList();
        }

        if (!WindowManager.ShowAdminGuidesWindow) OnClose();
        ImGui.End();
    }

    public void OnOpen()
    {
        _needsAdminRefresh = true; // Refresh when opened
        Console.WriteLine("Admin Guides Window Opened - Marked for refresh.");
    }

    public void OnClose()
    {
        _searchInputAdminGuides = "";
        _filteredAdminGuides.Clear();
        // Don't clear _allGuidesAdmin unless forcing reload every time
        Console.WriteLine("Admin Guides Window closed.");
        // Do not automatically close the editor if it was opened from here
    }

    public void UpdateDynamicColors(float r, float g, float b)
    {
        _dynamicRed = r;
        _dynamicGreen = g;
        _dynamicBlue = b;
    }

    #endregion

    #region UI Rendering

    private void RenderControls()
    {
        // Button to open the editor for creating a NEW guide
        if (ImGui.Button("Agregar Nueva Guía", new Vector2(150, 25)))
        {
            _guideEditorRenderer.OpenForNewGuide(); // Tell editor to open in 'new' mode
            WindowManager.ShowGuideEditorWindow = true;
            // Optionally close this admin window or keep it open? Let's keep it open.
        }

        ImGui.SameLine();
        // Button to force refresh
        if (ImGui.Button("Refrescar Lista", new Vector2(120, 25))) RefreshAdminGuides();
        ImGui.Spacing();

        // Search Bar
        ImGui.Text("Buscar:");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##SearchInputAdminGuides", "Buscar por ID, título, autor, tag...", ref _searchInputAdminGuides, 100);
    }

    private void RenderAdminGuideList()
    {
        var listHeight = ImGui.GetContentRegionAvail().Y - ImGui.GetStyle().ItemSpacing.Y;
        ImGui.BeginChild("AdminGuideListChild", new Vector2(0, listHeight), ImGuiChildFlags.Borders);

        if (_needsAdminRefresh)
        {
            ImGui.Text("Cargando guías...");
        }
        else if (_filteredAdminGuides.Count == 0)
        {
            ImGui.Text("No se encontraron guías con los filtros actuales.");
        }
        else
        {
            var defaultColor = ImGui.GetStyle().Colors[(int)ImGuiCol.Text];
            var featuredColor = new Vector4(_dynamicRed, _dynamicGreen, _dynamicBlue, 1.0f);

            // --- Simple foreach loop ---
            for (var i = 0; i < _filteredAdminGuides.Count; i++)
            {
                var guide = _filteredAdminGuides[i];
                var isFeatured = guide.IsFeatured;

                if (isFeatured) ImGui.PushStyleColor(ImGuiCol.Text, featuredColor);

                // Include ID for admin view
                var label = $"[{guide.Id}] {guide.Title} - {guide.Author}";

                ImGui.PushID($"admin_guide_{guide.Id}_{i}"); // Unique ID
                if (ImGui.Selectable(label))
                {
                    // Open this guide in the editor for editing
                    _guideEditorRenderer.OpenForEdit(guide);
                    WindowManager.ShowGuideEditorWindow = true;
                }

                ImGui.PopID();

                if (ImGui.IsItemHovered()) ImGui.SetTooltip($"{(isFeatured ? "[DESTACADA] " : "")}Click para editar la guía ID: {guide.Id}");

                if (isFeatured) ImGui.PopStyleColor();
            }
            // --- End simple foreach loop ---
        }

        ImGui.EndChild();
    }

    #endregion

    #region Logic

    private void RefreshAdminGuides()
    {
        Console.WriteLine("Refreshing admin guides list...");
        try
        {
            _allGuidesAdmin = ApiManager.GetGuidesFromDB() ?? new List<Guide>();
            _needsAdminRefresh = false;
            UpdateFilteredAdminGuides(); // Apply filter after refresh
            Console.WriteLine($"Admin guides refreshed: {_allGuidesAdmin.Count} loaded.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error refreshing admin guides: {ex.Message}");
            _allGuidesAdmin = new List<Guide>();
            _needsAdminRefresh = false;
        }
    }

    private void UpdateFilteredAdminGuides()
    {
        if (_needsAdminRefresh) return;

        if (string.IsNullOrWhiteSpace(_searchInputAdminGuides))
        {
            // No search: Show all, featured first, then by ID maybe?
            _filteredAdminGuides = _allGuidesAdmin.OrderByDescending(g => g.IsFeatured).ThenBy(g => g.Id).ToList();
        }
        else
        {
            var searchTerm = StringUtils.RemoveDiacritics(_searchInputAdminGuides.ToLowerInvariant());

            // Attempt to parse search term as ID first
            var isIdSearch = int.TryParse(searchTerm, out var searchId);


            _filteredAdminGuides = _allGuidesAdmin
                .Select(guide => new
                {
                    Guide = guide,
                    Score = CalculateAdminRelevance(guide, searchTerm, isIdSearch, searchId)
                })
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Guide.IsFeatured)
                .ThenByDescending(x => x.Score)
                .ThenBy(x => x.Guide.Id)
                .Select(x => x.Guide)
                .ToList();
        }
    }

    private int CalculateAdminRelevance(Guide guide, string searchTerm, bool isIdSearch, int searchId)
    {
        var score = 0;
        // High score for exact ID match
        if (isIdSearch && guide.Id == searchId) score += 100;
        // Standard relevance calculation (can reuse user one or make specific)
        if (StringUtils.RemoveDiacritics(guide.Title.ToLowerInvariant()).Contains(searchTerm)) score += 10;
        if (guide.Tags.Any(tag => StringUtils.RemoveDiacritics(tag.ToLowerInvariant()).Contains(searchTerm))) score += 5;
        if (StringUtils.RemoveDiacritics(guide.Author.ToLowerInvariant()).Contains(searchTerm)) score += 2;
        return score;
    }

    #endregion
}