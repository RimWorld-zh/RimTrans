using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;

namespace RimTrans.Lite.Util
{
    internal sealed class UserSettings : INotifyPropertyChanged
    {
        #region Variables

        //private static ResourceDictionary _local;
        private static ResourceDictionary _appData;
        private static readonly ResourceDictionary Default;

        public static UserSettings All { get; } = new UserSettings();

        #endregion

        static UserSettings()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return;

            // Paths
            //string local = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings.xaml");
            string appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "duduluu", "RimTrans", "Settings.xaml");

            // Only create an empty AppData settings file if there's no local settings defined.
            if (/*!File.Exists(local) &&*/ !File.Exists(appData))
            {
                string dir = Path.GetDirectoryName(appData);

                if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                // Just creates an empty file without writting enything
                File.Create(appData).Dispose();
            }

            // Load AppData settings
            if (File.Exists(appData))
            {
                _appData = LoadOrDefault(appData);
                Application.Current.Resources.MergedDictionaries.Add(_appData);
            }

            // Load local settings
            //if (File.Exists(local))
            //{
            //    _local = LoadOrDefault(local);
            //    Application.Current.Resources.MergedDictionaries.Add(_local);
            //}

            //Reads the default settings (It's loaded by default).
            Default = Application.Current.Resources.MergedDictionaries.FirstOrDefault(d => d.Source.OriginalString.EndsWith("/Settings.xaml"));

            if (string.IsNullOrWhiteSpace(All.RimWorldDirectory))
            {
                All.RimWorldDirectory = Steam.GetRimWorldDirectory();
            }
            if (string.IsNullOrWhiteSpace(All.WorkshopDirectory))
            {
                All.WorkshopDirectory = Steam.GetWorkshopDirectory();
            }
            Save();
        }

        public static void Save()
        {
            if (_appData == null)
                return;

            string appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "duduluu", "RimTrans", "Settings.xaml");

            string dir = Path.GetDirectoryName(appData);
            if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            using (XmlWriter xw = XmlWriter.Create(appData, new XmlWriterSettings { Indent = true }))
            {
                XamlWriter.Save(_appData, xw);
            }
        }

        private static ResourceDictionary LoadOrDefault(string path)
        {
            ResourceDictionary resource = null;

            try
            {
                using (var fs = new FileStream(path, FileMode.Open))
                {
                    try
                    {
                        //Read in ResourceDictionary File
                        resource = (ResourceDictionary)XamlReader.Load(fs);
                    }
                    catch (Exception)
                    {
                        //Sets a default value if null.
                        resource = new ResourceDictionary();
                    }
                }

                //Tries to load the resource from disk. 
                //resource = new ResourceDictionary {Source = new Uri(path, UriKind.RelativeOrAbsolute)};
            }
            catch (Exception)
            {
                //Sets a default value if null.
                resource = new ResourceDictionary();
            }

            return resource;
        }

        #region Property Changed

        public static object GetValue([CallerMemberName] string key = "")
        {
            if (Application.Current == null || Application.Current.Resources == null)
                return Default[key];

            if (Application.Current.Resources.Contains(key))
                return Application.Current.FindResource(key);

            return Default[key];
        }

        public static void SetValue(object value, [CallerMemberName] string key = "")
        {
            // Update or insert the value to the AddData resource
            if (_appData != null)
            {
                if (_appData.Contains(key))
                    _appData[key] = value;
                else
                    _appData.Add(key, value);
            }

            // Update or insert the current value of the resource
            if (Application.Current.Resources.Contains(key))
                Application.Current.Resources[key] = value;
            else
                Application.Current.Resources.Add(key, value);

            All.OnPropertyChanged(key);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Properties

        public string RimWorldDirectory
        {
            get { return (string)GetValue(); }
            set { SetValue(value); }
        }

        public string ModsDirectory
        {
            get { return Path.Combine(RimWorldDirectory, "Mods"); }
        }

        public string CoreDirectory
        {
            get { return Path.Combine(ModsDirectory, "Core"); }
        }

        public string WorkshopDirectory
        {
            get { return (string)GetValue(); }
            set { SetValue(value); }
        }

        public string CustomModSelectedFolder
        {
            get { return (string)GetValue(); }
            set { SetValue(value); }
        }

        #endregion
    }
}
