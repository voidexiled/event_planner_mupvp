using System;
using System.Collections.Generic;
namespace MUPVPUI
{
    public static class WindowManager
    {
        public static bool WINDOW_ITEM_LIST = false;

        public static bool WINDOW_REWARD_LIST = false;

        public static bool WINDOW_SET_LIST = false;

        public static bool WINDOW_GUIDES = false;

        public static bool WINDOW_SELECTED_GUIDE = false;

        public static bool WINDOW_USER_ADMINISTRATION = false;

        public static bool WINDOW_NEW_USER = false;

        public static bool WINDOW_ADMIN_GUIDES = false;
        public static bool WINDOW_SELECTED_ADMIN_GUIDE = false;
        public static bool WINDOW_RELATED_GUIDES = false;

        public static void CloseAllWindows()
        {
            WINDOW_ITEM_LIST = false;
            WINDOW_REWARD_LIST = false;
            WINDOW_SET_LIST = false;
            WINDOW_GUIDES = false;
            WINDOW_SELECTED_GUIDE = false;
            WINDOW_USER_ADMINISTRATION = false;
            WINDOW_NEW_USER = false;
            WINDOW_ADMIN_GUIDES = false;
            WINDOW_SELECTED_ADMIN_GUIDE = false;
        }
    }
}
