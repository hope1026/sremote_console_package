// 
// Copyright 2015 https://github.com/hope1026

using System;
using UnityEditor;
using UnityEngine;

namespace SPlugin
{
    internal static class SGuiResources
    {
        private const string LOG_ICON_PATH = "log.png";
        private const string WARNING_ICON_PATH = "warning.png";
        private const string ERROR_ICON_PATH = "error.png";
        private const string PAUSE_ICON_PATH = "pause.png";
        private const string STEP_ICON_PATH = "step.png";
        private const string PAUSE_PLAY_ICON_PATH = "pause_play.png";
        private const string STEP_PLAY_ICON_PATH = "step_play.png";
        private const string SPLUGIN_TITLE_ICON_PATH = "splugin_title.png";
        private const string OPTION_ICON_PATH = "option.png";
        private const string HIDE_WINDOW_ICON_PATH = "hide_window.png";
        private const string SHOW_WINDOW_ICON_PATH = "show_window.png";

        private static Texture2D _logIconTexture = null;
        private static Texture2D _warningIconTexture = null;
        private static Texture2D _errorIconTexture = null;
        private static Texture2D _pauseTexture = null;
        private static Texture2D _stepTexture = null;
        private static Texture2D _pausePlayTexture = null;
        private static Texture2D _stepPlayTexture = null;
        private static Texture2D _titleIconTexture = null;
        private static Texture2D _optionIconTexture = null;
        private static Texture2D _hideWindowIconTexture = null;
        private static Texture2D _showWindowIconTexture = null;

        public static Texture2D LogIconTexture { get { return InstanceResourceImage(LOG_ICON_PATH, ref _logIconTexture); } }
        public static Texture2D WarningIconTexture { get { return InstanceResourceImage(WARNING_ICON_PATH, ref _warningIconTexture); } }
        public static Texture2D ErrorIconTexture { get { return InstanceResourceImage(ERROR_ICON_PATH, ref _errorIconTexture); } }
        public static Texture2D PauseTexture { get { return InstanceResourceImage(PAUSE_ICON_PATH, ref _pauseTexture); } }
        public static Texture2D StepTexture { get { return InstanceResourceImage(STEP_ICON_PATH, ref _stepTexture); } }
        public static Texture2D PausePlayTexture { get { return InstanceResourceImage(PAUSE_PLAY_ICON_PATH, ref _pausePlayTexture); } }
        public static Texture2D StepPlayTexture { get { return InstanceResourceImage(STEP_PLAY_ICON_PATH, ref _stepPlayTexture); } }
        public static Texture2D TitleIconTexture { get { return InstanceResourceImage(SPLUGIN_TITLE_ICON_PATH, ref _titleIconTexture); } }
        public static Texture2D OptionIconTexture { get { return InstanceResourceImage(OPTION_ICON_PATH, ref _optionIconTexture); } }
        public static Texture2D HideWindowIconTexture { get { return InstanceResourceImage(HIDE_WINDOW_ICON_PATH, ref _hideWindowIconTexture); } }
        public static Texture2D ShowWindowIconTexture { get { return InstanceResourceImage(SHOW_WINDOW_ICON_PATH, ref _showWindowIconTexture); } }


        private static Texture2D InstanceResourceImage(string imageFileName_, ref Texture2D instanceTexture_)
        {
            if (null != instanceTexture_)
                return instanceTexture_;

            instanceTexture_ = AssetDatabase.LoadAssetAtPath<Texture2D>($"Packages/com.splugin.remoteconsole/Editor/Resources/Textures/{imageFileName_}");
            if (instanceTexture_ != null && true == imageFileName_.Equals(SPLUGIN_TITLE_ICON_PATH, StringComparison.OrdinalIgnoreCase))
            {
                SConsoleEditorWindow.ResetTitleIcon(instanceTexture_);
            }

            return instanceTexture_;
        }
    }
}