using CommunityToolkit.WinUI.UI.Controls;

using FileViewer.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace FileViewer.Views;

public sealed partial class BookmarksPage : Page
{
    public BookmarksViewModel ViewModel
    {
        get;
    }

    public BookmarksPage()
    {
        ViewModel = App.GetService<BookmarksViewModel>();
        InitializeComponent();
    }

    private void OnViewStateChanged(object sender, ListDetailsViewState e)
    {
        if (e == ListDetailsViewState.Both)
        {
            ViewModel.EnsureItemSelected();
        }
    }
}
