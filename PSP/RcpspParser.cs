using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PSP
{
    /// <summary>
    /// 资源受限的项目调度项目类，
    /// </summary>
    class RcpspParser
    {
        private LinkedList<RcpspJob> taskList = new LinkedList<RcpspJob>();
        public static void generateFromFile(string fileName) {
            fileName = "G:\\PSPFile\\j1206_1.sm";
            if (fileName.EndsWith(".sm")) {
                RcpspSolver sovlver = new RcpspSolver();
                generateFromSMFile(fileName,sovlver);
            }
            StreamReader fileSR = new StreamReader(fileName);
            string readLineStr = null;
            
            while ((readLineStr = fileSR.ReadLine()) != null)
            {
                Regex regex = new Regex(@"[-+]?\d*(\.(?=\d))?\d+");
                Console.WriteLine(readLineStr);
                MatchCollection match1 = regex.Matches(readLineStr);
                foreach (Match g in match1)
                {
                    Console.WriteLine(g.Value);
                }
                readLineStr.Substring(0);
            }
        }
        public static RcpspSolver parseMultiProjFile(string fileName) {
            RcpspSolver solver = new RcpspSolver();
            if (!fileName.EndsWith(".mp")) {
                solver.IsMultiProject = false;
            }
            solver.IsMultiProject = true;
            string directoryPath = fileName.Substring(0, fileName.LastIndexOf("\\"));
            StreamReader fileSR = new StreamReader(fileName);
            string readLineStr = null;
            int lineNumber = 0;
            int projNum = 0;
            int projIndex = 0;
            int resNum = 0;
            int perProjLine = 1;
            Regex numRegex = new Regex(@"[-+]?\d*(\.(?=\d))?\d+");
            while ((readLineStr = fileSR.ReadLine()) != null)
            {
                if (lineNumber == 0)
                {
                    //第0行总吨位
                    solver.TotalWuWeight = Convert.ToInt32(numRegex.Match(readLineStr).Value);
                }
                else if (lineNumber == 1)
                {
                    //资源数量
                    resNum = Convert.ToInt32(numRegex.Match(readLineStr).Value);
                }
                else if (lineNumber == 2)
                {
                    //资源详情
                    MatchCollection match1 = numRegex.Matches(readLineStr);
                    foreach (Match g in match1)
                    {
                        RcpspResource res = new RcpspResource();
                        res.renewable = true;
                        res.max_capacity = Convert.ToInt32(g.Value);
                        solver.ResourceList.AddLast(res);
                        resNum--;
                        if (resNum <= 0)
                            break;
                    }
                }
                else if (lineNumber == 3)
                {
                    //第1行项目数量
                    projNum = Convert.ToInt32(numRegex.Match(readLineStr).Value);
                }
                else if (lineNumber > 3 || lineNumber < (3 + projNum* perProjLine))//
                {
                    //依次为文件，每个文件每个文件解析
                    string smFileName = readLineStr.Trim(new char[] { ' ', '\t', '\r' });
                    RcspspProject proj =  generateFromSMFile(directoryPath + "\\" + smFileName,solver);
                    proj.ProjectId = Convert.ToString(projIndex + 1);
                    solver.ProjectList.AddLast(proj);
                    projIndex++;
                }

                lineNumber++;
            }
            return solver;
        }
        /// <summary>
        /// 从从*.sm文件构造Project，添加一个已经构造的solver构造多项目
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="solver"></param>
        /// <returns></returns>
        public static RcspspProject generateFromSMFile(string fileName, RcpspSolver solver) 
        {
            RcspspProject proj = new RcspspProject();
            LinkedList<RcpspResource> resourceList = solver.IsMultiProject ? solver.ResourceList : proj.Resources;
            proj.Resources = solver.ResourceList;
            LinkedList<RcpspJob> jobList = proj.Jobs;
            StreamReader fileSR = new StreamReader(fileName);
            string readLineStr = null;
            LoadStatus loadState = LoadStatus.HEADER_SECTION;
            long lineNumber = 0;
            //获取数字的正则表达式，包[-]2[.32]
            Regex numRegex = new Regex(@"[-+]?\d*(\.(?=\d))?\d+");
            while ((readLineStr = fileSR.ReadLine()) != null)
            {
                lineNumber++;
                readLineStr = readLineStr.TrimStart(new char[]{' ','\t','\r' });
                if (readLineStr.StartsWith("**") || readLineStr.StartsWith("--"))
                    continue;
                switch (loadState) {
                    case LoadStatus.HEADER_SECTION: {
                            if (readLineStr.StartsWith("file"))
                            {
                                proj.BaseFile = readLineStr.Substring(readLineStr.IndexOf(":")).Trim();//项目baseFile
                            }
                            else if (readLineStr.StartsWith("init"))
                            {
                                string numberStr = numRegex.Match(readLineStr).Value;
                                proj.RandomSeed = Convert.ToInt32(numberStr);//
                            }
                            else if (readLineStr.StartsWith("projects")|| readLineStr.StartsWith("jobs")) {
                                loadState = LoadStatus.PROJECT_SECTION;
                                goto case LoadStatus.PROJECT_SECTION;
                            } else {
                                throw new Exception("File Format error at line " + lineNumber + "!");
                            }
                        } break;
                    case LoadStatus.PROJECT_SECTION: {
                            if (readLineStr.StartsWith("projects"))
                            {
                                string numberStr = numRegex.Match(readLineStr).Value;//项目编号
                                proj.ProjectId = numberStr;
                            }
                            else if (readLineStr.StartsWith("jobs"))
                            {
                                int indexOfM = readLineStr.IndexOf(":");
                                string numberStr = numRegex.Match(readLineStr.Substring(indexOfM)).Value;
                                //获取项目内任务数量
                                proj.TaskNumber = Convert.ToInt32(numberStr);
                                int index = 0;
                                while (index < proj.TaskNumber)
                                {

                                    RcpspJob job = new RcpspJob();
                                    job.project = proj.ProjectId;
                                    job.id = Convert.ToString(index + 1);
                                    
                                    if (index == 0 || index == proj.TaskNumber - 1)
                                        job.isVirtual = true;
                                    if (index == 1)
                                        job.isFirst = true;
                                    if (index == proj.TaskNumber - 2)
                                        job.isLast = true;
                                    jobList.AddLast(job);
                                    index++;
                                }
                            }
                            else if (readLineStr.StartsWith("horizon"))
                            {
                                string numberStr = numRegex.Match(readLineStr).Value;
                                proj.Horizon = Convert.ToInt32(numberStr);
                            }
                            else if (readLineStr.StartsWith("RESOURCES"))
                            {
                            }
                            else if (readLineStr.StartsWith("weight"))
                            {
                                proj.Weight = Convert.ToInt32(numRegex.Match(readLineStr).Value);
                            }
                            else if (readLineStr.StartsWith("lambda"))
                            {
                                proj.Lambda = Convert.ToDouble(numRegex.Match(readLineStr).Value);
                            }
                            else if (readLineStr.Contains("- renewable"))
                            {
                                string numberStr = numRegex.Match(readLineStr).Value;
                                proj.RenewableResourceNumber = Convert.ToInt32(numberStr);
                                int index = proj.RenewableResourceNumber;
                                if (!solver.IsMultiProject)
                                {
                                    //不处理
                                    while (index > 0)
                                    {
                                        RcpspResource res = new RcpspResource();
                                        res.renewable = true;
                                        solver.ResourceList.AddLast(res);
                                        index--;
                                    }
                                }

                            }
                            else if (readLineStr.Contains("- nonrenewable"))
                            {
                                string numberStr = numRegex.Match(readLineStr).Value;
                                proj.NonrenewableResourceNumber = Convert.ToInt32(numberStr);
                                //int index = this._nonrenewableResourceNumber;
                                //while (index < 0)
                                //{
                                //    RcpspResource res = new RcpspResource();
                                //    res.renewable = false;
                                //    solver.ResourceList.AddLast(res);
                                //    index--;
                                //}
                            }
                            else if (readLineStr.Contains("- doubly constrained"))
                            {
                                string numberStr = numRegex.Match(readLineStr).Value;
                                proj.DoubleConstResourceNumber = Convert.ToInt32(numberStr);
                            }
                            else if (readLineStr.StartsWith("PROJECT INFORMATION"))
                            {
                                loadState = LoadStatus.PROJECT_INFO_SECTION;
                                goto case LoadStatus.PROJECT_INFO_SECTION;
                            }
                            else if (readLineStr.StartsWith("PRECEDENCE"))
                            {
                                loadState = LoadStatus.PRECEDENCE_SECTION;
                                goto case LoadStatus.PRECEDENCE_SECTION;
                            }
                            else if (readLineStr.StartsWith("REQUESTS/DURATIONS"))
                            {
                                loadState = LoadStatus.REQUEST_SECTION;
                                goto case LoadStatus.REQUEST_SECTION;
                            }
                            else if (readLineStr.StartsWith("RESOURCEAVAILABILITIES"))
                            {
                                loadState = LoadStatus.RESOURCE_SECTION;
                                goto case LoadStatus.RESOURCE_SECTION;
                            }
                            else
                            {
                                throw new Exception("File Format error at line " + lineNumber + "!");
                            }
                        } break;

                    case LoadStatus.PROJECT_INFO_SECTION: {
                            if (readLineStr.StartsWith("pronr."))
                            {
                            }
                            else if (readLineStr.StartsWith("PRECEDENCE"))
                            {
                                loadState = LoadStatus.PRECEDENCE_SECTION;
                                goto case LoadStatus.PRECEDENCE_SECTION;
                            }
                            else if (readLineStr.StartsWith("REQUESTS/DURATIONS"))
                            {
                                loadState = LoadStatus.REQUEST_SECTION;
                                goto case LoadStatus.REQUEST_SECTION;
                            }
                            else if (readLineStr.StartsWith("RESOURCEAVAILABILITIES"))
                            {
                                loadState = LoadStatus.RESOURCE_SECTION;
                                goto case LoadStatus.RESOURCE_SECTION;
                            } else if (readLineStr.StartsWith("PROJECT INFORMATION")) {
                            }
                            else
                            {
                                //MatchCollection matchColl = numRegex.Matches(readLineStr);
                                //int index = 0;
                                //foreach (Match g in matchColl)
                                //{
                                //    index++;
                                //    if (index == 0)
                                //    {
                                //        //项目号
                                //    }
                                //    else if (index == 1)
                                //    {
                                //        //任务数
                                //    }
                                //    else if (index == 2)
                                //    {
                                //        //rel.date
                                //    }
                                //    else if (index == 3)
                                //    {
                                //        //duedate
                                //    }
                                //    else if (index == 4)
                                //    {
                                //        //tardcost
                                //    }
                                //    else if (index == 5) {
                                //        //MPM-Time
                                //    }
                                //}
                            }
                        }
                        break;
                    case LoadStatus.PRECEDENCE_SECTION: {
                            if (readLineStr.StartsWith("REQUESTS/DURATIONS"))
                            {
                                loadState = LoadStatus.REQUEST_SECTION;
                                goto case LoadStatus.REQUEST_SECTION;
                            }
                            else if (readLineStr.StartsWith("RESOURCEAVAILABILITIES"))
                            {
                                loadState = LoadStatus.RESOURCE_SECTION;
                                goto case LoadStatus.RESOURCE_SECTION;
                            }
                            else if (readLineStr.StartsWith("jobnr."))
                            {
                                //
                            }
                            else if (readLineStr.StartsWith("PRECEDENCE"))
                            {
                                //
                            }
                            else
                            {
                                MatchCollection matchColl = numRegex.Matches(readLineStr);
                                int index = 0;
                                int numProd = 0;
                                RcpspJob job = null;
                                foreach (Match g in matchColl)
                                {
                                    int num = Convert.ToInt32(g.Value); ;
                                    if (index == 0)
                                    {
                                        //任务数
                                        job = jobList.ElementAt(num - 1);
                                        if (job == null)
                                        {
                                            throw new Exception("REQUESTS/DURATIONS not exist job nr. " + num);
                                        }
                                        index++;
                                    }
                                    else if (index == 1)
                                    {
                                        //mode
                                        if (job != null) job.isWX = num == 1 ? true : false;
                                        index++;
                                    }
                                    else if (index == 2)
                                    {
                                        //紧前任务数量
                                        index++;
                                        numProd = num;
                                        if (num == 0)
                                            break;
                                    }
                                    else if (index == 3)
                                    {
                                        RcpspJob predJob = jobList.ElementAt(num - 1);
                                        if (predJob == null) throw new Exception("Predecessor No. is error" + num);
                                        job.addPredecessor(predJob);
                                        numProd--;
                                        if (numProd <= 0)
                                        {
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        //if(numProd>0)throw new Exception("Predecessors num is not correct!");
                                    }
                                }
                            }
                        } break;
                    case LoadStatus.REQUEST_SECTION: {
                            if (readLineStr.StartsWith("jobnr.") || readLineStr.StartsWith("REQUESTS/DURATIONS"))
                            {
                            }
                            else if (readLineStr.StartsWith("RESOURCEAVAILABILITIES"))
                            {
                                loadState = LoadStatus.RESOURCE_SECTION;
                                goto case LoadStatus.RESOURCE_SECTION;
                            }
                            else if (readLineStr.StartsWith("PRECEDENCE")) {
                            } else
                            {
                                MatchCollection matchColl = numRegex.Matches(readLineStr);
                                int index = 0;
                                RcpspJob job = null;
                                foreach (Match g in matchColl)
                                {
                                    if (index == 0)
                                    {
                                        int num = Convert.ToInt32(g.Value);
                                        //任务数
                                        job = jobList.ElementAt(num - 1);
                                        if (job == null)
                                        {
                                            throw new Exception("REQUESTS/DURATIONS not exist job nr. " + num);
                                        }
                                        index++;
                                    }
                                    else if (index == 1)
                                    {
                                        int num = Convert.ToInt32(g.Value);
                                        //不处理
                                        index++;
                                    }
                                    else if (index == 2)
                                    {
                                        if (job.project == "5" && job.id == "2")
                                        {
                                            int idf = 0;
                                            idf = 82;
                                        }
                                        //duration
                                        double num = Convert.ToDouble(g.Value);
                                        index++;
                                        if (job != null) job.duration = num;
                                    }
                                    else {
                                        //不判断资源超界了
                                        int num = Convert.ToInt32(g.Value);
                                        if (num > 0) {
                                            RcpspResource res = resourceList.ElementAt(index - 3);
                                            if (res != null)
                                                job.addResourceDemand(res, num);
                                        }
                                        index++;

                                    }
                                }
                            }
                        } break;
                    case LoadStatus.RESOURCE_SECTION: {
                            if (readLineStr.StartsWith("PRECEDENCE"))
                            {
                                loadState = LoadStatus.PRECEDENCE_SECTION;
                                goto case LoadStatus.PRECEDENCE_SECTION;
                            }
                            else if (readLineStr.StartsWith("REQUESTS/DURATIONS"))
                            {
                                loadState = LoadStatus.REQUEST_SECTION;
                                goto case LoadStatus.REQUEST_SECTION;
                            }
                            else if (readLineStr.StartsWith("RESOURCEAVAILABILITIES")) {
                                //
                            }else
                            {
                                MatchCollection matchColl = numRegex.Matches(readLineStr);
                                if (!solver.IsMultiProject) {
                                    //
                                    int index = 0;
                                    foreach (Match g in matchColl)
                                    {
                                        index++;
                                        if (index == 0)
                                        {
                                            //任务数
                                        }
                                    }
                                }
                                
                            }
                        } break;
                    case LoadStatus.RESOURCE_MIN_SECTION: {

                        } break;
                    case LoadStatus.PARSING_FINISHED:break;
                    case LoadStatus.ERROR_FOUND:break;
                    default:break;
                }
            }
            //solver.ProjectList.AddLast(proj);
            return proj;
        }

        private enum LoadStatus
        {
            NOT_STARTED = 0,
            HEADER_SECTION,
            PROJECT_SECTION,
            PROJECT_INFO_SECTION,
            PRECEDENCE_SECTION,
            REQUEST_SECTION,
            RESOURCE_SECTION,
            RESOURCE_MIN_SECTION,
            PARSING_FINISHED,
            ERROR_FOUND
        };
    }
}
