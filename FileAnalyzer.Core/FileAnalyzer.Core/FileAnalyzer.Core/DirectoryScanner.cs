using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FileAnalyzer.Core
{
    public class DirectoryScanner
    {
        private readonly SemaphoreSlim _semaphore;
        private readonly CancellationToken _token;

        public DirectoryScanner(int maxThreads, CancellationToken token)
        {
            _semaphore = new SemaphoreSlim(maxThreads);
            _token = token;
        }

        public async Task<FileNode> ScanAsync(string path)
        {
            var root = new FileNode
            {
                Name = Path.GetFileName(path),
                FullPath = path,
                IsDirectory = true
            };

            var queue = new ConcurrentQueue<FileNode>();
            queue.Enqueue(root);

            var tasks = new List<Task>();

            while (queue.TryDequeue(out var node))
            {
                await _semaphore.WaitAsync(_token);

                var task = Task.Run(() =>
                {
                    try
                    {
                        ProcessDirectory(node, queue);
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                }, _token);

                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            CalculateSizes(root);

            return root;
        }

        private void ProcessDirectory(FileNode node, ConcurrentQueue<FileNode> queue)
        {
            if (_token.IsCancellationRequested)
                return;

            try
            {
                var dirInfo = new DirectoryInfo(node.FullPath);

                foreach (var file in dirInfo.GetFiles())
                {
                    if (file.LinkTarget != null) continue;

                    node.Children.Add(new FileNode
                    {
                        Name = file.Name,
                        FullPath = file.FullName,
                        Size = file.Length,
                        IsDirectory = false
                    });
                }

                foreach (var dir in dirInfo.GetDirectories())
                {
                    if (dir.LinkTarget != null) continue;

                    var child = new FileNode
                    {
                        Name = dir.Name,
                        FullPath = dir.FullName,
                        IsDirectory = true
                    };

                    node.Children.Add(child);
                    queue.Enqueue(child);
                }
            }
            catch
            {
                
            }
        }

        private long CalculateSizes(FileNode node)
        {
            if (!node.IsDirectory)
                return node.Size;

            long total = 0;

            foreach (var child in node.Children)
            {
                total += CalculateSizes(child);
            }

            node.Size = total;
            return total;
        }
    }
}
