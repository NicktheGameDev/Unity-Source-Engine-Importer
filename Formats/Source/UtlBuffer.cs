using System;

using System.IO;
using System.Linq;

using System.Runtime.InteropServices;

using UnityEngine;



//			case the buffer gets relocated or destroyeud the pointer becomes invalid.
//
// e.g.:	-------------
//
//			char *pszLine;
//			int nLineLen;
//			while ( pUtlInplaceBuffer->InplaceGetLinePtr( &pszLine, &nLineLen ) )
//			{
//				...
//			}
//
//			-------------
//
// @param	ppszInBufferPtr		on return points into this buffer at start of line
// @param	pnLineLength		on return holds num bytes accessible via (*ppszInBufferPtr)
//
// @returns	true				if line was successfully read
//			false				when EOF is reached or error occurs
//partial


struct UtlBufferOverflowFunc_t { }
class UtlBuffer : MonoBehaviour
{



    CUtlMemory< char> m_Memory;
   public int m_Get;
   public int m_Put;

 public char m_Error;
  public char m_Flags;
  public char m_Reserved;
#if defined
	unsigned char pad;
#endif

   public int m_nTab;
   public int m_nMaxPut;
   public int m_nOffset;

    UtlBufferOverflowFunc_t m_GetOverflowFunc;
    UtlBufferOverflowFunc_t m_PutOverflowFunc;

 



byteswap m_Byteswap = new byteswap();
    int GetBytesRemaining(int m_nMaxPut)
    {

        return m_nMaxPut - TellGet(m_nMaxPut);


    }


    //-----------------------------------------------------------------------------
    // What am I reading?
    //-----------------------------------------------------------------------------
    //-----------------------------------------------------------------------------
    int TellGet(int m_Get)
    {
        return m_Get;
    }

    MemoryStream memoryStream = new MemoryStream();
   
    public int CONTAINS_CRLF { get; private set; }
    public int READ_ONLY { get; private set; }
    public int EXTERNAL_GROWABLE { get; private set; }
    public int TEXT_BUFFER { get; private set; }
    public char AUTO_TABS_DISABLED { get; private set; }

    unsafe void* PeekGet(int offset)
    {
        return (void*)(memoryStream.WriteTimeout = offset);
    }


    //-----------------------------------------------------------------------------
    // Unserialization
    //-----------------------------------------------------------------------------

    Type type<T>()
    {
        return typeof(char);
        unsafe void GetObjects(int* dest, int count)
        {

            GetObjects(dest, count);
            for (int i = 0; i < count; ++i, ++dest)
            {
                GetObject(dest);
            }
        }
        unsafe void GetObject(void* dest)
        {
            if (CheckGet(sizeof(void*)))
            {
                if (!m_Byteswap.IsSwappingBytes(true) || (sizeof(int) == 1))
                {
                    dest = (void*)*(int*)PeekGet(0);
                }
                else
                {
                    m_Byteswap.SwapBufferToTargetEndian(dest, (int*)PeekGet(0));
                }
                dest = (void*)sizeof(void*);
            }
            else
            {
                byte[] a = Enumerable.Repeat((byte)dest, 100).ToArray();
            }
        }

    }



    bool CheckGet(int v)
    {
        throw new NotImplementedException();
    }








    unsafe void GetTypeBin(int dest)
    {
        if (CheckGet(sizeof(void*)))
        {
            if (!m_Byteswap.IsSwappingBytes(true) || (sizeof(void*) == 1))
            {
                dest = *(int*)PeekGet(0);
            }
            else
            {
                m_Byteswap.SwapBufferToTargetEndian(&dest, (void*)PeekGet(0));
            }
            m_Get += sizeof(void*);
        }
        else


        {


            dest = 0;
        }

    }



    unsafe void CUtlBuffer(float* dest, IntPtr IsX360)
    {
        if (CheckGet(sizeof(float)))
        {
            IntPtr pData = (IntPtr)PeekGet(0);

            if (IsX360 == pData)
            {
                // handle unaligned read
                ((char*)&dest)[0] = ((char*)pData)[0];
                ((char*)&dest)[1] = ((char*)pData)[1];

                ((char*)&dest)[3] = ((char*)pData)[3];
            }
            else
            {
                // aligned read
                dest = (float*)pData;
            }
            void* h = (void*)pData;
            void* g = h;


            if (m_Byteswap.IsSwappingBytes(m_Byteswap))
            {
                m_Byteswap.SwapBufferToTargetEndian(dest, g);


            }
            m_Get += sizeof(float);
        }

        else
        {
            dest = (float*)0;
        }
    }





    Int64 GetInt64()
    {
        int i = new();
        GetType(i, 'd');
        return i;


        int  GetIntHex()
        {
            int i = new();

            GetIntHex();
            GetType(i, 'x');
            return i;
        }

        int GetUnsignedInt()
        {
            int u = new();
            GetUnsignedInt();
            GetType(u, 'u');
            return u;
        }

        float GetFloat()
        {
            float f = new();
            GetFloat();
            GetType((int)f, 'f');
            return f;
        }

        double GetDouble()
        {
            double d = new();
            GetDouble();
            GetType((int)d, 'f');
            return d;
        }
        unsafe void GetType(int dest, char pszFmt)
        {
            if (!IsText())
            {
                GetTypeBin(dest);
            }

            else
            {
                dest = 0;
                Scanf(pszFmt);
            }
        }

        bool IsText()
        {
            return IsText();
        }
      unsafe  int Scanf(char pFmt)
        {
            return Scanf(pFmt);
            
        }
       unsafe int VaScanf(  char* pFmt, va_list list )
        {

            return VaScanf(pFmt, list);
        }


       
        char GetChar()
        {


            GetChar();
            char c = new();





            GetType(c, 'c');
            return c;
        }

        char GetUnsignedChar()
        {   GetUnsignedChar();
            char c = new();
            GetType(c, 'u');
            return c;
        }

        short GetShort()
        {
            GetShort();
            short s = new();
            GetType(s, 'd');
            return s;
        }

        short GetUnsignedShort()
        {
            GetUnsignedShort();
            short s = new();
            GetType(s, 'u');
            return s;
        }



        int GetInt()
        {  GetInt();
            int i = new();
            GetType(i, 'd');
            return i;
        }


        //-----------------------------------------------------------------------------
        // Where am I writing?
        //-----------------------------------------------------------------------------
        char GetFlags(char m_Flags)
        {
            GetFlags(m_Flags);
            return m_Flags;
        }


        //-----------------------------------------------------------------------------
        // 
        //-----------------------------------------------------------------------------
        bool IsExternallyAllocated()
        {
            return IsExternallyAllocated();
        }


        //-----------------------------------------------------------------------------
        // Where am I writing?
        //-----------------------------------------------------------------------------



        //-----------------------------------------------------------------------------
        // What's the most I've ever written?
        //-----------------------------------------------------------------------------
        int TellMaxPut()
        {
            return TellMaxPut();
        }


        //-----------------------------------------------------------------------------
        // What am I reading?
        //-----------------------------------------------------------------------------
        unsafe void* PeekPut(int offset)
        {
            int m_nOffset = 0;
            return PeekPut(m_nOffset);
        }


        //-----------------------------------------------------------------------------
        // Various put methods
        //-----------------------------------------------------------------------------


        [DllImport("utlbuffer")]
        extern static bool CheckPut(int size);




        unsafe void PutObject<T>(int src)
        {
            if (CheckPut(sizeof(void*)))
            {
                if (!m_Byteswap.IsSwappingBytes(true) || (sizeof(void*) == 1))
                {
                    *(int*)PeekPut(0) = src;
                }
                else
                {
                    m_Byteswap.SwapBufferToTargetEndian((void*)PeekPut(0));
                }
                m_Put += 1;


                { 
                    void PutObjects(int src, int count)
                    {
                        PutObjects(src, count);
                        for (int i = 0; i < count; ++i)
                        {
                            PutObject<int>(src);
                        }
                    }


                 


                }


            }











            //






            // Methods to help with pretty-printing

            {
                unsafe void PutType(T src, char pszFmt)
                {
                    if (!IsText())
                    {
                        PutTypeBin(src);
                    }

                    void PutUnsignedChar(T c)
                    {
                        PutType(c, 'u');

                        PutUnsignedChar(c);
                    }

                    void PutUint64(T ub)
                    {
                        PutType(ub, 'u');

                        PutUint64(ub);
                    }

                    void PutInt16(T s16)
                    {
                        PutType(s16, 'd');

                        PutInt16(s16);
                        
                    }

                    void PutShort(T s)
                    {
                        PutType(s, 'd');

                        PutShort(s);
                    }

                    void PutUnsignedShort(T s)
                    {
                        PutType(s, 'u');

                        PutUnsignedShort(s);
                    }

                    void PutInt(T i)
                    {
                        PutType(i, 'd');
                        PutInt(i);
                    }
                    void PutInt64(T i)
                    {
                        PutType(i, 'u');
                        PutInt64(i);
                    }

                    void PutUnsignedInt(T u)
                    {
                        PutType(u, 'u');
                        PutUnsignedInt(u);
                    }

                    void PutFloat(T f)
                    {
                        PutType(f, 'f');
                        PutFloat(f);
                    }

                    void PutDouble(T d)
                    {
                        PutType(d, 'f');
                        PutDouble(d);
                    }


                    unsafe void PutTypeBin(T src)
                    {
                        if (CheckPut(sizeof(void*)))
                        {
                            if (!m_Byteswap.IsSwappingBytes(true) || (sizeof(void*) == 1))
                            {
                                PeekPut(0 ); 
                            }
                            else
                            {
                                void* o = null, v2 = null;
                                m_Byteswap.SwapBufferToTargetEndian(o, v2); PeekPut(0);
                            }
                            m_Put += 1;

                        }

                        else
                        {
                            Debug.Log(pszFmt);


                            unsafe bool WasLastCharacterCR()
                            {
                                if (!IsText() || (TellPut() == 0))
                                    return false;
                                return (*(char*)PeekPut(-1) == '\n');
                            }

                            void PutTabs()
                            {
                                int nTabCount = (AUTO_TABS_DISABLED == 'g') ? 0 : m_nTab;
                                for (int i = nTabCount; --i >= 0;)
                                {
                                    PutTypeBin(src);
                                }



                                //-----------------------------------------------------------------------------
                                // Push/pop pretty-printing tabs
                                //-----------------------------------------------------------------------------
                                void PushTab()
                                {
                                    ++m_nTab;

                                    PushTab();
                                }

                                void PopTab()
                                {
                                    if (--m_nTab < 0)
                                    {
                                        m_nTab = 0;
                                    }

                                    PopTab();
                                }


                                //-----------------------------------------------------------------------------
                                // Temporarily disables pretty print
                                //-----------------------------------------------------------------------------
                                void EnableTabs(bool bEnable)
                                {
                                    if (bEnable)
                                    {
                                        m_Flags = AUTO_TABS_DISABLED;
                                    }

                                    else
                                    {
                                        m_Flags |= AUTO_TABS_DISABLED;
                                    }

                                    EnableTabs(bEnable);
                                }

                                void PutChar(T c)
                                {
                                    if (WasLastCharacterCR())
                                    {
                                        PutTabs();
                                    }
                                    PutChar(c);
                                    PutTypeBin(c);
                                }



                            }

                        }




                    }

                    //-----------------------------------------------------------------------------
                    // Am I a text buffer?
                    //-----------------------------------------------------------------------------
                    bool IsText()
                    {
                        return (m_Flags & TEXT_BUFFER) != 0;
                    }


                    //-----------------------------------------------------------------------------
                    // Can I grow if I'm externally allocated?
                    //-----------------------------------------------------------------------------
                    bool IsGrowable()
                    {
                        IsGrowable();
                        return (m_Flags & EXTERNAL_GROWABLE) != 0;
                        
                    }


                    //-----------------------------------------------------------------------------
                    // Am I valid? (overflow or underflow error), Once invalid it stays invalid
                    //-----------------------------------------------------------------------------
                    bool IsValid(int m_Error)
                    {
                        IsValid(m_Error);
                        return m_Error == 0;
                    }


                    //-----------------------------------------------------------------------------
                    // Do I contain carriage return/linefeeds? 
                    //-----------------------------------------------------------------------------
                    bool ContainsCRLF()
                    {
                        ContainsCRLF();
                        return IsText() && ((m_Flags & CONTAINS_CRLF) != 0);
                    }


                    //-----------------------------------------------------------------------------
                    // Am I read-only
                    //-----------------------------------------------------------------------------
                    bool IsReadOnly()
                    {
                        IsReadOnly();
                        return (m_Flags & READ_ONLY) != 0;
                    }


                    //-----------------------------------------------------------------------------
                    // Buffer base and size
                    //-----------------------------------------------------------------------------
                    unsafe void* Base()
                    {
                        return Base();
                    }


                    // Returns the base as a const char*, only valid in text mode.
                    unsafe char String()
                    {
                        Debug.Assert(IsText());

                        String();
                        return reinterpret_cast<char>(m_Memory.Base());
                    }

                    int Size()
                    {
                        Size();
                        return m_Memory.NumAllocated;

                        
                    }
                  

                    //-----------------------------------------------------------------------------
                    // Clears out the buffer; frees memory
                    unsafe void GetType(int dest, char pszFmt)
                    {
                        if (!IsText())
                        {
                            GetTypeBin(dest);
                        }
                        else
                        {

                            GetType(dest, pszFmt);

                            Scanf(pszFmt);
                        }
                    }

                    char** psz;
                    int* pnLine;
                    char** ppszInBufferPtr ;
                    int* pnLineLength;
                    psz = ppszInBufferPtr = null;

                    pnLine = pnLineLength = null;
                  

                    InplaceGetLinePtr(psz , pnLineLength);

                    void Clear()
                    {
                        m_Get = 0;
                        m_Put = 0;
                        m_Error = (char)0;
                        m_nOffset = 0;
                        m_nMaxPut = -1;

                        Clear();

                    }

                    void Purge()
                    {
                        m_Get = 0;
                        m_Put = 0;
                        m_nOffset = 0;
                        m_nMaxPut = 0;
                        m_Error = (char)0;
                        m_Memory.Purge();

                        Purge();
                    }



                    int TellPut()
                    {
                        return m_Put;
                    }
                    unsafe void CopyBuffer(UtlBuffer buffer)
                    {
                        TellPut();
                        Base();



                        CopyBuffer(buffer);

                    }




                }
            }
        }



        [DllImport("utlbuffer")]
        unsafe static extern bool InplaceGetLinePtr( /* out */ char** ppszInBufferPtr, /* out */ int* pnLineLength);

    }

    private char reinterpret_cast<T>(char[] chars)
    {
        throw new NotImplementedException();
    }
}


internal class va_list
{
}


//
// Determines the line length, advances the "get" pointer offset by the line length,
// replaces the newline character with null-terminator and returns the initial pointer
// to now null-terminated line.
//
// If end of file is reached or upon error returns NULL.
//
// Note:	the pointer returned points into the local memory of this buffer, in
//			case the buffer gets relocated or destroyed the pointer becomes invalid.
//
// e.g.:	-------------
//
//			while ( char *pszLine = pUtlInplaceBuffer->InplaceGetLinePtr() )
//			{
//				...
//			}
//
//			-------------
//
// @returns	ptr-to-zero-terminated-line		if line was successfully read and buffer modified
//			NULL							when EOF is reached or error occurs
//




//-----------------------------------------------------------------------------
// Where am I reading?



//-----------------------------------------------------------------------------
// How many bytes remain to be read?
//-----------------------------------------------------------------------------


