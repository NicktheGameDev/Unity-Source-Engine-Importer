using UnityEditor;
namespace ValveExporter.Editor
{
    public static class ValveExporterMenu
    {
        private const string MENU="Valve Exporter/";
        [MenuItem(MENU+"Export SMD")]
        private static void ESMD()=>Exporters.SMDExporter.ExportSelectionToSMD();
    }
}
