using System;

namespace uSource.Formats.Source.MDL
{
#if !DISABLE_POSTPROCESSOR

#endif

#if DISABLE_POSTPROCESSOR
    // Empty shim classes to satisfy compiler
    public class PostProcessorAssemblyResolver
    {
        public static void Initialize() {}
    }
    
    public class PostProcessorReflectionImporterProvider
    {
        public static void Initialize() {}
    }
#endif
}
