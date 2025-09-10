using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace uSource.Formats.Source.MDL
{
    public static unsafe class FlexDescExtensions
    {

        /// <summary>
        /// Odczytuje nazwę flexa (FACS) spod offsetu szFACSindex jako ANSI string.
        /// </summary>
        public static string GetFlexName(this ref mstudioflexdesc_t desc)
        {
            // 1) Uzyskaj wskaźnik do początku struktury
            byte* basePtr = (byte*)Unsafe.AsPointer(ref desc);
            

            // 2) Przejdź o szFACSindex bajtów
            byte* strPtr = basePtr + desc.szFACSindex;
            if (strPtr == null || *strPtr == 0)
                return string.Empty;

            // 3) Zamień null‐terminated ANSI C‐string na .NET string
            return Marshal.PtrToStringAnsi((IntPtr)strPtr);
        }
    }
}