using AJ.Engine.Interfaces.FileManager;
using AJ.Engine.Interfaces.ModuleManagement;
using AJ.Engine.Interfaces.TimeManagement;
using AJ.Engine.Interfaces.Util.Strings;
using AJ.Engine.Logging.Interfaces;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Text;

namespace AJ.Engine.FileManagement
{
    internal class FileManager : IFileManager, IModule
    {
        private readonly TimeSpan CHECK_FILE_CHANGE_INTERVAL = TimeSpan.FromSeconds(5);
        private const string ASSET_DIR = ".Assets.";

        private readonly ILogger _logger;
        private readonly IGameTime _gameTime;
        private readonly ConcurrentDictionary<string, FileHandle> _internalFileHandles;
        private readonly ConcurrentDictionary<string, FileHandle> _externalFileHandles;
        private readonly ITimer _checkForFileChangeTimer;

        internal FileManager(IModuleProvider moduleProvider)
        {
            _logger = moduleProvider.Get<ILogger>();
            _gameTime = moduleProvider.Get<IGameTime>();
            _internalFileHandles = new ConcurrentDictionary<string, FileHandle>();
            _externalFileHandles = new ConcurrentDictionary<string, FileHandle>();
            _checkForFileChangeTimer = _gameTime.CreateTimer(CHECK_FILE_CHANGE_INTERVAL);
        }

        public void ScanInternalFiles()
        {
            StringBuilder internalFilesFound = new StringBuilder();
            internalFilesFound.AppendLine("Initialized!");

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                string[] assetNames = assembly.GetManifestResourceNames();
                foreach (string assetName in assetNames)
                {
                    if (assetName.Contains(ASSET_DIR))
                    {
                        _internalFileHandles.TryAdd(assetName, new FileHandle(assetName, assembly));
                        internalFilesFound = internalFilesFound.AppendLine($"asset:\t{assetName}");
                    }
                }
            }

            internalFilesFound.Remove(internalFilesFound.Length - NewLine.Length, NewLine.Length);
            _logger.LogInfo("FileManager", internalFilesFound.ToString());
        }

        public IFileHandle LoadFile(string path)
        {
            if (path.StartsWith('*'))
                return LoadExternalFile(path);
            else
                return LoadInternalFile(path);
        }

        private FileHandle LoadExternalFile(string path)
        {
            string newPath = path.Replace("*", Directory.GetCurrentDirectory());
            newPath = newPath.Replace('\\', Path.DirectorySeparatorChar);
            newPath = newPath.Replace('/', Path.DirectorySeparatorChar);

            if (_externalFileHandles.ContainsKey(path))
                return _externalFileHandles[path];

            if (File.Exists(newPath))
            {
                var fh = new FileHandle(path, newPath);
                _externalFileHandles.TryAdd(path, fh);
                return fh;
            }

            _logger.LogWarning("FileManager", $"File {newPath} not found!");
            return null;
        }

        private FileHandle LoadInternalFile(string path)
        {
            if (_internalFileHandles.ContainsKey(path))
                return _internalFileHandles[path];

            _logger.LogWarning("FileManager", $"File {path} not found!");
            return null;
        }

        void IModule.Update()
        {
            if (_checkForFileChangeTimer.HasElapsed())
            {
                if (_externalFileHandles.Count > 0)
                {
                    foreach (var fh in _externalFileHandles.Values)
                        fh.Update();
                }
            }
        }
    }
}