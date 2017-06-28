#pragma once
#include "map.h"
class Solution
{
public:
	Solution();
	~Solution();
private:
	/**��Solution������Map�ĵ÷�**/
	double score;
	/**��Solution������Map**/
	Map* map;
	/**��Map��Сһ�µ��������飬��Ӧ��Map���±��һ�����з�ʽ**/
	int* v;
public:
	/**����v���������÷�**/
	void computeScore();
	void swapSolve(int i, int j);
	/**ͨ��Map����һ��Solution**/
	Solution(Map* map);
	/**�����ʼ��һ�������Ҽ�����÷�**/
	void initSolution();
	/**��ȡv�����key�������ֵ**/
	int getV(int key);
	/**�����Solution�ĵ÷�**/
	double getScore();
	/*����v�����key��Ԫ�ص�ֵ*/
	void set(int key, int value);
	/*�ͷ�����������ڴ�*/
	void free();
	/*��ӡ����*/
	void printPath();
};

