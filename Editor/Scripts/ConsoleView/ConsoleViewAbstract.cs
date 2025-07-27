// 
// Copyright 2015 https://github.com/hope1026

namespace SPlugin.RemoteConsole.Editor
{
    internal abstract class ConsoleViewAbstract
    {
        private ConsoleViewMain _consoleViewMainRef = null;
        private AppAbstract _currentAppRef = null;
        public ConsoleViewMain ConsoleViewMainRef => _consoleViewMainRef;
        public AppAbstract CurrentAppRef => _currentAppRef;
        public abstract ConsoleViewType ConsoleViewType { get; }

        public void Initialize(ConsoleViewMain consoleViewMain_, AppAbstract currentApp_)
        {
            _consoleViewMainRef = consoleViewMain_;
            _currentAppRef = currentApp_;
            OnInitialize();
        }

        public void Terminate()
        {
            OnTerminate();
            _consoleViewMainRef = null;
        }

        public void OnChangeCurrentApp(AppAbstract currentApp_)
        {
            _currentAppRef = currentApp_;
        }

        public void Show()
        {
            OnShow();
        }

        public void Hide()
        {
            OnHide();
        }

        public virtual void UpdateCustom() { }
        
        protected virtual void OnInitialize() { }
        protected virtual void OnShow() { }
        protected virtual void OnHide() { }
        protected virtual void OnTerminate() { }
    }
}