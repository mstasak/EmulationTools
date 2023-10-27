using FileViewer.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace FileViewer.Views;

public sealed partial class ViewFilePage : Page
{
    public ViewFileViewModel ViewModel
    {
        get;
    }

    public ViewFilePage()
    {
        ViewModel = App.GetService<ViewFileViewModel>();
        InitializeComponent();
    }
}
