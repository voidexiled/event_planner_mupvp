using ImGuiNET;
using ClickableTransparentOverlay;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Windows;
using Veldrid;
using SixLabors.Fonts;
namespace IMGUITEST
{
    public class RenderClass : Overlay
    {

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
        static readonly int screenHeight = GetSystemMetrics(SM_CYSCREEN);


        bool showItemListWindow = false;
        bool showRewardListWindow = false;
        bool showSetListWindow = false;
        private double time = 0.0;
        private double speed = 3.0;

        private string sessionKey = "";

        private string keyUserInput = "";
        private string rankUserInput = "";

        private bool isOpenUserAdministrationWindow = false;
        private bool isOpenNewUserWindow = false;

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
                isOpenUserAdministrationWindow = !isOpenUserAdministrationWindow;

            }
            if (isOpenUserAdministrationWindow)
            {
                ImGui.Begin("Administracion de usuarios", ImGuiWindowFlags.AlwaysAutoResize);
                if (ImGui.Button("Agregar nuevo usuario", new Vector2(240, 25)))
                {
                    isOpenNewUserWindow = !isOpenNewUserWindow;
                }
                if (isOpenNewUserWindow)
                {
                    ImGui.Begin("Agregar nuevo usuario", ImGuiWindowFlags.AlwaysAutoResize);
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
            ImGui.SeparatorText("Opciones de G.M.");
            if (ImGui.Button($"{(showItemListWindow ? "Ocultar" : "Abrir")} lista de items", new Vector2(240, 25)))
            {
                showItemListWindow = !showItemListWindow;
            }
            ImGui.Spacing();
            if (ImGui.Button($"{(showRewardListWindow ? "Ocultar" : "Abrir")} lista de recompensas", new Vector2(240, 25)))
            {
                showRewardListWindow = !showRewardListWindow;
            }
            ImGui.Spacing();
            if (ImGui.Button($"{(showSetListWindow ? "Ocultar" : "Abrir")} lista de sets", new Vector2(240, 25)))
            {
                showSetListWindow = !showSetListWindow;
            }


            if (showItemListWindow)
            {
                // Filtro y ordenamiento
                filteredItems = FilterAndSortItems(ItemManager.AllItems, nameFilter, classFilters, typeFilter, groupFilter, sortByTypeAscending);

                ImGui.SetNextWindowSize(new Vector2(340, 420), ImGuiCond.FirstUseEver);
                ImGui.Begin("Lista de Items", ref showItemListWindow, ImGuiWindowFlags.AlwaysAutoResize);

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
                        HandleSelectableClick(item);
                    }
                }
                ImGui.EndChild();
                ImGui.TextColored(new Vector4(1, 0, 0, 1), "Haz CLICK IZQUIERDO para añadirlo a la LISTA DE RECOMPENSAS.");
                ImGui.End();
            }

            if (showRewardListWindow)

            {

                ImGui.Begin("Lista de recompensas", ImGuiWindowFlags.AlwaysAutoResize);

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

            // TODO: LOCAL VARIABLES

            if (showSetListWindow)
            {
                filteredSets = FilterAndSortSets(setNameFilterString, classFilters, typeFilter);
                ImGui.Begin("Lista de sets");

                ImGui.InputText("Filtrar por nombre", ref setNameFilterString, 40);
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
            }
            ImGui.Spacing();
        }

        private void RenderizeUserTools()
        {
            ImGui.SeparatorText("Opciones de usuario");
            if (ImGui.Button("Abrir guias", new Vector2(240, 25)))
            {

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
            }
        }

        private void CheckSessionState()
        {

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



        /* private void ShowDebugWindow()
        
        // {
        //     ImGui.Begin("Debug");
        //     if (ImGui.BeginTabBar("Debug", ImGuiTabBarFlags.Reorderable))
        //     {
        //         if (ImGui.BeginTabItem("Informacion"))
        //         {
        //             ImGui.Text($"Screen Width: {screenWidth}");
        //             ImGui.Text($"Screen Height: {screenHeight}");
        //             ImGui.Text($"Overlay Width: {this.Size.Width}");
        //             ImGui.Text($"Overlay Height: {this.Size.Height}");
        //             ImGui.Text($"Overlay Enabled: {enabled_overlay}");
        //             ImGui.Text($"Session Key: {SessionManager.currentUser.Key}");
        //             ImGui.Text($"Session User Type: {SessionManager.currentUser.UserType}");
        //             ImGui.Text($"Session Logged In: {SessionManager.loggedIn}");
        //             ImGui.Text($"Session Error Message: {SessionManager.errorMessage}");
        //             ImGui.Text($"Session User Rank: {SessionManager.currentUser.GetRankAsString()}");
        //             ImGui.Text($"Session User Color: {GetUserColor(SessionManager.currentUser)}");
        //             ImGui.Text($"Session User Is Owner: {SessionManager.UserIsOwner()}");
        //             ImGui.Text($"Session User Is Administrator: {SessionManager.UserIsAdministrator()}");
        //             ImGui.Text($"Session User Is Community Manager: {SessionManager.UserIsCommunityManager()}");
        //             ImGui.Text($"Session User Is Game Manager: {SessionManager.UserIsGameManager()}");
        //             ImGui.Text($"Session User Is Tutor: {SessionManager.UserIsTutor()}");
        //             ImGui.Text($"Session User Is Premium Gold: {SessionManager.UserIsPremiumGold()}");
        //             ImGui.Text($"Session User Is Premium Silver: {SessionManager.UserIsPremiumSilver()}");
        //             ImGui.Text($"Session User Is Premium Bronze: {SessionManager.UserIsPremiumBronze()}");
        //             ImGui.Text($"Session User Is Vip: {SessionManager.UserIsVip()}");
        //             ImGui.Text($"Session User Is User: {SessionManager.UserIsUser()}");
        //             ImGui.EndTabItem();
        //         }

        //         if (ImGui.BeginTabItem("Interactivo"))
        //         {
        //             if (ImGui.Button("Owner"))
        //             {
        //                 SessionManager.currentUser.UserType = UserTypes.CREADOR;
        //             }

        //             if (ImGui.Button("Admin"))
        //             {
        //                 SessionManager.currentUser.UserType = UserTypes.ADMINISTRADOR;
        //             }

        //             if (ImGui.Button("CM"))
        //             {
        //                 SessionManager.currentUser.UserType = UserTypes.COMMUNITYMANAGER;
        //             }

        //             if (ImGui.Button("GM"))
        //             {
        //                 SessionManager.currentUser.UserType = UserTypes.GAMEMANAGER;
        //             }

        //             if (ImGui.Button("Tutor"))
        //             {
        //                 SessionManager.currentUser.UserType = UserTypes.TUTOR;
        //             }

        //             if (ImGui.Button("VIP2"))
        //             {
        //                 SessionManager.currentUser.UserType = UserTypes.VIP2;
        //             }

        //             if (ImGui.Button("VIP1"))
        //             {
        //                 SessionManager.currentUser.UserType = UserTypes.VIP1;
        //             }

        //             if (ImGui.Button("VIP0"))
        //             {
        //                 SessionManager.currentUser.UserType = UserTypes.VIP0;
        //             }

        //             if (ImGui.Button("User"))
        //             {
        //                 SessionManager.currentUser.UserType = UserTypes.USUARIO;
        //             }
        //             ImGui.EndTabItem();
        //         }

        //         ImGui.EndTabBar();
        //     }

        //     ImGui.End();
        // }*/


        protected override void Render()
        {
            ConfigureStylesImGui(); // Configuro los estilos por defecto que tendra la interfaz
            UpdateColor(); // Este metodo hace funcionar el color arcoiris que uso en los textos
            ConfigureHotkeys(); // Configuro los hotkeys que tendra la interfaz como Ocultar / Mostrar la interfaz
            ConfigureOverlaySize(); // Obtengo el tamaño de la pantalla y resizeo el overlay
            //ImGui.ShowDemoWindow();

            ImGui.Begin("Ocultar ventanas", ImGuiWindowFlags.NoDocking);

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
            ImGui.GetFont().Scale = 0.8f;
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

            if (!showRewardListWindow)
            {
                showRewardListWindow = true;
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

        // Manejar clics en selectables
        private void HandleSelectableClick(Item item)
        {
            // Lógica específica para cada item
            Console.WriteLine($"Clic en {item.Name}");

            // Agrega aquí la lógica que deseas realizar al hacer clic en un item específico
            ProcessedItem newItem = new(item.Group, item.Type, item.Name, item.Slot);
            processedItems.Add(newItem);
            if (!showRewardListWindow)
            {
                showRewardListWindow = true;
            }
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

    }
}