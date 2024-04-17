using System;
using System.IO;
using System.Reflection;
using AJ.Engine.Interfaces.FileManager;

namespace AJ.Engine.FileManagement
{
    internal class FileHandle : IFileHandle
    {
        public string Path
        {
            get;
            private set;
        }

        public string AbsolutePath
        {
            get;
            private set;
        }

        public bool IsExternal
        {
            get;
            private set;
        }

        private Assembly _assembly;
        private DateTime _lastWriteDate;

        public event OnFileChange OnFileChange;

        internal FileHandle(string path, string absolutePath)
        {
            Path = path;
            AbsolutePath = absolutePath;
            _assembly = null;
            IsExternal = true;
            _lastWriteDate = File.GetLastWriteTimeUtc(absolutePath);
        }

        internal FileHandle(string absolutePath, Assembly assembly)
        {
            Path = absolutePath;
            AbsolutePath = absolutePath;
            _assembly = assembly;
            IsExternal = false;
        }

        public Stream OpenRead()
        {
            if (IsExternal)
                return File.Open(AbsolutePath, FileMode.Open);
            else
                return _assembly.GetManifestResourceStream(AbsolutePath);
        }

        public Stream OpenWrite()
        {
            if (IsExternal)
                return File.OpenWrite(AbsolutePath);
            return null;
        }

        internal void Update()
        {
            var newLastWriteDate = File.GetLastWriteTimeUtc(AbsolutePath);

            if (DateTime.Compare(newLastWriteDate, _lastWriteDate) > 0)
            {
                OnFileChange?.Invoke(this);
                _lastWriteDate = newLastWriteDate;
            }
        }
    }
}
