    // UNITY_WRAPPED - Crowbar sources excluded from Unity compilation
    #if !UNITY_2018_1_OR_NEWER
    ﻿//INSTANT C# NOTE: Formerly VB project-level imports:
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;

namespace uSource.Formats.Source.MDL{
	public class SourceMdlFlexRule
	{

		//FROM: SourceEngineXXXX_source\public\studio.h
		//struct mstudioflexrule_t
		//{
		//	DECLARE_BYTESWAP_DATADESC();
		//	int					flex;
		//	int					numops;
		//	int					opindex;
		//	inline mstudioflexop_t *iFlexOp( int i ) const { return  (mstudioflexop_t *)(((byte *)this) + opindex) + i; };
		//};

		//	int					flex;
		public int flexIndex;
		//	int					numops;
		public int opCount;
		//	int					opindex;
		public int opOffset;



		public List<SourceMdlFlexOp> theFlexOps;

	}

}
    #endif
