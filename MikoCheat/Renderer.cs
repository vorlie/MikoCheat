﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ClickableTransparentOverlay;
using ImGuiNET;

namespace MikoCheat
{
    public class Renderer : Overlay
    {
        public bool aimBot = true;
        public bool targetTeam = false;
        public bool antiFlash = false;
        public bool boneESP = true;
        public bool aimLockCircle = true;
        public bool autoBunnyHop = false;
        public float headSizeFloat = 5;

        public float Radius = 10;

        public Vector2 screenSize = new Vector2(ScreenResolution.GetResolution().width, ScreenResolution.GetResolution().height);
        public Vector4 aimbotRadiusColor = new Vector4(1, 1, 1, 1);

        private ConcurrentQueue<Entity> entities = new ConcurrentQueue<Entity>();
        public Entity localPlayer = new Entity();
        private readonly object entityLock = new object();

        ImDrawListPtr drawList;
        Vector4 teamColor = new Vector4(0, 1, 1, 1);
        Vector4 enemyColor = new Vector4(1, 0, 0, 1);
        Vector4 boneColor = new Vector4(1, 1, 1, 1);
        Vector4 nameColor = new Vector4(1, 1, 1, 1);
        Vector4 nameColorShadow = new Vector4(0, 0, 0, 1);

        float boneThickness = 4;

        protected override void Render()
        {
            if (ImGui.Begin("MikoCheat"))
            {
                if (ImGui.BeginTabBar("Tabs"))
                {
                    // First Tab: Checkboxes and Sliders
                    if (ImGui.BeginTabItem("Settings"))
                    {
                        ImGui.Checkbox("Aimlock", ref aimBot);
                        ImGui.Checkbox("Target Teammates", ref targetTeam);
                        ImGui.Checkbox("AntiFlash", ref antiFlash);
                        ImGui.Checkbox("AimLock Circle", ref aimLockCircle);
                        ImGui.Checkbox("Auto Bunny Hop (Hold Space)", ref autoBunnyHop);
                        ImGui.Checkbox("ESP", ref boneESP);
                        ImGui.SliderFloat("Aimlock Radius", ref Radius, 10, 300);
                        ImGui.SliderFloat("Bone Head Size", ref headSizeFloat, 3, 10);
                        ImGui.SliderFloat("Bone Thickness", ref boneThickness, 4, 300);
                        if (ImGui.Button("Panic Button"))
                        {
                            PanicTerminate();
                        }
                        ImGui.EndTabItem();
                    }

                    // Second Tab: Color Pickers
                    if (ImGui.BeginTabItem("Colors"))
                    {
                        if (ImGui.CollapsingHeader("Radius Circle Color"))
                            ImGui.ColorPicker4("##circlecolor", ref aimbotRadiusColor);
                        if (ImGui.CollapsingHeader("ESP Team Color"))
                            ImGui.ColorPicker4("##espteamcolor", ref teamColor);
                        if (ImGui.CollapsingHeader("ESP Enemy Color"))
                            ImGui.ColorPicker4("##espenemycolor", ref enemyColor);
                        if (ImGui.CollapsingHeader("ESP Bone Color"))
                            ImGui.ColorPicker4("##espbonecolor", ref boneColor);
                        ImGui.EndTabItem();
                    }

                    ImGui.EndTabBar();
                }
                ImGui.End();
            }

            DrawOverlay();
            drawList = ImGui.GetWindowDrawList();
            if (aimLockCircle) { DrawAimLockCircle(); }
            if (boneESP)
            {
                foreach (var entity in entities)
                {
                    if (EntityOnScreen(entity))
                    {
                        DrawHealthBar(entity);
                        DrawBox(entity);
                        DrawLines(entity);
                        DrawBones(entity);
                        DrawName(entity, 20);
                    }
                }

            }
        }

        private void PanicTerminate()
        {
            Environment.Exit(0); // Terminate the application immediately
        }
        public void UpdateEntities(IEnumerable<Entity> newEntities)
        {
            entities = new ConcurrentQueue<Entity>(newEntities);
        }

        public void UpdateLocalPlayer(Entity newEntity)
        {
            lock (entityLock)
            {
                localPlayer = newEntity;
            }
        }

        bool EntityOnScreen(Entity entity)
        {
            if (entity.position2D.X > 0 && entity.position2D.X < screenSize.X && entity.position2D.Y > 0 && entity.position2D.Y < screenSize.Y)
            {
                return true;
            }
            return false;
        }
        void DrawAimLockCircle()
        {
            drawList.AddCircle(new Vector2(screenSize.X / 2, screenSize.Y / 2), Radius, ImGui.ColorConvertFloat4ToU32(aimbotRadiusColor));
        }

        private void DrawName(Entity entity, int yOffset)
        {
            Vector2 textLocation = new Vector2(entity.viewPosition2D.X,entity.viewPosition2D.Y - yOffset);
            Vector2 textLocation2 = new Vector2(entity.viewPosition2D.X + 1, entity.viewPosition2D.Y - yOffset + 1);
            drawList.AddText(textLocation2, ImGui.ColorConvertFloat4ToU32(nameColorShadow), $"{entity.name}");
            drawList.AddText(textLocation, ImGui.ColorConvertFloat4ToU32(nameColor), $"{entity.name}");
            
        }

        private void DrawHealthBar(Entity entity)
        {
            float entityHeight = entity.position2D.Y - entity.viewPosition2D.Y;

            float boxLeft = entity.viewPosition2D.X - entityHeight / 3;
            float boxRight = entity.position2D.X + entityHeight / 3;
            float barPercentWidth = 0.05f;
            float barPixelWidth = barPercentWidth * (boxRight - boxLeft);
            float barHeight = entityHeight * (entity.health / 100f);
            Vector2 barTop = new Vector2(boxLeft - barPixelWidth, entity.position2D.Y - barHeight);
            Vector2 barBottom = new Vector2(boxLeft, entity.position2D.Y);

            Vector4 barColor = new Vector4(0, 1, 0, 1);
            drawList.AddRectFilled(barTop, barBottom, ImGui.ColorConvertFloat4ToU32(barColor));
        }

        private void DrawBones(Entity entity)
        {
            uint uintColor = ImGui.ColorConvertFloat4ToU32(boneColor);

            float currentBoneThickness = boneThickness / entity.distance;
            float headSize = headSizeFloat;

            drawList.AddLine(entity.bones2d[1], entity.bones2d[2], uintColor, currentBoneThickness);
            drawList.AddLine(entity.bones2d[1], entity.bones2d[3], uintColor, currentBoneThickness);
            drawList.AddLine(entity.bones2d[1], entity.bones2d[6], uintColor, currentBoneThickness);
            drawList.AddLine(entity.bones2d[3], entity.bones2d[4], uintColor, currentBoneThickness);
            drawList.AddLine(entity.bones2d[6], entity.bones2d[7], uintColor, currentBoneThickness);
            drawList.AddLine(entity.bones2d[4], entity.bones2d[5], uintColor, currentBoneThickness);
            drawList.AddLine(entity.bones2d[7], entity.bones2d[8], uintColor, currentBoneThickness);
            drawList.AddLine(entity.bones2d[1], entity.bones2d[0], uintColor, currentBoneThickness);
            drawList.AddLine(entity.bones2d[0], entity.bones2d[9], uintColor, currentBoneThickness);
            drawList.AddLine(entity.bones2d[9], entity.bones2d[11], uintColor, currentBoneThickness);
            drawList.AddLine(entity.bones2d[9], entity.bones2d[10], uintColor, currentBoneThickness);
            drawList.AddLine(entity.bones2d[11], entity.bones2d[12], uintColor, currentBoneThickness);
            drawList.AddCircle(entity.bones2d[2], headSize + currentBoneThickness, uintColor);

        }

        private void DrawBox(Entity entity)
        {
            float entityHeight = entity.position2D.Y - entity.viewPosition2D.Y;

            Vector2 rectTop = new Vector2(entity.viewPosition2D.X - entityHeight / 3, entity.viewPosition2D.Y);
            Vector2 rectBottom = new Vector2(entity.position2D.X + entityHeight / 3, entity.position2D.Y);

            Vector4 boxColor = localPlayer.team == entity.team ? teamColor : enemyColor;

            drawList.AddRect(rectTop, rectBottom, ImGui.ColorConvertFloat4ToU32(boxColor));
        }

        private void DrawLines(Entity entity)
        {
            Vector4 lineColor = localPlayer.team == entity.team ? teamColor : enemyColor;

            drawList.AddLine(new Vector2(screenSize.X / 2, screenSize.Y), entity.position2D, ImGui.ColorConvertFloat4ToU32(lineColor));
        }

        void DrawOverlay()
        {
            ImGui.SetNextWindowSize(screenSize);
            ImGui.SetNextWindowPos(new Vector2(0, 0));
            ImGui.Begin("overlay", ImGuiWindowFlags.NoDecoration
                | ImGuiWindowFlags.NoBackground
                | ImGuiWindowFlags.NoBringToFrontOnFocus
                | ImGuiWindowFlags.NoMove
                | ImGuiWindowFlags.NoInputs
                | ImGuiWindowFlags.NoCollapse
                | ImGuiWindowFlags.NoScrollbar
                | ImGuiWindowFlags.NoScrollWithMouse
                );
        }
    }
}