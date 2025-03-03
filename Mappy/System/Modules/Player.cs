﻿using System;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using KamiLib;
using KamiLib.AutomaticUserInterface;
using Lumina.Excel.GeneratedSheets;
using Mappy.Abstracts;
using Mappy.Models;
using Mappy.Models.Enums;
using Mappy.Utility;
using Mappy.Views.Windows;

namespace Mappy.System.Modules;

[Category("ModuleColors")]
public interface IPlayerColorConfig
{
    [ColorConfig("OutlineColor", 0, 0, 0, 88)]
    public Vector4 OutlineColor { get; set; }
    
    [ColorConfig("FillColor", 163, 219, 255, 80)]
    public Vector4 FillColor { get; set; }
}

[Category("ModuleConfig")]
public class PlayerConfig : IModuleConfig, IIconConfig, IPlayerColorConfig
{
    public bool Enable { get; set; } = true;
    public int Layer { get; set; } = 10;
    public bool ShowIcon { get; set; } = true;
    public float IconScale { get; set; } = 0.40f;
    public Vector4 OutlineColor { get; set; } = new(0.0f, 0.0f, 0.0f, 0.345f);
    public Vector4 FillColor { get; set; } = new(0.639f, 0.858f, 1.0f, 0.313f);
    
    [BoolConfig("ShowCone")]
    public bool ShowCone { get; set; } = true;

    [FloatConfig("ConeRadius", 0.0f, 360.0f)]
    public float ConeRadius { get; set; } = 90.0f;

    [FloatConfig("ConeAngle", 0.0f, 180.0f)]
    public float ConeAngle { get; set; } = 90.0f;

    [FloatConfig("OutlineThickness", 0.5f, 5.0f)]
    public float OutlineThickness { get; set; } = 2.0f;
}

public unsafe class Player : ModuleBase
{
    public override ModuleName ModuleName => ModuleName.Player;
    public override IModuleConfig Configuration { get; protected set; } = new PlayerConfig();

    protected override bool ShouldDrawMarkers(Map map)
    {
        if (!IsPlayerInCurrentMap(map)) return false;
        if (!IsLocalPlayerValid()) return false;
        
        return base.ShouldDrawMarkers(map);
    }

    protected override void DrawMarkers(Viewport viewport, Map map)
    {
        var config = GetConfig<PlayerConfig>();
        
        if(config.ShowCone) DrawLookLine(Service.ClientState.LocalPlayer!);
        if(config.ShowIcon) DrawBluePlayerIcon(Service.ClientState.LocalPlayer!);
    }
    
    private void DrawBluePlayerIcon(GameObject player)
    {
        var config = GetConfig<PlayerConfig>();
        
        DrawUtilities.DrawIconRotated(60443, player, config.IconScale);
    }

    private void DrawLookLine(GameObject player)
    {
        var config = GetConfig<PlayerConfig>();
        if (MappySystem.MapTextureController is not { Ready: true, CurrentMap: var map }) return;
        if (KamiCommon.WindowManager.GetWindowOfType<MapWindow>() is not { } mapWindow) return;
        
        var angle = GetCameraRotation();

        var playerPosition = Position.GetTexturePosition(player, map);
        var drawPosition = mapWindow.Viewport.GetImGuiWindowDrawPosition(playerPosition);

        var lineLength = config.ConeRadius * mapWindow.Viewport.Scale;
        
        var halfConeAngle = DegreesToRadians(config.ConeAngle) / 2.0f;
        
        DrawAngledLineFromCenter(drawPosition, lineLength, angle - halfConeAngle);
        DrawAngledLineFromCenter(drawPosition, lineLength, angle + halfConeAngle);
        DrawLineArcFromCenter(drawPosition, lineLength, angle);
        
        DrawFilledSemiCircle(drawPosition, lineLength, angle);
    }

    private void DrawAngledLineFromCenter(Vector2 center, float lineLength, float angle)
    {
        var config = GetConfig<PlayerConfig>();
        
        var lineSegment = new Vector2(lineLength * MathF.Cos(angle), lineLength * MathF.Sin(angle));
        ImGui.GetWindowDrawList().AddLine(center, center + lineSegment, ImGui.GetColorU32(config.OutlineColor), config.OutlineThickness);
    }

    private void DrawLineArcFromCenter(Vector2 center, float distance, float rotation)
    {
        var config = GetConfig<PlayerConfig>();
        var halfConeAngle = DegreesToRadians(config.ConeAngle) / 2.0f;
        
        var start = rotation - halfConeAngle;
        var stop = rotation + halfConeAngle;
        
        ImGui.GetWindowDrawList().PathArcTo(center, distance, start, stop);
        ImGui.GetWindowDrawList().PathStroke(ImGui.GetColorU32(config.OutlineColor), ImDrawFlags.None, config.OutlineThickness);
    }

    private void DrawFilledSemiCircle(Vector2 center, float distance, float rotation)
    {
        var config = GetConfig<PlayerConfig>();
        var halfConeAngle = DegreesToRadians(config.ConeAngle) / 2.0f;
        
        var startAngle = rotation - halfConeAngle;
        var stopAngle = rotation + halfConeAngle;
        
        var startPosition = new Vector2(distance * MathF.Cos(rotation - halfConeAngle), distance * MathF.Sin(rotation - halfConeAngle));

        ImGui.GetWindowDrawList().PathArcTo(center, distance, startAngle, stopAngle);
        ImGui.GetWindowDrawList().PathLineTo(center);
        ImGui.GetWindowDrawList().PathLineTo(center + startPosition);
        ImGui.GetWindowDrawList().PathFillConvex(ImGui.GetColorU32(config.FillColor));
    }
    
    private float GetCameraRotation() => -DegreesToRadians(AtkStage.GetSingleton()->GetNumberArrayData()[24]->IntArray[3]) - 0.5f * MathF.PI;

    private float DegreesToRadians(float degrees) => MathF.PI / 180.0f * degrees;
}