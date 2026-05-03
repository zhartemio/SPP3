using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Input;
using System.Windows.Forms;
using FileAnalyzer.Core;


namespace FileAnalyzer.WPF
{
    
    public class MainViewModel : BaseViewModel
    {
        public ObservableCollection<FileNodeViewModel> Nodes { get; set; } = new();

        public ICommand ScanCommand { get; }
        public ICommand CancelCommand { get; }

        private CancellationTokenSource _cts;

        public MainViewModel()
        {
            ScanCommand = new RelayCommand(Scan);
            CancelCommand = new RelayCommand(Cancel);
        }

        private async void Scan()
        {
            var dialog = new FolderBrowserDialog();

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            _cts = new CancellationTokenSource();

            var scanner = new DirectoryScanner(4, _cts.Token);

            var result = await scanner.ScanAsync(dialog.SelectedPath);

            Nodes.Clear();
            Nodes.Add(new FileNodeViewModel(result, result.Size));
        }

        private void Cancel()
        {
            _cts?.Cancel();
        }
    }
}
