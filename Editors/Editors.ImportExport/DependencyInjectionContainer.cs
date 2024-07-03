﻿using Editors.ImportExport.Exporting;
using Editors.ImportExport.Exporting.Exporters;
using Editors.ImportExport.Exporting.Exporters.DdsToMaterialPng;
using Editors.ImportExport.Exporting.Exporters.DdsToPng;
using Editors.ImportExport.Exporting.Presentation;
using Editors.ImportExport.Exporting.Presentation.DdsToMaterialPng;
using Editors.ImportExport.Exporting.Presentation.DdsToPng;
using Microsoft.Extensions.DependencyInjection;
using Shared.Core.DependencyInjection;
using Shared.Ui.BaseDialogs.PackFileBrowser;

namespace Editors.ImportExport
{
    public class DependencyInjectionContainer : DependencyContainer
    {
        public override void Register(IServiceCollection services)
        {
            // Exporter ViewModels
            RegisterWindow<ExportWindow>(services);
            services.AddTransient<ExporterCoreViewModel>();
            services.AddTransient<IExporterViewModel, DdsToPngExporterViewModel>();
            services.AddTransient<IExporterViewModel, DdsToMaterialPngViewModel>();

            // Exporters
            services.AddTransient<DdsToMaterialPngExporter>();
            services.AddTransient<DdsToPngExporter>();

            // Helpers to ensure we can hook up to the UI
            services.AddTransient<IExportFileContextMenuHelper, ExportFileContextMenuHelper>();
            services.AddTransient<DisplayExportFileToolCommand>();
        }
    }
}
