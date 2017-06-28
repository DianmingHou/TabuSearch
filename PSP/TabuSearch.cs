using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSP
{
    public class NeighborSwap 
    {
        public RcpspJob from;
        public RcpspJob to;
        public double score;
        public NeighborSwap(RcpspJob from, RcpspJob to, double score) {
            this.from = from;
            this.to = to;
            this.score = score;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        //比较两个置换是否相等，部分先后，其中(a,b)==(a,b)；(a,b)==(b,a)
        public override bool Equals(object obj)
        {
            //判断与之比较的类型是否为null。这样不会造成递归的情况  
            if (obj == null)  
                return false;  
            if (GetType() != obj.GetType())  
                return false;
            NeighborSwap equObj = (NeighborSwap)obj;
            if ((this.from == equObj.from && this.to == equObj.to) || (this.from == equObj.to && this.to == equObj.from))
                return true;
            else
                return false;
    
        }
    }
    public static class TabuSearch
    {
        public static int numCandidate = 6;
        public static int maxIterationNum = 500;
        public static int tabuListSize = 10;//一般取任务总数的开根号，所得值的最大整数
        //置换
        public static int numberIter = 0;

        public static void print(LinkedList<RcpspJob> globalList, bool withLambda = false) {
            double bestCore = TabuSearch.calScheduleCore(globalList,withLambda);
            LinkedListNode<RcpspJob> recursiveNode = globalList.First;
            Console.WriteLine("total cost is " + bestCore);
            Console.WriteLine("jobnr\t\tduaration\t\tstarttime\t\tsucessors");
            while (recursiveNode != null)
            {
                Console.Write(recursiveNode.Value.id + "\t\t\t\t\t" + recursiveNode.Value.startTime + "\t\t" + recursiveNode.Value.duration + "\t\t");
                foreach (RcpspJob suces in recursiveNode.Value.successors)
                    Console.Write(suces.id + "\t");
                Console.WriteLine();
                recursiveNode = recursiveNode.Next;
            }
        }

        //禁忌处理，弹出，比较全局最优

        public static double solve(LinkedList<RcpspJob> globalList,bool withLambda = false) {
            //生成初始排序和初始解，作为当前最优
            getInitScheduleByMIT(globalList);
            double globalBestScore = calScheduleCore(globalList,withLambda);
            //将当前初始解赋值给全局最优解队列
            LinkedList<RcpspJob> projJobs = new LinkedList<RcpspJob>();
            LinkedListNode<RcpspJob> copyRecusive = globalList.First;
            while (copyRecusive != null) {
                projJobs.AddLast(copyRecusive.Value);
                copyRecusive = copyRecusive.Next;
            }
            //定义当前领域计算的最优解，初始赋值为全局最优
            double currentBestScore = globalBestScore;
            int numberIter = maxIterationNum;
            //长度确定的禁忌表，不足用null对象填充；对禁忌表的操作
            LinkedList<NeighborSwap> tabuList = TabuSearch.initTabuList(TabuSearch.tabuListSize) ;
            while ((--numberIter) > 0)
            {
#if DEBUG
                Console.Write("执行第 " + (maxIterationNum - numberIter + 1) + "次遍历:");
                print(projJobs);
#endif
                TabuSearch.numberIter++;
                //获取邻居最好的禁忌长度
                LinkedList<NeighborSwap> bestNeighborSwap = getBestNeighborBySwap(projJobs,withLambda);
                //是否优于globalBestScore,是，查询在不在禁忌表中，在的话放到开头，不在加到开头；否，查询非禁忌最优解
                LinkedListNode<NeighborSwap> recursiveNode = bestNeighborSwap.First;
                
                if (recursiveNode!=null&&recursiveNode.Value.score < globalBestScore)
                {
#if DEBUG
                    Console.WriteLine("best than global score: swap (" + recursiveNode.Value.from.id + "," + recursiveNode.Value.to.id + ") and core is " + recursiveNode.Value.score);
#endif
                    //置换
                    swapNeighbor(projJobs, projJobs.Find(recursiveNode.Value.from), projJobs.Find(recursiveNode.Value.to));
                    //赋值全局最优和当前最优
                    globalBestScore = currentBestScore = recursiveNode.Value.score;
                    //赋值该列表为全局最优列表
                    copyRecusive = projJobs.First;
                    globalList.Clear();
                    while (copyRecusive != null)
                    {
                        globalList.AddLast(copyRecusive.Value);
                        copyRecusive = copyRecusive.Next;
                    }
                    //更新禁忌表，如果在禁忌表中，将其从列表中移出并加入到最后；如果不存在，直接加到最后
                    LinkedListNode<NeighborSwap> findNode = tabuList.Find(recursiveNode.Value);
                    if (findNode != null)
                    {
                        tabuList.Remove(findNode);
                        tabuList.AddLast(findNode);
                    }
                    else 
                    {
                        tabuList.RemoveFirst();
                        tabuList.AddLast(recursiveNode.Value);
                    }

                }
                else {
                    //从前到后遍历当前最优解，查找是否在禁忌表中，如果不在，加入到禁忌表最后，并且置换，更新局部最优，但是不更新全局最优；
                    //查到，遍历下一个;当遍历完成都未满足，则最后差一个空的。
                    bool hasLocatedValue = false;
                    while (recursiveNode != null) {
                        if (!tabuList.Contains(recursiveNode.Value)) {
#if DEBUG
                            Console.WriteLine("find no in tabulist: swap (" + recursiveNode.Value.from.id + "," + recursiveNode.Value.to.id + ") and core is " + recursiveNode.Value.score);
#endif
                            hasLocatedValue = true;
                            tabuList.RemoveFirst();
                            tabuList.AddLast(recursiveNode.Value);
                            currentBestScore = recursiveNode.Value.score;
                            swapNeighbor(projJobs, projJobs.Find(recursiveNode.Value.from), projJobs.Find(recursiveNode.Value.to));
                            break;//直接返回
                        }
                        recursiveNode = recursiveNode.Next;
                    }
                    if (!hasLocatedValue) {
                        tabuList.RemoveFirst();
                        tabuList.AddLast(new NeighborSwap(null,null,0));
                    }


                }
#if DEBUG
                Console.Write("current list is :(");
                foreach (RcpspJob job in projJobs) {
                    Console.Write(job.id + ",");
                }
                Console.WriteLine(")");
                Console.Write("tabuList is :");
                foreach (NeighborSwap swap in tabuList) {
                    if(swap.from!=null&& swap.to!=null)
                        Console.Write("(" + swap.from.id + "," + swap.to.id + ")");
                }
                Console.WriteLine();
#endif
            }
            //执行最后，返回全局最优
            return globalBestScore;
        }
        public static LinkedList<NeighborSwap> initTabuList(int tabuLength) {
            LinkedList<NeighborSwap> tabuList = new LinkedList<NeighborSwap>();
            for (int i = 0; i < tabuLength; i++) {
                tabuList.AddLast(new NeighborSwap(null, null, 0));
            }
            return tabuList;
        }
        /// <summary>
        /// 通过置换得到numCadidate个最优领域解
        /// </summary>
        /// <param name="projJobs"></param>
        /// <returns></returns>
        public static LinkedList<NeighborSwap> getBestNeighborBySwap(LinkedList<RcpspJob> projJobs, bool withLambda)
        {
            LinkedList<NeighborSwap> bestSwaps = new LinkedList<NeighborSwap>();
            LinkedListNode<RcpspJob> recursiveNode = projJobs.First;
            while(recursiveNode!=null){
                if(recursiveNode.Next==null)
                    break;
                LinkedListNode<RcpspJob> recursiveAfterNode = recursiveNode.Next;
                int i = 3;//2为89,3为88
                while(recursiveAfterNode!=null){
                    swapNeighbor(projJobs,recursiveNode,recursiveAfterNode);
                    //检测满足紧后关系
                    if (checkFeasiable(projJobs)) {
                        double score = calScheduleCore(projJobs,withLambda);
                        NeighborSwap swap = new NeighborSwap(recursiveNode.Value,recursiveAfterNode.Value,score);
                        compareBestSwaps(bestSwaps, swap);
                    }
                    swapNeighbor(projJobs, recursiveAfterNode, recursiveNode);
                    recursiveAfterNode = recursiveAfterNode.Next;
                    if (i-- < 0) break;
                }
                recursiveNode = recursiveNode.Next;
            }
#if DEBUG
            foreach (NeighborSwap swp in bestSwaps) {
                System.Diagnostics.Debug.WriteLine("index :" + TabuSearch.numberIter + "swap (" + swp.from.id + "," + swp.to.id + ") and core is " + swp.score);
            }
#endif
            return bestSwaps;
        }

        public static LinkedList<NeighborSwap> getBestNeighborByRandomSwap(LinkedList<RcpspJob> projJobs,bool withLambda)
        {
            //随机位置
            Random rand = new Random(Convert.ToInt32((new DateTime()).ToBinary()));
            int size = TabuSearch.numCandidate;
            while (size > 0)
            {
                rand.Next(1, projJobs.Count);
            }
            LinkedList<NeighborSwap> bestSwaps = new LinkedList<NeighborSwap>();
            LinkedListNode<RcpspJob> recursiveNode = projJobs.First;
            while (recursiveNode != null)
            {
                if (recursiveNode.Next == null)
                    break;
                LinkedListNode<RcpspJob> recursiveAfterNode = recursiveNode.Next;
                int i = 3;
                while (recursiveAfterNode != null)
                {
                    swapNeighbor(projJobs, recursiveNode, recursiveAfterNode);
                    //检测满足紧后关系
                    if (checkFeasiable(projJobs))
                    {
                        double score = calScheduleCore(projJobs, withLambda);
                        NeighborSwap swap = new NeighborSwap(recursiveNode.Value, recursiveAfterNode.Value, score);
                        compareBestSwaps(bestSwaps, swap);
                    }
                    swapNeighbor(projJobs, recursiveAfterNode, recursiveNode);
                    recursiveAfterNode = recursiveAfterNode.Next;
                    if (i-- < 0) break;
                }
                recursiveNode = recursiveNode.Next;
            }
#if DEBUG
            foreach (NeighborSwap swp in bestSwaps)
            {
                System.Diagnostics.Debug.WriteLine("index :" + TabuSearch.numberIter + "swap (" + swp.from.id + "," + swp.to.id + ") and core is " + swp.score);
            }
#endif
            return bestSwaps;
        }
        public static LinkedList<NeighborSwap> getBestNeighborBySwapNoAfter(LinkedList<RcpspJob> projJobs, bool withLambda)
        {
            LinkedList<NeighborSwap> bestSwaps = new LinkedList<NeighborSwap>();
            LinkedListNode<RcpspJob> recursiveNode = projJobs.First;
            while (recursiveNode != null)
            {
                if (recursiveNode.Next == null)
                    break;
                LinkedListNode<RcpspJob> recursiveAfterNode = projJobs.First;
                //int i = 1;//2为89,3为88
                bool afterCur = false;
                while (recursiveAfterNode != null)
                {
                    if (recursiveAfterNode == recursiveNode)
                        afterCur = true;
                    else if (recursiveAfterNode.Value != recursiveNode.Value)
                    {

                        if (afterCur)
                            swapNeighbor(projJobs, recursiveNode, recursiveAfterNode);
                        else
                            swapNeighbor(projJobs, recursiveAfterNode, recursiveNode);

                        //检测满足紧后关系
                        if (checkFeasiable(projJobs))
                        {
                            double score = calScheduleCore(projJobs,withLambda);
                            NeighborSwap swap = new NeighborSwap(recursiveNode.Value, recursiveAfterNode.Value, score);
                            compareBestSwaps(bestSwaps, swap);
                        }
                        if (afterCur)
                            swapNeighbor(projJobs, recursiveAfterNode, recursiveNode);
                        else
                            swapNeighbor(projJobs, recursiveNode, recursiveAfterNode);
                        //if (i-- < 0) break;
                    }

                    recursiveAfterNode = recursiveAfterNode.Next;
                }
                recursiveNode = recursiveNode.Next;
            }
            return bestSwaps;
        }
        /// <summary>
        /// 将给定swap和邻域最优解队列比对，队列不满优选解总数TabuSearch.numCandidate则插入，否则比较得分；该队列为从小到大排列
        /// </summary>
        /// <param name="bestSwaps"></param>
        /// <param name="swap"></param>
        /// <returns></returns>
        public static void compareBestSwaps(LinkedList<NeighborSwap> bestSwaps, NeighborSwap swap) 
        {
            bool hasInsert = false;
            LinkedListNode<NeighborSwap> recursiveNode = bestSwaps.First;
            while (recursiveNode != null) {
                //if (swap.score < recursiveNode.Value.score) {
                //    //已经存在该值，不存储
                //    return;
                //}
                //else 
                if (swap.score < recursiveNode.Value.score)
                {
                    bestSwaps.AddBefore(recursiveNode, swap);
                    hasInsert = true;
                    break;
                }
                recursiveNode = recursiveNode.Next;
            }
            
            if (hasInsert) {
                //该节点已经插入，需要判断是否总长度超了
                if (bestSwaps.Count > TabuSearch.numCandidate)
                    bestSwaps.RemoveLast();
            }
            else if (bestSwaps.Count < TabuSearch.numCandidate)//比较完了，自己还没有插进去，但是总长度小，直接插在结尾
                bestSwaps.AddLast(swap);
        }


        /// <summary>
        /// 交换队列中两个节点位置
        /// </summary>
        /// <param name="projJobs"></param>
        /// <param name="firstNode"></param>
        /// <param name="afterNode"></param>
        public static void swapNeighbor(LinkedList<RcpspJob> projJobs,LinkedListNode<RcpspJob> beforeNode,LinkedListNode<RcpspJob> afterNode)
        {

            LinkedListNode<RcpspJob> afterBeforeNode = beforeNode.Previous;//afterNode要插入他之后
            //前边的要在其之前插入
            LinkedListNode<RcpspJob> insertBeforeNode = afterNode.Next;//beforeNode要插入到他之前
            projJobs.Remove(afterNode);
            projJobs.Remove(beforeNode);
            if (afterBeforeNode == null)
                projJobs.AddFirst(afterNode);
            else
                projJobs.AddAfter(afterBeforeNode, afterNode);
            if (insertBeforeNode == null)
                projJobs.AddLast(beforeNode);
            else
                projJobs.AddBefore(insertBeforeNode, beforeNode);
        }

        /// <summary>
        /// 对单项目的工作按最多紧后任务进行排序
        /// </summary>
        /// <param name="noMap">从0开始到N-1的多项目总大小</param>
        /// <returns></returns>
        public static double getInitScheduleByMIT(LinkedList<RcpspJob> projJobs,bool withLambda = false)
        {
            //int jobSize = singleProj.TotalJobNumber;
            //LinkedList<RcpspJob> projJobs = singleProj.Jobs;
            //第一个一定是任务0，不参与排序,最后一个虚拟任务紧后任务为0，按算法计算一定在最后；实际上第一个任务0按算法也能直接计算
            LinkedListNode<RcpspJob> curNode =  projJobs.First;
            while (curNode.Next != null) {
                //从后续任务中选出第i个任务，满足其紧前任务均已分配，并且紧后任务最多
                LinkedListNode<RcpspJob> recusiveNode = curNode.Next;
                LinkedListNode<RcpspJob> selectNode = null;
                while (recusiveNode!=null)
                {
                    if (!feasibleByPred(projJobs, curNode, recusiveNode.Value))
                        ;
                    else if (selectNode == null)
                        selectNode = recusiveNode;
                    else if (selectNode.Value.successors.Count < recusiveNode.Value.successors.Count)
                        selectNode = recusiveNode;
                    
                    recusiveNode = recusiveNode.Next;
                }
                //如果不为空，将其拿出队列插到当前位置
                if (selectNode != null)
                {
                    projJobs.Remove(selectNode);
                    projJobs.AddAfter(curNode, selectNode.Value);
                }

                //当前已经确定，可以进行下一个插入
                curNode = curNode.Next;
            }
            return calScheduleCore(projJobs, withLambda);
        }
        /// <summary>
        /// 检查当前任务的紧前任务是否在队列指定位置之前
        /// </summary>
        /// <param name="projJobs"></param>
        /// <param name="index"></param>
        /// <param name="curJob"></param>
        /// <returns></returns>
        public static bool feasibleByPred(LinkedList<RcpspJob> projJobs, LinkedListNode<RcpspJob> lastNode, RcpspJob curJob)
        {
            HashSet<RcpspJob> preds = curJob.predecessors;
            bool allFinded = true;
            foreach(RcpspJob job in preds){
                bool finded = false;
                LinkedListNode<RcpspJob> recursiveNode = lastNode;
                while (recursiveNode!= null) {
                    if (recursiveNode.Value == job)
                        finded = true;
                    recursiveNode = recursiveNode.Previous;
                }
                if (!finded) {
                    allFinded = false;
                    break;
                }
            }
            return allFinded;
        }

        public static bool checkFeasiable(LinkedList<RcpspJob> projJobs) {
            LinkedListNode<RcpspJob> recusiveNode = projJobs.First;
            while (recusiveNode != null) {
                foreach (RcpspJob predJob in recusiveNode.Value.predecessors) {
                    //检测紧前任务是否都在之前
				    bool hasExist = false;
                    LinkedListNode<RcpspJob> preRecNode = recusiveNode.Previous;
                    while (preRecNode != null) 
                    {
                        if (preRecNode.Value == predJob) {
                            hasExist = true;
                        }
                        preRecNode = preRecNode.Previous;
                    }
                    //只要有一个发现不在就返回错误
                    if (!hasExist)
                        return false;
                }
                recusiveNode = recusiveNode.Next;
            }
            return true;
        }
        /// <summary>
        /// 计算当前任务排列的总得分（最大完工时间），并且会计算任务列表中每一个任务的开始时间
        /// </summary>
        /// <param name="projJobs"></param>
        /// <returns></returns>
        public static double calScheduleCore(LinkedList<RcpspJob> projJobs,bool withLambda)
        {

            //当前调度最大完工时间
            double totalLamda = 0.0;//未兼容最早节点或者最终节点为坞修任务的情况
            Dictionary<RcspspProject, double> projsFirstJob = new Dictionary<RcspspProject, double>();
            double totalMaxTime = -1;
            //时间安排逐个进行遍历
            LinkedListNode<RcpspJob> curNode =  projJobs.First;
            while (curNode != null) {
                RcpspJob curJob = curNode.Value;
                //通过获取当前任务的所有紧前任务的结束时间，取其中最大，确定一个紧前最早开工时间
                HashSet<RcpspJob> preds = curJob.predecessors;
                double predEarliestStartTime = 0;
                foreach (RcpspJob job in preds) {
                    double endTime = job.startTime+job.duration;
                    if (predEarliestStartTime < endTime)
                        predEarliestStartTime = endTime;
                }
                //获取其需要的所有资源
                IDictionary<RcpspResource,int> resourceDemands = curJob.resourcesDemand;
                //计算资源占用时，以当前任务资源满足并且在持续时间duration内都满足，视为该任务可以安排在此时间
                int loadTime = 0;
                double resEarliestStartTime = predEarliestStartTime;
                while (loadTime <= curJob.duration) {
                    //当前时间点
                    double currentTime = resEarliestStartTime+loadTime;
                    bool isUpLimit = false;
                    //查找该时间点在执行的任务对该任务资源消耗的需求
                    foreach (RcpspResource resource in resourceDemands.Keys) {
                        //if (resourceDemands[resource] <= 0)continue;//资源需求为0的情况，此种在处理时就剔除了
                        int resourceUsed = resourceDemands[resource];
                        //从后往前遍历整个list，查看时间交集的任务
                        LinkedListNode<RcpspJob> recursiveNode = curNode.Previous;
                        while (recursiveNode != null) {
                            if (!recursiveNode.Value.resourcesDemand.ContainsKey(resource))
                            {//资源无交集，继续往前遍历
                            }
                            else if (currentTime >= recursiveNode.Value.startTime && currentTime < recursiveNode.Value.startTime + recursiveNode.Value.duration)
                            {//判断该任务时候有交集，大于等于开始时间，小于开始时间+持续时间

                                //累加资源消耗
                                resourceUsed += recursiveNode.Value.resourcesDemand[resource];
                                if (resourceUsed > resource.max_capacity)
                                {
                                    //当此时已经超了，直接跳出
                                    isUpLimit = true;
                                    break;
                                }
                            }
                            else 
                            { 
                                //资源有交集但是时间不交叉
                            }
                            //继续往前遍历
                            recursiveNode = recursiveNode.Previous;
                        }
                        //只要有一个资源超上限，都直接退出
                        if (isUpLimit)
                            break;
                    }
                    if (isUpLimit)
                    {//资源超总数，时间后移，并且累积时间归零
                        resEarliestStartTime++;
                        loadTime = 0;
                    }
                    else
                    { //资源总数不超，时间暂时不变，累积时间递增
                        loadTime++;
                    }
                }
                //设置当前项目的最早开始时间
                curJob.startTime = resEarliestStartTime;
                if (withLambda&&curJob.sourceProj!=null){
                	if(projsFirstJob.Keys.Contains(curJob.sourceProj)){
                		projsFirstJob[curJob.sourceProj] = curJob.startTime<projsFirstJob[curJob.sourceProj]?curJob.startTime:projsFirstJob[curJob.sourceProj];
                	}else
                    	projsFirstJob[curJob.sourceProj] = curJob.startTime;
                }
                if (withLambda && curJob.isLast && curJob.sourceProj !=null)//最后一个任务一定在最早一个任务的额后边出现
                {
                    totalLamda += curJob.sourceProj.Lambda * (curJob.startTime + curJob.duration - projsFirstJob[curJob.sourceProj]);
                }
                //保存一个全任务最大完工时间，也可以通过遍历任务队列的开始时间获得
                if (totalMaxTime < resEarliestStartTime + curJob.duration)
                    totalMaxTime = resEarliestStartTime + curJob.duration;

                //当前处理结束，进行下一个循环,计算队列下一个节点
                curNode = curNode.Next;
            }
            if (withLambda)
                return totalLamda;
            return totalMaxTime; 
        }
    }
}
