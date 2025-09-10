// The copyright to the contents herein is the property of Valve, L.L.C.
// The contents may be used and/or copied only with the written permission of
// Valve, L.L.C., or in accordance with the terms and conditions stipulated in
// the agreement/contract under which the contents have been supplied.
//
// $Header: $
// $NoKeywords: $
//
// Model loading and caching system converted from C++ to C# for Unity
// ----------------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;

namespace Valve.MDLCache
{
    //--------------------------------------------------------------------------
    // Forward declarations / stub types (fill in with actual definitions as needed)
    //--------------------------------------------------------------------------

    /// <summary>
    /// Represents the studio header data for a model.
    /// </summary>
    public class studiohdr_t
    {
        public int numincludemodels;
        // TODO: Add real fields/properties as needed.
    }

    /// <summary>
    /// Represents the hardware-specific studio data.
    /// </summary>
    public class studiohwdata_t
    {
        // TODO: Add real fields/properties as needed.
    }

    /// <summary>
    /// Represents collision data for a model.
    /// </summary>
    public class vcollide_t
    {
        // TODO: Add real fields/properties as needed.
    }

    /// <summary>
    /// Represents a virtual model.
    /// </summary>
    public class virtualmodel_t
    {
        // TODO: Add real fields/properties as needed.
        public object Groups { get; set; }
   
    }

    /// <summary>
    /// Represents vertex data for a model.
    /// </summary>
    public class vertexFileHeader_t
    {
        // TODO: Add real fields/properties as needed.
    }

    namespace OptimizedModel
    {
        /// <summary>
        /// Represents an optimized file header.
        /// </summary>

        
         [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public  struct FileHeader_t
        {
            // TODO: Add real fields/properties as needed.
        }
    }

    //--------------------------------------------------------------------------
    // Type Definitions
    //--------------------------------------------------------------------------

    /// <summary>
    /// Handle type for a loaded studio model.
    /// </summary>
 
         [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public  struct MDLHandle_t : IEquatable<MDLHandle_t>
    {
        private readonly ushort _handle;
        public MDLHandle_t(ushort handle) { _handle = handle; }
        public ushort Value => _handle;

        public bool Equals(MDLHandle_t other) => _handle == other._handle;
        public override bool Equals(object obj) => obj is MDLHandle_t other && Equals(other);
        public override int GetHashCode() => _handle.GetHashCode();
        public static bool operator ==(MDLHandle_t left, MDLHandle_t right) => left.Equals(right);
        public static bool operator !=(MDLHandle_t left, MDLHandle_t right) => !left.Equals(right);

        public override string ToString() => _handle.ToString();
    }

    /// <summary>
    /// Represents an invalid MDL handle (0xFFFF).
    /// </summary>
    public static class MDLHandleConstants
    {
        public static readonly MDLHandle_t MDLHANDLE_INVALID = new MDLHandle_t(ushort.MaxValue);
    }

    //--------------------------------------------------------------------------
    // Enums
    //--------------------------------------------------------------------------

    /// <summary>
    /// MDL cache data types.
    /// </summary>
    public enum MDLCacheDataType_t
    {
        MDLCACHE_STUDIOHDR = 0,
        MDLCACHE_STUDIOHWDATA,
        MDLCACHE_VCOLLIDE,

        // Callbacks NOT called when data is loaded or unloaded:
        MDLCACHE_ANIMBLOCK,
        MDLCACHE_VIRTUALMODEL,
        MDLCACHE_VERTEXES,
        MDLCACHE_DECODEDANIMBLOCK,
    }

    /// <summary>
    /// Flags for flushing the MDL cache.
    /// </summary>
    [Flags]
    public enum MDLCacheFlush_t
    {
        MDLCACHE_FLUSH_STUDIOHDR = 0x01,
        MDLCACHE_FLUSH_STUDIOHWDATA = 0x02,
        MDLCACHE_FLUSH_VCOLLIDE = 0x04,
        MDLCACHE_FLUSH_ANIMBLOCK = 0x08,
        MDLCACHE_FLUSH_VIRTUALMODEL = 0x10,
        MDLCACHE_FLUSH_AUTOPLAY = 0x20,
        MDLCACHE_FLUSH_VERTEXES = 0x40,

        MDLCACHE_FLUSH_IGNORELOCK = unchecked((int)0x80000000),
        MDLCACHE_FLUSH_ALL = unchecked((int)0xFFFFFFFF)
    }

    //--------------------------------------------------------------------------
    // Interfaces
    //--------------------------------------------------------------------------

    /// <summary>
    /// Base interface for application systems.
    /// </summary>
 
    /// </summary>
    /// <param name="interfaceName">The name of the interface to create.</param>
    /// <returns>An object representing the interface, or null if not found.</returns>
    public delegate object CreateInterfaceFn(string interfaceName);

    /// <summary>
    /// Return values for initialization.
    /// </summary>
    public enum InitReturnVal_t
    {
        INIT_FAILED = 0,
        INIT_OK,
        INIT_LAST_VAL,
    }

    /// <summary>
    /// Base interface for application systems.
    /// </summary>
    public interface IAppSystem
    {
        /// <summary>
        /// Called to allow the app system to connect with other systems.
        /// </summary>
        /// <param name="factory">A function used to create interfaces.</param>
        /// <returns>True if the connection is successful; otherwise false.</returns>
        bool Connect(CreateInterfaceFn factory);

        /// <summary>
        /// Called to disconnect the app system from others.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Queries for an interface implemented by this system.
        /// Returns null if the requested interface is not supported.
        /// </summary>
        /// <param name="pInterfaceName">The name of the interface to query.</param>
        /// <returns>An object representing the requested interface, or null.</returns>
        object QueryInterface(string pInterfaceName);

        /// <summary>
        /// Initializes the app system.
        /// </summary>
        /// <returns>An initialization return value.</returns>
        InitReturnVal_t Init();

        /// <summary>
        /// Shuts down the app system.
        /// </summary>
        void Shutdown();
    }

    //-------------------------------------------------------------------------
    // Helper empty implementation of an IAppSystem.
    //-------------------------------------------------------------------------
    /// <summary>
    /// A base class providing a default (empty) implementation of IAppSystem.
    /// The generic parameter T is intended to represent the derived interface.
    /// </summary>
    /// <typeparam name="T">The interface that the derived class implements.</typeparam>
    public abstract class CBaseAppSystem<T> : IAppSystem where T : class
    {
        /// <summary>
        /// Called to allow the app system to connect with other systems.
        /// </summary>
        /// <param name="factory">A function used to create interfaces.</param>
        /// <returns>Always returns true.</returns>
        public virtual bool Connect(CreateInterfaceFn factory)
        {
            return true;
        }

        /// <summary>
        /// Called to disconnect the app system from others.
        /// </summary>
        public virtual void Disconnect()
        {
            // Default: do nothing.
        }

        /// <summary>
        /// Queries for an interface implemented by this system.
        /// </summary>
        /// <param name="pInterfaceName">The name of the interface to query.</param>
        /// <returns>Always returns null.</returns>
        public virtual object QueryInterface(string pInterfaceName)
        {
            return null;
        }

        /// <summary>
        /// Initializes the app system.
        /// </summary>
        /// <returns>Always returns INIT_OK.</returns>
        public virtual InitReturnVal_t Init()
        {
            return InitReturnVal_t.INIT_OK;
        }

        /// <summary>
        /// Shuts down the app system.
        /// </summary>
        public virtual void Shutdown()
        {
            // Default: do nothing.
        }
    }

    //-------------------------------------------------------------------------
    // Helper implementation of an IAppSystem for tier0.
    //-------------------------------------------------------------------------
    /// <summary>
    /// A helper class for tier0 application systems.
    /// This class derives from CBaseAppSystem and tracks whether it is the primary app system.
    /// </summary>
    /// <typeparam name="T">The interface that the derived class implements.</typeparam>
    public abstract class CTier0AppSystem<T> : CBaseAppSystem<T> where T : class
    {
        private readonly bool m_bIsPrimaryAppSystem;

        /// <summary>
        /// Constructs a new tier0 app system.
        /// </summary>
        /// <param name="bIsPrimaryAppSystem">
        /// If true, this is considered the primary app system.
        /// </param>
        protected CTier0AppSystem(bool bIsPrimaryAppSystem = true)
        {
            m_bIsPrimaryAppSystem = bIsPrimaryAppSystem;
        }

        /// <summary>
        /// Checks if this is the primary app system.
        /// </summary>
        /// <returns>True if it is the primary app system; otherwise false.</returns>
        protected bool IsPrimaryAppSystem()
        {
            return m_bIsPrimaryAppSystem;
        }
    }

    //-------------------------------------------------------------------------
    // IAppSystemV0 interface.
    //-------------------------------------------------------------------------
    /// <summary>
    /// This is the version of IAppSystem shipped 10/15/04.
    /// NOTE: Never change this!!!
    /// </summary>
    public interface IAppSystemV0
    {
        /// <summary>
        /// Called to allow the app system to connect with other systems.
        /// </summary>
        /// <param name="factory">A function used to create interfaces.</param>
        /// <returns>True if the connection is successful; otherwise false.</returns>
        bool Connect(CreateInterfaceFn factory);

        /// <summary>
        /// Called to disconnect the app system from others.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Queries for an interface implemented by this system.
        /// </summary>
        /// <param name="pInterfaceName">The name of the interface to query.</param>
        /// <returns>An object representing the requested interface, or null.</returns>
        object QueryInterface(string pInterfaceName);

        /// <summary>
        /// Initializes the app system.
        /// </summary>
        /// <returns>An initialization return value.</returns>
        InitReturnVal_t Init();

        /// <summary>
        /// Shuts down the app system.
        /// </summary>
        void Shutdown();
    }

    /// <summary>
    /// Notification interface for MDL cache load/unload events.
    /// </summary>
    public interface IMDLCacheNotify
    {
        /// <summary>
        /// Called right after the data is loaded.
        /// </summary>
        /// <param name="type">The type of data loaded.</param>
        /// <param name="handle">The handle of the MDL.</param>
        void OnDataLoaded(MDLCacheDataType_t type, MDLHandle_t handle);

        /// <summary>
        /// Called right before the data is unloaded.
        /// </summary>
        /// <param name="type">The type of data about to be unloaded.</param>
        /// <param name="handle">The handle of the MDL.</param>
        void OnDataUnloaded(MDLCacheDataType_t type, MDLHandle_t handle);
    }

    /// <summary>
    /// Main interface for the MDL cache system.
    /// </summary>
    public interface IMDLCache : IAppSystem
    {
        // Used to install callbacks for when data is loaded/unloaded.
        // Returns the prior notify (if any).
        void SetCacheNotify(IMDLCacheNotify pNotify);

        // Finds (and references) an MDL given its relative path.
        MDLHandle_t FindMDL(string pMDLRelativePath);

        // Reference counting.
        int AddRef(MDLHandle_t handle);
        int Release(MDLHandle_t handle);
        int GetRef(MDLHandle_t handle);

        // Data accessors.
        studiohdr_t GetStudioHdr(MDLHandle_t handle);
        studiohwdata_t GetHardwareData(MDLHandle_t handle);
        vcollide_t GetVCollide(MDLHandle_t handle);

        /// <summary>
        /// Gets the animation block data.
        /// </summary>
        /// <param name="handle">The model handle.</param>
        /// <param name="nBlock">The animation block index.</param>
        /// <returns>Byte array representing the animation block.</returns>
        byte[] GetAnimBlock(MDLHandle_t handle, int nBlock);

        virtualmodel_t GetVirtualModel(MDLHandle_t handle);

        /// <summary>
        /// Gets the autoplay list.
        /// </summary>
        /// <param name="handle">The model handle.</param>
        /// <param name="pOut">Output array of autoplay indices.</param>
        /// <returns>The count of autoplay entries.</returns>
        int GetAutoplayList(MDLHandle_t handle, out ushort[] pOut);

        vertexFileHeader_t GetVertexData(MDLHandle_t handle);

        // Brings all data associated with an MDL into memory.
        void TouchAllData(MDLHandle_t handle);

        // Gets/sets user data associated with the MDL.
        void SetUserData(MDLHandle_t handle, object pData);
        object GetUserData(MDLHandle_t handle);

        // Checks if the MDL is using the error model.
        bool IsErrorModel(MDLHandle_t handle);

        // Flushes the cache; forces a full discard.
        void Flush(MDLCacheFlush_t nFlushFlags = MDLCacheFlush_t.MDLCACHE_FLUSH_ALL);

        // Flushes a particular model out of memory.
        void Flush(MDLHandle_t handle, MDLCacheFlush_t nFlushFlags = MDLCacheFlush_t.MDLCACHE_FLUSH_ALL);

        // Returns the name of the model (its relative path).
        string GetModelName(MDLHandle_t handle);

        // Faster access when you already have the studio header.
        virtualmodel_t GetVirtualModelFast(studiohdr_t pStudioHdr, MDLHandle_t handle);

        // Locking: all cache entries allocated or checked after BeginLock()
        // are considered "locked" and will not be freed when additional memory is needed.
        void BeginLock();
        void EndLock();

        // Returns a reference to a counter that is incremented every time the cache is unlocked (old version).
        ref int GetFrameUnlockCounterPtrOLD();

        // Finish all pending asynchronous operations.
        void FinishPendingLoads();

        // Extended collision interface.
        vcollide_t GetVCollideEx(MDLHandle_t handle, bool synchronousLoad = true);

        // Gets the size of the collision data.
        bool GetVCollideSize(MDLHandle_t handle, out int pVCollideSize);

        // Async loading flags.
        bool GetAsyncLoad(MDLCacheDataType_t type);
        bool SetAsyncLoad(MDLCacheDataType_t type, bool bAsync);

        // Map loading helpers.
        void BeginMapLoad();
        void EndMapLoad();
        void MarkAsLoaded(MDLHandle_t handle);

        // Preload data.
        void InitPreloadData(bool rebuild);
        void ShutdownPreloadData();

        // Data loaded check.
        bool IsDataLoaded(MDLHandle_t handle, MDLCacheDataType_t type);

        // Returns a reference to a counter for a specific data type.
     unsafe   ref int* GetFrameUnlockCounterPtr(MDLCacheDataType_t type);

        // Lock/unlock access to the studio header.
        studiohdr_t LockStudioHdr(MDLHandle_t handle);
        void UnlockStudioHdr(MDLHandle_t handle);

        // Preloads the model.
        bool PreloadModel(MDLHandle_t handle);

        // Hammer uses this: resets error model status so that a correct model can be reloaded.
        void ResetErrorModelStatus(MDLHandle_t handle);

        // Marks a new frame in the cache system.
        void MarkFrame();
    }

    //--------------------------------------------------------------------------
    // Critical Section Helper
    //--------------------------------------------------------------------------

    /// <summary>
    /// Helper class that calls BeginLock on construction and EndLock on disposal.
    /// Use with a using block to create a critical section scope.
    /// </summary>
    public class CMDLCacheCriticalSection : IDisposable
    {
        private readonly IMDLCache _cache;
        private bool _disposed;

        public CMDLCacheCriticalSection(IMDLCache cache)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _cache.BeginLock();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _cache.EndLock();
                _disposed = true;
            }
        }
    }

    //--------------------------------------------------------------------------
    // Usage Example Comments
    //--------------------------------------------------------------------------

    /*
        // Instead of using C++ macros like MDLCACHE_CRITICAL_SECTION, in C# you would use:

        // For fine-grained locking:
        using (new CMDLCacheCriticalSection(mdlcache))
        {
            // Critical section code here.
        }

        // For coarse locking (if implemented differently), you might create a similar helper.
    */
}
