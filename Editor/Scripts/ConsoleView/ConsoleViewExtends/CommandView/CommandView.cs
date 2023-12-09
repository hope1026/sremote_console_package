// 
// Copyright 2015 https://github.com/hope1026

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SPlugin
{
    internal class CommandView : ConsoleViewAbstract
    {
        public override ConsoleViewType ConsoleViewType => ConsoleViewType.COMMAND;
        private readonly SGuiDragLine _commandNameDragLine = new SGuiDragLine();
        private string _selectedCategoryName = string.Empty;

        protected override void OnShow()
        {
            ConsoleViewLayoutDefines.CommandView.OnChangeWindowSize();
        }

        public override void OnGuiCustom()
        {
            SGuiStyle.ActiveAppLabelStyle.fontStyle = FontStyle.Bold;
            SGuiStyle.ActiveAppLabelStyle.fontSize = 20;

            if (CurrentAppRef != null && CurrentAppRef.IsPlaying() == false)
            {
                GUILayout.BeginVertical();
                GUILayout.Space(20);
                GUILayout.Label("Command feature is valid in play mode.", SGuiStyle.ActiveAppLabelStyle);
                GUILayout.EndVertical();
                return;
            }
            else if (CurrentAppRef == null || CurrentAppRef.commandCollection.commandsByCategory.Count <= 0)
            {
                GUILayout.BeginVertical();
                GUILayout.Space(20);
                GUILayout.Label("You can add commands using the functions below.", SGuiStyle.ActiveAppLabelStyle);
                GUILayout.Label("SCommand.Register(categoryName, commandName, defaultValue, onChangedValueDelegate, displayPriority, tooltip)", SGuiStyle.LabelStyle);
                GUILayout.EndVertical();
                return;
            }

            GUILayout.BeginArea(ConsoleViewLayoutDefines.CommandView.categoryTapAreaRect);
            OnGuiCategoryTaps();
            GUILayout.EndArea();

            GUILayout.BeginArea(ConsoleViewLayoutDefines.CommandView.commandListAreaRect);
            OnGuiCommandList();
            OnGuiVerticalDragLine();
            GUILayout.EndArea();
        }

        private void OnGuiCategoryTaps()
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();

            int lineCount = 1;
            float lineWidth = 0f;
            float lineWidthMax = ConsoleViewLayoutDefines.CommandView.areaRect.width;

            const int WIDTH_PER_CHARACTER = 12;

            SGuiStyle.ButtonStyle.margin.left = 0;
            SGuiStyle.ButtonStyle.margin.right = 0;

            Dictionary<string, List<CommandItemAbstract>> commandsByCategory = CurrentAppRef.commandCollection.commandsByCategory;
            foreach (string categoryName in commandsByCategory.Keys)
            {
                if (string.IsNullOrEmpty(_selectedCategoryName))
                    _selectedCategoryName = categoryName;

                lineWidth += categoryName.Length * WIDTH_PER_CHARACTER + 20f;
                if (lineWidthMax <= lineWidth)
                {
                    lineCount++;
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    lineWidth = 0;
                }

                GUILayoutOption widthOption = GUILayout.ExpandWidth(true);
                GUILayoutOption heightOption = GUILayout.Height(ConsoleViewLayoutDefines.CommandView.CATEGORY_TAP_LINE_HEIGHT);

                bool prevSelected = _selectedCategoryName.Equals(categoryName);
                if (GUILayout.Toggle(prevSelected, categoryName, SGuiStyle.ButtonStyle, widthOption, heightOption))
                {
                    _selectedCategoryName = categoryName;
                }
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            if (ConsoleViewLayoutDefines.CommandView.categoryLineCount != lineCount)
            {
                ConsoleViewLayoutDefines.CommandView.categoryLineCount = lineCount;
                ConsoleViewLayoutDefines.CommandView.OnChangeWindowSize();
            }
        }

        private void OnGuiVerticalDragLine()
        {
            SGuiStyle.LineStyle.normal.background = EditorGUIUtility.whiteTexture;
            Color lineColor = new Color(0.15f, 0.15f, 0.15f);

            Rect dragLineRect = new Rect(ConsoleViewLayoutDefines.CommandView.commandNameDragLineRect);
            Vector2 mousePosition = Vector2.zero;
            Rect collisionOffset = new Rect(-5f, 0f, 10f, 0f);
            if (_commandNameDragLine.OnGuiCustom(ref mousePosition, dragLineRect, collisionOffset, lineColor, SGuiStyle.LineStyle, true))
            {
                const float COMMAND_NAME_WIDTH_MIN = 100f;
                if (COMMAND_NAME_WIDTH_MIN <= mousePosition.x)
                {
                    ConsoleViewLayoutDefines.CommandView.commandNameDragLineRect.xMin = mousePosition.x;
                    ConsoleViewLayoutDefines.CommandView.commandNameDragLineRect.width = 1f;
                }
            }

            SGuiStyle.LineStyle.normal.background = GUI.skin.box.normal.background;
        }

        private void OnGuiCommandList()
        {
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();

            string context = "CommandName";
            context = SGuiUtility.ReplaceBoldString(context);
            GUILayout.Label(context, SGuiStyle.BoxStyle, GUILayout.Width(ConsoleViewLayoutDefines.CommandView.commandNameDragLineRect.xMin));

            context = "CommandValue";
            context = SGuiUtility.ReplaceBoldString(context);
            GUILayout.Label(context, SGuiStyle.BoxStyle, GUILayout.ExpandWidth(expand: true));

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            if (string.IsNullOrEmpty(_selectedCategoryName))
                return;

            if (!CurrentAppRef.commandCollection.commandsByCategory.TryGetValue(_selectedCategoryName, out List<CommandItemAbstract> commandItemList) ||
                commandItemList == null ||
                commandItemList.Count <= 0)
                return;

            GUILayout.BeginVertical();
            GUILayout.Space(5);

            foreach (CommandItemAbstract commandItem in commandItemList)
            {
                commandItem.OnGui(ConsoleViewLayoutDefines.CommandView.commandNameDragLineRect.xMin);
            }

            GUILayout.EndVertical();
        }
    }
}