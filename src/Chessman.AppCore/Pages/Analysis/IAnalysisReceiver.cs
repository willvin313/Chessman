﻿using ChessEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chessman
{
    public interface IAnalysisReceiver
    {
        event AnalysisEventHandler AnalysisReceived;
        event AnalysisStateEventHandler AnalysisStateChanged;
    }
}
