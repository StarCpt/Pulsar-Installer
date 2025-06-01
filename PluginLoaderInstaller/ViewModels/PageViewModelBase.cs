using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginLoaderInstaller.ViewModels;

public abstract class PageViewModelBase(MainViewModel mainViewModel) : ObservableObject
{
    public abstract string Header { get; }
    public abstract string Description { get; }
    public abstract bool BackButtonEnabled { get; }
    public abstract bool NextButtonEnabled { get; }
    public abstract string NextButtonText { get; }

    protected MainViewModel MainViewModel { get; } = mainViewModel;
}
