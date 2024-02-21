using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Ink;
using JotWin.Model;
using Microsoft.Win32;
using System.IO;
using JotWin.ViewModel.Helpers;
using System.Windows.Threading;
using JotWin.ViewModel.Helpers.UndoManager;
using JotWin.ViewModel;
using JotWin.ViewModel.Tabs;
using System.Diagnostics;
using System.Windows.Media.Animation;
using JotWin.ViewModel.Helpers.MajicJot;
using JotWin.ViewModel.Helpers.SaveLoad;
using Newtonsoft.Json;
using System.Net.Http;
using System.Security.Cryptography;
using JWT.Builder;
using JWT.Algorithms;
using Newtonsoft.Json.Linq;
using Gma.System.MouseKeyHook;
using System.Runtime.InteropServices;

namespace JotWin.View
{
    public enum CanvasTemplate
    {
        Tracing, Blank, Ruled, Grid, Dot
    }

    public partial class MainAppWindow : Window
    {
        // SET FALSE FOR RELEASE
        readonly bool SKIP_LICENSE = false;

        public MainTabsVM mainTabsVM;
        public TabVM selectedTab;

        public readonly MouseHook mouseHook = new();

        readonly SystemFileControl system_control = new();

        public DrawingSet drawingSetting;
        private Point drawStartPoint;

        public PenMenu pen_menu;
        public saveTab? save_win;
        private LicenseWindow? licenseWindow;
        public DropJot? dropJot_win;

        public mouseKeyboardEventsHelpers mouseKeyboardHook;

        private readonly DispatcherTimer copyTimer;
        private readonly DispatcherTimer winResizeTimer;

        private TextBox? focusedTextBox;
        private bool editingTextBox = false;
        //private bool textBoxFocused = false;

        private readonly SolidColorBrush primary30 = new(new Color { A = 255, R = 219, G = 212, B = 252 });
        private readonly SolidColorBrush primary40 = new(new Color { A = 255, R = 143, G = 115, B = 247 });

        public string? userEmail;

        private bool erasing = false;
        //private int eraseCount = 1;

        private bool penBarrelBtnClick = false;

        private CanvasTemplate _canvasTemplate;
        public CanvasTemplate canvasTemplate
        {
            get => _canvasTemplate;
            set
            {
                _canvasTemplate = value;
                DrawingCanvasBorder.Background.Opacity = value == CanvasTemplate.Tracing ? 0 : 1;

                templateButton?.ClearValue(BackgroundProperty);
                templateButton?.ClearValue(Border.BorderBrushProperty);

                templateButton = value switch
                {
                    CanvasTemplate.Tracing => TemplateButton_Tracing,
                    CanvasTemplate.Blank => TemplateButton_Blank,
                    CanvasTemplate.Ruled => TemplateButton_Ruled,
                    CanvasTemplate.Grid => TemplateButton_Grid,
                    CanvasTemplate.Dot => TemplateButton_Dot,
                    _ => TemplateButton_Blank,
                };

                templateButton.Background = primary30;
                templateButton.BorderBrush = primary40;

                DrawCanvasTemplate();
            }
        }

        Button templateButton;

        public TabHelpers tabManager;
        public List<TabData> tabDataList = new();

        private readonly DoubleAnimation templatePanelAnimation;
        private readonly Storyboard templatePanelStoryboard;
        private bool templatePanelOpen = true;

        public static readonly RoutedCommand UndoCommand = new();
        public static readonly RoutedCommand RedoCommand = new();
        public static readonly RoutedCommand SaveCommand = new();
        public static readonly RoutedCommand OpenCommand = new();

        private static readonly HttpClient httpClient = new();

        public MainAppWindow()
        {
            Hide();
            InitializeComponent();

            mouseKeyboardHook = new mouseKeyboardEventsHelpers(this);
            mouseKeyboardHook.Subscribe(Hook.GlobalEvents());

            pen_menu = new PenMenu(this);
            dropJot_win = new DropJot(this);
            mainTabsVM = (MainTabsVM)FindResource("mainTabVM");
            selectedTab = mainTabsVM.Tabs[0];

            DrawingCanvas.StrokeCollected += InkCanvas_StrokeCollected;
            DrawingCanvas.SelectionChanged += InkCanvas_SelectionChanged;
            DrawingCanvas.SelectionMoved += InkCanvas_SelectionMoved;
            DrawingCanvas.SelectionResized += InkCanvas_SelectionResized;
            DrawingCanvas.StylusButtonDown += InkCanvas_StylusButtonDown;
            DrawingCanvas.StylusButtonUp += InkCanvas_StylusButtonUp;
            DrawingCanvas.PreviewStylusInAirMove += inkCanvas_PreviewStylusInAirMove;


            tabManager = new TabHelpers(this);

            drawingSetting = new DrawingSet();

            WindowsLowLevelHelpers.InitializeWindowHooks(this);
            mouseHook.InitializeMouseHooks(this);

            winResizeTimer = new DispatcherTimer();
            winResizeTimer.Interval = TimeSpan.FromMilliseconds(50);
            winResizeTimer.Tick += winResizeTimerTimer_Tick;

            copyTimer = new DispatcherTimer();
            copyTimer.Interval = TimeSpan.FromSeconds(3);
            copyTimer.Tick += copyTimer_Tick;

            templatePanelAnimation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = new Duration(TimeSpan.FromSeconds(0.25)),
            };

            templatePanelStoryboard = new Storyboard();
            templatePanelStoryboard.Children.Add(templatePanelAnimation);

            UndoCommand.InputGestures.Add(new KeyGesture(Key.Z, ModifierKeys.Control));
            RedoCommand.InputGestures.Add(new KeyGesture(Key.Z, ModifierKeys.Control | ModifierKeys.Shift));
            SaveCommand.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));
            OpenCommand.InputGestures.Add(new KeyGesture(Key.O, ModifierKeys.Control));

            templateButton = TemplateButton_Blank;

            if (!SKIP_LICENSE)
            {
                Hide();
                CheckLicense();
            }
            else
            {
                Show();
            }
        }



        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ExtenalMonitorInfo.resizeAppWindow(this);

            //dropJot_win.Show();

            canvasTemplate = CanvasTemplate.Blank;
            tabDataList[mainTabsVM.SelectedTab].canvasBackground = canvasTemplate;

            Storyboard.SetTargetName(templatePanelAnimation, TemplatePanelBorder.Name);
            Storyboard.SetTargetProperty(templatePanelAnimation, new PropertyPath(OpacityProperty));

            JumpListHelpers.InitializeJumpList();
        }

        public void triggerAdjustWinSize()
        {
            winResizeTimer.Start();
        }

        public static async Task<TResult?> PostObjectToWebsiteAsync<TResult>(Uri site, object objToPost)
        {
            using var req = new HttpRequestMessage(HttpMethod.Post, site);

            req.Content = new StringContent(
                JsonConvert.SerializeObject(objToPost),
                Encoding.UTF8,
                "application/json"
            );

            try
            {
                using var resp = await httpClient.SendAsync(req);

                if (resp.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Debug.WriteLine("bad status code", resp.StatusCode);
                    return default;
                }

                using var s = await resp.Content.ReadAsStreamAsync();
                using var sr = new StreamReader(s);
                using var jtr = new JsonTextReader(sr);

                return new JsonSerializer().Deserialize<TResult>(jtr);
            }
            catch (HttpRequestException)
            {
                string messageBoxText = "Jot requires an internet connection";
                string caption = "Network error";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Warning;
                MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);

                Application.Current.Shutdown();
                return default;
            }
        }

        private static RSA RsaFromFile(string filename)
        {
            RSA rsa = RSA.Create();
            using StreamReader sr = new(filename);
            string filestring = sr.ReadToEnd();
            rsa.ImportFromPem(filestring);
            return rsa;
        }

        private static RSA RsaFromString(string s)
        {
            RSA rsa = RSA.Create();
            rsa.ImportFromPem(s);
            return rsa;
        }

        public static async void Post(string endpoint, string useremail, Action<dynamic?> finished)
        {
            //RSA jotPrivate    = RsaFromFile("..\\..\\..\\..\\Resources\\private-key.pem");
            //RSA jotPublic     = RsaFromFile("..\\..\\..\\..\\Resources\\public-key.pem");
            //RSA backendPublic = RsaFromFile("..\\..\\..\\..\\Resources\\backend-public-key.pem");

            RSA jotPrivate = RsaFromString(Keys.backend_private_key);
            RSA jotPublic = RsaFromString(Keys.public_key);
            RSA backendPublic = RsaFromString(Keys.backend_public_key);

            var jwt = JwtBuilder.Create()
                  .WithAlgorithm(new RS256Algorithm(jotPublic, jotPrivate))
                  .AddClaim("exp", DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds())
                  .AddClaim("email", useremail)
                  .Encode();

            var parameters = new
            {
                token = jwt,
                email = useremail
            };

            var postResonse = await PostObjectToWebsiteAsync<object>(
                new Uri("https://my-api.espres.so/api/v1/" + endpoint), parameters);

            if (postResonse == null)
            {
                finished(null);
                return;
            }

            var json = JwtBuilder.Create()
                        .WithAlgorithm(new RS256Algorithm(backendPublic))
                        .MustVerifySignature()
                        .Decode((string?)((JObject)postResonse)["token"]);

            finished(JsonConvert.DeserializeObject(json));
        }

        private void HandleJotCheck(dynamic? response)
        {
            if (response == null)
            {
                Debug.WriteLine("jot-check -> null repsonse");
                return;
            }

            bool active = response.active ?? false;
            bool trial = response.trial ?? false;
            bool expired = response.expired ?? true;
            bool licence = response.licence ?? false;
            DateTime expiry = response.expiry ?? DateTime.Now.AddDays(-1);

            if (!expired && active)
            {
                if (!licence)
                {
                    var daysRemaining = expiry.Subtract(DateTime.Now).Days;
                    TrialRemainingText.Text = $"{daysRemaining} days left in trial";
                }

                Show();
                SetOpenOnStartup(true);
                return;
            }

            licenseWindow = new(this, active);
            licenseWindow.ShowDialog();
        }

        private static void SetOpenOnStartup(bool shouldOpen)
        {
            var path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
            RegistryKey? key = Registry.CurrentUser.OpenSubKey(path, true);
            if (key == null)
            {
                return;
            }

            Debug.WriteLine("setting open-on-startup " + shouldOpen);
            if (shouldOpen)
            {
                string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location.Replace(".dll", ".exe");
                key.SetValue("Jot", exePath);
            }
            else
            {
                key.DeleteValue("Jot", false);
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var tab = e.AddedItems[0];
                if (tab != null)
                {
                    selectedTab = (TabVM)tab;
                }
            }

            if (selectedTab != null && mainTabsVM != null)
            {
                mainTabsVM.updateTabSelected(selectedTab);
                tabManager.loadCanvasContent(selectedTab);
            }
        }

        private void TabControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TabControl tabControl = (TabControl)sender;

            var clickedTabItem = GenericHelpers.FindVisualParent<TabItem>((DependencyObject)e.OriginalSource);

            if (clickedTabItem != null)
            {
                if (clickedTabItem.DataContext is TabVM tabVM)
                {
                    if (mainTabsVM != null)
                    {
                        if (tabVM.isTabSelected)
                        {
                            TextBox textBox = GenericHelpers.FindVisualChild<TextBox>(clickedTabItem);
                            if (textBox != null)
                            {
                                tabVM.IsNotEditing = false;
                                tabVM.HeaderColorBorder = mainTabsVM.editingHeaderBorder;
                                tabVM.HeaderBorderThickness = 1;
                                textBox.Dispatcher.BeginInvoke(() =>
                                {
                                    textBox.Focus();
                                    textBox.SelectAll();
                                });

                                e.Handled = false;
                            }
                        }
                        else
                        {
                            mainTabsVM.selectTabByObject(tabVM);
                        }
                    }
                }
            }
        }


        private void TabTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                mainTabsVM.tabEditReset();
                DrawingCanvas.Focus();
            }
        }

        private void TabTextBox_GotFocus(object sender, RoutedEventArgs e)
        {

        }

        private void TabTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            mainTabsVM.tabEditReset();
        }

        private void TabTextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            TabItem tabItem = GenericHelpers.FindVisualParent<TabItem>((DependencyObject)sender);

            if (tabItem != null && tabItem.DataContext is TabVM tabVM)
            {

            }
        }

        private void CloseTabButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TabItem tabItem = GenericHelpers.FindVisualParent<TabItem>((DependencyObject)sender);

            if (tabItem != null && tabItem.DataContext is TabVM tabVM)
            {
                mainTabsVM.CloseTab(tabVM);
            }
        }

        private void InkCanvas_SelectionChanged(object? sender, EventArgs e)
        {

        }

        private async void InkCanvas_SelectionResized(object? sender, EventArgs e)
        {
            await Task.Delay(300);
            tabDataList[mainTabsVM.SelectedTab].tabUndoManager.SaveState(DrawingCanvas);
        }

        private async void InkCanvas_SelectionMoved(object? sender, EventArgs e)
        {
            await Task.Delay(300);
            tabDataList[mainTabsVM.SelectedTab].tabUndoManager.SaveState(DrawingCanvas);
        }

        private void InkCanvas_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            UIStroke uiStroke = new(e.Stroke);
            DrawingCanvas.Strokes.Remove(e.Stroke);

            Panel.SetZIndex(uiStroke, tabDataList[mainTabsVM.SelectedTab].zIndexCount);
            DrawingCanvas.Children.Add(uiStroke);
            tabDataList[mainTabsVM.SelectedTab].zIndexCount++;
            
            tabDataList[mainTabsVM.SelectedTab].tabUndoManager.SaveState(DrawingCanvas);
        }

        private void winResizeTimerTimer_Tick(object? sender, EventArgs e)
        {
            //WindowsLowLevelHelpers.AdjustWindowSize(this);
            ExtenalMonitorInfo.resizeAppWindow(this);
            DrawCanvasTemplate();
            winResizeTimer.Stop();
        }

        private void copyTimer_Tick(object? sender, EventArgs e)
        {
            copyText.Text = "Copy";

            canvasCopy.IsEnabled = true;

            copyTimer.Stop();
        }

        private void Store_Click(object sender, RoutedEventArgs e)
        {

            WindowState = WindowState.Minimized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        public void activateSaveTabWin()
        {
            if (save_win == null)
            {
                save_win = new saveTab(this);
                save_win.Owner = this;
                save_win.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                save_win.Closed += (s, args) =>
                {
                    save_win = null;
                };

                save_win.Loaded += (s, args) =>
                {
                    save_win.Visibility = Visibility.Visible;
                };
                save_win.Show();
            }
        }

        public void initiate_dropJot_win()
        {
            dropJot_win.Closed += (s, args) =>
            {
                IsEnabled = true;
            };

            dropJot_win.Loaded += (s, args) =>
            {
                dropJot_win.Visibility = Visibility.Visible;
            };

            StateChanged += (s, args) =>
            {
                if (WindowState == WindowState.Minimized) { }
                else { }
            };

            dropJot_win.Hide();
        }

        public void initiate_pen_menu()
        {
            pen_menu.Closed += (s, args) =>
            {
                IsEnabled = true;
            };

            pen_menu.Loaded += (s, args) =>
            {
                pen_menu.Visibility = Visibility.Visible;
            };

            StateChanged += (s, args) =>
            {
                if (WindowState == WindowState.Minimized) { }
                else { }
            };

            pen_menu.Hide();
        }

        private void closeTab_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        private void closeTab_MouseLeave(object sender, MouseEventArgs e)
        {

        }

        private void saveCanvas_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Button button)
            {
                if (button.Content is Border border)
                {
                    border.Background = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#B184E1")
                    );
                }
            }
        }

        private void saveCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Button button)
            {
                if (button.Content is Border border)
                {
                    border.Background = Brushes.Transparent;
                }
            }
        }

        private void toolBtn_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                double width = element.Width;
                double height = element.Height;

                element.Width = width + 5;
                element.Height = height + 5;
            }
        }

        private void toolBtn_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                double width = element.Width;
                double height = element.Height;

                element.Width = width - 5;
                element.Height = height - 5;
            }
        }

        private void DrawTools_Click(object sender, RoutedEventArgs e)
        {
            var radiobutton = (RadioButton)sender;
            string radioBPressed = radiobutton.Name;

            if (radioBPressed == "Draw")
            {
                SetActiveTool("draw");
                updateDrawingSettings();
                DrawingCanvas.EditingMode = InkCanvasEditingMode.Ink;
                hideAllToolMenus();
                draw_menu.Visibility = Visibility.Visible;
            }
            else if (radioBPressed == "Erase")
            {
                SetActiveTool("eraser");
                updateDrawingSettings();
                DrawingCanvas.EditingMode = InkCanvasEditingMode.None;
                hideAllToolMenus();
            }
            else if (radioBPressed == "Select")
            {
                SetActiveTool("select");
                DrawingCanvas.EditingMode = InkCanvasEditingMode.Select;
                hideAllToolMenus();
            }
            else if (radioBPressed == "Highlight")
            {
                SetActiveTool("highlighter");
                updateDrawingSettings();
                DrawingCanvas.EditingMode = InkCanvasEditingMode.Ink;
                hideAllToolMenus();
                highlighter_menu.Visibility = Visibility.Visible;
            }
            else if (radioBPressed == "Text")
            {
                SetActiveTool("text");
                updateDrawingSettings();
                DrawingCanvas.EditingMode = InkCanvasEditingMode.None;
                hideAllToolMenus();
                text_menu.Visibility = Visibility.Visible;
            }
            else if (radioBPressed == "Shapes")
            {
                SetActiveTool("shapes");
                updateDrawingSettings();
                DrawingCanvas.EditingMode = InkCanvasEditingMode.None;
                hideAllToolMenus();
                shapes_menu.UpdateSelectedShapeButton();
                shapes_menu.Visibility = Visibility.Visible;
            }
        }

        private void hideAllToolMenus()
        {
            draw_menu.Visibility = Visibility.Collapsed;
            highlighter_menu.Visibility = Visibility.Collapsed;
            color_menu.Visibility = Visibility.Collapsed;
            shapes_menu.Visibility = Visibility.Collapsed;
            text_menu.Visibility = Visibility.Collapsed;
            CloseTemplatePanel();
        }

        private void draw_menu_LostFocus(object sender, RoutedEventArgs e)
        {
            hideAllToolMenus();
        }

        private void canvas_GotFocus(object sender, RoutedEventArgs e)
        {
            hideAllToolMenus();
        }

        public void updateDrawingSettings()
        {
            Color highlightColor = Color.FromArgb(
                128,
                drawingSetting.SelectedColor.R,
                drawingSetting.SelectedColor.G,
                drawingSetting.SelectedColor.B
            );
            var inkDrawingAttributes = new DrawingAttributes
            {
                Color = drawingSetting.IsHighlighter
                    ? highlightColor
                    : drawingSetting.SelectedColor,
                Width = drawingSetting.IsPen
                    ? drawingSetting.PenSize
                    : drawingSetting.IsHighlighter ? drawingSetting.HighlighterSize : 2,
                Height = drawingSetting.IsPen
                    ? drawingSetting.PenSize
                    : drawingSetting.IsHighlighter ? drawingSetting.HighlighterSize : 2,
                StylusTip = drawingSetting.IsHighlighter ? StylusTip.Rectangle : StylusTip.Ellipse,
                FitToCurve = true,
                IgnorePressure = false,
            };

            DrawingCanvas.DefaultDrawingAttributes = inkDrawingAttributes;
        }

        public void SetActiveTool(string tool)
        {
            drawingSetting.IsPen = false;
            drawingSetting.IsHighlighter = false;
            drawingSetting.IsEraser = false;
            drawingSetting.IsSelect = false;
            drawingSetting.IsShapes = false;
            drawingSetting.IsText = false;
            drawingSetting.IsColorSwitch = false;

            switch (tool.ToLower())
            {
                case "select":
                    drawingSetting.IsSelect = true;
                    break;
                case "draw":
                    drawingSetting.IsPen = true;
                    break;
                case "highlighter":
                    drawingSetting.IsHighlighter = true;
                    break;
                case "eraser":
                    drawingSetting.IsEraser = true;
                    break;
                case "text":
                    drawingSetting.IsText = true;
                    break;
                case "shapes":
                    drawingSetting.IsShapes = true;
                    break;
                case "colorswitch":
                    drawingSetting.IsColorSwitch = true;
                    break;
                default:
                    throw new ArgumentException("Invalid tool specified", nameof(tool));
            }
        }

        public void canvasLogoAction(bool show_logo)
        {
            if (show_logo)
            {
                logo_1.Visibility = Visibility.Visible;
                canvasCopy.IsEnabled = false;
            }
            else
            {
                logo_1.Visibility = Visibility.Collapsed;
                if (!copyTimer.IsEnabled)
                {
                    canvasCopy.IsEnabled = true;
                }

                tabDataList[mainTabsVM.SelectedTab].hasSketch = true;

                UndoAPI.SetUndoButtonActive(this, true);
            }
        }

        private void InkCanvas_StylusButtonDown(object sender, StylusButtonEventArgs e)
        {

        }

        private void InkCanvas_StylusButtonUp(object sender, StylusButtonEventArgs e)
        {
            erasing = false;
            if (penBarrelBtnClick)
            {
                penBarrelBtnClick = false;
                if (drawingSetting.IsSelect)
                {
                    DrawingCanvas.EditingMode = InkCanvasEditingMode.Select;
                }
                else if (drawingSetting.IsText || drawingSetting.IsShapes || drawingSetting.IsEraser || drawingSetting.IsColorSwitch)
                {
                    DrawingCanvas.EditingMode = InkCanvasEditingMode.None;
                }
                else
                {
                    DrawingCanvas.EditingMode = InkCanvasEditingMode.Ink;
                }
            }

        }

        private void inkCanvas_PreviewStylusInAirMove(object sender, StylusEventArgs e)
        {

        }

        private void InkCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            HandleDrawingShapeStart(e.GetPosition(DrawingCanvas));
            if (drawingSetting.IsEraser)
            {
                erasing = true;
            }
        }

        private void InkCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            HandleDrawingShapeEnd();
            erasing = false;
        }

        private void InkCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            HandleDrawingShapeMove(e.GetPosition(DrawingCanvas));

            if (erasing)
            {
                EraseAt(e.GetPosition(DrawingCanvas));
            }
        }

        public void ColorSelected(Color color)
        {
            tabDataList[mainTabsVM.SelectedTab].tabUndoManager.SaveState(DrawingCanvas);

            foreach (Stroke stroke in DrawingCanvas.GetSelectedStrokes())
            {
                stroke.DrawingAttributes.Color = color;
            }

            SolidColorBrush brush = new(color);
            foreach (UIElement element in DrawingCanvas.GetSelectedElements())
            {
                if (element is Shape shape)
                {
                    shape.Stroke = brush;
                    if (((SolidColorBrush)shape.Fill).Color != Colors.Transparent)
                    {
                        shape.Fill = brush;
                    }
                }
                else if (element is TextBox textBox)
                {
                    textBox.Foreground = brush;
                }
            }
        }

        public void FontSizeSelected()
        {
            foreach (var e in DrawingCanvas.GetSelectedElements())
            {
                if (e is TextBox textBox)
                {
                    textBox.FontSize = drawingSetting.TextSize;
                }
            }
        }

        public void FontSelected()
        {
            foreach (var e in DrawingCanvas.GetSelectedElements())
            {
                if (e is TextBox textBox)
                {
                    textBox.FontFamily = new FontFamily(drawingSetting.selectedFont);
                }
            }
        }

        private void EraseAt(Point point)
        {
            List<UIElement> elementsToRemove = new();
            List<Stroke> strokesToRemove = new();

            foreach (FrameworkElement element in DrawingCanvas.Children)
            {
                Debug.WriteLine(element);
                var left = InkCanvas.GetLeft(element);
                var top = InkCanvas.GetTop(element);
                var width = element.Width;
                var height = element.Height;

                if (point.X > left &&
                    point.X < left + width &&
                    point.Y > top &&
                    point.Y < top + height)
                {
                    elementsToRemove.Add(element);
                }
            }

            foreach (Stroke stroke in DrawingCanvas.Strokes)
            {
                if (stroke.HitTest(point, 64))
                {
                    strokesToRemove.Add(stroke);
                }
            }

            if (elementsToRemove.Count == 0 && strokesToRemove.Count == 0)
            {
                return;
            }

            tabDataList[mainTabsVM.SelectedTab].tabUndoManager.SaveState(DrawingCanvas);

            foreach (UIElement element in elementsToRemove)
            {
                DrawingCanvas.Children.Remove(element);
            }

            foreach (Stroke stroke in strokesToRemove)
            {
                DrawingCanvas.Strokes.Remove(stroke);
            }
        }

        private void InkCanvas_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            canvasLogoAction(false);

            if (drawingSetting.IsText && e.ClickCount == 1)
            {
                if (focusedTextBox != null)
                {
                    focusedTextBox.Focusable = false;
                    focusedTextBox.Focus();
                    focusedTextBox.Focusable = true;
                    focusedTextBox = null;
                }
                else
                {
                    Point mousePosition = e.GetPosition(DrawingCanvas);
                    if (!IsMouseOverExistingTextBox(mousePosition))
                    {
                        insertTextBox(mousePosition);
                    }
                }
            }
        }

        private bool IsMouseOverExistingTextBox(Point mousePosition)
        {
            foreach (var child in DrawingCanvas.Children)
            {
                if (child is TextBox textBox)
                {
                    Point textBoxPosition = textBox.TranslatePoint(new Point(0, 0), DrawingCanvas);

                    Rect textBoxBounds = new(
                        textBoxPosition,
                        new Size(textBox.ActualWidth, textBox.ActualHeight)
                    );

                    if (textBoxBounds.Contains(mousePosition))
                    {
                        focusedTextBox = textBox;
                        return true;
                    }
                }
            }

            return false;
        }

        private async void insertTextBox(Point mousePosition)
        {
            RemoveEmptyTextBoxes();

            TextBox textBox = new()
            {
                Name = "TextBox_1",
                Width = 300,
                Height = 80,
                Background = Brushes.Transparent,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = false,
                FontFamily = new FontFamily(drawingSetting.selectedFont),
                FontSize = drawingSetting.TextSize,
                FontWeight = FontWeights.Regular,
                Foreground = new SolidColorBrush(drawingSetting.SelectedColor),
                IsReadOnly = false,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
                MinWidth = 50,
                MinHeight = 30,
            };

            InkCanvas.SetLeft(textBox, mousePosition.X);
            InkCanvas.SetTop(textBox, mousePosition.Y);

            Panel.SetZIndex(textBox, tabDataList[mainTabsVM.SelectedTab].zIndexCount);

            RotateTransform rotateTransform = new(0);
            textBox.RenderTransform = rotateTransform;

            textBox.GotFocus += TextBox_GotFocus;
            textBox.LostFocus += TextBox_LostFocus;
            textBox.MouseEnter += TextBox_MouseEnter;
            textBox.MouseLeave += TextBox_MouseLeave;
            textBox.TextChanged += TextBox_TextChanged;

            DrawingCanvas.Children.Add(textBox);
            tabDataList[mainTabsVM.SelectedTab].zIndexCount++;

            await Task.Delay(100);
            textBox.Focus();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            //textBoxFocused = true;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            editingTextBox = true;
        }

        private void RemoveEmptyTextBoxes()
        {
            for (int i = DrawingCanvas.Children.Count - 1; i >= 0; i--)
            {
                if (
                    DrawingCanvas.Children[i] is TextBox textBox
                    && string.IsNullOrWhiteSpace(textBox.Text)
                )
                {
                    DrawingCanvas.Children.RemoveAt(i);
                }
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                textBox.BorderBrush = Brushes.Transparent;
            }
            if (editingTextBox)
            {
                // historyControl.SaveState(DrawingCanvas);
                tabDataList[mainTabsVM.SelectedTab].tabUndoManager.SaveState(DrawingCanvas);
                editingTextBox = false;
            }
            //textBoxFocused = false;
        }

        private void TextBox_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                textBox.BorderBrush = Brushes.Blue;
            }
        }

        private void TextBox_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is TextBox textBox && !textBox.IsFocused)
            {
                textBox.BorderBrush = Brushes.Transparent;
            }
        }

        private void HandleDrawingShapeStart(Point startPoint)
        {
            if (DrawingCanvas.EditingMode == InkCanvasEditingMode.Select ||
                !drawingSetting.IsShapes)
            {
                return;
            }

            drawingSetting.isDrawing = true;
            drawStartPoint = startPoint;

            Brush brush = new SolidColorBrush(drawingSetting.SelectedColor);

            switch (drawingSetting.selectedShape)
            {
                case DrawingShape.Rectangle:
                    drawingSetting.shape = new Rectangle()
                    {
                        Stroke = brush,
                        StrokeThickness = 2,
                        Fill = drawingSetting.isShapeFill ? brush : Brushes.Transparent
                    };
                    break;

                case DrawingShape.Ellipse:
                    drawingSetting.shape = new Ellipse()
                    {
                        Stroke = brush,
                        StrokeThickness = 2,
                        Fill = drawingSetting.isShapeFill ? brush : Brushes.Transparent
                    };
                    break;
            }

            Panel.SetZIndex(drawingSetting.shape, tabDataList[mainTabsVM.SelectedTab].zIndexCount);
            DrawingCanvas.Children.Add(drawingSetting.shape);
            tabDataList[mainTabsVM.SelectedTab].zIndexCount++;
        }

        private void HandleDrawingShapeEnd()
        {
            if (drawingSetting.isDrawing)
            {
                // historyControl.SaveState(DrawingCanvas);
                tabDataList[mainTabsVM.SelectedTab].tabUndoManager.SaveState(DrawingCanvas);

                drawingSetting.isDrawing = false;
            }
        }

        private void HandleDrawingShapeMove(Point currentPosition)
        {
            if (!drawingSetting.isDrawing || drawingSetting.shape == null)
            {
                return;
            }

            var x = Math.Min(drawStartPoint.X, currentPosition.X);
            var y = Math.Min(drawStartPoint.Y, currentPosition.Y);

            var w = Math.Abs(currentPosition.X - drawStartPoint.X);
            var h = Math.Abs(currentPosition.Y - drawStartPoint.Y);

            drawingSetting.shape.Width = w;
            drawingSetting.shape.Height = h;

            InkCanvas.SetLeft(drawingSetting.shape, x);
            InkCanvas.SetTop(drawingSetting.shape, y);
        }

        private void canvasCopy_Click(object sender, RoutedEventArgs e)
        {
            canvasCopy.IsEnabled = false;

            copyText.Text = "Copied";

            SystemFileControl.CopyCanvasToClipboard(DrawingCanvas);

            copyTimer.Start();
        }

        private void canvasSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new()
            {
                Filter =
                "PNG Files (*.png)|*.png|JPEG Files (*.jpg)|*.jpg|All files (*.*)|*.*"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                SystemFileControl.SaveCanvasToFile(saveFileDialog.FileName, DrawingCanvas);
            }
        }

        private void canvasDone_Click(object sender, RoutedEventArgs e)
        {
            MajicHelpers.finishMajicJot();
            tabDataList[mainTabsVM.SelectedTab].isForMajicJot = false;
            doneCopyBtnControl("copy");
        }

        public void doneCopyBtnControl(string option = "copy")
        {
            if(option == "done")
            {
                canvasCopy.Visibility = Visibility.Collapsed;
                canvasDone.Visibility = Visibility.Visible;
            }
            else
            {
                canvasDone.Visibility = Visibility.Collapsed;
                canvasCopy.Visibility = Visibility.Visible;
            }
        }

        private void AddImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter =
                "Image Files|*.png;*.jpg;*.jpeg;*.gif;*.bmp|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                AddImageToCanvas(openFileDialog.FileName);
            }
        }

        private void AddImageToCanvas(string imagePath)
        {
            Image image = new Image();
            BitmapImage bitmapImage = new(new Uri(imagePath));
            image.Source = bitmapImage;

            double canvasWidth = DrawingCanvas.ActualWidth;
            double canvasHeight = DrawingCanvas.ActualHeight;

            double scaleFactor = 0.8;

            image.Width = canvasWidth * scaleFactor;
            image.Height = canvasHeight * scaleFactor;

            double leftPosition = (canvasWidth - image.Width) / 2;
            double topPosition = (canvasHeight - image.Height) / 2;

            InkCanvas.SetLeft(image, leftPosition);
            InkCanvas.SetTop(image, topPosition);

            Panel.SetZIndex(image, tabDataList[mainTabsVM.SelectedTab].zIndexCount);

            canvasLogoAction(false);

            DrawingCanvas.Children.Add(image);

            tabDataList[mainTabsVM.SelectedTab].tabUndoManager.SaveState(DrawingCanvas);

            tabDataList[mainTabsVM.SelectedTab].zIndexCount++;
        }

        public void activate_load_window(List<savedTab> saved_tabs)
        {
            IsEnabled = false;

            LoadSavedWindow saved_win = new LoadSavedWindow(saved_tabs, this);
            saved_win.Owner = this;
            saved_win.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            saved_win.Closed += (s, args) =>
            {
                IsEnabled = true;
            };

            saved_win.Loaded += (s, args) =>
            {
                saved_win.Visibility = Visibility.Visible;
            };
            saved_win.Show();
        }

        private void take_screenshot_Click(object sender, RoutedEventArgs e)
        {
            activate_screen_shot_window();
        }

        public void activate_screen_shot_window()
        {
            CapturedScreens? captured_screens = SystemFileControl.CaptureScreenArea();

            if (captured_screens == null)
            {
                return;
            }

            IsEnabled = false;

            ScreenshotWindow screenshots_win = new(captured_screens, this)
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            screenshots_win.Closed += (s, args) =>
            {
                IsEnabled = true;
            };

            screenshots_win.Loaded += (s, args) =>
            {
                screenshots_win.Visibility = Visibility.Visible;
            };
            screenshots_win.Show();
        }

        public void triggerPenMenu()
        {
            if(pen_menu.IsVisible)
            {
                pen_menu.Hide();
                pen_menu.Topmost = false;
            }
            else
            {
                ExtenalMonitorInfo.LoadData();
                int x = ExtenalMonitorInfo.CursorX;
                int y = ExtenalMonitorInfo.CursorY;

                pen_menu.Left = x - (pen_menu.Width / 2);
                pen_menu.Top = y - (pen_menu.Height /2 );

                pen_menu.WindowState = WindowState.Normal;
                pen_menu.Topmost = true;

                pen_menu.Show();
            }
        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            var undoManager = tabDataList[mainTabsVM.SelectedTab].tabUndoManager;

            if (undoManager == null || !undoManager.CanUndo)
            {
                return;
            }

            CanvasState? updatedCanvas = undoManager.Undo();

            if (updatedCanvas == null)
            {
                return;
            }

            CanvasAnalyzer.reconstructCanvas(DrawingCanvas, updatedCanvas);

            UndoAPI.SetUndoButtonActive(this, undoManager.CanUndo);
            UndoAPI.SetRedoButtonActive(this, undoManager.CanRedo);

            if (DrawingCanvas.Strokes.Count == 0 && DrawingCanvas.Children.Count == 0)
            {
                canvasLogoAction(true);
            }
        }

        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            var undoManager = tabDataList[mainTabsVM.SelectedTab].tabUndoManager;

            if (undoManager == null || !undoManager.CanRedo)
            {
                return;
            }

            CanvasState? updatedCanvas = undoManager.Redo();

            if (updatedCanvas == null)
            {
                return;
            }

            CanvasAnalyzer.reconstructCanvas(DrawingCanvas, updatedCanvas);

            UndoAPI.SetUndoButtonActive(this, undoManager.CanUndo);
            UndoAPI.SetRedoButtonActive(this, undoManager.CanRedo);

            if (DrawingCanvas.Strokes.Count != 0 || DrawingCanvas.Children.Count != 0)
            {
                canvasLogoAction(false);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            WindowsLowLevelHelpers.UnhookWindowsHookEx();
            MouseHook.UnhookWindowsHookEx(mouseHook._hookID);
        }

        void DrawCanvasTemplate()
        {
            int lineSpacing = 32;
            GeometryGroup geo = new();

            switch (canvasTemplate)
            {
                case CanvasTemplate.Ruled:
                    for (int i = lineSpacing; i < Height; i += lineSpacing)
                    {
                        geo.Children.Add(new LineGeometry(new Point(0, i), new Point(Width, i)));
                    }
                    geo.Children.Add(new LineGeometry(new Point(Width / 2, 0), new Point(Width / 2, Height)));
                    break;

                case CanvasTemplate.Grid:
                    for (int i = lineSpacing; i < Width; i += lineSpacing)
                    {
                        geo.Children.Add(new LineGeometry(new Point(i, 0), new Point(i, Height)));
                    }
                    for (int i = lineSpacing; i < Height; i += lineSpacing)
                    {
                        geo.Children.Add(new LineGeometry(new Point(0, i), new Point(Width, i)));
                    }
                    break;

                case CanvasTemplate.Dot:
                    for (int i = lineSpacing; i < Width; i += lineSpacing)
                    {
                        for (int j = lineSpacing; j < Height; j += lineSpacing)
                        {
                            geo.Children.Add(new EllipseGeometry(new Point(i, j), 1, 1));
                        }
                    }
                    break;
            }

            canvasTemplateDrawing.Drawing = new GeometryDrawing()
            {
                Geometry = geo,
                Pen = new Pen(new SolidColorBrush(new Color()
                {
                    A = 102,
                    R = 143,
                    G = 115,
                    B = 247
                }),
                1)
            };
        }

        void OpenTemplatePanel()
        {
            if (templatePanelOpen)
            {
                return;
            }
            templatePanelOpen = true;
            templatePanelAnimation.From = 0;
            templatePanelAnimation.To = 1;
            templatePanelStoryboard.Begin(this);
            TemplatePanelBorder.Visibility = Visibility.Visible;
        }

        async void CloseTemplatePanel()
        {
            if (!templatePanelOpen)
            {
                return;
            }
            templatePanelOpen = false;
            templatePanelAnimation.From = 1;
            templatePanelAnimation.To = 0;
            templatePanelStoryboard.Begin(this);
            await Task.Delay(250);
            TemplatePanelBorder.Visibility = Visibility.Collapsed;
        }

        void TemplateControl_Click(object sender, RoutedEventArgs e)
        {
            if (templatePanelOpen)
            {
                CloseTemplatePanel();
            }
            else
            {
                OpenTemplatePanel();
            }
        }

        void Template_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;

            canvasTemplate = button.Name switch
            {
                "TemplateButton_Tracing" => CanvasTemplate.Tracing,
                "TemplateButton_Blank" => CanvasTemplate.Blank,
                "TemplateButton_Ruled" => CanvasTemplate.Ruled,
                "TemplateButton_Grid" => CanvasTemplate.Grid,
                "TemplateButton_Dot" => CanvasTemplate.Dot,
                _ => CanvasTemplate.Blank
            };
            tabDataList[mainTabsVM.SelectedTab].canvasBackground = canvasTemplate;
        }

        void ColorSwitch_Click(object sender, RoutedEventArgs e)
        {
            hideAllToolMenus();

            if (color_menu.Visibility == Visibility.Visible)
            {
                color_menu.Visibility = Visibility.Collapsed;
            }
            else
            {
                color_menu.Visibility = Visibility.Visible;
            }

            DrawingCanvas.DefaultDrawingAttributes = new DrawingAttributes
            {
                Color = drawingSetting.SelectedColor,
            };
        }

        void HandleSaveCommand(object sender, EventArgs e)
        {
            activateSaveTabWin();
        }

        void HandleOpenCommand(object sender, EventArgs e)
        {
            SaveLoad.readSaveTabsDB(this);
        }

        void CheckLicense()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            path += "\\AppData\\Roaming\\espressoFlow\\config.json";
            try
            {
                using StreamReader r = new(path);
                string json = r.ReadToEnd();
                dynamic items = JsonConvert.DeserializeObject(json)
                    ?? throw new Exception("Failed to read config.json");
                userEmail = items["user"]["email"];

                if (userEmail == null)
                {
                    return;
                }

                Post("jot-check", userEmail, HandleJotCheck);
            }
            catch (FileNotFoundException)
            {
                Debug.WriteLine($"couldn't find flow config file @ {path}");
            }
        }
    }
}
