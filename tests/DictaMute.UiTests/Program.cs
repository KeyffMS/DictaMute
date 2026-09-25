using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml.Linq;

namespace DictaMute.UiTests;

public enum Reaction { Duck, Mute, Pause }
public sealed record Choice(string Name, string Label);
public sealed class Row
{
    public string Name { get; set; } = "";
    public ImageSource? Icon => null;
    public double Peak { get; set; }
    public string Status => "Dane demonstracyjne — bez dostępu do mikrofonu";
    public Reaction Mode { get; set; }
    public string? MediaSessionId { get; set; }
}

internal static class Program
{
    private static int _checks;
    private static readonly HashSet<string> Events = ["Loaded", "Closed", "SourceInitialized", "Click", "SelectionChanged", "RequestNavigate"];

    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            var repository = args.Length > 0 ? Path.GetFullPath(args[0]) : Directory.GetCurrentDirectory();
            var output = Path.Combine(repository, "artifacts", "ui-preview");
            Directory.CreateDirectory(output);
            var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            Console.WriteLine("Theme assembly: " + typeof(DictaMute.App).Assembly.GetName().Name);
            // Load the complete application dictionary, including application-level overrides.
            // The production App class is not instantiated: startup must not open real devices.
            XNamespace presentation = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
            var appDocument = XDocument.Load(Path.Combine(repository, "src", "DictaMute", "App.xaml"));
            var resourceElement = appDocument.Root?.Element(presentation + "Application.Resources")?.Element(presentation + "ResourceDictionary")
                ?? throw new InvalidOperationException("Missing application resources.");
            var resources = new XElement(resourceElement);
            resources.SetAttributeValue(XNamespace.Xmlns + "x", "http://schemas.microsoft.com/winfx/2006/xaml");
            app.Resources = (ResourceDictionary)XamlReader.Parse(resources.ToString(), new ParserContext
            { BaseUri = new Uri("pack://application:,,,/DictaMute;component/") });

            // Use the real view markup and compiled theme, but deliberately exclude event
            // handlers and the MainWindow constructor: no audio, settings, tray or hotkeys.
            // Compiling the application separately validates its real code-behind handlers.
            var document = XDocument.Load(Path.Combine(repository, "src", "DictaMute", "MainWindow.xaml"));
            var root = document.Root ?? throw new InvalidOperationException("Missing Window root.");
            root.Attribute(XName.Get("Class", "http://schemas.microsoft.com/winfx/2006/xaml"))?.Remove();
            foreach (var attribute in root.DescendantsAndSelf().Attributes().Where(a => Events.Contains(a.Name.LocalName)).ToArray())
                attribute.Remove();
            var window = (Window)XamlReader.Parse(document.ToString());
            window.WindowStartupLocation = WindowStartupLocation.Manual;
            window.Left = 0;
            window.Top = 0;
            window.Show();
            // Capturing Window.Content excludes the parent window's background. Paint the
            // same brush in the fixture so screenshots are opaque, like the real window.
            ((Panel)window.Content).Background = window.Background;
            Pump(window);
            Check(((SolidColorBrush)((Panel)window.Content).Background).Color == (Color)app.FindResource("WindowBackgroundColor"), "Preview includes the real window background");

            foreach (var name in new[] { "AutomationEnabled", "ProfileBox", "ProfileName", "SourcesGrid", "TargetsGrid", "TargetModeColumn", "CaptureApps", "PlaybackApps", "ThresholdSlider", "HoldSlider", "DuckSlider", "FadeSlider", "AllExceptSources", "AnyMicrophone", "GlobalModeBox", "SourceHotkey", "TargetHotkey", "ToggleHotkey", "MediaSessions", "PeakText", "StatusText", "PeakBar", "NoticeText" })
                Check(window.FindName(name) is not null, "Existing controller contract: " + name);

            var palette = new Dictionary<string, string>
            {
                ["WindowBackgroundColor"] = "#FF14171F", ["HeaderBackgroundColor"] = "#FF181C26",
                ["SurfaceColor"] = "#FF1D222D", ["SurfaceAlternateColor"] = "#FF202632",
                ["SurfaceRaisedColor"] = "#FF242A37", ["SurfaceHoverColor"] = "#FF2D3545",
                ["BorderColor"] = "#FF363F51", ["TextPrimaryColor"] = "#FFEFF3FA",
                ["TextSecondaryColor"] = "#FFBEC8D8", ["TextMutedColor"] = "#FF97A4B8",
                ["AccentColor"] = "#FF708BFF", ["AccentHoverColor"] = "#FF829AFF",
                ["AccentPressedColor"] = "#FF5B75E5", ["AccentSoftColor"] = "#FF323E69",
                ["SuccessColor"] = "#FF4DD3A9", ["SuccessSoftColor"] = "#FF1E4C43",
                ["DangerColor"] = "#FFFF6F80", ["DangerSoftColor"] = "#FF542B36",
                ["DangerHoverColor"] = "#FF602F3B", ["DangerPressedColor"] = "#FF692F3C",
                ["DangerBorderColor"] = "#FF783A46", ["SelectionColor"] = "#FF344369"
            };
            foreach (var (key, expected) in palette)
                Check(((Color)app.FindResource(key)).ToString() == expected, "SightAdapt palette: " + key);
            Check((CornerRadius)app.FindResource("CardRadius") == new CornerRadius(12), "Card radius is 12");
            Check((CornerRadius)app.FindResource("ButtonRadius") == new CornerRadius(9), "Button radius is 9");
            Check(Find<DataGrid>(window, "SourcesGrid").RowHeight == 42, "Rows match SightAdapt height");
            Check(Find<DataGrid>(window, "TargetsGrid").ColumnHeaderHeight == 28, "Main table headers use compact height");
            Check(Find<StackPanel>(window, "SourcesEmptyState").IsVisible, "Source empty state is visible");
            Check(Find<StackPanel>(window, "TargetsEmptyState").IsVisible, "Target empty state is visible");
            Find<TextBlock>(window, "NoticeText").Text = "Podgląd interfejsu — dane demonstracyjne, bez aktywnego silnika audio.";
            Capture((FrameworkElement)window.Content, Path.Combine(output, "01-empty.png"));

            var profiles = Find<ComboBox>(window, "ProfileBox");
            profiles.ItemsSource = new[] { new Choice("Dyktowanie", "Dyktowanie"), new Choice("Spotkania", "Spotkania") };
            profiles.SelectedIndex = 0;
            Find<TextBox>(window, "ProfileName").Text = "Dyktowanie";
            var sources = new ObservableCollection<Row> { new() { Name = "Dyktowanie.exe", Peak = 42 }, new() { Name = "Rozmowa.exe", Peak = 18 } };
            var targets = new ObservableCollection<Row> { new() { Name = "Odtwarzacz.exe", Mode = Reaction.Duck }, new() { Name = "Muzyka.exe", Mode = Reaction.Pause } };
            Find<DataGrid>(window, "SourcesGrid").ItemsSource = sources;
            var targetGrid = Find<DataGrid>(window, "TargetsGrid");
            targetGrid.ItemsSource = targets;
            var modeColumn = (DataGridComboBoxColumn)window.FindName("TargetModeColumn");
            modeColumn.ItemsSource = Enum.GetValues<Reaction>();
            var global = Find<ComboBox>(window, "GlobalModeBox");
            global.ItemsSource = Enum.GetValues<Reaction>();
            global.SelectedIndex = 0;
            var captureApps = Find<ComboBox>(window, "CaptureApps");
            var playbackApps = Find<ComboBox>(window, "PlaybackApps");
            Check(
                ((SolidColorBrush)captureApps.Background).Color == (Color)app.FindResource("SurfaceRaisedColor") &&
                ((SolidColorBrush)playbackApps.Background).Color == (Color)app.FindResource("SurfaceRaisedColor"),
                "Compact app selectors keep dark background");

            foreach (var name in new[] { "CaptureApps", "PlaybackApps" })
            {
                var box = Find<ComboBox>(window, name);
                box.ItemsSource = new[] { new Choice("Aplikacja", name == "CaptureApps" ? "Dyktowanie — Mikrofon" : "Odtwarzacz") };
                box.SelectedIndex = 0;
            }
            Find<TextBox>(window, "SourceHotkey").Text = "Ctrl+Alt+X";
            Find<TextBox>(window, "TargetHotkey").Text = "Ctrl+Alt+Y";
            Find<TextBox>(window, "ToggleHotkey").Text = "Ctrl+Alt+M";
            Find<ComboBox>(window, "MediaSessions").ItemsSource = new[] { "Odtwarzacz.exe" };
            Find<ComboBox>(window, "MediaSessions").SelectedIndex = 0;
            Find<TextBlock>(window, "StatusText").Text = "Oczekiwanie na sygnał ze źródeł";
            Find<TextBlock>(window, "PeakText").Text = "42,0%";
            Find<ProgressBar>(window, "PeakBar").Value = 42;
            var toggle = Find<CheckBox>(window, "AutomationEnabled");
            toggle.IsChecked = true;
            Pump(window);
            Check(!Find<StackPanel>(window, "SourcesEmptyState").IsVisible, "Source empty state hides when populated");
            Check(!Find<StackPanel>(window, "TargetsEmptyState").IsVisible, "Target empty state hides when populated");
            var track = (Border)toggle.Template.FindName("Track", toggle);
            Check(((SolidColorBrush)track.Background).Color == (Color)app.FindResource("AccentColor"), "Switch uses family accent when enabled");
            Check(Find<TextBlock>(window, "AutomationStateText").Text == "GOTOWA", "Status is expressed in text, not only color");
            Check(Find<Button>(window, "SaveButton").MinHeight == 40, "Actions retain 40-DIP minimum height");
            Capture((FrameworkElement)window.Content, Path.Combine(output, "02-configured.png"));

            profiles.IsDropDownOpen = true;
            Pump(window);
            var popup = (Popup)profiles.Template.FindName("PART_Popup", profiles);
            Check(popup.IsOpen, "ComboBox popup opens");
            var popupBorder = (Border)profiles.Template.FindName("DropDown", profiles);
            Check(((SolidColorBrush)popupBorder.Background).Color == (Color)app.FindResource("SurfaceColor"), "Dropdown remains dark");
            Capture(popupBorder, Path.Combine(output, "03-dropdown.png"));
            profiles.SelectedIndex = 1;
            Check(profiles.SelectedItem is Choice { Name: "Spotkania" }, "ComboBox selection still works");
            profiles.IsDropDownOpen = false;
            profiles.SelectedIndex = 0;

            targetGrid.SelectedIndex = 0;
            targetGrid.CurrentCell = new DataGridCellInfo(targets[0], modeColumn);
            targetGrid.Focus();
            Check(targetGrid.BeginEdit(), "Target reaction cell enters edit mode");
            Pump(window);
            var editor = Descendants<ComboBox>(targetGrid).First(c => c.IsHitTestVisible);
            editor.SelectedItem = Reaction.Mute;
            targetGrid.CommitEdit(DataGridEditingUnit.Cell, true);
            targetGrid.CommitEdit(DataGridEditingUnit.Row, true);
            Check(targets[0].Mode == Reaction.Mute, "Target reaction edit is written back");

            foreach (var name in new[] { "ThresholdSlider", "HoldSlider", "DuckSlider", "FadeSlider" })
            {
                var slider = Find<Slider>(window, name);
                slider.ApplyTemplate();
                Check(slider.Template.FindName("PART_Track", slider) is Track, "Slider template contract: " + name);
                var previous = slider.Value;
                Slider.IncreaseSmall.Execute(null, slider);
                Check(slider.Value > previous, "Keyboard slider command: " + name);
            }
            var advanced = Find<Expander>(window, "AdvancedOptions");
            advanced.IsExpanded = true;
            Pump(window);
            var advancedHeader = (ToggleButton)advanced.Template.FindName("Header", advanced);
            Check(((SolidColorBrush)advancedHeader.Foreground).Color == (Color)app.FindResource("TextPrimaryColor"), "Advanced header keeps readable light text");
            Find<ScrollViewer>(window, "MainScroll").ScrollToEnd();
            Pump(window);
            Check(advanced.IsExpanded, "Advanced settings expand");
            Capture((FrameworkElement)window.Content, Path.Combine(output, "04-settings.png"));
            window.Width = 960;
            window.Height = 680;
            Find<ScrollViewer>(window, "MainScroll").ScrollToTop();
            Pump(window);
            Check(Find<ScrollViewer>(window, "MainScroll").ViewportHeight > 100, "Small window keeps a usable scroll viewport");
            Check(Find<ScrollViewer>(window, "MainScroll").ScrollableHeight > 0, "Settings remain reachable through scrolling");
            Check(Find<Border>(window, "ProjectInfoCard").ActualWidth > 800, "Publisher footer remains laid out");
            Capture((FrameworkElement)window.Content, Path.Combine(output, "05-small-window.png"));
            window.Close();
            app.Shutdown();
            Console.WriteLine($"{_checks} UI checks passed. PNG previews: {output}");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }

    private static T Find<T>(Window window, string name) where T : class => window.FindName(name) as T ?? throw new InvalidOperationException("Missing " + name);
    private static void Check(bool result, string name) { if (!result) throw new InvalidOperationException("FAIL " + name); _checks++; Console.WriteLine("PASS " + name); }
    private static void Pump(Window window) { window.UpdateLayout(); window.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle); window.UpdateLayout(); }
    private static IEnumerable<T> Descendants<T>(DependencyObject parent) where T : DependencyObject
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T item) yield return item;
            foreach (var nested in Descendants<T>(child)) yield return nested;
        }
    }
    private static void Capture(FrameworkElement element, string path)
    {
        var width = Math.Max(1, (int)Math.Ceiling(element.ActualWidth));
        var height = Math.Max(1, (int)Math.Ceiling(element.ActualHeight));
        var image = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        image.Render(element);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(image));
        using var stream = File.Create(path);
        encoder.Save(stream);
    }
}
