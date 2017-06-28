#pragma once
#include "map.h"
#include "solution.h"
class TSPTabuSolver
{
public:
	TSPTabuSolver();
	~TSPTabuSolver();
public:
	/*从文件初始化禁忌搜索*/
	TSPTabuSolver(string filePath);
	/*执行求解操作*/
	void solve(int);
	/*获取最优的邻居Solution*/
	Solution* getBestNearbySolution(int);
	void resetTabuList();
private:
	double bestSolverScore;
	/*禁忌表和，一个N*N数组，N为map的大小*/
	int** tabu_list;
	int** tabu_f_list;
	Map* map;
	Solution* s;
};

