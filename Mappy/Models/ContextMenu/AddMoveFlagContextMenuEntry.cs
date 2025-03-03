﻿using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Mappy.Abstracts;
using Mappy.Models.Enums;
using Mappy.System;
using Mappy.System.Localization;

namespace Mappy.Models.ContextMenu;

public unsafe class AddMoveFlagContextMenuEntry : IContextMenuEntry
{
    public bool Enabled => AgentMap.Instance() is not null;

    public PopupMenuType Type => PopupMenuType.AddMoveFlag;
    
    public string Label => AgentMap.Instance()->IsFlagMarkerSet is 0 ? Strings.AddFlag : Strings.MoveFlag;
    
    public void ClickAction(Vector2 clickPosition)
    {
        if (MappySystem.MapTextureController is not { Ready: true, CurrentMap: var map }) return;
        
        var agent = AgentMap.Instance();
        agent->IsFlagMarkerSet = 0;
        agent->SetFlagMapMarker(map.TerritoryType.Row, map.RowId, clickPosition.X, clickPosition.Y);

        AgentChatLog.Instance()->InsertTextCommandParam(1048, false);
    }
}