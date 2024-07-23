using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Scriban;
using System.Collections.ObjectModel;

namespace ScribanLiveEditor
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseManager _dbManager;
        private string _templateText = "";
        private string _jsonText = "{\n  \"name\": \"John Doe\",\n  \"age\": 30,\n  \"hobbies\": [\"Reading\", \"Coding\", \"Gaming\"]\n}";
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
            }
            catch (JsonException jsonEx)
            {
                PreviewText = $"JSON Error: {jsonEx.Message}";
            }
            catch (Exception ex)
            {
                PreviewText = $"Template Error: {ex.Message}";
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
