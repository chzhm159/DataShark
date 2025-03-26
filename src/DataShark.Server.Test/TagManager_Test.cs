using DataShark.Server.tag;
using log4net;
using System.Formats.Asn1;

namespace DataShark.Server.Test {
    public class TagManager_Test {
        private static readonly ILog log = LogManager.GetLogger(typeof(TagManager_Test));
        public TagManager_Test() {
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo("config/log4net.config"));
        }
        [Fact]
        public void OnServerLoad_Test() {
            TagManager tagmanager = new TagManager();
            tagmanager.OnServerLoad();
            int count = tagmanager.TagTables.Count;
            bool HasDeviceID = !string.IsNullOrEmpty(tagmanager.TagTables[0].Device.Id);
            log.DebugFormat("TagTables count:{0}, DeviceID:{1}", count, tagmanager.TagTables[0].Device.Id);
            Assert.True(HasDeviceID);
        }
    }
}
