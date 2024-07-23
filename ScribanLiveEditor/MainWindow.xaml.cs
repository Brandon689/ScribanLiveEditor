using System.Windows;
using System.Windows.Controls;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Highlighting;
using System.Text.Json;

namespace ScribanLiveEditor
{
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;
        private bool _isUpdatingText = false;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;
            // Apply custom Scriban highlighting
            var scribanHighlighting = LoadScribanHighlighting();
            if (scribanHighlighting != null)
            {
                TemplateEditor.SyntaxHighlighting = scribanHighlighting;
            }

            TemplateEditor.TextChanged += (s, e) =>
            {
                if (!_isUpdatingText)
                {
                    _isUpdatingText = true;
                    _viewModel.TemplateText = TemplateEditor.Text;
                    UpdatePreviews();
                    _isUpdatingText = false;
                }
            };

            JsonEditor.TextChanged += (s, e) =>
            {
                if (!_isUpdatingText)
                {
                    _isUpdatingText = true;
                    _viewModel.JsonText = FormatJson(JsonEditor.Text);
                    Console.WriteLine(_viewModel.JsonText);
                    
                    UpdatePreviews();
                    _isUpdatingText = false;
                }
            };

            _viewModel.PropertyChanged += (s, e) =>
            {
                if (!_isUpdatingText)
                {
                    _isUpdatingText = true;
                    if (e.PropertyName == nameof(MainViewModel.TemplateText))
                        TemplateEditor.Text = _viewModel.TemplateText;
                    else if (e.PropertyName == nameof(MainViewModel.JsonText))
                        JsonEditor.Text = _viewModel.JsonText;
                    else if (e.PropertyName == nameof(MainViewModel.PreviewText))
                    {
                        UpdateHtmlPreview();
                        UpdateCSharpPreview();
                        UpdateHtmlEditPreview();
                    }
                    _isUpdatingText = false;
                }
            };

            // Initialize editor content
            TemplateEditor.Text = _viewModel.TemplateText;
            JsonEditor.Text = _viewModel.JsonText;
        }

        private void UpdatePreviews()
        {
            _viewModel.UpdatePreview();
            UpdateHtmlPreview();
            UpdateCSharpPreview();
            UpdateHtmlEditPreview();
        }
        static string FormatJson(string jsonString)
        {
            try
            {
                // Parse the JSON string
                using (JsonDocument document = JsonDocument.Parse(jsonString))
                {
                    // Create options for formatting
                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true
                    };

                    // Serialize the document back to a string with formatting
                    return JsonSerializer.Serialize(document, options);
                }
            }
            catch (JsonException ex)
            {
                return $"Invalid JSON: {ex.Message}";
            }
        }
        private void UpdateHtmlPreview()
        {
            if (!string.IsNullOrEmpty(_viewModel.PreviewText))
            {
                HtmlPreview.NavigateToString(_viewModel.PreviewText);
            }
            else
            {
                HtmlPreview.NavigateToString("<html><body><p>No preview available</p></body></html>");
            }
        }

        private void UpdateCSharpPreview()
        {
            CSharpPreview.Text = _viewModel.PreviewText;
        }

        private void UpdateHtmlEditPreview()
        {
            RazorPreview.Text = _viewModel.PreviewText;
        }

        private void UpdateJsonEditPreview()
        {
            RazorPreview.Text = _viewModel.PreviewText;
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(SaveNameTextBox.Text))
            {
                _viewModel.SaveCurrentTemplate(SaveNameTextBox.Text);
                SaveNameTextBox.Clear();
            }
            else
            {
                MessageBox.Show("Please enter a name for the template.", "Save Template", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private IHighlightingDefinition LoadScribanHighlighting()
        {
            string xshdPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScribanHighlighting.xshd");
            if (System.IO.File.Exists(xshdPath))
            {
                using (var stream = new System.IO.FileStream(xshdPath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    using (var reader = new System.Xml.XmlTextReader(stream))
                    {
                        return ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(reader, HighlightingManager.Instance);
                    }
                }
            }
            else
            {
                System.Windows.MessageBox.Show("ScribanHighlighting.xshd file not found.", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return null;
            }
        }

        private void FormatJSON_Click(object sender, RoutedEventArgs e)
        {
            JsonEditor.Text = _viewModel.JsonText;
        }
    }
}
