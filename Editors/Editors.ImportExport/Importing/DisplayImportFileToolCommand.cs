using System.Windows.Forms;
using Editors.ImportExport.Exporting.Presentation;
using Shared.Core.Events;
using Shared.Core.Events.Global;
using Shared.Core.Misc;
using Shared.Ui.BaseDialogs.PackFileBrowser;
using Shared.Core.PackFiles.Models;
using TreeNode = Shared.Ui.BaseDialogs.PackFileBrowser.TreeNode;
using Shared.Core.PackFiles;

namespace Editors.ImportExport.Importing
{
    public class DisplayImportFileToolCommand : IUiCommand
    {
        // TODO: ?
        //private readonly IAbstractFormFactory<ImportWindow> _importWindowFactory;
        private readonly PackFileService _packFileService;

        public DisplayImportFileToolCommand(/*IAbstractFormFactory<ImporttWindow> exportWindowFactory,*/ PackFileService packFileService)
        {
            // TODO: ?
            //_ImportWindowFactory = importWindowFactory;
            _packFileService = packFileService;
        }
            
        public void  Execute(TreeNode clickedNode)
        {
            var glftFilePath = GetFileFromDiskDialog();

            // TODO: var rmv2File = GtlftImporter.Import(filePath);

            // INFO: how to add  files to a .pack file
            var modelPackFileMemory = new MemorySource(new byte[1024]); // stand in for gltf->RMV2->pack
            var modePackFile = new PackFile("fake_model.rmv2", modelPackFileMemory);
            _packFileService.AddFileToPack(clickedNode.FileOwner, "", modePackFile);

            // INFO: insert packed files in folder likes this 
            for (int i = 0; i < 5; i++)
            {
                // INFO: the binary packed files data (in this case crap data, ofc)
                var packFileMemory = new MemorySource(new byte[1024]); // stand in for texture DDS binary

                // INFO: make packed file, with file name a data
                var packFile = new PackFile($"fake_{i}.dds", packFileMemory);

                // INFO: add packedfile to loaded .pack (the .pack file and pack folder you righ clicked on)
                _packFileService.AddFileToPack(clickedNode.FileOwner, "text\\", packFile);
            }
        }

        private static string GetFileFromDiskDialog()
        {
            string filePath = "";
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "All files (*.*)|*.*|GLTF model files (*.gltf)|*.gltf";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    filePath = openFileDialog.FileName;
                }
            }
            return filePath;
        }
    }
}
