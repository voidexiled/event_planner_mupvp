using System.Numerics;
using event_planner_mupvp.classes.core;
using event_planner_mupvp.classes.utils;
using event_planner_mupvp.lib;
using ImGuiNET;
using SixLabors.ImageSharp.Memory;

namespace event_planner_mupvp.classes.UI.Renderers;

// classes/UI/Renderers/GuideBrowserWindowRenderer.cs

public class GuideBrowserWindowRenderer : IWindowRenderer
{
    #region State

    public static Dictionary<int, byte[]> ImageBytes = new();
    private List<Guide> _allGuides = ApiManager.GetGuidesFromDB();
    private List<Guide> _filteredGuides = new();
    private string _searchInputGuides = "";
    private bool _needsRefresh = true; // Flag to reload guides on open

    // Dynamic colors for featured guides
    private float _dynamicRed = 1.0f;
    private float _dynamicGreen = 1.0f;
    private float _dynamicBlue;

    // Dependency
    private readonly SelectedGuideWindowRenderer _selectedGuideRenderer;

    #endregion

    #region Constructor

    public GuideBrowserWindowRenderer(SelectedGuideWindowRenderer selectedGuideRenderer)
    {
        _selectedGuideRenderer = selectedGuideRenderer;
    }

    #endregion

    #region IWindowRenderer Implementation

    public void Render()
    {
        if (_needsRefresh) RefreshGuides(); // Load guides if needed

        UpdateFilteredGuides(); // Filter on each frame

        ImGui.SetNextWindowSize(new Vector2(380, 280), ImGuiCond.FirstUseEver);
        if (ImGui.Begin("Explorar Guías", ref WindowManager.ShowGuideBrowserWindow))
        {
            RenderSearchBar();
            ImGui.Separator();
            RenderGuideList();
        }

        if (!WindowManager.ShowGuideBrowserWindow) OnClose();
        ImGui.End();
    }

    public void OnOpen()
    {
        _needsRefresh = true; // Mark for refresh when opened
        Console.WriteLine("Guide Browser Opened - Marked for refresh.");
    }

    public void OnClose()
    {
        _searchInputGuides = ""; // Clear search on close
        // Don't clear _allGuides unless you want to force reload every time
        _filteredGuides.Clear();
        Console.WriteLine("Guide Browser Closed - Search cleared.");
        // Do NOT close SelectedGuideWindow here automatically, only if user closes browser explicitly
    }

    public void UpdateDynamicColors(float r, float g, float b)
    {
        _dynamicRed = r;
        _dynamicGreen = g;
        _dynamicBlue = b;
    }

    #endregion

    #region UI Rendering

    private void RenderSearchBar()
    {
        ImGui.Text("Buscar:");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(-1); // Ocupar resto de espacio
        if (ImGui.InputTextWithHint("##SearchInputGuides", "Buscar por título, autor, tag...", ref _searchInputGuides, 100))
        {
            // Filtering happens in UpdateFilteredGuides
        }
    }

    private void RenderGuideList()
    {
        float listHeight = 150; // Altura fija como en original
        ImGui.BeginChild("GuideListChild", new Vector2(0, listHeight), ImGuiChildFlags.Borders);

        if (_filteredGuides.Count == 0 && !_needsRefresh)
        {
            ImGui.Text("No se encontraron guías.");
        }
        else if (_needsRefresh)
        {
            ImGui.Text("Cargando guías...");
        }
        else
        {
            Vector4 defaultColor = ImGui.GetStyle().Colors[(int)ImGuiCol.Text];
            Vector4 featuredColor = new Vector4(_dynamicRed, _dynamicGreen, _dynamicBlue, 1.0f);

            // --- Simple foreach loop ---
            for (int i = 0; i < _filteredGuides.Count; i++)
            {
                var guide = _filteredGuides[i];
                bool isFeatured = guide.IsFeatured;

                if (isFeatured) ImGui.PushStyleColor(ImGuiCol.Text, featuredColor);

                string label = $"{guide.Title} - {guide.Author}";

                ImGui.PushID($"guide_{guide.Id}_{i}"); // Unique ID
                if (ImGui.Selectable(label))
                {
                    HandleGuideSelection(guide);
                }
                ImGui.PopID();

                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip($"{(isFeatured ? "[DESTACADA] " : "")}Click para ver la guía '{guide.Title}'");
                }

                if (isFeatured) ImGui.PopStyleColor();
            }
            // --- End simple foreach loop ---
        }
        ImGui.EndChild();
    }

    #endregion

    #region Logic

    public async Task LoadAllGuideImagesAsync()
    {
        using var webClient = new HttpClient();
        Console.Write($"All Guides: {_allGuides.Count}");
        foreach (var guide in _allGuides)
        {
            var image = webClient.GetAsync(guide.Image).Result.Content.ReadAsByteArrayAsync().Result;
            GuideBrowserWindowRenderer.ImageBytes.Add(guide.Id, image);
            // Image img = Image.Load(image);

        }
        
        Console.WriteLine($"Imagenes cargadas {ImageBytes.Count}");
        // await Task.Delay(100); // Simular carga
    }
    
    private void RefreshGuides()
    {
        Console.WriteLine("Refreshing guides from API...");
        try
        {
            // Assuming ApiManager.GetGuidesFromDB() fetches the latest guides
            _allGuides = ApiManager.GetGuidesFromDB() ?? new List<Guide>();
            _needsRefresh = false; // Mark as refreshed
            UpdateFilteredGuides(); // Update filter immediately after refresh
            Console.WriteLine($"Guides refreshed: {_allGuides.Count} loaded.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error refreshing guides: {ex.Message}");
            // Handle error, maybe show a message in the UI
            _allGuides = new List<Guide>(); // Clear list on error
            _needsRefresh = false; // Avoid retry loop if API is down
        }
    }

    private void UpdateFilteredGuides()
    {
        if (_needsRefresh) return; // Don't filter if data isn't loaded

        if (string.IsNullOrWhiteSpace(_searchInputGuides))
        {
            // No search term, show all, featured first
            _filteredGuides = _allGuides.OrderByDescending(g => g.IsFeatured).ThenBy(g => g.Title).ToList();
        }
        else
        {
            // Apply search logic (case-insensitive, diacritic-insensitive)
            string searchTerm = StringUtils.RemoveDiacritics(_searchInputGuides.ToLowerInvariant());

            // Simple relevance: Title > Tags > Author
            _filteredGuides = _allGuides
                .Select(guide => new
                {
                    Guide = guide,
                    Score = CalculateRelevance(guide, searchTerm)
                })
                .Where(x => x.Score > 0) // Only include guides with some match
                .OrderByDescending(x => x.Guide.IsFeatured) // Featured first among matches
                .ThenByDescending(x => x.Score) // Then by relevance score
                .ThenBy(x => x.Guide.Title) // Then alphabetically
                .Select(x => x.Guide)
                .ToList();
        }
    }

    private int CalculateRelevance(Guide guide, string searchTerm)
    {
        var score = 0;
        if (StringUtils.RemoveDiacritics(guide.Title.ToLowerInvariant()).Contains(searchTerm)) score += 10;
        if (guide.Tags.Any(tag => StringUtils.RemoveDiacritics(tag.ToLowerInvariant()).Contains(searchTerm))) score += 5;
        if (StringUtils.RemoveDiacritics(guide.Author.ToLowerInvariant()).Contains(searchTerm)) score += 2;
        // Add relevance for body search if needed (can be slow)
        // if (guide.Body.Any(p => StringUtils.RemoveDiacritics(p.ToLowerInvariant()).Contains(searchTerm))) score += 1;
        return score;
    }

    private void HandleGuideSelection(Guide selectedGuide)
    {
        Console.WriteLine($"Guide selected: {selectedGuide.Title}");
        _selectedGuideRenderer.SetSelectedGuide(selectedGuide); // Pass guide to the viewer
        WindowManager.ShowSelectedGuideWindow = true; // Show the viewer window
        // Optionally close the browser or keep it open? Current logic keeps it open.
    }

    #endregion
}