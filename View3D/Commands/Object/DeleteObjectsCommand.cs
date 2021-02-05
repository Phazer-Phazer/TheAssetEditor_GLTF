﻿using Common;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using View3D.Components.Component;
using View3D.Components.Component.Selection;
using View3D.Rendering;

namespace View3D.Commands.Object
{
    public class DeleteObjectsCommand : ICommand
    {
        ILogger _logger = Logging.Create<DeleteObjectsCommand>();

        List<RenderItem> _itemsToDelete;
        SceneManager _sceneManager;
        SelectionManager _selectionManager;

        ISelectionState _oldState;
        public DeleteObjectsCommand(List<RenderItem> itemsToDelete, SceneManager sceneManager, SelectionManager selectionManager)
        {
            _itemsToDelete = new List<RenderItem>(itemsToDelete);
            _sceneManager = sceneManager;
            _selectionManager = selectionManager;
        }

        public void Execute()
        {
            _oldState = _selectionManager.GetStateCopy();

            _logger.Here().Information($"Executing DeleteObjectsCommand Items[{string.Join(',', _itemsToDelete.Select(x => x.Name))}]");
            foreach (var item in _itemsToDelete)
                _sceneManager.RenderItems.Remove(item);

            if (_selectionManager.GetState() is ObjectSelectionState objectState)
                objectState.Clear();
        }

        public void Undo()
        {
            _logger.Here().Information($"Undoing DeleteObjectsCommand"); 
            foreach (var item in _itemsToDelete)
                _sceneManager.RenderItems.Add(item);

            _selectionManager.SetState(_oldState);
        }
    }
}
