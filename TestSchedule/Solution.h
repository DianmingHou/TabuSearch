#pragma once
#include "map.h"
class Solution
{
public:
	Solution();
	~Solution();
private:
	/**该Solution关联的Map的得分**/
	double score;
	/**该Solution关联的Map**/
	Map* map;
	/**和Map大小一致的整形数组，对应于Map的下标的一个排列方式**/
	int* v;
public:
	/**根据v的排序计算得分**/
	void computeScore();
	void swapSolve(int i, int j);
	/**通过Map构造一个Solution**/
	Solution(Map* map);
	/**随机初始化一个排序并且计算其得分**/
	void initSolution();
	/**获取v数组第key个坐标的值**/
	int getV(int key);
	/**计算该Solution的得分**/
	double getScore();
	/*设置v数组第key个元素的值*/
	void set(int key, int value);
	/*释放排序数组的内存*/
	void free();
	/*打印排序*/
	void printPath();
};

