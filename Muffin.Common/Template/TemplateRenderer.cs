using Muffin.Common.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Muffin.Common.Template
{
    public sealed class TemplateRenderer
    {
        #region Singleton

        private TemplateRenderer() { }
        private static TemplateRenderer _instance = null;
        private static object _initLock = new object();
        public static TemplateRenderer Instance
        {
            get
            {
                lock (_initLock)
                    if (_instance == null)
                        _instance = new TemplateRenderer();
                return _instance;
            }
        }

        #endregion

        #region Properties

        private Dictionary<string, string> _templates = new Dictionary<string, string>();

        #endregion

        #region Config

        public List<string> TemplateSearchPaths { get; private set; } = new List<string>();
        public void AddTemplateSearchPath(string path)
        {
            if (!Directory.Exists(path))
                throw new ArgumentException("Path does not exist. " + path);

            if (TemplateSearchPaths.Contains(path))
                return;

            TemplateSearchPaths.Add(path);
        }

        public Dictionary<string, string> Templates { get; private set; } = new Dictionary<string, string>();
        public void AddTemplate(string name, string template)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name is null.");
            if (string.IsNullOrWhiteSpace(template))
                throw new ArgumentNullException("template is null.");

            if (Templates.ContainsKey(name))
                return;
            Templates.Add(name, template);
        }

        #endregion

        #region Initialisation

        public void LoadTemplates()
        {
            foreach (var templateSearchPath in TemplateSearchPaths)
            {
                var fileNames = Directory.EnumerateFiles(templateSearchPath, "*.*", SearchOption.AllDirectories);
                foreach (var fileName in fileNames)
                {
                    var templateName = Path.GetFileNameWithoutExtension(fileName);
                    var templateContent = File.ReadAllText(Path.Combine(templateSearchPath, fileName));

                    AddTemplate(templateName, templateContent);
                }
            }
        }

        #endregion

        #region Render

        public const string KEY_PATTERN = @"\@\{.*?\}";
        public static readonly Regex KeyRegex = new Regex(KEY_PATTERN);

        public string RenderTemplate<T>(string templateName, T model)
        {
            if (!Templates.ContainsKey(templateName))
                throw new ArgumentException("template not found: " + templateName);

            var template = string.Copy(Templates[templateName]);
            var matches = KeyRegex.Matches(template)
                    .Cast<Match>()
                    .Select(p => p.Value)
                    .ToArray();

            foreach (var match in matches)
            {
                var keyPath = match.Replace("@{", "").Replace("}", "");

                object value;
                if (keyPath.Contains("Template:"))
                {
                    var innerTemplate = keyPath.Split(':')[1];
                    var innerKeyPath = keyPath.Split(':')[2];
                    var innerModel = PropertyHelper.GetPropertyValue(model, innerKeyPath);
                    if (innerModel != null && innerModel.GetType().IsArray)
                        value = RenderTemplate(innerTemplate, (IEnumerable)innerModel);
                    else
                        value = RenderTemplate(innerTemplate, innerModel);
                }
                else
                    value = PropertyHelper.GetPropertyValue(model, keyPath);

                string stringValue = null;

                if (value != null)
                    stringValue = value.ToString();
                else
                    stringValue = "";
                template = template.Replace(match, stringValue);
            }
            return template;
        }

        public string RenderTemplate<T>(string templateName, T[] model)
        {
            var sb = new StringBuilder();
            foreach (var m in model)
                sb.Append(RenderTemplate(templateName, m));
            return sb.ToString();
        }

        public string RenderTemplate<T>(string templateName, IEnumerable<T> model)
        {
            var sb = new StringBuilder();
            foreach (var m in model)
                sb.Append(RenderTemplate(templateName, m));
            return sb.ToString();
        }

        public string RenderTemplate(string templateName, IEnumerable model)
        {
            var sb = new StringBuilder();
            foreach (var m in model)
                sb.Append(RenderTemplate(templateName, m));
            return sb.ToString();
        }

        #endregion
    }
}
