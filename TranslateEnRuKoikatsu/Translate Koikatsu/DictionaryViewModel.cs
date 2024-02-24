using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using System.Windows.Threading;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using Microsoft.Xaml.Behaviors.Core;
using Translate_Koikatsu;

namespace TranslateEnRuKoikatsu;

public class DictionaryViewModel : ViewModelBase
{
    private IDialogCoordinator DialogCoordinator;
    private List<WordAndFileViewModel> AllTexts { get; }
    public ObservableCollection<WordAndFileViewModel> SearchedTexts { get; }

    private string _searchingText;
    public string SearchingText
    {
        get => _searchingText;
        set => SetField(ref _searchingText, value);
    }

    private string _searchingTextJa;
    public string SearchingTextJa
    {
        get => _searchingTextJa;
        set => SetField(ref _searchingTextJa, value);
    }

    private string _initDirectory;
    public string InitDirectory
    {
        get => _initDirectory;
        set => SetField(ref _initDirectory, value);
    }
    private string _openDirError;
    public string OpenDirError
    {
        get => _openDirError;
        set => SetField(ref _openDirError, value);
    }

    private bool _isFailedOpenFolder;
    public bool IsFailedOpenFolder
    {
        get => _isFailedOpenFolder;
        set => SetField(ref _isFailedOpenFolder, value);
    }


    public ICommand SearchCommand { get; set; }
    public ICommand OpenInitFolderCommand { get; set; }

    public DictionaryViewModel(IDialogCoordinator dialogCoordinator)
    {
        SearchCommand = new ActionCommand(SearchText);
        OpenInitFolderCommand = new ActionCommand(InitFolder);
        AllTexts = new List<WordAndFileViewModel>();
        SearchedTexts = new ObservableCollection<WordAndFileViewModel>();
        DialogCoordinator = dialogCoordinator;
        //Init();
    }

    private void InitFolder()
    {
        var dialog = new OpenFolderDialog();
        var success = dialog.ShowDialog();
        SearchedTexts.Clear();

        if (success.HasValue && success.Value)
        {
            ClearAll();
            var initPath = dialog.FolderName;
            if (!Directory.Exists(Path.Combine(initPath, "BepInEx")))
            {
                OpenDirError = "Директории BepInEx не существует!";
                IsFailedOpenFolder = true;
                return;
            }
            initPath = Path.Combine(initPath, "BepInEx");
            if (!Directory.Exists(Path.Combine(initPath, "Translation")))
            {
                OpenDirError = $"Директории {initPath}/Translation не существует!";
                IsFailedOpenFolder = true;
                return;
            }
            initPath = Path.Combine(initPath, "Translation");
            if (!Directory.Exists(Path.Combine(initPath, "ru")))
            {
                OpenDirError = $"Директории {initPath}/ru не существует!";
                IsFailedOpenFolder = true;
                return;
            }
            initPath = Path.Combine(initPath, "ru"); 
            if (!Directory.Exists(Path.Combine(initPath, "RedirectedResources")))
            {
                OpenDirError = $"Директории {initPath}/RedirectedResources не существует!";
                IsFailedOpenFolder = true;
                return;
            }
            initPath = Path.Combine(initPath, "RedirectedResources");
            if (!Directory.Exists(Path.Combine(initPath, "assets")))
            {
                OpenDirError = $"Директории {initPath}/assets не существует!";
                IsFailedOpenFolder = true;
                return;
            }
            initPath = Path.Combine(initPath, "assets");
            if (!Directory.Exists(Path.Combine(initPath, "abdata")))
            {
                OpenDirError = $"Директории {initPath}/abdata не существует!";
                IsFailedOpenFolder = true;
                return;
            }
            initPath = Path.Combine(initPath, "abdata");
            InitDirectory = initPath;
            OpenDirError = "";
            IsFailedOpenFolder = false;
            Task.Run(Init);
        }
    }

    private void  ClearAll()
    {
        AllTexts.Clear();
        GC.Collect(GC.MaxGeneration);
    }

    public async Task Init()
    {
        _token = new CancellationTokenSource();
        ProgressDialogController controller =
            await DialogCoordinator.ShowProgressAsync(this, "Пожалуйста подождите...", "Загрузка словаря",
                true);
        try
        {
            controller.Canceled += Controller_Canceled;
            var initPath = InitDirectory;
            var files = Directory.GetFiles(initPath, "translation.txt", SearchOption.AllDirectories);
            var count = files.Length;
            controller.Maximum = count;
            controller.Minimum = 0;
            int i = 0;

            var dispatcher = App.Current.Dispatcher;
            var tasks = new Task[count];
            foreach (var file in files.Select((value, index) => new { index, value }))
            {
                tasks[file.index] = Task.Run(() =>
                {
                    var lines = File.ReadAllLines(file.value);
                    foreach (var item in lines.Select((value, index) => new { index, value }))
                    {
                        if(_token.IsCancellationRequested) break;

                        var s = item.value.Split('=');
                        if (s.Length == 2)
                        {
                            AllTexts.Add(new WordAndFileViewModel(s[0], s[1], file.value, item.index+1));
                        }
                    }

                    dispatcher.Invoke(async () =>
                    {
                        var incremented_counter = Interlocked.Increment(ref i);
                        controller.SetMessage($"Прочитано файлов: {incremented_counter}");
                        controller.SetProgress(incremented_counter);
                    }, DispatcherPriority.Render, _token.Token);
                        
                }, _token.Token);
            }

            Task.WaitAll(tasks, _token.Token);
            var t = controller.IsOpen;
        }
        catch (TaskCanceledException)
        {
            await Task.Delay(2000);
            ClearAll();
        }
        catch (OperationCanceledException)
        {
            await Task.Delay(2000);
            ClearAll();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            await controller.CloseAsync();
            controller.Canceled -= Controller_Canceled;
        }
    }

    private CancellationTokenSource _token;

    private void Controller_Canceled(object? sender, EventArgs e)
    {
        _token.Cancel();
        _token.Dispose();
    }

    public void SearchText()
    {
        var str = SearchingText ?? "";
        App.Current.Dispatcher.Invoke(() =>
        {
            SearchedTexts.Clear();
            if(str != "")
                foreach (var item in AllTexts.Where(x => x != null && (x.Ru.Contains(str) || x.Ja.Contains(str))))
                {
                    SearchedTexts.Add(item);
                }
            else
            {
                foreach (var item in AllTexts.Where(x => x is { Ru: null or "" } || x is { Ja: null or "" }))
                {
                    SearchedTexts.Add(item);
                }
            }
        });
    }
}