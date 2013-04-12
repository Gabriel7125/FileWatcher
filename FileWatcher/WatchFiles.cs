using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace FileWatcher
{
    public class WatchFiles
    {
        private static ProcessFiles processFiles = new ProcessFiles();

        public static string configFile = Path.Combine(Environment.CurrentDirectory, "folders.xml");
        public static Dictionary<string, string> folders = new Dictionary<string,string>();
        public static Dictionary<string, FileSystemWatcher> fileWatchers = new Dictionary<string, FileSystemWatcher>();
        public static bool isFileLoaded = false;

        public WatchFiles()
        {
            LoadConfigFile();
            if (isFileLoaded)
            {
                SetupFileWatcher();
            }
            else
            {
                MessageBox.Show(string.Format("Can not find XML file at: \n{0}.\n\nFile watcher will NOT be started.", configFile));
            }
        }

        public static void SetupFileWatcher()
        {
            if (folders != null)
            {
                foreach (var folder in folders)
                {
                    Logger.Info(string.Format("setting up file watcher for: {0}", folder), "SetupFileWatcher");
                    FileSystemWatcher watcher = new FileSystemWatcher();
                    watcher.Path = folder.Key;
                    watcher.Filter = "*.*"; //need to do multiple watchers if more than one file type
                    watcher.IncludeSubdirectories = true;
                    //watcher.NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.Size |
                    //                        NotifyFilters.CreationTime | NotifyFilters.LastAccess | NotifyFilters.LastWrite;
                    watcher.NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.CreationTime | NotifyFilters.LastWrite;
                    watcher.Changed += watcher_Changed;
                    watcher.Created += watcher_Changed;
                    watcher.Deleted += watcher_Changed;
                    watcher.Renamed += watcher_Renamed;
                    watcher.EnableRaisingEvents = true;

                    if (!fileWatchers.ContainsKey(folder.Key))
                    {
                        fileWatchers.Add(folder.Key, watcher);
                        Logger.Info(string.Format("done setting up file watcher for: {0}", folder), "SetupFileWatcher");
                    }
                }
            }
        }

        public static void LoadConfigFile()
        {
            try
            {
                if (File.Exists(configFile))
                {
                    XDocument xdoc = XDocument.Load(configFile);

                    foreach (var element in xdoc.Elements("root").Elements("folder"))
                    {
                        string fromCheck = processFiles.FormatDirectory(element.Element("from").Value);
                        string toCheck = processFiles.FormatDirectory(element.Element("to").Value);

                        if (Directory.Exists(fromCheck))
                        {
                            folders.Add(fromCheck, toCheck);
                            Logger.Info(string.Format("added {0} & {1} to dictionary", fromCheck, toCheck), "LoadConfigFile");
                        }
                        else
                        {
                            Logger.Warning(string.Format("{0} does not exist", fromCheck), "LoadConfigFile");
                            MessageBox.Show(string.Format("{0} \nDoes not exist.\n\nThis direcory will NOT be monitored.", fromCheck));
                        }
                    }

                    //some directories might not have existed; recreate xml file based on folders dictionary.
                    XDocument xdocument = new XDocument(
                        new XDeclaration("1.0", null, null),
                        new XElement("root",
                        folders.Select(pair =>
                            new XElement("folder",
                                new XElement("from", pair.Key),
                                new XElement("to", pair.Value)))));
                    xdocument.Save(configFile);
                    Logger.Info("saved config file", "LoadConfigFile");

                    isFileLoaded = true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "LoadConfigFile");
                MessageBox.Show(ex.Message);
            }
        }

        private static void watcher_Renamed(object sender, RenamedEventArgs e)
        {
            Logger.Info(string.Format("queuing file: {0} to be renamed", e.OldFullPath), "watcher_Renamed");
            object lockObject = new object();
            lock (lockObject)
            {
                processFiles.queue.Enqueue(e);
            }
        }

        private static void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            object lockObject = new object();   
            FileInfo fi = new FileInfo(e.FullPath);
            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Changed:
                case WatcherChangeTypes.Created:
                    if (fi.Exists)
                    {
                        Logger.Info(string.Format("queuing file: {0} to be copied", e.FullPath), "watcher_Changed");
                        lock (lockObject)
                        {
                            processFiles.queue.Enqueue(e);
                        }
                    }
                    break;
                case WatcherChangeTypes.Deleted: 
                    Logger.Info(string.Format("queuing file: {0} to be deleted", e.FullPath), "watcher_Changed");
                    lock (lockObject)
                    {
                        processFiles.queue.Enqueue(e);
                    }
                    break;
            }
        }
    }
}
