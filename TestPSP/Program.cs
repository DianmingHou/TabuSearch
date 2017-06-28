/*
 * Created by SharpDevelop.
 * User: asus
 * Date: 2017/4/4
 * Time: 18:19
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using PSP;
namespace TestPSP
{
	class Program
	{
		class Edge {
            public RcpspJob from;
            public RcpspJob to;
            public Edge() {
                from = to = null;
            }
            public Edge(RcpspJob from,RcpspJob to) {
                this.from = from;
                this.to = to;
            }

        }
        public static void allMIT(RcpspSolver solve)
        {

            solve.generateAllPossiblePartation();
            string comb1 = "(1,2,5)";
            string comb2 = "(4)";
            string comb3 = "(3)";

            solve.EveryCombBestProj[comb1] = RcpspSolver.calCombBestScoreByTabuSearch(comb1, solve.EveryComb[comb1],true);
            solve.EveryCombBestProj[comb2] = RcpspSolver.calCombBestScoreByTabuSearch(comb2, solve.EveryComb[comb2], true);
            solve.EveryCombBestProj[comb3] = RcpspSolver.calCombBestScoreByTabuSearch(comb3, solve.EveryComb[comb3], true);
            foreach (string str in solve.EveryCombBestProj.Keys)
            {
                RcspspProject proj = solve.EveryCombBestProj[str];
                Console.WriteLine("comb core " + str + "is " + proj.BestSocre + " and list is : ");
                Console.Write("             ");
                foreach (RcpspJob job in proj.Jobs)
                {
                    Console.Write("(" + job.id + "__" + job.project + "__" + job.duration + ")");
                }
                Console.WriteLine();
            }
            List<List<RcspspProject>> projectPartition = new List<List<RcspspProject>>();
            projectPartition.Add(solve.EveryComb[comb1]);
            projectPartition.Add(solve.EveryComb[comb2]);
            projectPartition.Add(solve.EveryComb[comb3]);
            RcspspProject bestproj = RcpspSolver.calPartitionBestScoreByTabuSearch(projectPartition, solve.EveryCombBestProj, true);
            Console.WriteLine("best core is " + bestproj.BestSocre + " and partition is " + bestproj.BestCombStr);
            foreach (RcpspJob job in bestproj.Jobs)
            {
                //Console.Write("[" + job.id + "__" + job.project + "__" + job.startTime+"__"+job.duration + "]");
                if (job.isWX)
                {
                    RcspspProject wxProject = solve.EveryCombBestProj[job.project];
                    foreach (RcpspJob wxJob in wxProject.Jobs)
                    {
                        addJob2Dic(solve.ProjectList, wxJob, job.startTime);
                    }
                }
                else
                {
                    addJob2Dic(solve.ProjectList, job);
                }
            }
            //TabuSearch.printGUI(bestproj.Jobs);
            Console.WriteLine("total time is " + TabuSearch.calScheduleCore(bestproj.Jobs, false));
            foreach (RcspspProject proj in solve.ProjectList)
            {
                Console.WriteLine("Project " + proj.ProjectId);
                foreach (RcpspJob projJob in proj.Jobs)
                {
                    Console.WriteLine(projJob.id + "\t" + projJob.startTime + "\t" + (projJob.startTime + projJob.duration));
                }
            }

        }
        static void Main(string[] args)
        {
            //Dictionary<string, int> did = new Dictionary<string, int>();
            //did.Add("sad", 11);
            //Console.WriteLine("d is " + did["sad"] + " and d1 is " + did["sad1"]);
            //int i = 1023;
            //int j = i / 100;
            //int k = i % 100;
            //Console.WriteLine("d is " + i / 100 + " and d1 is " + k);
            //string fileName = args[0];
            string fileName = "G:\\Projects\\VS2012\\算例\\000.mp";//args[0];
            RcpspSolver solve = new RcpspSolver();
            solve = RcpspParser.parseMultiProjFile(fileName);
            allMIT(solve);
            return;
            //solve.TotalWuWeight = 30;
            ////产生所有可行划分和所有划分组合
            solve.generateAllPossiblePartation();
            ////计算所有组合的最优时间和最优分组
            solve.calcAllCombMultiProcess();
            foreach(string str in solve.EveryCombBestProj.Keys)
            {
                RcspspProject proj = solve.EveryCombBestProj[str];
                Console.WriteLine("comb core "+ str + "is " + proj.BestSocre + " and list is : ");
                Console.Write("             ");
                foreach (RcpspJob job in proj.Jobs) {
                    Console.Write("(" + job.id + "__" + job.project + "__"+job.duration+")");
                }
                Console.WriteLine();
            }

            RcspspProject bestproj =  solve.calAllPartitionScore();
            
            Console.WriteLine("all project end time is  ");
            foreach (RcspspProject proj in solve.ProjectList) {
                if (proj.Jobs.Count > 1) {
                    RcpspJob lastJob =proj.Jobs.Last.Previous.Value; 
                    Console.Write("   " + (lastJob.startTime+lastJob.duration));
                }
            }
            Console.WriteLine();
            Console.WriteLine("best core is " + bestproj.BestSocre + " and partition is " + bestproj.BestCombStr);
            Console.Write("this List is : ");
            foreach (RcpspJob job in bestproj.Jobs)
            {
                //Console.Write("[" + job.id + "__" + job.project + "__" + job.startTime+"__"+job.duration + "]");
                if(job.isWX){
                	RcspspProject wxProject  = solve.EveryCombBestProj[job.project];
                	foreach(RcpspJob wxJob in wxProject.Jobs){
                		addJob2Dic(solve.ProjectList,wxJob,job.startTime);
                	}
                }else{
                	addJob2Dic(solve.ProjectList,job);
                }
            }
            //TabuSearch.printGUI(bestproj.Jobs);
            Console.WriteLine("total time is " + TabuSearch.calScheduleCore(bestproj.Jobs,true));
            foreach(RcspspProject proj in solve.ProjectList){
            	Console.WriteLine("Project "+proj.ProjectId);
            	foreach(RcpspJob projJob in proj.Jobs){
            		Console.WriteLine(projJob.id+"\t"+projJob.startTime+"\t"+(projJob.startTime+projJob.duration));
            	}
            }

            return;
        }
        public static void addJob2Dic(LinkedList<RcspspProject> projs,RcpspJob job,double additionTime=0){
        	if(job.project=="0")
        		return ;
        	//查找到所在的project
        	foreach(RcspspProject proj in projs){
        		if(job.project.Equals(proj.ProjectId)){
        			foreach(RcpspJob projJob in proj.Jobs){
        				if(job.id==projJob.id){
        					projJob.startTime =additionTime+job.startTime;
        					break;
        				}
        			}
        			break;
        		}
        	}
        }
        public static void generateRandomProj(RcspspProject spspProject)
        {
            Random rd = new Random();
            for (int i = 0; i < 5; i++)
            {
                RcpspResource resource = new RcpspResource();
                resource.max_capacity = rd.Next(1, 10);
                spspProject.Resources.AddLast(resource);
            }
            RcpspJob zeroJob = new RcpspJob();
            zeroJob.duration = 0;
            zeroJob.id = "0";
            spspProject.Jobs.AddLast(zeroJob);
            //初始Job，每一个job都有不允许建立的关系，即前置已经建立的关系，存放起来Dictionary<Job,Job>存在
            LinkedList<Edge> existEdges = new LinkedList<Edge>();
            int numberJobs = 5;
            int durationMax = 11;
            for (int j = 1; j < numberJobs + 1; j++)
            {
                RcpspJob job = new RcpspJob();
                job.id = Convert.ToString(j);
                //检查可以生成的边的数量
                int edgeCount = 0;
                LinkedListNode<RcpspJob> node = spspProject.Jobs.First;
                while (node != null)
                {
                    //检测是否不允许
                    bool isAllow = true;
                    //遍历列表
                    foreach (Edge ed in existEdges)
                    {
                        if (ed.from == job && ed.to == node.Value)
                        {
                            isAllow = false;
                            break;
                        }
                    }
                    if (!isAllow) continue;
                    //是否生成
                    if (rd.Next(0, 2) > 0)
                    {
                        edgeCount++;
                        job.addPredecessor(node.Value);
                        Edge edThis = new Edge();
                        edThis.from = job;
                        edThis.to = node.Value;
                        existEdges.AddLast(edThis);
                        //将前置的所有不允许加入当前的
                        LinkedListNode<Edge> lastNode = existEdges.Last;
                        while (lastNode != null)
                        {
                            Edge ed = lastNode.Value;
                            if (ed.from == node.Value)
                            {
                                Edge edNew = new Edge();
                                edNew.from = job;
                                edNew.to = ed.to;
                                existEdges.AddLast(edNew);
                            }
                            lastNode = lastNode.Previous;
                        }
                    }
                    node = node.Next;
                }
                if (edgeCount == 0)
                {
                    job.addPredecessor(spspProject.Jobs.First.Value);
                    existEdges.AddLast(new Edge(job, spspProject.Jobs.First.Value));
                }
                if (j == numberJobs - 1)
                {
                    job.duration = 0;
                }
                else
                {
                    job.duration = rd.Next(1, durationMax);
                    //随机资源使用
                    for (int i = 0; i < 5; i++)
                    {
                        RcpspResource resource = spspProject.Resources.ElementAt(i);
                        int resourceDemand = rd.Next(0, resource.max_capacity + 1);
                        if (resourceDemand > 0)
                        {
                            job.addResourceDemand(resource, resourceDemand);
                        }
                    }
                }


                spspProject.Jobs.AddLast(job);
            }
            //添加最后工序
            RcpspJob lastJob = new RcpspJob();
            lastJob.duration = 0;
            lastJob.id = Convert.ToString(numberJobs + 1);
            //所有没有紧后任务的工序都设一条到该点的边
            for (LinkedListNode<RcpspJob> recuNode = spspProject.Jobs.First; recuNode != null; recuNode = recuNode.Next)
            {
                if (recuNode.Value.successors.Count <= 0)
                    lastJob.addPredecessor(recuNode.Value);
            }
            spspProject.Jobs.AddLast(lastJob);
        }

        public static void generateProj(RcspspProject spspProject)
        {
            int[] resCap = {3,4,2,3,6};
            for (int i = 0; i < 5; i++)
            {
                RcpspResource resource = new RcpspResource();
                resource.max_capacity = resCap[i];
                spspProject.Resources.AddLast(resource);
            }
            int numberJobs = 22;
            int[] durations = { 0,10,8,20,4,8, 4,10,6,12,5, 22,13,4,10,8, 2,16,25,8,2,0 };
            int[,] resUsed = {
                                  {0,    0,    0 ,   0,		0},//0
                                  {2,    0,    2,    1,		0},//1
      {0,    1,    0,    2,		5},//2
      {1,    0,    2,    0,		0},//3
      {0,    0,    0,    3,		5},//4
     {0,    2,    0,    0,		2},//5
      {0,    2,    1,    0,		0},//6
      {2,    0,    2,    0,		0},//7
      {1,    0,    0,    2,		5},//8
      {0,    1,    0,    0,		2},//9
      {0,    0,    0,    3,		0},//10
      {1,    0,    0,    0,		0},//11
      {2,    2,    0,    0,		0},//12
      {0,    0,    2,    3,		0},//13
      {0,    0,    0,    1,		4},//14
     { 1,    0,    0,    1,		3},//15
      {0,    0,    0,    1,		2},//16
     { 2,    3,    0,    0,		0},//17
     { 1,    0,    2,    0,		0},//18
      {0,    2,    0,    3,		0},//19
     { 0,    3,    0,    0,		6},//20
      {0,    0,    0,    0, 	0}//21
                             };
            int [][]pp = new int[22][];
            pp[0] = new int[]{0};//1
		    pp[1] = new int[]{0};//2
		    pp[2] = new int[]{1};//3
		    pp[3] = new int[]{0};//4
		    pp[4] = new int[]{3};//5
		    pp[5] = new int[]{2,4};//6

		    pp[6] = new int[]{0};//7
		    pp[7] = new int[]{6};//8
		    pp[8] = new int[]{7};//9
		    pp[9] = new int[]{7};//10
		    pp[10] = new int[]{8,9};//11
    
		    pp[11] = new int[]{0};//12
		    pp[12] = new int[]{11};//13
		    pp[13] = new int[]{11};//14
		    pp[14] = new int[]{12,13};//15
		    pp[15] = new int[]{14};//16

		    pp[16] = new int[]{0};//17
		    pp[17] = new int[]{16};//18
		    pp[18] = new int[]{0};//19
		    pp[19] = new int[]{17,18};//20
		    pp[20] = new int[]{19};//21
            pp[21] = new int[]{5,10,15,20};//22

            for (int j = 0; j < numberJobs; j++)
            {
                RcpspJob job = new RcpspJob();
                job.id = Convert.ToString(j);
                job.duration = durations[j];
                for (int resN = 0; resN < 5; resN++) {
                    if (resUsed[j,resN] > 0) {
                        job.addResourceDemand(spspProject.Resources.ElementAt(resN), resUsed[j, resN]);
                    }
                }
                for (int preIndex = 0; preIndex < pp[j].Length; preIndex++) {
                    if (pp[j][preIndex] != j)
                        job.addPredecessor(spspProject.Jobs.ElementAt(pp[j][preIndex]));
                }
                spspProject.Jobs.AddLast(job);
            }
        }
    }
}