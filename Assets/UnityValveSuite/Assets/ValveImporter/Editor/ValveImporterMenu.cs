using UnityEditor;
using ValveImporter.Editor.Importers;

namespace ValveImporter.Editor
{
    public static class ValveImporterMenu
    {
        [MenuItem("uSource/Quick Import QC")]
        private static void QuickImportQC()
        {
            QCImporter.ImportQCFile();
        }
    }
}
