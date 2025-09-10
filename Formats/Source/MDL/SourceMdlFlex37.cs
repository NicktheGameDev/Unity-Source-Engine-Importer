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
	public class SourceMdlFlex37
	{

		//struct mstudioflex_t
		//{
		//	int	flexdesc;	// input value
		//
		//	float	target0;	// zero
		//	float	target1;	// one
		//	float	target2;	// one
		//	float	target3;	// zero
		//
		//	int	numverts;
		//	int	vertindex;
		//	inline	mstudiovertanim_t *pVertanim( int i ) const { return  (mstudiovertanim_t *)(((byte *)this) + vertindex) + i; };
		//};

		public int flexDescIndex;

		public double target0;
		public double target1;
		public double target2;
		public double target3;

		public int vertCount;
		public int vertOffset;

		public List<SourceMdlVertAnim37> theVertAnims;

	}

}
    #endif
