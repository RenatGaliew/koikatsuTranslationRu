using System.IO;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors.Core;

namespace TranslateEnRuKoikatsu;

public class WordAndFileViewModel : ViewModelBase
{
    private string _ruOld;
    private string _ja;
    private string _ru;
    private string _fileName;
    private bool _isEdited;

    public string Ja
    {
        get => _ja;
        set => SetField(ref _ja, value);
    }
    public string Ru
    {
        get => _ru;
        set
        {
            IsEdited = value != _ruOld;
            SetField(ref _ru, value);
        }
    }

    public string FileName
    {
        get => _fileName;
        set => SetField(ref _fileName, value);
    }

    public bool IsEdited
    {
        get => _isEdited;
        set => SetField(ref _isEdited, value);
    }

    public int Line { get; set; }

    public ICommand SaveCommand { get; set; }

    public WordAndFileViewModel(string ja, string ru, string fileName, int line)
    {
        Line = line;
        FileName = fileName;
        Ru = ru;
        _ruOld = ru;
        Ja = ja;
        IsEdited = false;
        SaveCommand = new ActionCommand(Save);
    }

    private void Save()
    {
        string[] arrLine = File.ReadAllLines(FileName);
        arrLine[Line-1] = $"{Ja}={Ru}";
        _ruOld = Ru;
        File.WriteAllLines(FileName, arrLine);
    }
}