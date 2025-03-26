using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataShark.Server.tag {
    public class TagTable {
        public Device Device { get; set; }
        public List<Tag> Tags { get; set; }
    }
    public class Device {
        public string Id { get; set; }
        public string Ip { get; set; }
        /// <summary>
        /// ModBus-TCP|ModBus-ASCII|S7|
        /// </summary>
        public string Protocol { get; set; }
        public int Port { get; set; }
        public string Factory { get; set; }
        public string Workshop { get; set; }
        public string Line { get; set; }
        public string Station { get; set; }
        public string Module { get; set; }
        public bool Enabled { get; set; } = true;
        public string Remark { get; set; }
    }
    public class Tag {
        private static readonly ILog log = LogManager.GetLogger(typeof(Tag));

        internal Device Device { get; set; }

        public string Id { get; set; }
        public string Addr { get; set; }
        public string DataType { get; set; }
        public int Count { get; set; }
        public int Interval { get; set; }
        public string Remark { get; set; }
        /// <summary>
        /// 此变量的操作模式: 
        /// <br>0: 默认模式</br>
        /// <br>1: 只读模式: 会在启动时,或者变量新建时,直接完成协议帧的指令生成</br>
        /// <br>2: 写入模式: 指令需要动态生成</br>
        /// <br>3: 读写模式: 指令动态生成</br>
        /// </summary>
        public int Operate { get; set; } = 0;
        public int IntValue {  get; set; }
        public int[] IntValues {  get; set; }=new int[1];

        public bool Sync() {
            if(this.Device==null) return false;

            return true;
        }

    }
}
