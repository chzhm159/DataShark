using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataShark.Server {
    public  class AppEventBus {
        private static readonly ILog log = LogManager.GetLogger(typeof(AppEventBus));
        private static readonly Lazy<AppEventBus> _instance = new Lazy<AppEventBus>(() => new AppEventBus());
        public static AppEventBus Inst => _instance.Value;
        
        static StringComparison SC = StringComparison.OrdinalIgnoreCase;

        private AppEventBus() {

            
        }
    }
}
