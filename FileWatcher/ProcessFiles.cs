using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using System.Configuration;
using System.Collections.Concurrent;

namespace FileWatcher
{
    public class ProcessFiles
    {
        public ConcurrentQueue<object> queue = new ConcurrentQueue<object>();
        private static System.Timers.Timer timer = new System.Timers.Timer();

        public ProcessFiles()
        {
            double interval;
            double.TryParse(ConfigurationManager.AppSettings["FileWatcherTimeInSeconds"], out interval);
            Logger.Info(string.Format("setting timer to {0} seconds", interval), "ProcessFiles");
            interval *= 1000;

            timer.Interval = interval;
            timer.Elapsed += timer_Elapsed;
            timer.Start();
            Logger.Info("timer started", "ProcessFiles");
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (queue.Count > 0)
            {
                timer.Stop();

                object result;
                while (queue.TryDequeue(out result))
                {
                    if (result.GetType() == typeof(FileSystemEventArgs))
                    {
                        FileSystemEventArgs arg = (FileSystemEventArgs)result;
                        switch (arg.ChangeType)
                        {
                            case WatcherChangeTypes.Changed:
                            case WatcherChangeTypes.Created: CopyFile(arg.FullPath); break;
                            case WatcherChangeTypes.Deleted: DeleteFile(arg.FullPath); break;
                        }
                    }
                    else
                    {
                        RenamedEventArgs arg = (RenamedEventArgs)result;
                        RenameFile(arg.OldFullPath, arg.FullPath);
                    }
                }

                timer.Start();
            }
        }

        #region File Operations
        private void DeleteFile(string path)
        {
            try
            {
                if (!string.IsNullOrEmpty(path))
                {
                    FileInfo fromFile = new FileInfo(path);
                    FileInfo toFile = GetToFile(fromFile.DirectoryName, fromFile.Name);
                    toFile.Delete();
                    Logger.Info(string.Format("deleted file: {0}", toFile.FullName), "DeleteFile");
                }
                else
                {
                    Logger.Warning(string.Format("{0} does not exist", path), "DeleteFile");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "DeleteFile");
                MessageBox.Show(ex.Message);
            }
        }

        private void CopyFile(string path)
        {
            try
            {
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    FileInfo fromFile = new FileInfo(path);
                    FileInfo toFile = GetToFile(fromFile.DirectoryName, fromFile.Name);
                    Directory.CreateDirectory(toFile.DirectoryName);
                    fromFile.CopyTo(toFile.FullName, true);
                    Logger.Info(string.Format("copying file from {0} to {1}", fromFile.FullName, toFile.FullName), "CopyFile");
                }
                else
                {
                    Logger.Warning(string.Format("{0} does not exist", path), "CopyFile");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "CopyFile");
                MessageBox.Show(ex.Message);
            }
        }

        private void RenameFile(string oldFullPath, string fullPath)
        {
            try
            {
                if(!string.IsNullOrEmpty(fullPath) && File.Exists(fullPath))
                {
                    FileInfo origFile = new FileInfo(oldFullPath);
                    FileInfo renamedFile = new FileInfo(fullPath);
                    FileInfo toFile = GetToFile(origFile.DirectoryName, origFile.Name);
                    string result = Path.Combine(toFile.DirectoryName, renamedFile.Name);
                    toFile.MoveTo(result);
                    Logger.Info(string.Format("renaming file from {0} to {1}", toFile.FullName, result), "RenameFile");
                }
                else
                {
                    Logger.Warning(string.Format("{0} does not exist", fullPath), "RenameFile");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "RenameFile");
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        #region helper methods
        private FileInfo GetToFile(string fromDirectory, string fileName)
        {
            string path = string.Empty;

            try
            {
                path = (from folder in WatchFiles.folders
                        let toPath = fromDirectory.Replace(folder.Key, folder.Value)
                        where fromDirectory.Contains(folder.Key)
                        select Path.Combine(toPath, fileName)).FirstOrDefault();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "GetToFile");
            }

            return new FileInfo(path);
        }

        public string FormatDirectory(string directory)
        {
            if (!directory.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                directory = string.Concat(directory, Path.DirectorySeparatorChar);
            }
            return directory;
        }
        #endregion
    }
}
