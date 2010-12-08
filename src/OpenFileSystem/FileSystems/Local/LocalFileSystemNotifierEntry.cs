﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenFileSystem.IO.FileSystems.Local
{
    public class LocalFileSystemNotifierEntry : IFileSytemNotifierEntry
    {
        readonly FileSystemWatcher _watcher;
        readonly FileSystemNotificationIdentifier _identity;
        int _refCounter;
        readonly List<Action<IFile>> _actions;

        public LocalFileSystemNotifierEntry(FileSystemNotificationIdentifier identity)
        {
            _actions = new List<Action<IFile>>();
            _identity = identity;
            _watcher = new FileSystemWatcher
            {
                Filter = identity.Filter,
                IncludeSubdirectories = identity.IncludeSubDirectories,
                Path = identity.Path.FullPath
            };
        }

        void HandleFileEvent(object sender, FileSystemEventArgs e)
        {
            var target = LocalFileSystem.Instance.GetFile(e.FullPath);
            var errors = new List<Exception>();
            IEnumerable<Action<IFile>> actions;
            lock (_actions)
            {
                actions = _actions.Copy();
            }

            foreach (var action in actions)
            {
                try
                {
                    action(target);
                }
                catch (Exception ex)
                {
                    errors.Add(ex);
                }
            }
            if (errors.Count > 0)
            {
                throw new AggregateException(errors);
            }
        }

        public IDisposable AddNotifiers(params Action<IFile>[] entries)
        {
            var cookie = new DisposableCookie(this, entries);
            lock (_actions)
            {
                _actions.AddRange(entries.Where(x=>x != null));
                _refCounter++;
                if (_refCounter == 1)
                    Activate();
            }
            return cookie;
        }
        bool IsWatching(WatcherChangeTypes changeType)
        {
            return (_identity.ChangeTypes | changeType) == changeType;
        }
        void Activate()
        {
            if (IsWatching(WatcherChangeTypes.Created)) _watcher.Created += HandleFileEvent;
            if (IsWatching(WatcherChangeTypes.Deleted)) _watcher.Deleted += HandleFileEvent;
            if (IsWatching(WatcherChangeTypes.Changed)) _watcher.Changed += HandleFileEvent;
            if (IsWatching(WatcherChangeTypes.Renamed)) _watcher.Renamed += HandleFileEvent;
            _watcher.EnableRaisingEvents = true;
        }
        void Deactivate()
        {
            _watcher.EnableRaisingEvents = false;
            if (IsWatching(WatcherChangeTypes.Created)) _watcher.Created -= HandleFileEvent;
            if (IsWatching(WatcherChangeTypes.Deleted)) _watcher.Deleted -= HandleFileEvent;
            if (IsWatching(WatcherChangeTypes.Changed)) _watcher.Changed -= HandleFileEvent;
            if (IsWatching(WatcherChangeTypes.Renamed)) _watcher.Renamed -= HandleFileEvent;
        }
        class DisposableCookie : IDisposable
        {
            readonly LocalFileSystemNotifierEntry _entry;
            readonly Action<IFile>[] _notifications;

            public DisposableCookie(LocalFileSystemNotifierEntry entry, params Action<IFile>[] notifications)
            {
                _entry = entry;
                _notifications = notifications.ToArray();
            }

            public void Dispose()
            {
                lock (_entry._actions)
                {
                    foreach (var value in _notifications)
                        _entry._actions.Remove(value);
                    _entry._refCounter--;
                    if (_entry._refCounter == 0)
                    {
                        _entry.Deactivate();
                    }
                }
            }
        }
    }
}