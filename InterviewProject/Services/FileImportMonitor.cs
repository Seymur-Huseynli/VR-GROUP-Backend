using InterviewProject.Repositories;

namespace InterviewProject.Services
{
    public class FileImportMonitor
    {
        private readonly string _monitoredFolderPath;
        private readonly BoxRepository _boxRepository;

        public FileImportMonitor(string monitoredFolderPath, BoxRepository boxRepository)
        {
            _monitoredFolderPath = monitoredFolderPath;
            _boxRepository = boxRepository;
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

        private async Task OnCreatedAsync(object source, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Created)
                return;

            int batchSize = 2;

            using var boxFileReader = new BoxFileReader(e.FullPath, batchSize);

            await foreach (var boxes in boxFileReader)
            {
                await _boxRepository.BulkInsertAsync(boxes);
            }
        }
    }
}
