#pragma once
#include <string>

using namespace std;
class Map
{
public:
	Map();
	~Map();
public:
	int numVertex;
	double** coordinate;
	Map(string file);
	double getDistance(int, int);
	string file;
	void free();
};

