﻿using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using KamiLib.AutomaticUserInterface;
using KamiLib.Utilities;
using Lumina.Excel.GeneratedSheets;
using Mappy.Abstracts;
using Mappy.Models;
using Mappy.Models.Enums;
using Mappy.Utility;
using Mappy.Views.Attributes;

namespace Mappy.System.Modules;

[Category("IconSelection")]
public class PetConfig : IModuleConfig, IIconConfig, ITooltipConfig
{
    public bool Enable { get; set; } = true;
    public int Layer { get; set; } = 5;
    public bool ShowIcon { get; set; } = true;
    public float IconScale { get; set; } = 0.75f;
    public bool ShowTooltip { get; set; } = true;
    public Vector4 TooltipColor { get; set; } = KnownColor.MediumPurple.AsVector4();
    
    [IconSelection(60961)]
    public uint SelectedIcon { get; set; } = 60961;
}

public class Pet : ModuleBase
{
    public override ModuleName ModuleName => ModuleName.Pets;
    public override IModuleConfig Configuration { get; protected set; } = new PetConfig();

    protected override bool ShouldDrawMarkers(Map map)
    {
        if (!IsPlayerInCurrentMap(map)) return false;

        return base.ShouldDrawMarkers(map);
    }
    
    protected override void DrawMarkers(Viewport viewport, Map map)
    {
        foreach (var obj in Service.PartyList)
        {
            DrawPet(obj.ObjectId, viewport, map);
        }

        if (Service.PartyList.Length is 0 && Service.ClientState.LocalPlayer is { } player)
        {
            DrawPet(player.ObjectId, viewport, map);
        }
    }
    
    private void DrawPet(uint ownerID, Viewport viewport, Map map)
    {
        var config = GetConfig<PetConfig>();
        
        foreach (var obj in OwnedPets(ownerID))
        {
            DrawUtilities.DrawMapIcon(new MappyMapIcon
            {
                IconId = config.SelectedIcon,
                ObjectPosition = new Vector2(obj.Position.X, obj.Position.Z),
                
                Tooltip = obj.Name.TextValue,
            }, config, viewport, map);
        }
    }
    
    private static IEnumerable<GameObject> OwnedPets(uint objectID) => Service.ObjectTable
        .Where(obj => obj.OwnerId == objectID)
        .Where(obj => obj.ObjectKind == ObjectKind.BattleNpc && IsPetOrChocobo(obj));

    private static bool IsPetOrChocobo(GameObject gameObject) => (BattleNpcSubKind?) (gameObject as BattleNpc)?.SubKind switch
    {
        BattleNpcSubKind.Chocobo => true,
        BattleNpcSubKind.Enemy => false,
        BattleNpcSubKind.None => false,
        BattleNpcSubKind.Pet => true,
        _ => false
    };
}