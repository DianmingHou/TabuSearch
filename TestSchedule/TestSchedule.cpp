// TestSchedule.cpp : �������̨Ӧ�ó������ڵ㡣
//
#include "stdafx.h"
#include <iostream>
#include "tsptabusolver.h"
using namespace std;
#define TASK_NUM 4
#define PER_PROCESS_NUM 5
#define RESOURCE_NUM 5
FILE * file = NULL;

#define max(a,b) (a)>(b)?(a):(b)
#define min(a,b) (a)<(b)?(a):(b)


int RC[5] = { 3,4,2,3,6 };
int PT[4][5] = {
	{ 10, 8,20, 4, 8 },
	{ 4,10, 6,12, 5 },
	{ 22,13, 4,10, 8 },
	{ 2,16,25, 8, 2 } 
};
int RR[4][5][5] = {
	{
		{ 2, 0, 2, 1, 0 },
		{ 0, 1, 0, 2, 5 },
		{ 1, 0, 2, 0, 0 },
		{ 0, 0, 0, 3, 5 },
		{ 0, 2, 0, 0, 2 }
	},
	{
		{ 0, 2, 1, 0, 0 },
		{ 2, 0, 2, 0, 0 },
		{ 1, 0, 0, 2, 5 },
		{ 0, 1, 0, 0, 2 },
		{ 0, 0, 0, 3, 0 }
	},
	{
		{ 1, 0, 0, 0, 0 },
		{ 2, 2, 0, 0, 0 },
		{ 0, 0, 2, 3, 0 },
		{ 0, 0, 0, 1, 4 },
		{ 1, 0, 0, 1, 3 }
	},
	{
		{ 0, 0, 0, 1, 2 },
		{ 2, 3, 0, 0, 0 },
		{ 1, 0, 2, 0, 0 },
		{ 0, 2, 0, 3, 0 },
		{ 0, 3, 0, 0, 6 }
	}
};
int PP[4][5][5] = {
	{
		{ 0, 0, 0, 0, 0 },
		{ 1, 0, 0, 0, 0 },
		{ 0, 0, 0, 0, 0 },
		{ 0, 0, 1, 0, 0 },
		{ 1, 1, 1, 1, 0 }
	},
	{
		{ 0, 0, 0, 0, 0 },
		{ 1, 0, 0, 0, 0 },
		{ 1, 1, 0, 0, 0 },
		{ 1, 1, 0, 0, 0 },
		{ 1, 1, 1, 1, 0 }
	},
	{
		{ 0, 0, 0, 0, 0 },
		{ 1, 0, 0, 0, 0 },
		{ 1, 0, 0, 0, 0 },
		{ 1, 1, 1, 0, 0 },
		{ 1, 1, 1, 1, 0 }
	},
	{
		{ 0, 0, 0, 0, 0 },
		{ 1, 0, 0, 0, 0 },
		{ 0, 0, 0, 0, 0 },
		{ 1, 1, 1, 0, 0 },
		{ 1, 1, 1, 1, 0 }
	}
};

bool checkFeasiable(int salcArray[], int size);
int calPlanTime(int salcArray[], int size,bool printed=false);
int getBestNearbySolution(int array[PER_PROCESS_NUM*TASK_NUM], int it, int tabu_list[][PER_PROCESS_NUM*TASK_NUM], int tabu_f_list[][PER_PROCESS_NUM*TASK_NUM]);

//�����㷨��س���
#define TABU_LENGTH 30
#define NUM_INTERATION 3000
#define PENAL_LONG_TERM 10
#define LONG_TERM_LENGTH 100
#define TIME_TRY 500

int DT[4][5] = { -1 };
//Future paths for integer programming and links to artificial intelligence
int scheduleArray[] = { 0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19 };
/*
numCandidate: ����������ÿ�ֶ��������㷨����һ��
*/
int tabuSearch(int numCandidate) {
	int bestSolutionScore = INT_MAX;
	int bestSolutionArray[PER_PROCESS_NUM*TASK_NUM]= { 0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19 };
	for (int loopCount = 0; loopCount < numCandidate; loopCount++) {
		//����һ����ʼ���кͳ�ʼ��
		int initArray[PER_PROCESS_NUM*TASK_NUM] = { 0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19 };
		int bestSolverScore = calPlanTime(initArray, PER_PROCESS_NUM*TASK_NUM);//��ֵΪ��ʼ��
		//��ʼ�����ɱ�
		int tabu_list[PER_PROCESS_NUM*TASK_NUM][PER_PROCESS_NUM*TASK_NUM] = { 0 };
		int tabu_f_list[PER_PROCESS_NUM*TASK_NUM][PER_PROCESS_NUM*TASK_NUM] = { 0 };
		//һֱ�ò���һ�����Ż���ʱ��ѭ������
		int countTime = 0;
		//����󽻻���������ѭ������
		for (int interationCount = 0; interationCount < NUM_INTERATION; interationCount++) {
			//��ȡ�������Ž�
			int nearbyScore = getBestNearbySolution(initArray, interationCount, tabu_list, tabu_f_list);
			fprintf(file,"�������Ž⣺%5d ��ǰ���Ž�%5d\n", nearbyScore, bestSolverScore);
			if (nearbyScore < bestSolverScore) {
				bestSolverScore = nearbyScore;
				//�õ�һ�����Ž⣬����ѭ������Ϊ0
				countTime = 0;
				//����ȫ�����������ֵ
				if (bestSolverScore < bestSolutionScore) {
					//��ֵΪ��������
					bestSolutionScore = bestSolverScore;
					for (int copyIndex = 0; copyIndex < PER_PROCESS_NUM*TASK_NUM; copyIndex++)
						bestSolutionArray[copyIndex] = initArray[copyIndex];
				}
			}
			else {
				countTime++;
				//������ڳ��Դ������˳�
				if (countTime > TIME_TRY) break;
			}

		}

	}
	calPlanTime(bestSolutionArray, PER_PROCESS_NUM*TASK_NUM,true);
	return 0;

}

void swapSolve(int *array, int i, int j) {
	int tmp = array[i];
	array[i] = array[j];
	array[j] = tmp;
}

int getBestNearbySolution(int array[PER_PROCESS_NUM*TASK_NUM], int it, int tabu_list[][PER_PROCESS_NUM*TASK_NUM], int tabu_f_list[][PER_PROCESS_NUM*TASK_NUM]) {
	int bestTime = INT_MAX;
	int vertexA = 0;
	int vertexB = 1;
	for (int i = 0; i < PER_PROCESS_NUM*TASK_NUM; i++) {
		int df = i;
		for (int j = (i + 1); j < PER_PROCESS_NUM*TASK_NUM; j++) {
			//swap for new solution
			swapSolve(array, i, j);
			bool cheched = checkFeasiable(array, PER_PROCESS_NUM*TASK_NUM);
			//printf("swap %d,%d check result = %s\n", array[j],array[i],(cheched ? "OK" : "FAILED"));
			if (cheched) {
				int curentMaxTime = calPlanTime(array, PER_PROCESS_NUM*TASK_NUM);
				//�ͷ�����
				int penalScore = curentMaxTime + PENAL_LONG_TERM * tabu_f_list[i][j];
				if ((bestTime > penalScore && tabu_list[i][j] <= it) || curentMaxTime < bestTime) {
					vertexA = i;
					vertexB = j;
					bestTime = penalScore;
					tabu_list[i][j] = (it + TABU_LENGTH);
					tabu_list[j][i] = (it + TABU_LENGTH);
				}
			}
			// back to orginal solution
			swapSolve(array, j, i);
			if (tabu_f_list[i][j] > 0 && it > LONG_TERM_LENGTH) tabu_f_list[i][j] -= 1;
		}
	}
	tabu_f_list[vertexA][vertexB] += 2;
	swapSolve(array, vertexA, vertexB);
	return bestTime;
}

/*
�����������������������������ʱ�䣬������������ʱ��
salcArray: �����ǰ��ϵ����������
size: salcArray�ĳ��ȣ��˴�Ϊ20
*/

int calPlanTime(int salcArray[], int size,bool printed) {
	for (int i = 0; i<4; i++)
	for (int j = 0; j<PER_PROCESS_NUM; j++)DT[i][j] = -1;
	int totalMax = -1;
	for (int i = 0; i < size; i++) {
		int task = salcArray[i];
		int minTime = 0;
		int *preProArray = PP[task / PER_PROCESS_NUM][task % PER_PROCESS_NUM];
		//�����ǰ��������ʱ��
		for (int j = 0; j < PER_PROCESS_NUM; j++) {
			if (PP[task / PER_PROCESS_NUM][task % PER_PROCESS_NUM][j] == 1)
				minTime = max(minTime, DT[task / PER_PROCESS_NUM][j] + 1);
		}
		int continueTime = PT[task / PER_PROCESS_NUM][task % PER_PROCESS_NUM];
		int *thisRR = RR[task / PER_PROCESS_NUM][task % PER_PROCESS_NUM];
		int resEalestTime = minTime;//������Դ���ļ�������翪ʼʱ��
		int loadTime = 0;//�洢�ۻ���Դ����ʱ�䣬�ڵ�������ʱʱ�˳�ѭ��
		for (; loadTime < continueTime;) {
			//��ʱ����Դ����
			int resC[RESOURCE_NUM] = { thisRR[0],thisRR[1],thisRR[2],thisRR[3],thisRR[4] };
			//�������и�ʱ��㿪��������
			bool isuploadThisTime = false;//��Դ���������Ƿ�
			//Ҫ�ŵ��������֮ǰ������һ����������
			for (int preIndex = 0; preIndex<i; preIndex++) {
				int curPreTask = salcArray[preIndex];
				//��ǰ������Ҫ���ǽ���
				//if(PP[]) 
				if (DT[curPreTask / PER_PROCESS_NUM][curPreTask%PER_PROCESS_NUM] - PT[curPreTask / PER_PROCESS_NUM][curPreTask%PER_PROCESS_NUM] <= resEalestTime&&DT[curPreTask / PER_PROCESS_NUM][curPreTask%PER_PROCESS_NUM] >= resEalestTime) {
					//�����������Դ���ļӵ�resC
					for (int resI = 0; resI<RESOURCE_NUM; resI++) {
						resC[resI] += RR[curPreTask / PER_PROCESS_NUM][curPreTask%PER_PROCESS_NUM][resI];
						if (resC[resI]>RC[resI]) {
							isuploadThisTime = true;//��Դ���������Ѿ���
							break;
						}
					}
				}
				if (isuploadThisTime)break;
			}
			if (isuploadThisTime) {//��Դ��������ʱ����ƣ������ۻ�ʱ�����
				resEalestTime++;
				loadTime = 0;
			}
			else {//��Դ����������ʱ����ʱ���䣬�ۻ�ʱ�����
				loadTime++;
			}
		}
		int finalTime = max(minTime, resEalestTime);
		DT[task / PER_PROCESS_NUM][task % PER_PROCESS_NUM] = finalTime + PT[task / PER_PROCESS_NUM][task % PER_PROCESS_NUM];
		totalMax = max(totalMax, finalTime + PT[task / PER_PROCESS_NUM][task % PER_PROCESS_NUM]);
		if(printed)fprintf(file,"work %d,%d  starttime=%d , endtime is %d\n", task / PER_PROCESS_NUM + 1, task%PER_PROCESS_NUM + 1, finalTime, finalTime + PT[task / PER_PROCESS_NUM][task % PER_PROCESS_NUM]);
	}
	return totalMax;

}

/**
* ����ǰ��ϵ�����������������е���Ч��
**/
bool checkFeasiable(int salcArray[], int size) {
	for (int i = 0; i < size; i++) {
		int task = salcArray[i];
		int minTime = 0;
		int *preProArray = PP[task / PER_PROCESS_NUM][task % PER_PROCESS_NUM];
		for (int j = 0; j < PER_PROCESS_NUM; j++) {
			if (PP[task / PER_PROCESS_NUM][task % PER_PROCESS_NUM][j] == 1) {
				//����ǰ�����Ƿ���֮ǰ
				bool hasExist = false;
				for (int checkI = 0; checkI<i; checkI++) {
					int checkTask = salcArray[checkI];
					if (task / PER_PROCESS_NUM == checkTask / PER_PROCESS_NUM&&j == checkI% PER_PROCESS_NUM)
						hasExist = true;
				}
				if (hasExist == false)
					return false;
			}
		}
	}
	return true;
}
#include <list>
#include <set>
#include <algorithm>
#include <vector>
using namespace std;
 list<list<list<int>>> calAllPosible(list<int> allProjs){
	 list<list<list<int>>> allComb;  
	list<list<int>> all;
	//partition(
	all.push_back(allProjs);
	allComb.push_back(all);
	int indexs = allProjs.size();
	while(true){
		int i,index;
		for(i=indexs-1;;--i){
			if(i<=0)
				break;
		}

	};
	return allComb;
}
 int partitioncount(std::vector<int> & v, int n, int m)  
{  
    if(n<m || n<1 || m<1)  
        return 0;  
    if(m==1)  
        return 1;  
    return partitioncount(v, n-1, m-1) + m * partitioncount(v, n-1, m);  
}  
int main()
{
	//bool cheched = checkFeasiable(scheduleArray, 20);
	//printf("check result = %s\n", cheched ? "OK" : "FAILED");
	//calPlanTime(scheduleArray, 20,true);
	file = fopen("D:\\out.txt", "w+");
	vector<int> nums;
	nums.push_back(1);
	nums.push_back(2);
	nums.push_back(3);
	nums.push_back(4);
	nums.push_back(5);
	tabuSearch(1);

	fclose(file);

	//TSPTabuSolver solver2("C:\\Users\\houdianming.DLGONA\\Documents\\visual studio 2015\\Projects\\TestSchedule\\Debug\\tsp0.txt");
	//solver2.solve(6);
	///TSPTabuSolver solver1("C:\\Users\\houdianming.DLGONA\\Documents\\visual studio 2015\\Projects\\TestSchedule\\Debug\\tsp1.txt");
	//solver1.solve(5);
	//TSPTabuSolver solver3("C:\\Users\\houdianming.DLGONA\\Documents\\visual studio 2015\\Projects\\TestSchedule\\Debug\\tsp2.txt");
	//solver3.solve(7);
	return 0;

}