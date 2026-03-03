using System;
using System.ComponentModel;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;

namespace Win_Plus_V;

public class ClipboardItem : INotifyPropertyChanged
{
    public string? Text
    {
        get; set;
    }

    public ClipboardItemType? Type
    {
        get; set;
    }

    public DateTime Timestamp
    {
        get; set;
    }

    // Stored in JSON
    public byte[]? ImageBytes
    {
        get; set;
    }

    // NOT stored in JSON
    [JsonIgnore]
    private BitmapImage? _image;
    [JsonIgnore]
    public BitmapImage? Image
    {
        get => _image;
        set
        {
            if (_image != value)
            {
                _image = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Image)));
            }
        }
    }

    public ClipboardItem()
    {
    }
    
    public ClipboardItem(string text, DateTime timestamp)
    {
        Text = text;
        Type = ClipboardItemType.Text;
        Timestamp = timestamp;
    }

    public ClipboardItem(BitmapImage image, byte[] imageBytes, DateTime timestamp)
    {
        Image = image;
        ImageBytes = imageBytes;
        Type = ClipboardItemType.Image;
        Timestamp = timestamp;
    }
    public async Task LoadImageAsync()
    {
        if (ImageBytes == null)
            return;

        var stream = new InMemoryRandomAccessStream();
        await stream.WriteAsync(ImageBytes.AsBuffer());
        stream.Seek(0);

        var bitmap = new BitmapImage();
        await bitmap.SetSourceAsync(stream);

        Image = bitmap; // setter will raise PropertyChanged
    }

    [JsonIgnore]
    public Visibility TextVisible =>Type == ClipboardItemType.Text ? Visibility.Visible : Visibility.Collapsed;

    [JsonIgnore]
    public Visibility ImageVisible => Type == ClipboardItemType.Image ? Visibility.Visible : Visibility.Collapsed;

    public event PropertyChangedEventHandler? PropertyChanged;
}

public enum ClipboardItemType
{
    Text,
    Image
}