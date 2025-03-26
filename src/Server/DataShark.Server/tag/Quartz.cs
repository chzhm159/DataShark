using log4net;
using Quartz.Impl.Matchers;
using Quartz;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataShark.Server.tag {
    internal class Quartz {
        private static readonly ILog log = LogManager.GetLogger(typeof(Quartz));
        private static readonly Lazy<Quartz> _instance = new Lazy<Quartz>(() => new Quartz());
        public static Quartz Inst => _instance.Value;
        private IScheduler scheduler;
        private TriggerBuilder triggerBuilder;
        private bool moniting = false;
        public void Start() {
            if (scheduler != null || (scheduler != null && (!scheduler.IsStarted || scheduler.IsShutdown))) {
                Task.Run(() => {
                    // InitScheduler();
                    // BeginMonitor();
                    scheduler.Start();
                    moniting = true;
                });
            }
        }

        public void Stop() {
            if (scheduler != null && scheduler.IsStarted) {
                Task.Run(() => {
                    scheduler.Clear();
                    scheduler.Shutdown(false);
                    triggerBuilder = null;
                    scheduler = null;
                    moniting = false;
                });

            }
        }
        Random rand = new Random();
        /// <summary>
        /// </summary>
        /// <param name="threadCount"></param>
        /// <param name="maxConcurrency"></param>
        /// <param name="misfire"></param>
        public void InitScheduler(string threadCount = "1", string maxConcurrency = "1", string misfire = "30000") {
            string schName = "sch_" + Guid.NewGuid().ToString();
            string schThreadName = "schThreadName" + DateTime.Now.ToLongTimeString();
            NameValueCollection schedulerConfig = new NameValueCollection {
                {"quartz.threadPool.threadCount", threadCount},
                {"quartz.threadPool.maxConcurrency", maxConcurrency},
                {"quartz.scheduler.threadName", schThreadName},
                {"quartz.scheduler.instanceName", schName},
                {"org.quartz.jobStore.misfireThreshold",misfire }, // 任务延迟判断阈值
                {"org.quartz.scheduler.makeSchedulerThreadDaemon","true" },

            };
            scheduler = SchedulerBuilder.Create(schedulerConfig).BuildScheduler().Result;
            triggerBuilder = TriggerBuilder.Create();
        }
        private void BeginMonitor() {
            if (moniting) {
                return;
            }
            // RegisterRepeatJob("plcId", "tag");
        }
        public void ClearTagJob() {
            if (scheduler == null || scheduler.IsShutdown) {
                return;
            }
            scheduler.Clear();
        }
        public void DelTagReadJob(string page, string id) {
            if (scheduler == null || scheduler.IsShutdown) {
                return;
            }
            // string jobG = GetJobGroup(page);
            string trgG = GetTrgGroup(page);
            log.InfoFormat("页面变量读取任务删除...TrgGroup:{0},TrgName:{1}", trgG, id);
            Task<IReadOnlyCollection<TriggerKey>> trgKeysTask = scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.GroupEquals(trgG));
            trgKeysTask.Wait();
            IReadOnlyCollection<TriggerKey> trgKeys = trgKeysTask.Result;
            if (trgKeys != null) {
                TriggerKey? trgKey = trgKeys.ToList().Find(tk => {
                    return tk.Name == id;
                });
                if (trgKey != null) {
                    Task<bool> delTask = scheduler.UnscheduleJob(trgKey);
                    delTask.Wait();
                    log.InfoFormat("TrgGroup:{0},TrgName:{1} 已删除读取任务 ", trgG, id);
                } else {
                    log.InfoFormat("TrgGroup:{0},TrgName:{1} 未匹配到任务,删除无效 ", trgG, id);
                }
            } else {
                log.InfoFormat("TrgGroup:{0},TrgName:{1} Group未匹配到", trgG, id);
            }
        }
        public string GetJobGroup(string page) {
            return string.Format("page_job_group:{0}", page);
        }
        public string GetTrgGroup(string dev) {
            return string.Format("dev:{0}", dev);
        }
        public void AddTagReadJob<T>(string dev, string id, int interval) where T : IJob {

            string jobG = GetJobGroup(dev);
            string trgG = GetTrgGroup(dev);

            JobBuilder jobBuilder = JobBuilder.Create<T>();
            jobBuilder.WithIdentity(id, jobG);

            IJobDetail job = jobBuilder.Build();
            job.JobDataMap.Add("job_group", jobG);
            job.JobDataMap.Add("trg_group", trgG);
            job.JobDataMap.Add("page", dev);
            job.JobDataMap.Add("id", id);

            ITrigger trigger = triggerBuilder.ForJob(job.Key)
            .WithIdentity(id, trgG)
            .WithSimpleSchedule(x => x
            .WithMisfireHandlingInstructionIgnoreMisfires()
            .WithInterval(TimeSpan.FromMilliseconds(interval))
            .RepeatForever())
            .Build();
            log.InfoFormat("读取任务添加...TrgGroup:{0},TrgName:{1},Interval:{2}", trgG, id, interval);
            scheduler.ScheduleJob(job, trigger);
        }
    }
    [DisallowConcurrentExecution]
    public class PageTagMonitor : IJob {
        private static readonly ILog log = LogManager.GetLogger(typeof(PageTagMonitor));
        public Task Execute(IJobExecutionContext context) {
            try {
                AppEventBus bus = AppEventBus.Inst;
                JobKey key = context.JobDetail.Key;
                JobDataMap dataMap = context.JobDetail.JobDataMap;
                // bus.OnPageTag(dataMap);
                return Task.FromResult(true);
            } catch (Exception e) {                
                // log.ErrorFormat("Job Failed:{0},{1}",e.Message,e.StackTrace);
                return Task.FromResult(true);
            }
        }
    }
}
