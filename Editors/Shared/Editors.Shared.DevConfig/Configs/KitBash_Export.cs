using Editors.ImportExport.Exporting.Exporters.RmvToGltf;
using Editors.Shared.DevConfig.Base;
using Shared.Core.PackFiles;
using Shared.Core.PackFiles.Models;
using Shared.Core.Services;
using Shared.EmbeddedResources;


namespace Editors.Shared.DevConfig.Configs
{
    internal class KitBash_Export : IDeveloperConfiguration
    {
        private readonly PackFileService _packFileService;
        private readonly RmvToGltfExporter _exporter;

        public KitBash_Export(PackFileService packFileService, RmvToGltfExporter exporter)
        {
            _packFileService = packFileService;
            _exporter = exporter;
        }

        public void OpenFileOnLoad()
        {
            var meshPackFile = _packFileService.FindFile(@"variantmeshes\wh_variantmodels\hu1\emp\emp_karl_franz\emp_karl_franz.rigid_model_v2");
            var animPackFile = _packFileService.FindFile(@"animations\battle\humanoid01\subset\skeleton_warriors\sword_and_shield\combat_idles\hu1_sk_sws_combat_idle_03.anim");

            // obtains user's document folder 
            var documentPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var destPath = $"{documentPath}\\AE_Export_Handedness\\";

            // clear folder, if it exists
            DirectoryInfo dir = new DirectoryInfo(destPath);
            if (dir.Exists)
            {
                foreach (FileInfo file in dir.GetFiles())
                {
                    file.Delete();
                }
            }

            System.IO.Directory.CreateDirectory(destPath);


            var settings = new RmvToGltfExporterSettings(new List<PackFile>() { meshPackFile }, new List<PackFile>() { animPackFile }, destPath, true, true, true, true);
            _exporter.Export(settings);
        }

        public void OverrideSettings(ApplicationSettings currentSettings)
        {
            currentSettings.LoadCaPacksByDefault = true;
            currentSettings.CurrentGame = GameTypeEnum.Warhammer3;
            var packFile = ResourceLoader.GetDevelopmentDataFolder();// + "\\Karl_and_celestialgeneral.pack";
            _packFileService.Load(packFile, false, true);
        }
    }
}




//public static class KitbashEditor_Debug
//{
//    public static void CreateSlayerHead(IEditorCreator creator, IToolFactory toolFactory, PackFileService packfileService)
//    {
//        var packFile = packfileService.FindFile(@"variantmeshes\wh_variantmodels\hu3\dwf\dwf_slayers\head\dwf_slayers_head_01.rigid_model_v2");
//        creator.OpenFile(packFile);
//    }
//    public static void CreateSlayerBody(IEditorCreator creator, IToolFactory toolFactory, PackFileService packfileService)
//    {
//        var packFile = packfileService.FindFile(@"variantmeshes\wh_variantmodels\hu3\dwf\dwf_slayers\body\dwf_slayers_body_01.rigid_model_v2");
//        creator.OpenFile(packFile);
//    }
//    public static void CreateLoremasterHead(IEditorCreator creator, IToolFactory toolFactory, PackFileService packfileService)
//    {
//        var packFile = packfileService.FindFile(@"variantmeshes\wh_variantmodels\hu1d\hef\hef_loremaster_of_hoeth\hef_loremaster_of_hoeth_head_01.rigid_model_v2");
//        creator.OpenFile(packFile);
//    }
//
//    public static void CreatePaladin(IEditorCreator creator, IToolFactory toolFactory, PackFileService packfileService)
//    {
//        var packFile = packfileService.FindFile(@"variantmeshes\wh_variantmodels\hu1\brt\brt_paladin\head\brt_paladin_head_01.rigid_model_v2");
//        creator.OpenFile(packFile);
//    }
//
//    public static void CreateSkavenSlaveHead(IEditorCreator creator, IToolFactory toolFactory, PackFileService packfileService)
//    {
//        var packFile = packfileService.FindFile(@"variantmeshes\wh_variantmodels\hu17\skv\skv_clan_rats\head\skv_clan_rats_head_04.rigid_model_v2");
//        creator.OpenFile(packFile);
//    }
//
//    public static void CreatePrincessBody(IEditorCreator creator, IToolFactory toolFactory, PackFileService packfileService)
//    {
//        var packFile = packfileService.FindFile(@"variantmeshes/wh_variantmodels/hu1b/hef/hef_princess/hef_princess_body_01.rigid_model_v2");
//        creator.OpenFile(packFile);
//    }
//
//    public static void CreateOgre(IEditorCreator creator, IToolFactory toolFactory, PackFileService packfileService)
//    {
//        var packFile = packfileService.FindFile(@"variantmeshes\wh_variantmodels\hu13\ogr\ogr_maneater\ogr_maneater_body_01.rigid_model_v2");
//        creator.OpenFile(packFile);
//    }
//}
