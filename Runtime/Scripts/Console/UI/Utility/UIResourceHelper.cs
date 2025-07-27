//
// Copyright 2015 https://github.com/hope1026

using UnityEngine;
using UnityEngine.UIElements;

namespace SPlugin.RemoteConsole.Runtime
{
    /// <summary>
    /// Helper class for managing common UI resources like fonts, styles, and built-in assets
    /// </summary>
    internal static class UIResourceHelper
    {
        private static Font _defaultRuntimeFont;
        private static StyleSheet _baseRuntimeStyles;

        /// <summary>
        /// Gets the default font for runtime UI elements
        /// </summary>
        public static Font DefaultRuntimeFont
        {
            get
            {
                if (_defaultRuntimeFont == null)
                {
                    _defaultRuntimeFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                }
                return _defaultRuntimeFont;
            }
        }

        /// <summary>
        /// Gets the base runtime style sheet
        /// </summary>
        public static StyleSheet BaseRuntimeStyles
        {
            get
            {
                if (_baseRuntimeStyles == null)
                {
                    _baseRuntimeStyles = Resources.Load<StyleSheet>("UI/BaseRuntimeStyles");
                }
                return _baseRuntimeStyles;
            }
        }

        /// <summary>
        /// Applies default font to a VisualElement
        /// </summary>
        /// <param name="element_">The VisualElement to apply font to</param>
        public static void ApplyDefaultFont(VisualElement element_)
        {
            if (element_ != null && DefaultRuntimeFont != null)
            {
                element_.style.unityFont = DefaultRuntimeFont;
            }
        }

        /// <summary>
        /// Applies default font to multiple VisualElements
        /// </summary>
        /// <param name="elements_">Array of VisualElements to apply font to</param>
        public static void ApplyDefaultFont(params VisualElement[] elements_)
        {
            if (elements_ == null) return;

            Font defaultFont = DefaultRuntimeFont;
            if (defaultFont == null) return;

            foreach (VisualElement element in elements_)
            {
                if (element != null)
                {
                    element.style.unityFont = defaultFont;
                }
            }
        }

        /// <summary>
        /// Loads a StyleSheet from Resources/UI directory
        /// </summary>
        /// <param name="stylesheetName_">Name of the stylesheet (without .uss extension)</param>
        /// <returns>The loaded StyleSheet or null if not found</returns>
        public static StyleSheet LoadStyleSheet(string stylesheetName_)
        {
            if (string.IsNullOrEmpty(stylesheetName_)) return null;
            return Resources.Load<StyleSheet>($"UI/{stylesheetName_}");
        }

        /// <summary>
        /// Loads a VisualTreeAsset from Resources/UI directory
        /// </summary>
        /// <param name="uxmlName_">Name of the UXML file (without .uxml extension)</param>
        /// <returns>The loaded VisualTreeAsset or null if not found</returns>
        public static VisualTreeAsset LoadUXML(string uxmlName_)
        {
            if (string.IsNullOrEmpty(uxmlName_)) return null;
            return Resources.Load<VisualTreeAsset>($"UI/{uxmlName_}");
        }

        /// <summary>
        /// Applies base styles to a VisualElement
        /// </summary>
        /// <param name="element_">The VisualElement to apply styles to</param>
        public static void ApplyBaseStyles(VisualElement element_)
        {
            if (element_ != null && BaseRuntimeStyles != null)
            {
                element_.styleSheets.Add(BaseRuntimeStyles);
            }
        }

        /// <summary>
        /// Creates a Button with default font applied
        /// </summary>
        /// <param name="text_">Button text</param>
        /// <returns>Button with default font applied</returns>
        public static Button CreateButton(string text_ = "")
        {
            Button button = new Button();
            button.text = text_;
            ApplyDefaultFont(button);
            return button;
        }

        /// <summary>
        /// Creates a Label with default font applied
        /// </summary>
        /// <param name="text_">Label text</param>
        /// <returns>Label with default font applied</returns>
        public static Label CreateLabel(string text_ = "")
        {
            Label label = new Label();
            label.text = text_;
            ApplyDefaultFont(label);
            return label;
        }

        /// <summary>
        /// Creates a TextField with default font applied
        /// </summary>
        /// <param name="placeholder_">Placeholder text</param>
        /// <returns>TextField with default font applied</returns>
        public static TextField CreateTextField(string placeholder_ = "")
        {
            TextField textField = new TextField();
            if (!string.IsNullOrEmpty(placeholder_))
            {
                textField.value = placeholder_;
            }
            ApplyDefaultFont(textField);
            return textField;
        }

        /// <summary>
        /// Recursively applies default font to a VisualElement and all its children
        /// </summary>
        /// <param name="element_">The root VisualElement to apply font to</param>
        public static void ApplyDefaultFontRecursive(VisualElement element_)
        {
            if (element_ == null) return;

            Font defaultFont = DefaultRuntimeFont;
            if (defaultFont == null) return;

            // Apply font to current element
            element_.style.unityFont = defaultFont;

            // Apply font to all child elements recursively
            foreach (VisualElement child in element_.Children())
            {
                ApplyDefaultFontRecursive(child);
            }
        }
    }
}