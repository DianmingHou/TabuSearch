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
	//���ļ���ȡMap
	map = new Map(filePath);
	//ͨ����Map����һ���������
	s = new Solution(map);
	//���ر����������double���ֵ
	bestSolverScore = std::numeric_limits<double>::max();
	//��ʼ�����ɱ�ͣ���ʼ��Ϊһ��N*N���飬NΪ
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
		//����󽻻���������ѭ������
		for (int i = 0; i < NUM_INTERATION; i++) {
			//��ȡ��õ��ھӽ��
			s = this->getBestNearbySolution(i);
			double score = s->getScore();
			if (score < bestSolverScore) {
				bestSolverScore = score;
				//�õ�һ�����Ž⣬����ѭ������Ϊ0
				countTime = 0;
				//����ȫ�����������ֵ
				if (bestSolverScore < bestSolutionScore) {
					for (int j = 0; j < map->numVertex; j++) {
						bestSolution.set(j, s->getV(j));
					}
					bestSolutionScore = bestSolverScore;
				}
			}
			else {
				countTime++;
				//������ڳ��Դ������˳�
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
