using ImGuiNET;
using ClickableTransparentOverlay;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Globalization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics;
using System.Net;

namespace MUPVPUI
{
    public class RenderClass : Overlay
    {
        // static string guidesJson = System.IO.File.ReadAllText("guides.json"); // obtener las guias
        // List<Guide> guides = JsonConvert.DeserializeObject<List<Guide>>(guidesJson);
        readonly string currentVersion = "0.0.0.1";
        string urlVersion = "";

        List<Guide> filteredGuides = new();
        List<Guide> filteredAdminGuides = new();
        Guide selectedGuide = null;
        Guide selectedAdminGuide = null;

        List<Guide> guidesHistory = new();
        string searchInputGuides = "";
        string searchInputAdminGuides = "";
        ImGuiStylePtr oldStyle = ImGui.GetStyle();
        ImGuiStylePtr style = ImGui.GetStyle();

        bool enabled_overlay = true;

        private List<ProcessedItem> processedItems = new();
        private ProcessedItem selectedItem = new();

        private float excRedColor = 1.0f;
        private float excGreenColor = 1.0f;
        private float excBlueColor = 0.0f;

        private int selectedGroup = 0;
        private int selectedType = 0;
        private int selectedPlusLevel = 0;
        private bool selectedSkill = false;
        private bool selectedLuck = false;
        private bool fullOption = false;
        private int selectedOption = 0;

        private int selectedExcellentOption = 0;
        private int selectedTime = 0;

        private List<Option> selectedOptions = new();

        private List<Item> filteredItems = new();
        private List<ArmorSet> filteredSets = new();
        private string setNameFilterString = "";
        private string nameFilter = "";
        private List<ClassTypes> classFilters = new();
        private int typeFilter = -1;
        private ItemGroups groupFilter = ItemGroups.ALL_ITEMS;
        private bool sortByTypeAscending = true;

        [StructLayout(LayoutKind.Sequential)]
        struct ScreenSize
        {
            public int Width;
            public int Height;
        }

        // Importa la función GetSystemMetrics
        [DllImport("user32.dll")]
        static extern int GetSystemMetrics(int nIndex);

        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(int nKey);

        // Constantes para GetSystemMetrics
        const int SM_CXSCREEN = 0;
        const int SM_CYSCREEN = 1;

        // Obtiene el ancho y alto de la pantalla
        static readonly int screenWidth = GetSystemMetrics(SM_CXSCREEN);
        static readonly int screenHeight = GetSystemMetrics(SM_CYSCREEN) - 50;

        Vector2 centerOfScreenView = new(screenWidth * 0.5f, screenHeight * 0.5f);
        Vector2 previousGuideWindowPosition = Vector2.Zero;
        private double time = 0.0;
        private double speed = 3.0;

        private string sessionKey = "";

        private string keyUserInput = "";
        private string rankUserInput = "";

        private string selectedGuideTitle = "";
        private string selectedGuideAuthor = "";
        private string selectedGuideImage = "";
        private string selectedGuideVideo = "";
        private string selectedGuideBody = "";
        private string selectedGuideTags = "";
        private string selectedGuideRelatedGuides = "";
        private bool selectedGuideIsFeatured = false;

        private void UpdateColor()
        {
            time += 0.016 * speed;

            double red = Math.Sin(time) * 0.5 + 0.5;
            double green = Math.Sin(time + 2.0 * Math.PI / 3.0) * 0.5 + 0.5;
            double blue = Math.Sin(time + 4.0 * Math.PI / 3.0) * 0.5 + 0.5;

            excRedColor = (float)red;
            excGreenColor = (float)green;
            excBlueColor = (float)blue;
            //Console.Write($"{excRedColor} {excGreenColor} {excBlueColor} \n");

        }

        private void RenderizeOwnerTools()
        {
            ImGui.SeparatorText("Opciones de Owner");
            if (ImGui.Button("Administracion de usuarios", new Vector2(240, 25)))
            {
                // WindowManager.HandleWindow(WindowName.UserAdministration);
                WindowManager.WINDOW_USER_ADMINISTRATION = !WindowManager.WINDOW_USER_ADMINISTRATION;
            }
            if (WindowManager.WINDOW_USER_ADMINISTRATION)
            {
                ImGui.Begin("Administracion de usuarios", ref WindowManager.WINDOW_USER_ADMINISTRATION, ImGuiWindowFlags.AlwaysAutoResize);
                if (ImGui.Button("Agregar nuevo usuario", new Vector2(240, 25)))
                {
                    WindowManager.WINDOW_NEW_USER = !WindowManager.WINDOW_NEW_USER;
                }
                if (WindowManager.WINDOW_NEW_USER)
                {
                    ImGui.Begin("Agregar nuevo usuario", ref WindowManager.WINDOW_NEW_USER, ImGuiWindowFlags.AlwaysAutoResize);
                    ImGui.Text("Llave de licencia:");
                    ImGui.InputText("", ref keyUserInput, 50, ImGuiInputTextFlags.ReadOnly);
                    ImGui.SameLine();
                    if (ImGui.Button("Generar llave"))
                    {
                        Guid g = Guid.NewGuid();
                        keyUserInput = g.ToString();
                    }
                    // Combobox para seleccionar el ClassTypes
                    ImGui.Text("Selecciona el rango del usuario:");
                    if (ImGui.BeginCombo("##RankUser", rankUserInput))
                    {
                        // Asegúrate de agregar "ALL_ITEMS" como la primera opción
                        if (ImGui.Selectable(UserTypes.USUARIO.ToString(), rankUserInput == UserTypes.USUARIO.ToString()))
                        {
                            rankUserInput = UserTypes.USUARIO.ToString();
                        }

                        foreach (UserTypes group in Enum.GetValues(typeof(UserTypes)))
                        {
                            if (group == UserTypes.USUARIO) continue; // Ya se agregó como la primera opción

                            bool isSelected = group.ToString() == rankUserInput;
                            if (ImGui.Selectable(group.ToString(), isSelected))
                            {
                                rankUserInput = group.ToString();
                            }
                            if (isSelected)
                            {
                                ImGui.SetItemDefaultFocus();
                            }
                        }
                        ImGui.EndCombo();
                    }
                    if (ImGui.Button("Agregar"))
                    {
                        User user = new()
                        {
                            Key = keyUserInput,
                            UserType = (UserTypes)Enum.Parse(typeof(UserTypes), rankUserInput)
                        };
                        ApiManager.RegisterNewUser(user);

                    }

                    ImGui.TextColored(ApiManager.messageColor, $"{ApiManager.adminMessage}");


                    ImGui.End();
                }

                ImGui.End();
            }
            ImGui.Spacing();
        }

        private void RenderizeAdministratorTools()
        {
            ImGui.SeparatorText("Opciones de Administrador");
            if (ImGui.Button("Administracion de staff", new Vector2(240, 25)))
            {
            }
            ImGui.Spacing();
        }

        private void RenderizeCommunityManagerTools()
        {
            ImGui.SeparatorText("Opciones de C.M.");
            if (ImGui.Button("Lista de comandos", new Vector2(240, 25)))
            {
            }
            ImGui.Spacing();
        }


        private void RenderizeGameManagerTools()
        {
            // GM BUTTONS 
            ImGui.SeparatorText("Opciones de G.M.");
            if (ImGui.Button($"{(WindowManager.WINDOW_ITEM_LIST ? "Ocultar" : "Abrir")} lista de items", new Vector2(240, 25)))
            {
                WindowManager.WINDOW_ITEM_LIST = !WindowManager.WINDOW_ITEM_LIST;
            }
            ImGui.Spacing();
            if (ImGui.Button($"{(WindowManager.WINDOW_REWARD_LIST ? "Ocultar" : "Abrir")} lista de recompensas", new Vector2(240, 25)))
            {
                WindowManager.WINDOW_REWARD_LIST = !WindowManager.WINDOW_REWARD_LIST;
            }
            ImGui.Spacing();
            if (ImGui.Button($"{(WindowManager.WINDOW_SET_LIST ? "Ocultar" : "Abrir")} lista de sets", new Vector2(240, 25)))
            {
                WindowManager.WINDOW_SET_LIST = !WindowManager.WINDOW_SET_LIST;
            }


            if (WindowManager.WINDOW_ITEM_LIST)
            {
                // Filtro y ordenamiento
                filteredItems = FilterAndSortItems(ItemManager.AllItems, nameFilter, classFilters, typeFilter, groupFilter, sortByTypeAscending);

                ImGui.SetNextWindowSize(new Vector2(340, 420), ImGuiCond.FirstUseEver);
                ImGui.Begin("Lista de Items", ref WindowManager.WINDOW_ITEM_LIST, ImGuiWindowFlags.AlwaysAutoResize);

                // Barra de búsqueda por nombre
                ImGui.InputText("Filtrar por Nombre", ref nameFilter, 100);


                // Barra de búsqueda por tipo
                ImGui.InputInt("Filtrar por Type", ref typeFilter);


                // Dropdown para filtrar por grupo
                ImGui.Text("Filtrar por Group:");

                if (ImGui.BeginCombo("##GroupFilter", groupFilter.ToString()))
                {
                    // Asegúrate de agregar "ALL_ITEMS" como la primera opción
                    if (ImGui.Selectable(ItemGroups.ALL_ITEMS.ToString(), groupFilter == ItemGroups.ALL_ITEMS))
                    {
                        groupFilter = ItemGroups.ALL_ITEMS;
                    }

                    foreach (ItemGroups group in Enum.GetValues(typeof(ItemGroups)))
                    {
                        if (group == ItemGroups.ALL_ITEMS) continue; // Ya se agregó como la primera opción

                        bool isSelected = group == groupFilter;
                        if (ImGui.Selectable(group.ToString(), isSelected))
                        {
                            groupFilter = group;
                        }
                        if (isSelected)
                        {
                            ImGui.SetItemDefaultFocus();
                        }
                    }
                    ImGui.EndCombo();
                }



                // Organizar checkboxes en una cuadrícula
                const int checkboxesPerRow = 4; // Puedes ajustar esto según tus necesidades
                int checkboxesInRow = 0;
                // Checkboxes para filtrar por clase
                ImGui.Text("Filtrar por Clase:");
                foreach (ClassTypes classType in Enum.GetValues(typeof(ClassTypes)))
                {
                    bool isSelected = classFilters.Contains(classType);
                    if (ImGui.Checkbox(classType.ToString(), ref isSelected))
                    {
                        if (isSelected)
                        {
                            classFilters.Add(classType);
                        }
                        else
                        {
                            classFilters.Remove(classType);
                        }
                    }

                    checkboxesInRow++;
                    if (checkboxesInRow >= checkboxesPerRow)
                    {
                        checkboxesInRow = 0;

                    }
                    else
                    {
                        ImGui.SameLine(); // Continuar en la misma línea
                    }
                }




                // Botón de ordenamiento
                if (ImGui.Button("Cambiar Orden"))
                {
                    sortByTypeAscending = !sortByTypeAscending;
                }



                ImGui.BeginChild("Item List", new Vector2(340, 140), ImGuiChildFlags.Border | ImGuiChildFlags.FrameStyle);
                // Mostrar items filtrados
                foreach (var item in filteredItems)
                {
                    ImGui.Selectable($"{item.Type} - {item.Name}");
                    if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                    {
                        ProcessedItem newItem = new(item.Group, item.Type, item.Name, item.Slot);
                        processedItems.Add(newItem);
                        if (!WindowManager.WINDOW_REWARD_LIST)
                        {
                            WindowManager.WINDOW_REWARD_LIST = true;
                        }
                    }
                }
                ImGui.EndChild();
                ImGui.TextColored(new Vector4(1, 0, 0, 1), "Haz CLICK IZQUIERDO para añadirlo a la LISTA DE RECOMPENSAS.");
                ImGui.End();
            }

            if (WindowManager.WINDOW_REWARD_LIST)
            {
                ImGui.Begin("Lista de recompensas", ref WindowManager.WINDOW_REWARD_LIST, ImGuiWindowFlags.AlwaysAutoResize);

                Vector4 oldTextColor = style.Colors[(int)ImGuiCol.Text];
                if (selectedItem.Name != "")
                {
                    string group = selectedGroup.ToString();
                    string type = selectedType.ToString();
                    string name = selectedItem.Name;
                    ImGui.InputText("Name", ref name, 30, ImGuiInputTextFlags.ReadOnly);
                    ImGui.InputText("Group", ref group, 5, ImGuiInputTextFlags.ReadOnly);
                    ImGui.InputText("Type", ref type, 5, ImGuiInputTextFlags.ReadOnly);

                    /*
                    TODO: DROPDOWN SELECT 1-7 EXCELENT
                    */
                    ImGui.SliderInt("Excellent", ref selectedOption, 1, 7);

                    ImGui.SliderInt("+ Level", ref selectedPlusLevel, 0, 13);

                    ImGui.Checkbox("Con Skill", ref selectedSkill);
                    ImGui.SameLine();
                    ImGui.Checkbox("Con Luck", ref selectedLuck);
                    ImGui.SameLine();

                    ImGui.Checkbox("Full Option", ref fullOption);

                    List<Option> oldOptions = new();
                    oldOptions.AddRange(selectedOptions);

                    ImGui.Text("Selecciona los Option");
                    List<Option> optionsToRemove = new();
                    // SOURCE
                    if (ExcManager.IsWeaponPendant(selectedItem.Slot))
                    {
                        if (fullOption)
                        {
                            selectedOptions.Clear();
                            selectedOptions.AddRange(ExcManager.WeaponPendantOptions);
                        }
                        else
                        {
                            selectedOptions.Clear();
                            selectedOptions.AddRange(oldOptions);
                        }
                        ImGui.BeginChild("Seleccionables", new Vector2(250, 150), ImGuiChildFlags.Border);
                        style.Colors[(int)ImGuiCol.Text] = new Vector4(0.4f, 0.3f, 0.9f, 0.9f);
                        ImGui.SeparatorText("Disponibles");
                        style.Colors[(int)ImGuiCol.Text] = new Vector4(0.5f, 0.4f, 1f, 0.8f);
                        foreach (var option in ExcManager.WeaponPendantOptions)
                        {
                            ImGui.Selectable(option.Name);
                            if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                            {
                                HandleSelectOption(option);
                            }
                        }
                        ImGui.EndChild();
                        ImGui.SameLine();
                        ImGui.BeginChild("No seleccionados", new Vector2(250, 150), ImGuiChildFlags.Border);
                        style.Colors[(int)ImGuiCol.Text] = new Vector4(0.0f, 0.9f, 0.0f, 0.9f);
                        ImGui.SeparatorText("Seleccionados");
                        style.Colors[(int)ImGuiCol.Text] = new Vector4(0.2f, 1f, 0.2f, 0.8f);
                        foreach (var option in selectedOptions)
                        {
                            ImGui.Selectable(option.Name);
                            if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                            {
                                optionsToRemove.Add(option);
                            }
                        }
                        ImGui.EndChild();
                    }



                    if (ExcManager.IsSetShieldRing(selectedItem.Slot))
                    {
                        if (fullOption)
                        {
                            selectedOptions.Clear();
                            selectedOptions.AddRange(ExcManager.SetShieldRingOptions);
                        }
                        else
                        {
                            selectedOptions.Clear();
                            selectedOptions.AddRange(oldOptions);
                        }
                        ImGui.BeginChild("Seleccionables", new Vector2(250, 150), ImGuiChildFlags.Border);
                        style.Colors[(int)ImGuiCol.Text] = new Vector4(0.4f, 0.3f, 0.9f, 0.9f);
                        ImGui.SeparatorText("Disponibles");
                        style.Colors[(int)ImGuiCol.Text] = new Vector4(0.5f, 0.4f, 1f, 0.8f);
                        foreach (var option in ExcManager.SetShieldRingOptions)
                        {
                            ImGui.Selectable(option.Name);
                            if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                            {
                                HandleSelectOption(option);
                            }
                        }
                        ImGui.EndChild();
                        ImGui.SameLine();
                        ImGui.BeginChild("No seleccionados", new Vector2(250, 150), ImGuiChildFlags.Border);
                        style.Colors[(int)ImGuiCol.Text] = new Vector4(0.0f, 0.9f, 0.0f, 0.9f);
                        ImGui.SeparatorText("Seleccionados");
                        style.Colors[(int)ImGuiCol.Text] = oldStyle.Colors[(int)ImGuiCol.Text];
                        foreach (var option in selectedOptions)
                        {
                            ImGui.Selectable(option.Name);
                            if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                            {
                                optionsToRemove.Add(option);
                            }
                        }
                        ImGui.EndChild();
                    }
                    style.Colors[(int)ImGuiCol.Text] = oldTextColor;

                    foreach (var optionToRemove in optionsToRemove)
                    {
                        selectedOptions.Remove(optionToRemove);
                    }



                    if (ImGui.Button("Terminar de editar item"))
                    {

                        int indexof = processedItems.IndexOf(selectedItem);

                        processedItems[indexof].Group = (ItemGroups)selectedGroup;
                        processedItems[indexof].Type = selectedType;
                        processedItems[indexof].PlusLevel = selectedPlusLevel;
                        processedItems[indexof].Skill = selectedSkill;
                        processedItems[indexof].Luck = selectedLuck;
                        processedItems[indexof].Option = selectedOption;
                        processedItems[indexof].OptionsList = selectedOptions;
                        processedItems[indexof].Name = selectedItem.Name;
                        processedItems[indexof].Slot = selectedItem.Slot;
                        Console.WriteLine(processedItems[indexof]);
                    }
                    ImGui.SameLine();
                    if (ImGui.Button($"Copiar {selectedItem.GetCommand()}"))
                    {
                        ImGui.SetClipboardText(selectedItem.GetCommand());
                    }
                }


                ImGui.BeginChild("Objetos:", new Vector2(400, 140), ImGuiChildFlags.Border | ImGuiChildFlags.FrameStyle);
                // Mostrar items filtrados
                List<ProcessedItem> itemsToRemove = new();

                foreach (var processedItem in processedItems)
                {
                    string newItemName = ExcManager.GetItemModifiedName(processedItem);
                    //                    style.Colors[(int)ImGuiCol.Text] = new Vector4(255f / 255f, 255f / 255f, 255f / 255f, 0.9f);
                    bool is_full = processedItem.Option > 0 && processedItem.OptionsList.Count > 0;
                    if (is_full)
                    {
                        style.Colors[(int)ImGuiCol.Text] = new Vector4(excRedColor, excGreenColor, excBlueColor, 0.9f);
                    }
                    ImGui.Selectable($"{processedItem.Type} - {newItemName}");
                    if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                    {
                        if (selectedItem.Name != "")
                        {
                            int indexof = processedItems.IndexOf(selectedItem);

                            processedItems[indexof].Group = (ItemGroups)selectedGroup;
                            processedItems[indexof].Type = selectedType;
                            processedItems[indexof].PlusLevel = selectedPlusLevel;
                            processedItems[indexof].Skill = selectedSkill;
                            processedItems[indexof].Luck = selectedLuck;
                            processedItems[indexof].Option = selectedOption;
                            processedItems[indexof].OptionsList = selectedOptions;
                            processedItems[indexof].Name = selectedItem.Name;
                            processedItems[indexof].Slot = selectedItem.Slot;
                            Console.WriteLine(processedItems[indexof]);
                        }
                        selectedGroup = 0;
                        selectedType = 0;
                        selectedPlusLevel = 0;
                        selectedSkill = false;
                        selectedLuck = false;
                        selectedOption = 0;
                        selectedOptions = new List<Option>();
                        Console.WriteLine($"Clic Izquierdo en {processedItem.Name}");
                        HandleSelectItem(processedItem);

                    }
                    if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                    {
                        Console.WriteLine($"Clic derecho en {processedItem.Name}");
                        itemsToRemove.Add(processedItem);
                        if (selectedItem == processedItem)
                        {
                            selectedItem = new();
                        }
                    }


                    style.Colors[(int)ImGuiCol.Text] = oldTextColor;
                }
                // Elimina los elementos marcados para eliminación
                foreach (var itemToRemove in itemsToRemove)
                {

                    processedItems.Remove(itemToRemove);
                }



                ImGui.EndChild();
                ImGui.TextColored(new Vector4(0, 1, 0, 1), "+ Haz CLICK IZQUIERDO para editar el item.");
                ImGui.Spacing();
                ImGui.TextColored(new Vector4(1, 0, 0, 1), "- Haz CLICK DERECHO para eliminarlo.");
                ImGui.Spacing();
                ImGui.TextColored(new Vector4(0, 0, 1, 1), "NOTA: para actualizar el item, haz CLICK IZQUIERDO en algun item de la lista.");
                ImGui.End();
            }

            // SET LIST

            if (WindowManager.WINDOW_SET_LIST)
            {
                filteredSets = FilterAndSortSets(setNameFilterString, classFilters, typeFilter);
                ImGui.Begin("Lista de sets", ref WindowManager.WINDOW_SET_LIST, ImGuiWindowFlags.AlwaysAutoResize);

                ImGui.InputText("Filtrar por nombre", ref setNameFilterString, 40);
                const int checkboxesPerRow = 4; // Checkbox por fila
                int checkboxesInRow = 0;
                // Checkboxes CLASS FILTER
                ImGui.Text("Filtrar por Clase:");
                foreach (ClassTypes classType in Enum.GetValues(typeof(ClassTypes)))
                {
                    bool isSelected = classFilters.Contains(classType);
                    if (ImGui.Checkbox(classType.ToString(), ref isSelected))
                    {
                        if (isSelected)
                        {
                            classFilters.Add(classType);
                        }
                        else
                        {
                            classFilters.Remove(classType);
                        }
                    }

                    checkboxesInRow++;
                    if (checkboxesInRow >= checkboxesPerRow)
                    {
                        checkboxesInRow = 0;

                    }
                    else
                    {
                        ImGui.SameLine();
                    }
                }

                ImGui.Spacing();
                ImGui.BeginChild("Item List", new Vector2(340, 140), ImGuiChildFlags.Border | ImGuiChildFlags.FrameStyle);
                // Mostrar items filtrados
                foreach (var set in filteredSets)
                {
                    ImGui.Selectable($"{set.Name}");
                    if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                    {
                        HandleAddSetToRewardList(set);
                    }
                }
                ImGui.EndChild();
                ImGui.TextColored(new Vector4(1, 0, 0, 1), "Haz CLICK IZQUIERDO para añadirlo a la LISTA DE RECOMPENSAS.");


                ImGui.End();

            }
        }

        private void RenderizeTutorTools()
        {
            ImGui.SeparatorText("Opciones de Tutor");
            if (ImGui.Button("Administracion de guias", new Vector2(240, 25)))
            {
                WindowManager.WINDOW_ADMIN_GUIDES = !WindowManager.WINDOW_ADMIN_GUIDES;
            }
            if (WindowManager.WINDOW_ADMIN_GUIDES)
            {
                ImGui.Begin("Administracion de guias", ref WindowManager.WINDOW_ADMIN_GUIDES, ImGuiWindowFlags.AlwaysAutoResize);

                ImGui.Text("Buscar ");
                ImGui.SameLine();
                ImGui.InputTextWithHint("##SearchInputGuides", "Buscar guía", ref searchInputAdminGuides, 100);
                ImGui.BeginChild("Guías", new Vector2(440, 150), ImGuiChildFlags.Border | ImGuiChildFlags.FrameStyle);

                filteredAdminGuides = FilterGuides();
                foreach (var guide in filteredAdminGuides)
                {
                    Vector4 prevColor = ImGui.GetStyle().Colors[(int)ImGuiCol.Text];
                    if (guide.IsFeatured)
                    {
                        Vector4 _color = ColorManager.FromRGBA(excRedColor, excGreenColor, excBlueColor, 1f);
                        ImGui.GetStyle().Colors[(int)ImGuiCol.Text] = _color;
                    }
                    if (ImGui.Selectable($"{guide.Id} {guide.Title} - {guide.Author}"))
                    {
                        int guideId = guide.Id;
                        selectedAdminGuide = guide;
                        WindowManager.WINDOW_SELECTED_ADMIN_GUIDE = true;
                    }
                    if (guide.IsFeatured)
                    {
                        ImGui.GetStyle().Colors[(int)ImGuiCol.Text] = prevColor;
                    }
                }
                ImGui.EndChild();
                // if (ImGui.Button("Popular guias"))
                // {
                //     ApiManager.PopulateGuidesToDatabase(guides);
                // }
                ImGui.End();
            }

            if (WindowManager.WINDOW_SELECTED_ADMIN_GUIDE)
            {
                if (selectedAdminGuide != null)
                {
                    Vector4 prevColor = ImGui.GetStyle().Colors[(int)ImGuiCol.Text];
                    if (selectedAdminGuide.IsFeatured)
                    {
                        Vector4 _color = ColorManager.FromRGBA(excRedColor, excGreenColor, excBlueColor, 1f);
                        ImGui.GetStyle().Colors[(int)ImGuiCol.Text] = _color;
                    }
                    ImGui.SetNextWindowPos(centerOfScreenView, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));

                    ImGui.Begin($"{selectedAdminGuide.Title} - {selectedAdminGuide.Author}", ref WindowManager.WINDOW_SELECTED_ADMIN_GUIDE);
                    if (selectedAdminGuide.IsFeatured)
                    {
                        ImGui.GetStyle().Colors[(int)ImGuiCol.Text] = prevColor;
                    }

                    ImGui.Text("Titulo:");
                    ImGui.InputText("##Title", ref selectedGuideTitle, 100);
                    ImGui.Text("Autor:");
                    ImGui.InputText("##Author", ref selectedGuideAuthor, 100);
                    ImGui.Text("Imagen:");
                    ImGui.InputText("##Image", ref selectedGuideImage, 100);
                    ImGui.Text("Video:");
                    ImGui.InputText("##Video", ref selectedGuideVideo, 100);
                    ImGui.Text("Cuerpo:");
                    ImGui.InputTextMultiline("##Body", ref selectedGuideBody, 1000, new Vector2(400, 200));
                    ImGui.Text("Tags:");
                    ImGui.InputText("##Tags", ref selectedGuideTags, 100);
                    ImGui.Text("Relacionadas:");
                    ImGui.InputText("##RelatedGuides", ref selectedGuideRelatedGuides, 100);
                    ImGui.Checkbox("Destacada", ref selectedGuideIsFeatured);
                    if (ImGui.Button("Guardar"))
                    {
                        // ApiManager.UpdateGuide(selectedAdminGuide);
                    }
                    ImGui.End();
                }
                ImGui.Spacing();
            }
        }



        private List<Guide> FilterGuides()
        {
            if (searchInputGuides == "")
            {
                return guides.OrderByDescending(guide => guide.IsFeatured).ToList();
            }
            else
            {
                string searchTerm = RemoveDiacritics(searchInputGuides.ToLowerInvariant());

                return guides
                    .OrderByDescending(guide =>
                        (RemoveDiacritics(guide.Title.ToLowerInvariant()).Contains(searchTerm) ? 1 : 0) +
                        guide.Tags.Count(tag => RemoveDiacritics(tag.ToLowerInvariant()).Contains(searchTerm)) +
                        (RemoveDiacritics(guide.Author.ToLowerInvariant()).Contains(searchTerm) ? 1 : 0))
                    .ToList();
            }
        }

        private List<Guide> FilterAdminGuides()
        {
            if (searchInputAdminGuides == "")
            {
                return guides.OrderByDescending(guide => guide.IsFeatured).ToList();
            }
            else
            {
                string searchTerm = RemoveDiacritics(searchInputAdminGuides.ToLowerInvariant());

                return guides
                    .OrderByDescending(guide =>
                        (RemoveDiacritics(guide.Title.ToLowerInvariant()).Contains(searchTerm) ? 1 : 0) +
                        guide.Tags.Count(tag => RemoveDiacritics(tag.ToLowerInvariant()).Contains(searchTerm)) +
                        (RemoveDiacritics(guide.Author.ToLowerInvariant()).Contains(searchTerm) ? 1 : 0))
                    .ToList();
            }
        }

        private void RenderizeUserTools()
        {
            // USER BUTTONS

            ImGui.SeparatorText("Opciones de usuario");
            if (ImGui.Button("Abrir guias", new Vector2(240, 25)))
            {
                WindowManager.WINDOW_GUIDES = !WindowManager.WINDOW_GUIDES;
            }

            // RENDER LIST GUIDES WINDOW
            if (WindowManager.WINDOW_GUIDES)
            {
                ImGui.Begin("Guías", ref WindowManager.WINDOW_GUIDES, ImGuiWindowFlags.AlwaysAutoResize);
                ImGui.Text("Buscar ");
                ImGui.SameLine();
                ImGui.InputTextWithHint("##SearchInputGuides", "Buscar guía", ref searchInputGuides, 100);
                ImGui.BeginChild("Guías", new Vector2(340, 140), ImGuiChildFlags.Border | ImGuiChildFlags.FrameStyle);
                filteredGuides = FilterGuides();
                foreach (var guide in filteredGuides)
                {
                    Vector4 prevColor = ImGui.GetStyle().Colors[(int)ImGuiCol.Text];
                    if (guide.IsFeatured)
                    {
                        Vector4 _color = ColorManager.FromRGBA(excRedColor, excGreenColor, excBlueColor, 1f);
                        ImGui.GetStyle().Colors[(int)ImGuiCol.Text] = _color;
                    }
                    if (ImGui.Selectable($"{guide.Title} - {guide.Author}"))
                    {

                        int guideId = guide.Id;
                        if (selectedGuide == null)
                        {

                            guidesHistory.Add(guide);
                            selectedGuide = guide;
                            WindowManager.WINDOW_SELECTED_GUIDE = true;


                        }
                        else
                        {
                            if (selectedGuide.Id == guideId)
                            {
                                selectedGuide = null;
                                WindowManager.WINDOW_SELECTED_GUIDE = false;
                                guidesHistory = new();
                            }
                            else
                            {
                                guidesHistory.Add(guide);
                                selectedGuide = guide;
                                WindowManager.WINDOW_SELECTED_GUIDE = true;

                            }
                        }
                        Console.WriteLine($"Seleccionaste la guía {guide.Title}");

                    }
                    if (guide.IsFeatured)
                    {
                        ImGui.GetStyle().Colors[(int)ImGuiCol.Text] = prevColor;
                    }
                }

                ImGui.EndChild();
                ImGui.End();
            }


            // RENDER SELECTED GUIDE WINDOW
            if (WindowManager.WINDOW_SELECTED_GUIDE)
            {
                if (selectedGuide != null)
                {
                    if (previousGuideWindowPosition == Vector2.Zero)
                    {

                        ImGui.SetNextWindowPos(centerOfScreenView, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
                    }
                    else
                    {
                        ImGui.SetNextWindowPos(previousGuideWindowPosition, ImGuiCond.Appearing, new Vector2(0f, 0f));
                    }
                    Vector4 prevColor = ImGui.GetStyle().Colors[(int)ImGuiCol.Text];
                    if (selectedGuide.IsFeatured)
                    {
                        Vector4 _color = ColorManager.FromRGBA(excRedColor, excGreenColor, excBlueColor, 1f);
                        ImGui.GetStyle().Colors[(int)ImGuiCol.Text] = _color;
                    }


                    var estimatedWidth = ImGui.GetStyle().FramePadding.X * 2 + 400;
                    ImGui.SetNextWindowSize(new Vector2(estimatedWidth, 500), ImGuiCond.Appearing);
                    ImGui.Begin($"{selectedGuide.Title} - {selectedGuide.Author}", ref WindowManager.WINDOW_SELECTED_GUIDE);
                    previousGuideWindowPosition = ImGui.GetWindowPos();
                    if (selectedGuide.IsFeatured)
                    {
                        ImGui.GetStyle().Colors[(int)ImGuiCol.Text] = prevColor;
                    }
                    Console.WriteLine(guidesHistory.Count);
                    if (guidesHistory.Count > 1)
                    {
                        ImGui.ArrowButton("##back", ImGuiDir.Left);
                        if (ImGui.IsItemClicked())
                        {
                            selectedGuide = guidesHistory[guidesHistory.Count - 2];
                            guidesHistory.RemoveAt(guidesHistory.Count - 1);
                        }
                    }
                    // ImGui.MenuItem("Atras", "CTRL+Z", guidesHistory.Length > 1, true);

                    // ImGui.EndMenu();

                    // Render image 
                    if (selectedGuide.Image != "")
                    {
                        byte[] imgBytes;
                        imageBytes.TryGetValue(selectedGuide.Id, out imgBytes);
                        Image<Rgba32> img = Image.Load<Rgba32>(imgBytes);
                        AddOrGetImagePointer($"{selectedGuide.Id}_guide_img", img, true, out imageHandle);
                        ImGui.Image(imageHandle, new Vector2(400, 280));
                    }
                    ImGui.Spacing();
                    // Render body
                    foreach (var paragraph in selectedGuide.Body)
                    {
                        ImGui.TextWrapped(paragraph);
                        // ImGui.TextWrapped("HOLA\nComo estas");
                    }
                    // Render links
                    Vector4 normalTextColor = ImGui.GetStyle().Colors[(int)ImGuiCol.Text];
                    ImGui.GetStyle().Colors[(int)ImGuiCol.Text] = new Vector4(0.41568627450980394f, 0.32941176470588235f, 0.7764705882352941f, 1.0f);
                    ImGui.BeginChild("selected_guide_links", new Vector2(400, 80), ImGuiChildFlags.Border | ImGuiChildFlags.FrameStyle);
                    ImGui.SeparatorText("Enlaces");
                    style.Colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.41568627450980394f, 0.32941176470588235f, 0.7764705882352941f, 1.0f);
                    ImGui.GetStyle().Colors[(int)ImGuiCol.Text] = normalTextColor;
                    if (selectedGuide.Video.Length > 0)
                    {
                        ImGui.Text("Video: ");
                        ImGui.SameLine();
                        ImGui.Selectable(selectedGuide.Video);
                        if (ImGui.IsItemClicked())
                        {
                            var info = new ProcessStartInfo
                            {
                                FileName = selectedGuide.Video,
                                UseShellExecute = true
                            };
                            Process.Start(info);
                        }
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                        }
                    }
                    ImGui.EndChild();
                    ImGui.BeginChild("related_guides", new Vector2(400, 80), ImGuiChildFlags.Border | ImGuiChildFlags.FrameStyle);
                    ImGui.GetStyle().Colors[(int)ImGuiCol.Text] = new Vector4(0.41568627450980394f, 0.32941176470588235f, 0.7764705882352941f, 1.0f);
                    ImGui.SeparatorText("Guías relacionadas");
                    ImGui.GetStyle().Colors[(int)ImGuiCol.Text] = normalTextColor;
                    if (selectedGuide.RelatedGuides.Count > 0)
                    {
                        foreach (var relatedGuide in selectedGuide.RelatedGuides)
                        {
                            string titleGuide = guides.Where(guide => guide.Id == relatedGuide).Select(guide => guide.Title).FirstOrDefault();
                            ImGui.Selectable(titleGuide);
                            if (ImGui.IsItemClicked())
                            {
                                var guide = guides.Find(guide => guide.Id == relatedGuide);
                                if (guide != null)
                                {
                                    // TODO: HISTORY OF GUIDES (BACK PAGE, NEXT PAGE)
                                    guidesHistory.Add(guide);
                                    selectedGuide = guide;
                                }
                            }
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                            }
                        }
                    }

                    ImGui.EndChild();
                    style.Colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.1803921610116959f, 0.1882352977991104f, 0.196078434586525f, 1.0f);

                    ImGui.End();

                }
            }
            ImGui.Spacing();
        }

        private void ConfigureStylesImGui()
        {
            //ImGui.StyleColorsClassic();
            ImGui.StyleColorsDark();
            oldStyle = ImGui.GetStyle();
            style = ImGui.GetStyle();

            style.ChildRounding = 6f;
            style.WindowRounding = 8f;
            style.FrameRounding = 6f;
            style.PopupRounding = 6f;
            style.ScrollbarRounding = 6f;
            style.GrabRounding = 6f;
            style.TabRounding = 10f;
            style.SeparatorTextAlign = new Vector2(0.5f, 0.5f);
            style.SeparatorTextBorderSize = 1f;
            style.SeparatorTextPadding = new Vector2(0f, 2f);
        }

        private void ConfigureOverlaySize()
        {
            if (this.Size.Width != screenWidth || this.Size.Height != screenHeight)
            {
                this.Size = new(screenWidth, screenHeight);
                this.Position = new(0, 0);
            }
        }

        private Vector4 GetUserColor(User user)
        {
            return user.UserType switch
            {
                UserTypes.USUARIO => ColorManager.userColor,
                UserTypes.VIP0 => ColorManager.vip0Color,
                UserTypes.VIP1 => ColorManager.vip1Color,
                UserTypes.VIP2 => ColorManager.vip2Color,
                UserTypes.TUTOR => ColorManager.tutorColor,
                UserTypes.GAMEMANAGER => ColorManager.gmColor,
                UserTypes.COMMUNITYMANAGER => ColorManager.cmColor,
                UserTypes.ADMINISTRADOR => ColorManager.FromRGBA(excRedColor, excGreenColor, excBlueColor, 1f),
                UserTypes.CREADOR => ColorManager.FromRGBA(excRedColor, excGreenColor, excBlueColor, 1f),
                _ => ColorManager.userColor,
            };
        }

        private void ConfigureHotkeys()
        {
            if (GetAsyncKeyState(0x79) < 0)
            {
                enabled_overlay = !enabled_overlay;
                Thread.Sleep(200);
            }
        }


        public static void SetupImGuiStyle()
        {
            // Moonlight styleMadam-Herta from ImThemes
            var style = ImGuiNET.ImGui.GetStyle();

            style.Alpha = 1.0f;
            style.DisabledAlpha = 1.0f;
            style.WindowPadding = new Vector2(12.0f, 12.0f);
            style.WindowRounding = 11.5f;
            style.WindowBorderSize = 0.0f;
            style.WindowMinSize = new Vector2(20.0f, 20.0f);
            style.WindowTitleAlign = new Vector2(0.5f, 0.5f);
            style.WindowMenuButtonPosition = ImGuiDir.Right;
            style.ChildRounding = 0.0f;
            style.ChildBorderSize = 1.0f;
            style.PopupRounding = 0.0f;
            style.PopupBorderSize = 1.0f;
            style.FramePadding = new Vector2(20.0f, 3.400000095367432f);
            style.FrameRounding = 11.89999961853027f;
            style.FrameBorderSize = 0.0f;
            style.ItemSpacing = new Vector2(4.300000190734863f, 5.5f);
            style.ItemInnerSpacing = new Vector2(7.099999904632568f, 1.799999952316284f);
            style.CellPadding = new Vector2(12.10000038146973f, 9.199999809265137f);
            style.IndentSpacing = 0.0f;
            style.ColumnsMinSpacing = 4.900000095367432f;
            style.ScrollbarSize = 11.60000038146973f;
            style.ScrollbarRounding = 15.89999961853027f;
            style.GrabMinSize = 3.700000047683716f;
            style.GrabRounding = 20.0f;
            style.TabRounding = 0.0f;
            style.TabBorderSize = 0.0f;
            style.TabMinWidthForCloseButton = 0.0f;
            style.ColorButtonPosition = ImGuiDir.Right;
            style.ButtonTextAlign = new Vector2(0.5f, 0.5f);
            style.SelectableTextAlign = new Vector2(0.0f, 0.0f);

            style.Colors[(int)ImGuiCol.Text] = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            style.Colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.2745098173618317f, 0.3176470696926117f, 0.4509803950786591f, 1.0f);
            style.Colors[(int)ImGuiCol.WindowBg] = new Vector4(0.0784313753247261f, 0.08627451211214066f, 0.1019607856869698f, 1.0f);
            style.Colors[(int)ImGuiCol.ChildBg] = new Vector4(0.09411764889955521f, 0.1019607856869698f, 0.1176470592617989f, 1.0f);
            style.Colors[(int)ImGuiCol.PopupBg] = new Vector4(0.0784313753247261f, 0.08627451211214066f, 0.1019607856869698f, 1.0f);
            style.Colors[(int)ImGuiCol.Border] = new Vector4(0.1568627506494522f, 0.168627455830574f, 0.1921568661928177f, 1.0f);
            style.Colors[(int)ImGuiCol.BorderShadow] = new Vector4(0.0784313753247261f, 0.08627451211214066f, 0.1019607856869698f, 1.0f);
            style.Colors[(int)ImGuiCol.FrameBg] = new Vector4(0.1137254908680916f, 0.125490203499794f, 0.1529411822557449f, 1.0f);
            style.Colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.1568627506494522f, 0.168627455830574f, 0.1921568661928177f, 1.0f);
            style.Colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.1568627506494522f, 0.168627455830574f, 0.1921568661928177f, 1.0f);
            style.Colors[(int)ImGuiCol.TitleBg] = new Vector4(0.0470588244497776f, 0.05490196123719215f, 0.07058823853731155f, 1.0f);
            style.Colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.0470588244497776f, 0.05490196123719215f, 0.07058823853731155f, 1.0f);
            style.Colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(0.0784313753247261f, 0.08627451211214066f, 0.1019607856869698f, 1.0f);
            style.Colors[(int)ImGuiCol.MenuBarBg] = new Vector4(0.09803921729326248f, 0.105882354080677f, 0.1215686276555061f, 1.0f);
            style.Colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0.0470588244497776f, 0.05490196123719215f, 0.07058823853731155f, 1.0f);
            style.Colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.1176470592617989f, 0.1333333402872086f, 0.1490196138620377f, 1.0f);
            style.Colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.1568627506494522f, 0.168627455830574f, 0.1921568661928177f, 1.0f);
            style.Colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(0.1176470592617989f, 0.1333333402872086f, 0.1490196138620377f, 1.0f);
            style.Colors[(int)ImGuiCol.CheckMark] = new Vector4(0.9725490212440491f, 1.0f, 0.4980392158031464f, 1.0f);
            style.Colors[(int)ImGuiCol.SliderGrab] = new Vector4(0.9725490212440491f, 1.0f, 0.4980392158031464f, 1.0f);
            style.Colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(1.0f, 0.7960784435272217f, 0.4980392158031464f, 1.0f);
            style.Colors[(int)ImGuiCol.Button] = new Vector4(0.1176470592617989f, 0.1333333402872086f, 0.1490196138620377f, 1.0f);
            style.Colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.1803921610116959f, 0.1882352977991104f, 0.196078434586525f, 1.0f);
            style.Colors[(int)ImGuiCol.ButtonActive] = new Vector4(0.1529411822557449f, 0.1529411822557449f, 0.1529411822557449f, 1.0f);
            style.Colors[(int)ImGuiCol.Header] = new Vector4(0.1411764770746231f, 0.1647058874368668f, 0.2078431397676468f, 1.0f);
            style.Colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.105882354080677f, 0.105882354080677f, 0.105882354080677f, 1.0f);
            style.Colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.0784313753247261f, 0.08627451211214066f, 0.1019607856869698f, 1.0f);
            style.Colors[(int)ImGuiCol.Separator] = new Vector4(0.1294117718935013f, 0.1490196138620377f, 0.1921568661928177f, 1.0f);
            style.Colors[(int)ImGuiCol.SeparatorHovered] = new Vector4(0.1568627506494522f, 0.1843137294054031f, 0.250980406999588f, 1.0f);
            style.Colors[(int)ImGuiCol.SeparatorActive] = new Vector4(0.1568627506494522f, 0.1843137294054031f, 0.250980406999588f, 1.0f);
            style.Colors[(int)ImGuiCol.ResizeGrip] = new Vector4(0.1450980454683304f, 0.1450980454683304f, 0.1450980454683304f, 1.0f);
            style.Colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(0.9725490212440491f, 1.0f, 0.4980392158031464f, 1.0f);
            style.Colors[(int)ImGuiCol.ResizeGripActive] = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            style.Colors[(int)ImGuiCol.Tab] = new Vector4(0.0784313753247261f, 0.08627451211214066f, 0.1019607856869698f, 1.0f);
            style.Colors[(int)ImGuiCol.TabHovered] = new Vector4(0.1176470592617989f, 0.1333333402872086f, 0.1490196138620377f, 1.0f);
            style.Colors[(int)ImGuiCol.TabActive] = new Vector4(0.1176470592617989f, 0.1333333402872086f, 0.1490196138620377f, 1.0f);
            style.Colors[(int)ImGuiCol.TabUnfocused] = new Vector4(0.0784313753247261f, 0.08627451211214066f, 0.1019607856869698f, 1.0f);
            style.Colors[(int)ImGuiCol.TabUnfocusedActive] = new Vector4(0.125490203499794f, 0.2745098173618317f, 0.572549045085907f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotLines] = new Vector4(0.5215686559677124f, 0.6000000238418579f, 0.7019608020782471f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotLinesHovered] = new Vector4(0.03921568766236305f, 0.9803921580314636f, 0.9803921580314636f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotHistogram] = new Vector4(0.8823529481887817f, 0.7960784435272217f, 0.5607843399047852f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotHistogramHovered] = new Vector4(0.95686274766922f, 0.95686274766922f, 0.95686274766922f, 1.0f);
            style.Colors[(int)ImGuiCol.TableHeaderBg] = new Vector4(0.0470588244497776f, 0.05490196123719215f, 0.07058823853731155f, 1.0f);
            style.Colors[(int)ImGuiCol.TableBorderStrong] = new Vector4(0.0470588244497776f, 0.05490196123719215f, 0.07058823853731155f, 1.0f);
            style.Colors[(int)ImGuiCol.TableBorderLight] = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            style.Colors[(int)ImGuiCol.TableRowBg] = new Vector4(0.1176470592617989f, 0.1333333402872086f, 0.1490196138620377f, 1.0f);
            style.Colors[(int)ImGuiCol.TableRowBgAlt] = new Vector4(0.09803921729326248f, 0.105882354080677f, 0.1215686276555061f, 1.0f);
            style.Colors[(int)ImGuiCol.TextSelectedBg] = new Vector4(0.9372549057006836f, 0.9372549057006836f, 0.9372549057006836f, 1.0f);
            style.Colors[(int)ImGuiCol.DragDropTarget] = new Vector4(0.4980392158031464f, 0.5137255191802979f, 1.0f, 1.0f);
            style.Colors[(int)ImGuiCol.NavHighlight] = new Vector4(0.2666666805744171f, 0.2901960909366608f, 1.0f, 1.0f);
            style.Colors[(int)ImGuiCol.NavWindowingHighlight] = new Vector4(0.4980392158031464f, 0.5137255191802979f, 1.0f, 1.0f);
            style.Colors[(int)ImGuiCol.NavWindowingDimBg] = new Vector4(0.196078434586525f, 0.1764705926179886f, 0.5450980663299561f, 0.501960813999176f);
            style.Colors[(int)ImGuiCol.ModalWindowDimBg] = new Vector4(0.196078434586525f, 0.1764705926179886f, 0.5450980663299561f, 0.501960813999176f);
        }
        public List<Guide> guides = new();
        public RenderClass()
        {
            this.ReplaceFont("C:\\Windows\\Fonts\\segoeui.ttf", 18, FontGlyphRangeType.English);
            guides = ApiManager.GetGuidesFromDB();
            Console.WriteLine($"Guides count: {guides.ToArray()[0].Image}");
            LoadImagesFromGuides();
            try
            {
                // urlVersion = (WebRequest.Create("https://raw.githubusercontent.com/voidexiled/event_planner_mupvp/main/version.txt") as HttpWebRequest).GetResponse().ResponseUri.ToString();
                HttpClient client = new();
                var req = client.GetStringAsync("https://raw.githubusercontent.com/voidexiled/event_planner_mupvp/main/version.txt");
                urlVersion = req.Result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }


            var tempGithubVersion = urlVersion.Replace(".", "");
            Console.WriteLine($"Github version: {tempGithubVersion}");
            var tempLocalVersion = currentVersion.Replace(".", "");
            Console.WriteLine($"Local version: {tempLocalVersion}");

            if (int.Parse(tempGithubVersion) > int.Parse(tempLocalVersion))
            {
                outdated = true;
            }
            else
            {
                outdated = false;
            }

        }
        bool outdated = false;
        private Dictionary<int, byte[]> imageBytes = new();
        private IntPtr imageHandle;
        private uint imageWidth;
        private uint imageHeight;
        private void LoadImagesFromGuides()
        {

            using var webClient = new HttpClient();
            foreach (var guide in guides)
            {
                byte[] image = webClient.GetAsync(guide.Image).Result.Content.ReadAsByteArrayAsync().Result;
                imageBytes.Add(guide.Id, image);
                // Image img = Image.Load(image);

            }
            Console.WriteLine($"Imagenes cargadas {imageBytes.Count}");
            webClient.Dispose();




        }
        float downloadUpdateProgress = 0.0f;
        protected override void Render()
        {
            ConfigureStylesImGui(); // Configuro los estilos por defecto que tendra la interfaz
            UpdateColor(); // Este metodo hace funcionar el color arcoiris que uso en los textos
            ConfigureHotkeys(); // Configuro los hotkeys que tendra la interfaz como Ocultar / Mostrar la interfaz
            ConfigureOverlaySize(); // Obtengo el tamaño de la pantalla y resizeo el overlay
                                    //ImGui.ShowDemoWindow();
            SetupImGuiStyle();

            if (outdated)
            {
                ImGui.SetNextWindowSize(new Vector2(250, 400), ImGuiCond.FirstUseEver);
                ImGui.Begin("Actualización disponible",
                        ImGuiWindowFlags.AlwaysAutoResize);
                ImGui.Text("Hay una nueva versión disponible");


                if (ImGui.Button("Descargar", new Vector2(240, 25)))
                {
                    Process.Start("Updater.exe");
                    this.Close();
                }

                // ImGui.ProgressBar(downloadUpdateProgress, new Vector2(240, 25));
                ImGui.End();
                return;
            }


            // ImGui.ShowFontSelector("Font Selector");

            ImGui.Begin("Ocultar ventanas", ImGuiWindowFlags.NoDocking);

            // foreach (KeyValuePair<int, byte[]> image in imageBytes)
            // {
            //     Image<Rgba32> img = Image.Load<Rgba32>(image.Value);
            //     AddOrGetImagePointer($"{image.Key}_guide_img", img, true, out imageHandle);
            // }

            if (ImGui.Button($"{(enabled_overlay ? "Ocultar" : "Mostrar")}", new Vector2(240, 25)))
            {
                enabled_overlay = !enabled_overlay;
            }
            ImGui.End();

            if (!enabled_overlay)
            {
                return;
            }
            if (!SessionManager.loggedIn)
            {
                ImGui.SetNextWindowSize(new Vector2(250, 400), ImGuiCond.FirstUseEver);
                ImGui.Begin("Verificar clave de licencia",
                        ImGuiWindowFlags.AlwaysAutoResize);
                ImGui.SeparatorText("Ingresa tu clave de licencia");
                ImGui.InputText("", ref sessionKey, 50);
                ImGui.Spacing();
                if (SessionManager.errorMessage != String.Empty)
                {
                    ImGui.TextColored(new Vector4(1, 0, 0, 1), SessionManager.errorMessage);
                    ImGui.Spacing();
                }

                if (ImGui.Button("Verificar", new Vector2(100, 35)) || ImGui.IsKeyPressed(ImGuiKey.Enter))
                {

                    if (!SessionManager.SaveSession(sessionKey))
                    {
                        //ImGui.OpenPopup($"Error: {SessionManager.errorMessage}");
                    }

                }
                ImGui.SameLine();
                if (ImGui.Button("Salir", new Vector2(100, 35)))
                {
                    Environment.Exit(0);
                }
                //ImGui.PopStyleColor();
                ImGui.End();
                return;

            }

            if (SessionManager.currentUser.Key == String.Empty)
            {
                SessionManager.DeleteSession();
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(250, 400), ImGuiCond.FirstUseEver);
            ImGui.Begin("MU PVP ONLINE - STAFF HELPER",
                    ImGuiWindowFlags.AlwaysAutoResize);

            float oldSize = ImGui.GetFont().Scale;
            ImGui.GetFont().Scale = 0.95f;
            ImGui.PushFont(ImGui.GetFont());
            ImGui.Text("Desarrollado por Void Exiled (GM Kxacez)");
            ImGui.Text("Versión BETA 0.1");

            ImGui.GetFont().Scale = oldSize;
            ImGui.PopFont();
            ImGui.SeparatorText("Información de usuario");
            ImGui.Text("Rango de usuario: ");
            ImGui.SameLine();
            string userTypeString = SessionManager.currentUser.GetRankAsString();

            Vector4 _userColor = GetUserColor(SessionManager.currentUser);

            ImGui.TextColored(_userColor, userTypeString);

            //ShowDebugWindow();


            /* Renderizar dependiendo del tipo de usuario */
            /* OWNER */
            if (SessionManager.UserIsOwner())
            {
                RenderizeOwnerTools();
            }

            /* ADMINISTRATOR */
            if (SessionManager.UserIsAdministrator())
            {
                RenderizeAdministratorTools();
            }

            /* COMMUNITY MANAGER */
            if (SessionManager.UserIsCommunityManager())
            {
                RenderizeCommunityManagerTools();
            }

            /* GAME MANAGER */
            if (SessionManager.UserIsGameManager())
            {
                RenderizeGameManagerTools();
            }
            /* TUTOR */
            if (SessionManager.UserIsTutor())
            {
                RenderizeTutorTools();
            }

            // TODO:DENTRO DE USUARIO RENDERIZAREMOS DEPENDIENDO DEL VIP RANK
            /* USER */
            if (SessionManager.UserIsUser())
            {
                RenderizeUserTools();
            }

            ImGui.SeparatorText("Otras opciones");
            ImGui.Spacing();

            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.8f, 0.1f, 0.1f, 1f));

            if (ImGui.Button("Cerrar sesión", new Vector2(240, 25)))
            {
                SessionManager.DeleteSession();
            }
            ImGui.Spacing();
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.8f, 0.1f, 0.1f, 1f));

            if (ImGui.Button("Salir de la aplicacion", new Vector2(240, 25)))
            {
                Environment.Exit(0);
            }
            ImGui.PopStyleColor();


            ImGui.End();

            // style.Colors[(int)ImGuiCol.TitleBg] = oldStyle.Colors[(int)ImGuiCol.TitleBg];
            // style.Colors[(int)ImGuiCol.TitleBgActive] = oldStyle.Colors[(int)ImGuiCol.TitleBgActive];
            // style.Colors[(int)ImGuiCol.TitleBgCollapsed] = oldStyle.Colors[(int)ImGuiCol.TitleBgCollapsed];

        }

        private List<ArmorSet> FilterAndSortSets(string nameSetFilter, List<ClassTypes> classFilters, int typeFilter)
        {
            // Filtrar por nombre
            var filteredSets = ItemManager.ArmorSets.Where(item => item.Name.Contains(nameSetFilter, StringComparison.OrdinalIgnoreCase)).ToList();

            // Filtrar por clase
            if (classFilters.Count > 0)
            {
                filteredSets = filteredSets.Where(set =>
                {

                    return classFilters.Any(c =>
                    {

                        return set.Classes.Contains(c);
                    });


                }).ToList();
            }

            // Filtrar por tipo
            if (typeFilter >= 0)
            {
                filteredSets = filteredSets.Where(set => set.Type == typeFilter).ToList();
            }

            return filteredSets;
        }

        private List<Item> FilterAndSortItems(List<Item> items, string nameFilter, List<ClassTypes> classFilters, int typeFilter, ItemGroups groupFilter, bool ascending)
        {
            // Filtrar por nombre
            var filteredItems = items.Where(item => item.Name.Contains(nameFilter, StringComparison.OrdinalIgnoreCase)).ToList();

            // Filtrar por clase
            if (classFilters.Count > 0)
            {
                filteredItems = filteredItems.Where(item =>
                {
                    if (item.GetType().IsSubclassOf(typeof(Equipable)))
                    {
                        return classFilters.Any(c =>
                        {
                            Equipable convertedItem = (Equipable)item;
                            return convertedItem.Classes.Contains(c);
                        });
                    }
                    else return false;
                }).ToList();
            }

            // Filtrar por tipo
            if (typeFilter >= 0)
            {
                filteredItems = filteredItems.Where(item => item.Type == typeFilter).ToList();
            }

            // Filtrar por grupo
            if (groupFilter != ItemGroups.ALL_ITEMS)
            {
                filteredItems = filteredItems.Where(item => item.Group == groupFilter).ToList();
            }

            // Ordenar por tipo (puedes cambiar esto según tus necesidades)
            if (ascending)
            {
                filteredItems = filteredItems.OrderBy(item => item.Type).ToList();
            }
            else
            {
                filteredItems = filteredItems.OrderByDescending(item => item.Type).ToList();
            }

            return filteredItems;
        }


        private void HandleAddSetToRewardList(ArmorSet set)
        {
            ProcessedItem newItem = new(set.Type, set.Name);
            processedItems.Add(newItem);


            if (!WindowManager.WINDOW_REWARD_LIST)
            {
                WindowManager.WINDOW_REWARD_LIST = true;
            }

            selectedGroup = (int)newItem.Group;
            selectedType = (int)newItem.Type;
            selectedPlusLevel = (int)newItem.PlusLevel;
            selectedSkill = (bool)newItem.Skill;
            selectedLuck = (bool)newItem.Luck;
            selectedOption = (int)newItem.Option;
            selectedOptions.Clear();

            //Console.WriteLine(item.Option);
            if (newItem.OptionsList.Count > 0)
            {
                selectedOptions.AddRange(newItem.OptionsList);
            }
            selectedItem = newItem;
        }

        private void HandleSelectItem(ProcessedItem item)
        {
            Console.WriteLine(item.Type);
            selectedGroup = (int)item.Group;
            selectedType = (int)item.Type;
            selectedPlusLevel = (int)item.PlusLevel;
            selectedSkill = (bool)item.Skill;
            selectedLuck = (bool)item.Luck;
            selectedOption = (int)item.Option;
            selectedOptions.Clear();

            Console.WriteLine(item.Option);
            if (item.OptionsList.Count > 0)
            {

                selectedOptions.AddRange(item.OptionsList);
            }
            selectedItem = item;



        }

        private void HandleSelectOption(Option option)
        {
            if (!selectedOptions.Contains(option))
            {
                selectedOptions.Add(option);
            }
        }
        private void HandleUnselectOption(Option option)
        {
            if (selectedOptions.Contains(option))
            {
                int indexof = selectedOptions.IndexOf(option);
                Console.Write(indexof);
                selectedOptions.RemoveAt(indexof);
            }
        }

        public static string RemoveDiacritics(string text)
        {
            string normalized = text.Normalize(System.Text.NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (char c in from c in normalized
                               let u = CharUnicodeInfo.GetUnicodeCategory(c)
                               where u != UnicodeCategory.NonSpacingMark
                               select c)
            {
                sb.Append(c);
            }
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

    }

}