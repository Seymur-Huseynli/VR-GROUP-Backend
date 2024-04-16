using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterviewProject.Services
{
    public class FileImportMonitor
    {
        private readonly string _monitoredFolderPath;

        public FileImportMonitor(string monitoredFolderPath)
        {
            _monitoredFolderPath = monitoredFolderPath;
        }

        public void StartMonitoring()
        {
            using (FileSystemWatcher watcher = new FileSystemWatcher())
            {
                watcher.Path = _monitoredFolderPath;
                watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime;
                watcher.Filter = "*.txt";
                watcher.Created += async (sender, eventArgs) => await OnCreatedAsync(sender, eventArgs); ;
                watcher.EnableRaisingEvents = true;
                Console.WriteLine($"Starting monitoring folder: {_monitoredFolderPath}");
                while (true);
            }
        }

        private static async Task OnCreatedAsync(object source, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Created)
                return;

            int batchSize = 2;

            using var boxFileReader = new BoxFileReader(e.FullPath, batchSize);

            await foreach (var boxes in boxFileReader)
            {
                Console.WriteLine(boxes.Count.ToString());
            }
        }
    }
}
