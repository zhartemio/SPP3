using System;
using System.Collections.ObjectModel;
using FileAnalyzer.Core;


namespace FileAnalyzer.WPF
{
    
    public class FileNodeViewModel : BaseViewModel
    {
        public string Name { get; }
        public long Size { get; }
        public double Percentage { get; }
        public bool IsDirectory { get; }

        public ObservableCollection<FileNodeViewModel> Children { get; }

        public FileNodeViewModel(FileNode node, long parentSize)
        {
            Name = node.Name;
            Size = node.Size;
            IsDirectory = node.IsDirectory;

            Percentage = parentSize == 0 ? 0 : (double)Size / parentSize * 100;

            Children = new ObservableCollection<FileNodeViewModel>(
                node.Children.Select(c => new FileNodeViewModel(c, node.Size)));
        }
    }
}
