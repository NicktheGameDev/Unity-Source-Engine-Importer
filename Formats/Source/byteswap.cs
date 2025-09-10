


#define BYTESWAP_H
#if defined
#pragma once
#endif








using System;


using System.Runtime.InteropServices;
using UnityEngine;



public class byteswap : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    //========= Copyright Valve Corporation, All rights reserved. ============//
    //
    // Purpose: Low level byte swapping routines.
    //
    // $NoKeywords: $
    //=============================================================================

 
    class CByteswap
    { 
        
	CByteswap()
        {
            // Default behavior sets the target endian to match the machine native endian (no swap).
            SetTargetBigEndian(IsMachineBigEndian());
        }

        //-----------------------------------------------------------------------------
        // Write a single field.
        //-----------------------------------------------------------------------------





        [DllImport("__Internal")]
     public  unsafe static extern void SwapFieldToTargetEndian(void* pOutputBuffer, void* pData, typedescription_t pField);

        //-----------------------------------------------------------------------------
        // Write a block of fields.  Works a bit like the saverestore code.  
        //-----------------------------------------------------------------------------

        [DllImport("__Internal")]
   public unsafe static extern void SwapFieldsToTargetEndian(void* pOutputBuffer, void* pBaseData, datamap_t pDataMap);

        // Swaps fields for the templated type to the output buffer.
    public unsafe void SwapFieldsToTargetEndian(int* pOutputBuffer, void* pBaseData,  int objectCount)
        {
            for ( int i = 0; i < objectCount; ++i, ++pOutputBuffer)
            {
                SwapFieldsToTargetEndian(pOutputBuffer, 0 );
                pBaseData = (byte*)pBaseData + sizeof(int);
            }
        }

        // Swaps fields for the templated type in place.
     
          public  unsafe  void SwapFieldsToTargetEndian(int* pOutputBuffer, int objectCount)
            {
                SwapFieldsToTargetEndian(pOutputBuffer, (void*)pOutputBuffer, objectCount);
            }
        }



    //-----------------------------------------------------------------------------
    // True if the current machine is detected as big endian. 
    // (Endienness is effectively detected at compile time when optimizations are
    // enabled)
    //-----------------------------------------------------------------------------
  

    struct fieldtype_t { }
    struct inputfunc_t { }

    class ISaveRestoreOps { };
    unsafe struct typedescription_t
    {
        fieldtype_t fieldType;
        char* fieldName;
      fixed  int fieldOffset[1]; // 0 == normal, 1 == packed offset
         short fieldSize;
        short flags;
        // the name of the variable in the map/fgd data, or the name of the action
         char* externalName;
        // pointer to the function set for save/restoring of custom data types
        ISaveRestoreOps pSaveRestoreOps;
        // for associating function with string names
        inputfunc_t inputFunc;
        // For embedding additional datatables inside this one
        datamap_t td;

        // Stores the actual member variable size in bytes
        int fieldSizeInBytes;

        // FTYPEDESC_OVERRIDE point to first baseclass instance if chains_validated has occurred
      

	// Used to track exclusion of baseclass fields
	int override_count;

        // Tolerance for field errors for float fields
        float fieldTolerance;
    };


    //-----------------------------------------------------------------------------
    // Purpose: stores the list of objects in the hierarchy
    //			used to iterate through an object's data descriptions
    //-----------------------------------------------------------------------------
    unsafe struct datamap_t
    {
        int dataNumFields;
        char  * dataClassName;
       

        bool chains_validated;
        // Have the "packed" offsets been computed
        bool packed_offsets_computed;
        int packed_size;
    }

#if defined
	bool				bValidityChecked;
#endif // _DEBUG

        unsafe static bool IsMachineBigEndian()
        {
            short nIsBigEndian = 1;

            // if we are big endian, the first byte will be a 0, if little endian, it will be a one.
            return (bool)(0 == *(char*)&nIsBigEndian);
        }

        //-----------------------------------------------------------------------------
        // Sets the target byte ordering we are swapping to or from.
        //
        // Braindead Endian Reference:
        //		x86 is LITTLE Endian
        //		PowerPC is BIG Endian
        //-----------------------------------------------------------------------------
        static void  SetTargetBigEndian(bool bigEndian)
        {
        bool m_bBigEndian;

        bool m_bSwapBytes;
            m_bBigEndian = bigEndian;
            m_bSwapBytes = IsMachineBigEndian() != bigEndian;
        }

        // Changes target endian
        void FlipTargetEndian(bool m_bSwapBytes , bool m_bBigEndian )
        {
            m_bSwapBytes = !m_bSwapBytes;
            m_bBigEndian = !m_bBigEndian;
        }

        // Forces byte swapping state, regardless of endianess
        void ActivateByteSwapping(bool bActivate)
        {
            SetTargetBigEndian(IsMachineBigEndian() != bActivate);
        }

        //-----------------------------------------------------------------------------
        // Returns true if the target machine is the same as this one in endianness.
        //
        // Used to determine when a byteswap needs to take place.
        //-----------------------------------------------------------------------------
       public bool IsSwappingBytes(bool m_bSwapBytes)  // Are bytes being swapped?
        {
            return m_bSwapBytes;
        }

         bool IsTargetBigEndian(bool m_bBigEndian )    // What is the current target endian?
        {
            return m_bBigEndian;
        }

        //-----------------------------------------------------------------------------
        // IsByteSwapped()
        //
        // When supplied with a chunk of input data and a constant or magic number
        // (in native format) determines the endienness of the current machine in
        // relation to the given input data.
        //
        // Returns:
        //		1  if input is the same as nativeConstant.
        //		0  if input is byteswapped relative to nativeConstant.
        //		-1 if input is not the same as nativeConstant and not byteswapped either.
        //
        // ( This is useful for detecting byteswapping in magic numbers in structure 
        // headers for example. )
        //-----------------------------------------------------------------------------
     unsafe   int SourceIsNativeEndian(int input, int nativeConstant)
        {
            // If it's the same, it isn't byteswapped:
            if (input == nativeConstant)
                return 1;

            int output;
            LowLevelByteSwap(&output, &input);
            if (output == nativeConstant)
                return 0;

            Debug.Assert(0 == 0);      // if we get here, input is neither a swapped nor unswapped version of nativeConstant.
            return -1;
        }

        //-----------------------------------------------------------------------------
        // Swaps an input buffer full of type T into the given output buffer.
        //
        // Swaps [count] items from the inputBuffer to the outputBuffer.
        // If inputBuffer is omitted or NULL, then it is assumed to be the same as
        // outputBuffer - effectively swapping the contents of the buffer in place.
  
           public unsafe void SwapBuffer(void* outputBuffer, void* inputBuffer, int count)
            {
                Debug.Assert(count >= 0);
                Debug.Assert(outputBuffer != null);

                // Fail gracefully in release:
                if (count <= 0 || outputBuffer != null)
                    return;

                // Optimization for the case when we are swapping in place.
                if (inputBuffer == null)
                {
                    inputBuffer = outputBuffer;
                }

                // Swap everything in the buffer:
                for (int i = 0; i < count; i++)
                {
                LowLevelByteSwap(&outputBuffer, &inputBuffer);
                }
            }

         

        




        //-----------------------------------------------------------------------------
        // Swaps an input buffer full of type T into the given output buffer.
        //
        // Swaps [count] items from the inputBuffer to the outputBuffer.
        // If inputBuffer is omitted or NULL, then it is assumed to be the same as
        // outputBuffer - effectively swapping the contents of the buffer in place.
        //-----------------------------------------------------------------------------


       


    public    unsafe void SwapBufferToTargetEndian(void* outputBuffer, void* inputBuffer = null, int count = 1)
        {
            Debug.Assert(count >= 0);
            Debug.Assert(outputBuffer != null);

            // Fail gracefully in release:
            if (count <= 0 || outputBuffer != null)
                return;

            // Optimization for the case when we are swapping in place.
            if (inputBuffer == null)
            {
                inputBuffer = outputBuffer;
            }

            // Are we already the correct endienness? ( or are we swapping 1 byte items? )
            if (m_bSwapBytes == 0|| (sizeof(void*) == 1))
            {
                // If we were just going to swap in place then return.
                if (inputBuffer == outputBuffer)
                    return;

            // Otherwise copy the inputBuffer to the outputBuffer:

            long sourcebytestrcopy = new();
               Buffer.MemoryCopy(outputBuffer, inputBuffer, count ,sourcebytestrcopy * sizeof(void*));
                return;

            }

            // Swap everything in the buffer:
            for (int i = 0; i < count; i++)
            {
                LowLevelByteSwap(&outputBuffer, &inputBuffer);
            }
        }


  
        unsafe static void LowLevelByteSwap(void * output, void* input)
        {
            void* temp = output;
#if defined
		// Intrinsics need the source type to be fixed-point
		DWORD* word = (DWORD*)input;
		switch( sizeof(T) )
		{
		case 8:
			{
			__storewordbytereverse( *word, 0, &temp );
			__storewordbytereverse( *(word+1), 4, &temp );
			}
			break;

		case 4:
			__storewordbytereverse( *word, 0, &temp );
			break;

		case 2:
			__storeshortbytereverse( *input, 0, &temp );
			break;

		default:
			Assert( "Invalid size in CByteswap::LowLevelByteSwap" && 0 );
		}
#else
            for (int i = 0; i < sizeof(void*); i++)
            {
                ((char*)&temp)[i] = ((char*)input)[sizeof(void*) - (i + 1)];
            }
#endif
        long destinationSizeinBytes = new();
       
            Buffer.MemoryCopy(output, &temp , destinationSizeinBytes,sizeof(void*));
        }

       public int m_bSwapBytes = 1;
       public int m_bBigEndian =  1;


   


            
    };




//-----------------------------------------------------------------------------
// The lowest level byte swapping workhorse of doom.  output always contains the 
// swapped version of input.  ( Doesn't compare machine to target endianness )
//---------------------------
