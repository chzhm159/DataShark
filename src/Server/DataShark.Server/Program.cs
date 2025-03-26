namespace DataShark.Server {
    internal class Program {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Program));
        static void Main(string[] args) {
            log4net.Config.XmlConfigurator.Configure(new FileInfo("config/log4net.config"));

            log.Info("Hello, World!");
            //ModBusFactory modBusFactory = new ModBusFactory();
            //modBusFactory.BuildMultipleWriteRegsRequestTest();
        }
    }
}
