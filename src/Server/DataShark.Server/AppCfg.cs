using log4net;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataShark.Server {
    internal class AppCfg {
        private static readonly ILog log = LogManager.GetLogger(typeof(AppCfg));
        private static readonly Lazy<AppCfg> _instance = new Lazy<AppCfg>(() => new AppCfg());
        public static AppCfg Inst => _instance.Value;

        private static IConfigurationRoot cfgRoot { get; set; }

        private AppCfg() {
            ConfigurationBuilder appCfgBuider = new ConfigurationBuilder();
            appCfgBuider.SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "config"))
            .AddJsonFile("appsettings.json", true, reloadOnChange: true);
            cfgRoot = appCfgBuider.Build();

        }
        public static string GetStringVaue(string key, string defaultValue = "none") {
            return cfgRoot.GetValue<string>(key) ?? defaultValue;
        }
        public static void SetString(string key, string defaultValue = "none") {
            // cfgRoot.GetValue<string>(key) ?? defaultValue;
            
        }
        public static int GetIntVaue(string key, int defaultValue = -1) {
            int v = cfgRoot.GetValue<int>(key);
            if (v == 0) {
                return defaultValue;
            } else {
                return v;
            }
        }
        public static float GetFloatVaue(string key, float defaultValue = -1) {
            float v = cfgRoot.GetValue<float>(key);
            if (v == 0) {
                return defaultValue;
            } else {
                return v;
            }
        }
        public static double GetDoubleVaue(string key, double defaultValue = -1) {
            double v = cfgRoot.GetValue<double>(key);
            if (v == 0) {
                return defaultValue;
            } else {
                return v;
            }
        }

    }
}
