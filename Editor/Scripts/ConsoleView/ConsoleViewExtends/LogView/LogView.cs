// 
// Copyright 2015 https://github.com/hope1026

namespace SPlugin
{
    internal class LogView : ConsoleViewAbstract
    {
        public override ConsoleViewType ConsoleViewType => ConsoleViewType.LOG;
        private LogTapToolbarWidget _logTapToolbarWidget = null;
        private SearchAndExcludeWidget _searchAndExcludeWidget = null;
        private LogListWidget _logWidget = null;

        public LogTapToolbarWidget LogTapToolbarWidget => _logTapToolbarWidget;

        protected override void OnInitialize()
        {
            ConsoleEditorPrefs.ReadPrefs();

            _logTapToolbarWidget = new LogTapToolbarWidget();
            _logTapToolbarWidget.Initialize(this);

            _searchAndExcludeWidget = new SearchAndExcludeWidget();
            _searchAndExcludeWidget.Initialize(this);

            _logWidget = new LogListWidget();
            _logWidget.Initialize(this);
        }

        public override void OnGuiCustom()
        {
            if (null == ConsoleViewMainRef || CurrentAppRef == null)
                return;

            _logTapToolbarWidget?.OnGuiCustom();
            _searchAndExcludeWidget?.OnGuiCustom();
            _logWidget?.OnGuiCustom();
        }

        public override void UpdateCustom()
        {
            if (null == ConsoleViewMainRef || CurrentAppRef == null)
                return;
            
            _logTapToolbarWidget?.UpdateCustom();
            _logWidget?.UpdateCustom();

            ClearLogItemsIfClickedClearButton();
        }

        private void ClearLogItemsIfClickedClearButton()
        {
            if (true == ConsoleEditorPrefs.CanClearLog)
            {
                CurrentAppRef?.logCollection.ClearItems();
                ConsoleEditorPrefs.CanClearLog = false;
            }
        }

        public void ForceLogViewScrollBarBottomFixed()
        {
            if (null != _logWidget)
            {
                _logWidget.SetBottomFixedScrollBar(true);
            }
        }

        public void ForcePause(bool isPause_, bool canStep_)
        {
            _logTapToolbarWidget?.ForcePause(isPause_, canStep_);
        }
    }
}