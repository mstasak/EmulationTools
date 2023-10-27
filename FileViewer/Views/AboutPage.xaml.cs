﻿using FileViewer.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace FileViewer.Views;

public sealed partial class AboutPage : Page
{
    public AboutViewModel ViewModel
    {
        get;
    }

    public AboutPage()
    {
        ViewModel = App.GetService<AboutViewModel>();
        InitializeComponent();
    }
}
