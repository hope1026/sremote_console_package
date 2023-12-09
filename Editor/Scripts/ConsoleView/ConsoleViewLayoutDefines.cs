// 
// Copyright 2015 https://github.com/hope1026

using UnityEngine;

namespace SPlugin
{
    internal static class ConsoleViewLayoutDefines
    {
        public static Vector2 windowSize = new Vector2(900.0f, 450.0f);
        public const float WIDTH_PER_CHAR = 10.0f;
        public const float VERTICAL_SCROLL_WIDTH = 15.0f;

        public static class WindowID
        {
            public const int SYSTEM_MESSAGE_VIEW = 1;
        }

        public static class ViewSelectMenu
        {
            public static Rect areaRect = new Rect(0f, 0f, windowSize.x, 20f);

            public static void OnChangeWindowSize()
            {
                areaRect.Set(0f, 0f, windowSize.x, 20f);
            }
        }
        
        public static class View
        {
            public static Rect areaRect = new Rect(0f, ViewSelectMenu.areaRect.yMax, windowSize.x, windowSize.y - ViewSelectMenu.areaRect.height);

            public static void OnChangeWindowSize()
            {
                areaRect.Set(0f, ViewSelectMenu.areaRect.yMax, windowSize.x, windowSize.y - ViewSelectMenu.areaRect.height);
            }
        }

        public static class CommandView
        {
            public const float CATEGORY_TAP_LINE_HEIGHT = 20f;
            public static int categoryLineCount = 1;

            //뷰 전체 영역
            public static Rect areaRect = new Rect(0f, View.areaRect.yMin, View.areaRect.width, View.areaRect.height);

            public static Rect categoryTapAreaRect = new Rect(0f, 0f, areaRect.width, (CATEGORY_TAP_LINE_HEIGHT * categoryLineCount) + 5);
            public static Rect commandListAreaRect = new Rect(0f, categoryTapAreaRect.yMax, areaRect.width, areaRect.height - categoryTapAreaRect.height);
            public static Rect commandNameDragLineRect = new Rect(300f, 0, 1f, commandListAreaRect.height);

            public static void OnChangeWindowSize()
            {
                areaRect.Set(0f, View.areaRect.yMin, View.areaRect.width, View.areaRect.height);
                categoryTapAreaRect.Set(0f, 0f, areaRect.width, (CATEGORY_TAP_LINE_HEIGHT * categoryLineCount) + 5);
                commandListAreaRect.Set(0f, categoryTapAreaRect.yMax, areaRect.width, areaRect.height - categoryTapAreaRect.height);
                commandNameDragLineRect.Set(300f, 0, 1f, commandListAreaRect.height);
            }
        }

        public static class ApplicationView
        {
            public static Rect systemInfoWindowAreaRect = new Rect(0f, 0f, 600f, 360f);
        }

        public static class LogViewToolbarWidget
        {
            public static Rect areaRect = new Rect(0f, 0f, View.areaRect.width, 20f);

            public static void OnChangeWindowSize()
            {
                areaRect.Set(0f, 0f, View.areaRect.width, 20f);
            }
        }

        public static class LogViewExcludeFilterAndSearchMenuWidget
        {
            public const float LINE_HEIGHT = 20f;
            public static int lineCount = 1;
            public const float SEARCH_LABEL_WIDTH = 60f;
            public const float EXCLUDE_FILTER_LABEL_WIDTH = 100f;
            public const float SEARCH_TEXT_FIELD_WIDTH = 60f;
            public const float QUICK_SEARCH_LABEL_WIDTH = 100f;
            public static Rect areaRect = new Rect(0f, LogViewToolbarWidget.areaRect.yMax, LogViewToolbarWidget.areaRect.width, (LINE_HEIGHT + 5f) * lineCount);

            public static void OnChangeWindowSize()
            {
                areaRect.Set(0f, LogViewToolbarWidget.areaRect.yMax, LogViewToolbarWidget.areaRect.width, (LINE_HEIGHT + 5f) * lineCount);
            }
        }

        public static class LogListWidget
        {
            //로그뷰 전체 영역
            public static Rect areaRect = new Rect(0f, LogViewExcludeFilterAndSearchMenuWidget.areaRect.yMax, LogViewToolbarWidget.areaRect.width,
                                                   windowSize.y - LogViewToolbarWidget.areaRect.height - LogViewExcludeFilterAndSearchMenuWidget.areaRect.height);

            //로그뷰 기준 상대 영역 
            public static class Area
            {
                public static Rect areaTitleRect = new Rect(0f, 0f, areaRect.width, 20f);
                public const float ICON_WIDTH = 30f;
                public static float timeWidth = 100f;
                public static float frameCountWidth = 100f;
                public static float objectNameWidth = 100f;

                public static float logListAreaRatio = 0.8f;
                public static Rect logListAreaRect = new Rect(0f, areaTitleRect.yMax, areaRect.width, (areaRect.height - areaTitleRect.height) * logListAreaRatio);

                public static class AreaItemList
                {
                    //로그 1개(아이템) 세로 크기
                    public const float ITEM_HEIGHT = 35.0f;

                    //스크롤 영역
                    public static Rect scrollVerticalRect = new Rect(logListAreaRect.xMax - VERTICAL_SCROLL_WIDTH, 0f, VERTICAL_SCROLL_WIDTH, logListAreaRect.height);

                    public static void OnChangeAreaItemListWindowSize()
                    {
                        scrollVerticalRect.Set(logListAreaRect.xMax - VERTICAL_SCROLL_WIDTH, 0f, VERTICAL_SCROLL_WIDTH, logListAreaRect.height);
                    }
                }

                //로그 스택 트레이스 영역
                public static Rect areaStackTraceRect = new Rect(0f, logListAreaRect.yMax, areaRect.width, areaRect.height - logListAreaRect.height - areaTitleRect.height);

                public static void OnChangeLogViewWindowSize()
                {
                    logListAreaRatio = Mathf.Min(logListAreaRatio, 0.9f);
                    areaTitleRect.Set(0f, 0f, areaRect.width, 20f);
                    logListAreaRect.Set(0f, areaTitleRect.yMax, areaRect.width, (areaRect.height - areaTitleRect.height) * logListAreaRatio);
                    areaStackTraceRect.Set(0f, logListAreaRect.yMax, areaRect.width, areaRect.height - logListAreaRect.height - areaTitleRect.height);
                    AreaItemList.OnChangeAreaItemListWindowSize();
                }
            }

            public static void OnChangeWindowSize()
            {
                areaRect.Set(0f, LogViewExcludeFilterAndSearchMenuWidget.areaRect.yMax, LogViewToolbarWidget.areaRect.width,
                             windowSize.y - LogViewToolbarWidget.areaRect.height - LogViewExcludeFilterAndSearchMenuWidget.areaRect.height);
                Area.OnChangeLogViewWindowSize();
            }
        }

        public static void OnChangeWindowSize(float newWindowWidth_, float newWindowHeight_)
        {
            windowSize.x = newWindowWidth_;
            windowSize.y = newWindowHeight_;
            
            ViewSelectMenu.OnChangeWindowSize();
            View.OnChangeWindowSize();
            LogViewToolbarWidget.OnChangeWindowSize();
            LogViewExcludeFilterAndSearchMenuWidget.OnChangeWindowSize();
            CommandView.OnChangeWindowSize();
            LogListWidget.OnChangeWindowSize();
        }
    }
}