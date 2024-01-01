﻿using JumpKingModifiersMod.Settings;
using JumpKingModifiersMod.Settings.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace JumpKingRavensMod.Install.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly InstallerViewModel installerViewModel;

        public MainWindow()
        {
            InitializeComponent();

            ToolTipService.InitialShowDelayProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(2000));
            ToolTipService.BetweenShowDelayProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(0));
            ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(10000));

            // Create the Modifier Tab Stack Panel programmatically
            ScrollViewer scrollViewer = new ScrollViewer();
            scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            scrollViewer.SetValue(Grid.RowProperty, 2);

            StackPanel modifiersStack = new StackPanel();

            scrollViewer.Content = modifiersStack;
            tabItemModifiersGrid.Children.Add(scrollViewer);

            // Create the data context and pass it the modifiers stack
            installerViewModel = new InstallerViewModel(modifiersStack);
            this.DataContext = installerViewModel;
            Closing += installerViewModel.OnWindowClosing;           
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void Quit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.ShowDialog();
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (tabControl != null)
            {
                if (tabControl.SelectedItem == tabItemYouTube)
                {
                    tabControl.SelectedItem = tabItemTwitch;
                }
            }
        }

        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            if (tabControl != null)
            {
                if (tabControl.SelectedItem == tabItemTwitch)
                {
                    tabControl.SelectedItem = tabItemYouTube;
                }

                //if (tabControl.SelectedItem == tabItemChatDisplay)
                //{
                //    tabControl.SelectedItem = tabItemYouTube;
                //}
            }
        }

        private void ToggleButtonFeedbackDevice_Checked(object sender, RoutedEventArgs e)
        {
            if (tabControl != null)
            {
                if (tabControl.SelectedItem == tabItemPiShock || tabControl.SelectedItem == tabItemPunishment)
                {
                    tabControl.SelectedItem = tabItemRavens;
                }
            }
        }
    }
}
