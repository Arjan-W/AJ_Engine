﻿namespace AJ.Core
{
    public abstract class Application
    {
        public string Title => _title;

        private string _title;

        protected Application(string title) 
        {
            _title = title;
        }

        internal void Initialize()
        {
            OnInitialize();
        }

        protected abstract void OnInitialize();

        internal void Deinitialize()
        {
            OnDeinitialize();
        }

        protected abstract void OnDeinitialize();
    }
}
