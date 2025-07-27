// 
// Copyright 2015 https://github.com/hope1026

using UnityEngine.UIElements;

namespace SPlugin.RemoteConsole.Runtime
{
    internal abstract class ConsoleViewAbstract
    {
        protected SConsoleApp currentAppRef = null;
        protected VisualElement _rootElement = null;
        public SConsoleApp CurrentAppRef => currentAppRef;
        public abstract ConsoleViewType ConsoleViewType { get; }

        public void Initialize(VisualElement rootElement_, SConsoleApp currentApp_)
        {
            _rootElement = rootElement_;
            currentAppRef = currentApp_;
            OnInitialize();
        }

        public void Terminate()
        {
            OnTerminate();
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