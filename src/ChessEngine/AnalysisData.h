#pragma once
#include "Move.h"

namespace ChessEngine
{
	public ref class AnalysisData sealed
	{
	public:
		property Platform::Array<Move^>^	Analysis;
		property float						Score;
		property bool						IsBestMove;
		property int						Depth;
		property int						NodesPerSecond;
		property int						MultiPV;
	};
}
