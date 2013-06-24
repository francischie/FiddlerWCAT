﻿using System;
using System.IO;
using System.Windows.Forms;
using FiddlerWCAT.Helper;

namespace FiddlerWCAT.Entities
{
    public class Settings
    {
        private const string FileName = "wcat_settings.xml";
        private static Settings _instance;
        private static readonly object PadLock = new Object();

        public static Settings Instance
        {
            get
            {
                if (_instance == null || _instance.HasChange())
                {
                    lock (PadLock)
                    {
                        if (_instance == null || _instance.HasChange())
                        {
                            _instance = Load();
                        }
                    }
                }
                return _instance;
            }
        }


        public string WcatHomeDirectory { get; set; }
        public int Duration { get; set; }
        public int Cooldown { get; set; }
        public int Warmup { get; set; }
        public int VirtualClient { get; set; }

        private bool HasChange()
        {
            //-- routine code to invalidate the singleton instance such as reading some configuration 
            return false;
        }
        
        private static Settings Load()
        {
            var filename = Path.GetDirectoryName(Application.ExecutablePath);
            filename = filename + @"\" + FileName;

            var fileStream = File.Open(filename, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);
            var reader = new StreamReader(fileStream);
            var data = reader.ReadToEnd();
            reader.Close();

            
            //-- create default settings 
            var settings = new Settings
            {
                WcatHomeDirectory = TrySearchWcatDefaultInstallation(),
                Duration = 60,
                Warmup = 10,
                Cooldown = 20,
                VirtualClient = 1
            };

            if (String.IsNullOrEmpty(data))
                return settings;

            try
            {
                settings = Serializer.DeserializeObject<Settings>(data);
                if (settings.VirtualClient < 1) settings.VirtualClient = 1; 
            }
            catch
            {
                settings.Save();
            }

            return settings;
        }

        


        public void Save()
        {
            var filename = Path.GetDirectoryName(Application.ExecutablePath);
            filename = filename + @"\" + FileName;

            var data = Serializer.SerializeObject(this);
            var file = new StreamWriter(filename);
            file.Write(data);
            file.Flush();
            file.Close();
        }

        private static string TrySearchWcatDefaultInstallation()
        {
            var wcatDefault = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "wcat");
            wcatDefault = Directory.Exists(wcatDefault) ? wcatDefault : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "wcat");
            wcatDefault = Directory.Exists(wcatDefault) ? wcatDefault : "";
            return wcatDefault; 
        }

    }
}