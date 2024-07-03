﻿using GameWorld.Core.SceneNodes;

namespace Editors.KitbasherEditor.ViewModels.SceneExplorer.Nodes
{
    public interface ISceneNodeViewModel : IDisposable
    {
        void Initialize(ISceneNode node);
    }
}
