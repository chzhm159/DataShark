using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace DataShark.Server.tag {
    public class TagManager {
        private static readonly ILog log = LogManager.GetLogger(typeof(TagManager));
        private string TagConfigDir { get; set; } = "config/devices/";
        
        public List<TagTable> TagTables = new List<TagTable>();
        public void OnServerLoad() {
            List<FileInfo> tagfilse = TryGetTagFiles();
            if (tagfilse == null) {
                log.WarnFormat("未检测到任何点位采集配置文件!!!");
                return; 
            }            
            LoadDevices(tagfilse);
        }

        private List<FileInfo> TryGetTagFiles() {
            string startFolder = TagConfigDir;
            DirectoryInfo dir = new DirectoryInfo(startFolder);
            var fileList = dir.GetFiles("*.*", SearchOption.AllDirectories);
            IEnumerable<FileInfo> fileQuery = from file in fileList
                                              where file.Name.StartsWith("tag_")
                                              select file;
            if (fileQuery.Any()) {
                return fileQuery.ToList();
            } else {
                log.WarnFormat("点位配置文件不存在! 搜索目录:{0} ", startFolder);
                return null;
            }
        }

        public void LoadDevices(List<FileInfo> tagfs) {
            if (tagfs == null || tagfs.Count < 1) { return; }
            foreach (var tagF in tagfs) {
                try {
                    TagTable tt= Load(tagF.FullName);
                    if (tt != null) {
                        TagTables.Add(tt);
                    }
                } catch (Exception e) {
                    string pnm = this.GetType().Name;
                    log.ErrorFormat("点位文件加载异常:{0},{1},{2}", tagF, e.Message, e.StackTrace);
                }
            }
        }
        public TagTable? Load(string tagPath) {            
            using (StreamReader file = File.OpenText(tagPath)) {
                JsonSerializer serializer = new JsonSerializer();
                TagTable tag_table = (TagTable)serializer.Deserialize(file, typeof(TagTable))!;
                if (tag_table != null && tag_table.Tags != null) {
                    // TODO
                    tag_table.Tags.ForEach(t => {                        
                        // 做一些处理
                    });
                }
                return tag_table;
            }
        }
    }
}
