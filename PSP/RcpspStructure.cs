using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PSP
{
    /// <summary>
    /// 
    /// </summary>
    class RcpspSolver {
        //
        private LinkedList<RcpspResource> _resourceList = new LinkedList<RcpspResource>();
        /// <summary>
        /// 资源队列
        /// </summary>
        public LinkedList<RcpspResource> ResourceList
        {
            get { return _resourceList; }
        }
        private LinkedList<RcspspProject> _projectList = new LinkedList<RcspspProject>();
        /// <summary>
        /// 项目列表
        /// </summary>
        public LinkedList<RcspspProject> ProjectList
        {
            get { return _projectList; }
        }
        private int totalWuWeight = 30;
        /// <summary>
        /// 船坞总吨位
        /// </summary>
        public int TotalWuWeight
        {
            get { return totalWuWeight; }
            set { totalWuWeight = value; }
        }
        private bool isMultiProject;
        /// <summary>
        /// 是否为多项目
        /// </summary>
        public bool IsMultiProject
        {
            get { return isMultiProject; }
            set { isMultiProject = value; }
        }
        private LinkedList<List<List<RcspspProject>>> allPartition = new LinkedList<List<List<RcspspProject>>>();
        /// <summary>
        /// 记载所有可行的的坞修划分
        /// </summary>
        public LinkedList<List<List<RcspspProject>>> AllPartition
        {
            get { return allPartition; }
        }
        //private Dictionary<List<List<RcspspProject>>, int> allPartitionScore = new Dictionary<List<List<RcspspProject>>, int>();
        ///// <summary>
        ///// 记载所有划分的得分
        ///// </summary>
        //internal Dictionary<List<List<RcspspProject>>, int> AllPartitionScore
        //{
        //    get
        //    {
        //        return allPartitionScore;
        //    }

        //    set
        //    {
        //        allPartitionScore = value;
        //    }
        //}
        private Dictionary<String, List<RcspspProject>> everyComb = new Dictionary<string, List<RcspspProject>>();
        /// <summary>
        /// string为(proj1,proj2...)格式字符串,everyComb用来记载组合详细
        ///     多个划分中有很多重复的组合如1,2,3,4的划分，不同的划分中会多次出现(2,3)组合在，
        ///     该string用来唯一区分一个组合，并且统一计算
        /// </summary>
        public Dictionary<String, List<RcspspProject>> EveryComb
        {
            get { return everyComb; }
        }
        private Dictionary<String, RcspspProject> everyCombBestProj = new Dictionary<string, RcspspProject>();
        /// <summary>
        /// everyCombBestList用来记载每个坞修组合最优排序的项目
        /// </summary>
        public Dictionary<String, RcspspProject> EveryCombBestProj
        {
            get { return everyCombBestProj; }
        }

        /// <summary>
        /// 通过吨位计算所有有效的全部船的划分组合情况，赋值给，allPartition，并且记载所有船组合情况到everyComb
        /// </summary>
        public void generateAllPossiblePartation(){
            allPartition.Clear();
            foreach (var part in GetAllPartitions(_projectList.ToArray()))
            {
                //用于形成所有划分中所有组合情况和Project组合的详细
                Dictionary<String, List<RcspspProject>> everyComb1 = new Dictionary<string, List<RcspspProject>>();
                //复制part中的排列
                List<List<RcspspProject>> partition = new List<List<RcspspProject>>();
                bool isOverloop = false;
                //遍历所有船坞组合，判定该划分中每个组合是否超过总吨位，只要有一个组合超过，废弃该划分
                foreach (List<RcspspProject> listComb in part)
                {
                    List<RcspspProject> copyList = new List<RcspspProject>();
                    int sum = 0;
                    string uniqueStr = null;
                    foreach (RcspspProject proj in listComb)
                    {
                        copyList.Add(proj);
                        sum += proj.Weight;
                        if (uniqueStr == null)
                            uniqueStr += proj.ProjectId;
                        else
                            uniqueStr += "," + proj.ProjectId;
                    }
                    partition.Add(copyList);
                    uniqueStr = "(" + uniqueStr + ")";
                    everyComb1[uniqueStr] = copyList;//字符串对应关系
                    if (sum > totalWuWeight) {
                        isOverloop = true;
                        break;
                    }

                }
                if (!isOverloop)
                {
                    //全部分组不超空间，可以进行合并，并且保存在allPartition中
                    foreach(string key in everyComb1.Keys){
                        //everyComb.Add(
                        if (!everyComb.ContainsKey(key))//不包含时才新建
                            everyComb[key] = everyComb1[key];
                        //everyCombCore[key] = -1;
                    }
                    //
                    allPartition.AddLast(partition);
                }
                else 
                {
                    partition.Clear();
                    everyComb1.Clear();//清理
                }
            }
        }
        
        /// <summary>
        /// 获取类型T的数组的所有划分情况，集合划分
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="elements"></param>
        /// <returns></returns>
        public IEnumerable<List<List<T>>> GetAllPartitions<T>(T[] elements)
        {
            var lists = new List<List<T>>();
            var indexes = new int[elements.Length];
            lists.Add(new List<T>());
            lists[0].AddRange(elements);
            for (; ; )
            {
                yield return lists;
                int i, index;
                for (i = indexes.Length - 1; ; --i)
                {
                    if (i <= 0)
                        yield break;
                    index = indexes[i];
                    lists[index].RemoveAt(lists[index].Count - 1);
                    if (lists[index].Count > 0)
                        break;
                    lists.RemoveAt(index);
                }
                ++index;
                if (index >= lists.Count)
                    lists.Add(new List<T>());
                for (; i < indexes.Length; ++i)
                {
                    indexes[i] = index;
                    lists[index].Add(elements[i]);
                    index = 0;
                }
            }
        }
        /// <summary>
        /// 通过多任务计算所有组合的坞修最优时间好排序
        /// </summary>
        public void calcAllCombMultiProcess()
        {
            //使用task进行多进程异步计算所有的工作
            List<Task<RcspspProject>> tasks = new List<Task<RcspspProject>>();
            int num = everyComb.Keys.Count;
            //每一个combo组合建立一个任务
            foreach (String str in everyComb.Keys)
            {
                if (everyComb[str] == null)
                    return;
                tasks.Add(Task<RcspspProject>.Factory.StartNew(() =>
                {
                    return calCombBestScoreByTabuSearch(str, everyComb[str]);
                }));
            }
            //等待所有任务结束
            Task.WaitAll(tasks.ToArray());
            //为计算结果进行赋值
            foreach (Task<RcspspProject> task in tasks)
            {
                RcspspProject project = task.Result;
                everyCombBestProj[project.BestCombStr] = project;
            }
        }
        
        /// <summary>
        /// 将组合的项目形成一个单项目，并且计算任务的最优排列和最优解
        /// </summary>
        private static RcspspProject calCombBestScoreByTabuSearch(String uniqeId, List<RcspspProject> projComb)//不要使用引用ref或者out，会引起子任务异常
        {
            
            RcspspProject singleProject = new RcspspProject();
            singleProject.BestSocre = int.MaxValue;
            RcpspJob zeroJob = new RcpspJob();
            zeroJob.id = "0";
            zeroJob.isWX = true;
            zeroJob.isVirtual = true;
            zeroJob.project = "0";
            singleProject.Jobs.AddFirst(zeroJob);

            singleProject.BestCombStr += uniqeId;


            //旧Job和复制的新Job的对应表
            Dictionary<RcpspJob, RcpspJob> copyMapping = new Dictionary<RcpspJob, RcpspJob>();
            //资源添加紧前紧后会更改原有的job，因此需要复制Job，资源和资源使用不会更改resource内容，不用复制，然后除去非坞修任务，添加最前和最后
            foreach (RcspspProject proj in projComb) {
                foreach (RcpspJob job in proj.Jobs) {
                    if (!job.isWX)//只处理坞修任务
                        continue;
                    RcpspJob newJob = job.cloneJob();
                    //处理紧前关系
                    foreach (RcpspJob preJob in job.predecessors)
                    {
                        if (preJob.isWX)
                        {
                            newJob.addPredecessor(copyMapping[preJob]);
                        }
                        else
                        {
                            newJob.addPredecessor(zeroJob);
                        }
                    }
                    copyMapping.Add(job, newJob);
                    singleProject.Jobs.AddLast(newJob);
                    //ngleProject//
                }
            }
            //处理整体，对没有紧后任务的添加虚拟结束节点
            RcpspJob lastJob = new RcpspJob();
            lastJob.id = Convert.ToString(singleProject.Jobs.Count) ;
            lastJob.isWX = true;
            lastJob.isVirtual = true;
            lastJob.project = "0";
            foreach (RcpspJob job in singleProject.Jobs) {
                if (job.successors.Count <= 0) {
                    lastJob.addPredecessor(job);
                }
            }
            singleProject.Jobs.AddLast(lastJob);
            //计算最优时间
            singleProject.BestSocre = TabuSearch.solve(singleProject.Jobs);
            //Thread.Sleep(5000);
            //Console.WriteLine("uniqeId=" + uniqeId + " ---------");
            return singleProject;

        }

        /// <summary>
        /// 将每一个划分中的项目非坞修和坞修大任务形成一个单项目，计算任务的最优解。然后提取所有组合中的最优解
        /// </summary>
        public RcspspProject calAllPartitionScore()
        {
            int num = allPartition.Count;
            List<Task<RcspspProject>> tasks = new List<Task<RcspspProject>>();
            //每一个combo组合建立一个任务
            foreach (List<List<RcspspProject>> projectPartition in this.allPartition)
            {
                tasks.Add(Task<RcspspProject>.Factory.StartNew(() =>
                {
                    return calPartitionBestScoreByTabuSearch(projectPartition, this.everyCombBestProj);
                }));
            }
            //等待所有任务结束
            Task.WaitAll(tasks.ToArray());

            //为计算结果进行赋值
            double allPartitionBestScore = double.MaxValue;
            RcspspProject bestPartitionProj = null;
            foreach (Task<RcspspProject> task in tasks) {
                RcspspProject project = task.Result;
//#if DEBUG
                Console.WriteLine("best core is " + project.BestSocre + " and partition is " + project.BestCombStr);
                Console.Write("this List is : ");
                foreach (RcpspJob job in project.Jobs)
                {
                    Console.Write("[" + job.id + "__" + job.project + "__" + job.duration + "]");
                }
                Console.WriteLine();
//#endif
                if (allPartitionBestScore > project.BestSocre)
                {
                    allPartitionBestScore = project.BestSocre;
                    bestPartitionProj = project;
                }
            }
            return bestPartitionProj;
        }
        /// <summary>
        /// 将所有船舶按划分情况进行合并，坞修替换成划分内组合的坞修大任务，并需求一个坞修资源，然后形成一个新的总项目，进行禁忌搜索
        /// </summary>
        /// <param name="projectPartition"></param>
        /// <param name="everyCombBestProj"></param>
        /// <returns></returns>
        private static RcspspProject calPartitionBestScoreByTabuSearch(List<List<RcspspProject>> projectPartition, Dictionary<String, RcspspProject> everyCombBestProj) {
            //
            RcspspProject singleProject = new RcspspProject();
            singleProject.BestSocre = int.MaxValue;
            RcpspJob zeroJob = new RcpspJob();
            zeroJob.id = "0";
            zeroJob.isWX = true;
            zeroJob.isVirtual = true;
            zeroJob.project = "0";
            singleProject.Jobs.AddFirst(zeroJob);
            //旧Job和复制的新Job的对应表
            Dictionary<RcpspJob, RcpspJob> copyMapping = new Dictionary<RcpspJob, RcpspJob>();
            RcpspResource resWx = new RcpspResource();
            resWx.max_capacity = 1;
            resWx.renewable = true;
            singleProject.Resources.AddLast(resWx);
            //资源添加紧前紧后会更改原有的job，因此需要复制Job，资源和资源使用不会更改resource内容，不用复制，然后除去非坞修任务，添加最前和最后
            foreach (List<RcspspProject> projComb in projectPartition)
            {
                //先形成字符串()
                String projCombStr = null;
                foreach (RcspspProject proj in projComb) {
                    if (projCombStr == null)
                        projCombStr += proj.ProjectId;
                    else
                        projCombStr += "," + proj.ProjectId;
                }
                projCombStr = "(" + projCombStr + ")";
                singleProject.BestCombStr += projCombStr;
                

                //生成总坞修任务
                RcspspProject wxProj = everyCombBestProj[projCombStr];
                if (wxProj == null)
                    throw new Exception("total outer cal " + projCombStr + " cannot find WXCombProject");
                string wxId = null;
                foreach (RcpspJob job1 in wxProj.Jobs) {
                    if (wxId == null)
                        wxId = "(" + job1.id + "," + job1.project + ")";
                    else
                        wxId += "→(" + job1.id + "," + job1.project + ")";
                }
                RcpspJob wxJob = new RcpspJob();
                wxJob.id = wxId;
                wxJob.duration = wxProj.BestSocre;
                wxJob.addResourceDemand(resWx, 1);
                wxJob.project = projCombStr;
                wxJob.isWX = true;
                singleProject.Jobs.AddLast(wxJob);

                //再遍历一遍，逐个任务替换并新加到singleProject
                foreach (RcspspProject proj in projComb)
                {
                    foreach (RcpspJob job in proj.Jobs)
                    {
                        //if (job.isWX)//坞修任务，不处理
                        //{
                        //    //如果其紧前为非坞修，要加入到大任务中
                        //}
                        if (job.isVirtual)//项目的开始和结束虚拟任务都去除，用新的替换
                            continue;
                        RcpspJob newJob = job.isWX?wxJob:job.cloneJob();
                        //处理紧前关系
                        foreach (RcpspJob preJob in job.predecessors)
                        {
                            if (preJob.isWX)//坞修的替换成大坞修任务
                            {
                                //自身就是大坞修任务，不需要处理，自身不是大坞修任务，添加到大坞修任务的紧前关系
                                if(!newJob.isWX)
                                    newJob.addPredecessor(wxJob);
                            }
                            else if (preJob.isVirtual) {//虚拟的替换成总虚拟节点
                                newJob.addPredecessor(zeroJob);
                            }
                            else
                            {
                                //既不是虚拟也不是坞修任务的紧前关系，添加到紧前关系中，此处当前包含坞修
                                newJob.addPredecessor(copyMapping[preJob]);//正常的查找对应的新任务，添加进入紧前队列
                            }
                        }
                        copyMapping.Add(job, newJob);
                        if(!newJob.isWX)//坞修大任务已经添加过一遍
                            singleProject.Jobs.AddLast(newJob);
                    }
                }

            }
            //处理整体，对没有紧后任务的添加虚拟结束节点
            RcpspJob lastJob = new RcpspJob();
            lastJob.id = Convert.ToString(singleProject.Jobs.Count);
            lastJob.isWX = true;
            lastJob.isVirtual = true;
            lastJob.project = "0";
            foreach (RcpspJob job in singleProject.Jobs)
            {
                if (job.successors.Count <= 0)
                {
                    lastJob.addPredecessor(job);
                }
                else if (job.predecessors.Count <= 0) {
                    job.addPredecessor(zeroJob);
                }
            }
            singleProject.Jobs.AddLast(lastJob);
            //计算最优时间
            singleProject.BestSocre = TabuSearch.solve(singleProject.Jobs,true);

            return singleProject;
        }

    }
    /// <summary>
    /// 资源受限多项目调度项目类
    /// </summary>
    class RcspspProject {
        private string projectId;
        /// <summary>
        /// 项目ID
        /// </summary>
        public string ProjectId
        {
            get { return projectId; }
            set { projectId = value; }
        }
        private double bestSocre = -1;
        /// <summary>
        /// 该项目最高得分
        /// </summary>
        public double BestSocre
        {
            get { return bestSocre; }
            set { bestSocre = value; }
        }

        private string bestCombStr = "";
        /// <summary>
        /// 获取项目组合
        /// </summary>
        public string BestCombStr
        {
            get { return bestCombStr; }
            set { bestCombStr = value; }
        }

        private int weight;
        /// <summary>
        /// 项目船坞吨位
        /// </summary>
        public int Weight
        {
            get { return weight; }
            set { weight = value; }
        }

        private double lambda = 0.0;
        /// <summary>
        /// 项目权重
        /// </summary>
        public double Lambda
        {
            get { return lambda; }

            set { lambda = value; }
        }
        /// <summary>
        /// 项目任务总数(算开始和结尾的虚拟任务)
        /// </summary>
        public int TotalJobNumber
        {
            get { return _jobs.Count; }
        }
        /// <summary>
        /// 项目实际数量(排除项目开始和结尾的虚拟任务)
        /// </summary>
        public int RealJobNumber
        {
            get { return _jobs.Count - 2; }
        }
        private LinkedList<RcpspJob> _jobs = new LinkedList<RcpspJob>();
        /// <summary>
        /// 项目列表
        /// </summary>
        public LinkedList<RcpspJob> Jobs {
            get { return _jobs; }
        }
        private LinkedList<RcpspResource> _resources = new LinkedList<RcpspResource>();
        /// <summary>
        /// 项目列表，单项目使用该resource，多项目该resource=solver的resource
        /// </summary>
        public LinkedList<RcpspResource> Resources {
            get { return _resources; }
            set { _resources = value; }
        }
        public RcspspProject() {
            //
        }
        public RcspspProject(string projId, int weight) {
            this.projectId = projId;
            this.weight = weight;
        }
        public bool removeJob(RcpspJob job) {
            LinkedListNode<RcpspJob> jobNode = _jobs.Find(job);
            if (jobNode == null)
                return true;
            job.removeAllProdecessors();
            job.removeAllSuccessors();
            _jobs.Remove(jobNode);
            return true;
        }



        private string _baseFile;
        public string BaseFile
        {
            get { return _baseFile; }
            set { _baseFile = value; }
        }
        private int _randomSeed;
        public int RandomSeed
        {
            get { return _randomSeed; }
            set { _randomSeed = value; }
        }
        private int _projs;
        public int Projs
        {
            get { return _projs; }
            set { _projs = value; } 
        }
        private int _taskNumber;
        public int TaskNumber
        {
            get { return _taskNumber; }
            set { _taskNumber = value; } 
        }
        private int _horizon;
        public int Horizon
        {
            get { return _horizon; }
            set { _horizon = value; }
        }
        private int _renewableResourceNumber;
        public int RenewableResourceNumber
        {
            get { return _renewableResourceNumber; }
            set { _renewableResourceNumber = value; }
        }
        private int _nonrenewableResourceNumber;
        public int NonrenewableResourceNumber
        {
            get { return _nonrenewableResourceNumber; }
            set { _nonrenewableResourceNumber = value; }
        }
        private int _doubleConstResourceNumber;
        public int DoubleConstResourceNumber
        {
            get { return _doubleConstResourceNumber; }
            set { _doubleConstResourceNumber = value; }
        }


    }
    /// <summary>
    /// 资源类，描述资源总数和资源可更新状态
    /// </summary>
    class RcpspResource
    {
        public int max_capacity = 0;
        public int min_capacity = 0;
        public int unit_cost = 1;
        public bool renewable = true;
    }

    /// <summary>
    /// 任务类，保存项目调度中每个任务，作为项目调度的基本单位
    ///    其中紧前紧后同步更新，便于查询;
    ///    每个项目都有两个虚拟的节点，第0个和第N+1个，其id为字符串0和1，项目合并时便于计算和处理
    /// </summary>
    class RcpspJob
    {
        /// <summary>
        /// 任务序号，guid产生，避免重复
        /// </summary>
        public string id = "";

        /// <summary>
        /// 该任务所属的项目
        /// </summary>
        public string project = "";
        public bool isFirst = false;
        public bool isLast = false;
        public bool isVirtual = false;

        /// <summary>
        /// 紧前工序
        /// </summary>
        public HashSet<RcpspJob> successors = new HashSet<RcpspJob>();

        /// <summary>
        /// 紧后工序
        /// </summary>
        public HashSet<RcpspJob> predecessors = new HashSet<RcpspJob>();

        /// <summary>
        /// 持续时间
        /// </summary>
        public double duration = 0;

        /// <summary>
        /// 从时间0开始的当前任务的开始时间
        /// </summary>
        public double startTime = -1;

        /// <summary>
        /// 对资源的需求数量
        /// </summary>
        public Dictionary<RcpspResource, int> resourcesDemand = new Dictionary<RcpspResource,int>();

        /// <summary>
        /// 是否坞修
        /// </summary>
        public bool isWX = false;

        public void addPredecessor(RcpspJob pred) {
            pred.successors.Add(this);
            this.predecessors.Add(pred);
        }
        
        public void addResourceDemand(RcpspResource resource, int num) {
            this.resourcesDemand[resource] = num;
        }
        public void removeAllProdecessors() {
            foreach(RcpspJob job in this.predecessors) {
                job.successors.Remove(this);
            }
            this.predecessors.Clear();
        }
        public void removeAllSuccessors() {
            foreach (RcpspJob job in this.successors)
            {
                job.successors.Remove(this);
            }
            this.successors.Clear();
        }
        public RcpspJob cloneJob() {
            RcpspJob cloneJob = new RcpspJob();
            cloneJob.duration = this.duration;
            cloneJob.project = this.project;
            cloneJob.isWX = this.isWX;
            cloneJob.resourcesDemand = this.resourcesDemand;
            cloneJob.id = this.id;
            cloneJob.isFirst = this.isFirst;
            cloneJob.isLast = this.isLast;
            return cloneJob;
        }
    }
}
