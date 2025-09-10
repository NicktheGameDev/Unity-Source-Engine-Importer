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
	public class SourceMdlFlexDesc
	{

		//struct mstudioflexdesc_t
		//{
		//	DECLARE_BYTESWAP_DATADESC();
		//	int					szFACSindex;
		//	inline char * const pszFACS( void ) const { return ((char *)this) + szFACSindex; }
		//};

		//	int					szFACSindex;
		public int nameOffset;

		public string theName;
		public bool theDescIsUsedByFlex = false;
		public bool theDescIsUsedByFlexRule = false;
		public bool theDescIsUsedByEyelid = false;

		//Public theVtaFrameIndex As Integer

	}

}
    #endif
