﻿using Common;
using CommonControls.Services;
using KitbasherEditor.ViewModels;
using KitbasherEditor.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace KitbasherEditor
{
    public class KitbasherEditor_DependencyInjectionContainer
    {
        public static void Register(IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<KitbasherView>();
            serviceCollection.AddTransient<KitbasherViewModel>();

        }

        public static void RegisterTools(IToolFactory factory)
        {
            factory.RegisterTool<KitbasherViewModel, KitbasherView>(".rigid_model_v2");
        }
    }


    public static class KitbashEditor_Debug
    {
        public static void CreateSlayerHead(IEditorCreator creator, IToolFactory toolFactory, PackFileService packfileService)
        {
            var packFile = packfileService.FindFile(@"variantmeshes\wh_variantmodels\hu3\dwf\dwf_slayers\head\dwf_slayers_head_01.rigid_model_v2");
            creator.OpenFile(packFile);
        }
    }
}
