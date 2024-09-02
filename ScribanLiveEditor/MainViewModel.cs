using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Scriban;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using System.IO;

namespace ScribanLiveEditor
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseManager _dbManager;
        private string _templateText = File.ReadAllText("sample-template.txt");
        private string _jsonText = File.ReadAllText("sample-data.json");
        private string _previewText = "";
        private string _selectedTemplateName;

        public MainViewModel()
        {
            _dbManager = new DatabaseManager();
            LoadTemplateNames();
        }

        public string TemplateText
        {
            get => _templateText;
            set
            {
                if (_templateText != value)
                {
                    _templateText = value;
                    OnPropertyChanged();
                    UpdatePreview();
                }
            }
        }

        public string JsonText
        {
            get => _jsonText;
            set
            {
                if (_jsonText != value)
                {
                    _jsonText = value;
                    OnPropertyChanged();
                    UpdatePreview();
                }
            }
        }

        public string PreviewText
        {
            get => _previewText;
            set
            {
                if (_previewText != value)
                {
                    _previewText = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _htmlPreviewText;
        public string HtmlPreviewText
        {
            get => _htmlPreviewText;
            set
            {
                if (_htmlPreviewText != value)
                {
                    _htmlPreviewText = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<string> TemplateNames { get; } = new ObservableCollection<string>();

        public string SelectedTemplateName
        {
            get => _selectedTemplateName;
            set
            {
                if (_selectedTemplateName != value)
                {
                    _selectedTemplateName = value;
                    OnPropertyChanged();
                    LoadSelectedTemplate();
                }
            }
        }

        public void UpdatePreview()
        {
            try
            {
                var template = Template.Parse(TemplateText);
                var data = JsonSerializer.Deserialize<dynamic>(JsonText);
                var result = template.Render(data);
                PreviewText = result;
                HtmlPreviewText = result;
            }
            catch (JsonException jsonEx)
            {
                PreviewText = $"JSON Error: {jsonEx.Message}";
                HtmlPreviewText = $"<html><body><p>JSON Error: {jsonEx.Message}</p></body></html>";
            }
            catch (Exception ex)
            {
                PreviewText = $"Template Error: {ex.Message}";
                HtmlPreviewText = $"<html><body><p>Template Error: {ex.Message}</p></body></html>";
            }
        }

        public void SaveCurrentTemplate(string name)
        {
            _dbManager.SaveTemplate(name, TemplateText, JsonText);
            LoadTemplateNames();
        }

        private void LoadSelectedTemplate()
        {
            if (!string.IsNullOrEmpty(SelectedTemplateName))
            {
                var (templateText, jsonText) = _dbManager.LoadTemplate(SelectedTemplateName);
                if (templateText != null && jsonText != null)
                {
                    TemplateText = templateText;
                    JsonText = jsonText;
                }
            }
        }

        private void LoadTemplateNames()
        {
            TemplateNames.Clear();
            foreach (var name in _dbManager.GetTemplateNames())
            {
                TemplateNames.Add(name);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
