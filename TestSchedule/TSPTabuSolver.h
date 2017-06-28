#pragma once
#include "map.h"
#include "solution.h"
class TSPTabuSolver
{
public:
	TSPTabuSolver();
	~TSPTabuSolver();
public:
	/*���ļ���ʼ����������*/
	TSPTabuSolver(string filePath);
	/*ִ��������*/
	void solve(int);
	/*��ȡ���ŵ��ھ�Solution*/
	Solution* getBestNearbySolution(int);
	void resetTabuList();
private:
	double bestSolverScore;
	/*���ɱ�ͣ�һ��N*N���飬NΪmap�Ĵ�С*/
	int** tabu_list;
	int** tabu_f_list;
	Map* map;
	Solution* s;
};

