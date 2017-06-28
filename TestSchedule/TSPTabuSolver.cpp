#include "stdafx.h"
#include "TSPTabuSolver.h"
#include <string>
#include <iostream>

using namespace std;
#define TABU_LENGTH 30
#define NUM_INTERATION 3000
#define PENAL_LONG_TERM 10
#define LONG_TERM_LENGTH 100
#define TIME_TRY 500

TSPTabuSolver::TSPTabuSolver(string filePath) {
	//从文件获取Map
	map = new Map(filePath);
	//通过该Map构造一个解决方案
	s = new Solution(map);
	//返回编译器允许的double最大值
	bestSolverScore = std::numeric_limits<double>::max();
	//初始化禁忌表和，初始化为一个N*N数组，N为
	tabu_list = new int*[map->numVertex];
	tabu_f_list = new int*[map->numVertex];
	for (int i = 0; i < map->numVertex; i++) {
		tabu_f_list[i] = new int[map->numVertex];
		tabu_list[i] = new int[map->numVertex];
	}

	resetTabuList();
}

void TSPTabuSolver::resetTabuList() {
	for (int i = 0; i < map->numVertex; i++) {
		for (int j = 0; j < map->numVertex; j++) {
			tabu_list[i][j] = 0;
			tabu_f_list[i][j] = 0;
		}
	}
}

/*
numCandidate : times that solver run to get the best score
*/
void TSPTabuSolver::solve(int numCandidate) {
	Solution bestSolution(map);
	double bestSolutionScore = bestSolution.getScore();

	for (int loopCount = 0; loopCount < numCandidate; loopCount++) {
		s->initSolution();
		resetTabuList();
		//cout << "Init Score : " << s->getScore() << endl;
		int countTime = 0;
		bestSolverScore = std::numeric_limits<double>::max();
		//按最大交互次数进行循环运算
		for (int i = 0; i < NUM_INTERATION; i++) {
			//获取最好的邻居结果
			s = this->getBestNearbySolution(i);
			double score = s->getScore();
			if (score < bestSolverScore) {
				bestSolverScore = score;
				//得到一个更优解，设置循环计数为0
				countTime = 0;
				//更新全局最优排序和值
				if (bestSolverScore < bestSolutionScore) {
					for (int j = 0; j < map->numVertex; j++) {
						bestSolution.set(j, s->getV(j));
					}
					bestSolutionScore = bestSolverScore;
				}
			}
			else {
				countTime++;
				//如果大于尝试次数，退出
				if (countTime > TIME_TRY) {
					break;
				}
			}
		}

	}
	cout << "Best score : " << bestSolutionScore << endl;
	bestSolution.printPath();
}

Solution* TSPTabuSolver::getBestNearbySolution(int it) {
	double bestScore = std::numeric_limits<double>::max();;
	int vertexA = 0;
	int vertexB = 1;
	for (int i = 0; i < map->numVertex; i++) {
		for (int j = (i + 1); j < map->numVertex; j++) {
			//swap for new solution
			s->swapSolve(i, j);
			double currentScore = s->getScore();
			double penalScore = currentScore + PENAL_LONG_TERM * tabu_f_list[i][j];
			if ((bestScore > penalScore && this->tabu_list[i][j] <= it) || currentScore < bestSolverScore) {
				vertexA = i;
				vertexB = j;
				bestScore = penalScore;
				this->tabu_list[i][j] = (it + TABU_LENGTH);
				this->tabu_list[j][i] = (it + TABU_LENGTH);
			}
			// back to orginal solution
			s->swapSolve(j, i);
			if (tabu_f_list[i][j] > 0 && it > LONG_TERM_LENGTH) tabu_f_list[i][j] -= 1;
		}
	}
	tabu_f_list[vertexA][vertexB] += 2;
	s->swapSolve(vertexA, vertexB);
	return s;
}


TSPTabuSolver::TSPTabuSolver()
{
}


TSPTabuSolver::~TSPTabuSolver()
{
}
