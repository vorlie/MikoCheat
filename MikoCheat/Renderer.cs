using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ClickableTransparentOverlay;
using ImGuiNET;

namespace MikoCheat
{
    public class Renderer : Overlay
    {
        public string version = "v1.2";
        public bool aimBot = true;
        public bool targetTeam = false;
        public bool antiFlash = false;
        public bool boneESP = true;
        public bool aimBotCircle = true;
        public bool autoBunnyHop = false;
        public float headSizeFloat = 5;
        public float smoothingFactor = 0.5f;
        public bool menuVisible = true;
        private bool insertPressed = false;
        private DateTime lastKeyCheck = DateTime.MinValue;

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

        [DllImport("user32.dll")]
        public static extern int GetAsyncKeyState(int vKey);

        protected override void Render()
        {
            if ((DateTime.Now - lastKeyCheck).TotalMilliseconds >= 50)
            {
                lastKeyCheck = DateTime.Now;

                int INSERT = 0x2D; // Virtual key code for Insert
                if ((GetAsyncKeyState(INSERT) & 0x8000) != 0)
                {
                    if (!insertPressed)
                    {
                        menuVisible = !menuVisible;
                        insertPressed = true;
                    }
                }
                else
                {
                    insertPressed = false;
                }
            }

            if (menuVisible)
            {
                if (ImGui.Begin($"MikoCheat {version}"))
                {
                    ImGui.TextColored(new Vector4(1, 0, 0, 1),
                        "Disclaimer: Auto Bunny Hop and Anti Flash modify game memory and may be detected.");
                    ImGui.TextColored(new Vector4(1, 1, 1, 1),
                        "Note: Use the Insert key to toggle the menu.");

                    if (ImGui.BeginTabBar("Tabs"))
                    {   
                        // Presets Section
                        if (ImGui.BeginTabItem("Presets"))
                        {
                            if (ImGui.Button("Legit"))
                            {
                                aimBot = true;
                                targetTeam = false;
                                antiFlash = false;
                                autoBunnyHop = false;
                                smoothingFactor = 0.5f;
                                Radius = 10f;
                            }
                            if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Enable a configuration optimized for Legit Bot."); }

                            if (ImGui.Button("ESP Only"))
                            {
                                aimBot = false;
                                antiFlash = false;
                                autoBunnyHop = false;
                                boneESP = true;
                            }
                            if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Enable ESP only with no active aimbot or cheats."); }
                            ImGui.EndTabItem();
                        }

                        // First Tab: Checkboxes and Sliders
                        if (ImGui.BeginTabItem("Settings"))
                        {
                            ImGui.Checkbox("Aimbot", ref aimBot);
                            if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Enable or disable the aimbot feature."); }

                            ImGui.Checkbox("Target Teammates", ref targetTeam);
                            if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Toggle whether the aimbot targets teammates."); }

                            ImGui.Checkbox("AntiFlash", ref antiFlash);
                            if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Prevent flashbang effects from blinding you."); }

                            ImGui.Checkbox("Aimbot Circle", ref aimBotCircle);
                            if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Show or hide the aimbot circle."); }

                            ImGui.Checkbox("Auto Bunny Hop (Hold Space)", ref autoBunnyHop);
                            if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Automatically jump repeatedly when holding the spacebar."); }

                            ImGui.Checkbox("ESP", ref boneESP);
                            if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Toggle bone ESP to show the skeletons of entities."); }

                            ImGui.SliderFloat("Aimbot Radius", ref Radius, 10, 300);
                            if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Set the radius within which the aimbot will target entities."); }

                            ImGui.SliderFloat("Bone Head Size", ref headSizeFloat, 3, 10);
                            if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Adjust the size of the ESP head circles."); }

                            ImGui.SliderFloat("Bone Thickness", ref boneThickness, 4, 300);
                            if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Set the thickness of the ESP bone lines."); }

                            ImGui.SliderFloat("Smoothing Factor", ref smoothingFactor, 0.1f, 1.0f);
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.SetTooltip("Adjust the smoothness of the aim transition. \nA lower value makes it smoother, while a higher value makes it snappier.");
                            }

                            if (ImGui.Button("Panic Button")) { PanicTerminate(); }
                            if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Immediately terminates the cheat process."); }

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
            }

            DrawOverlay();
            drawList = ImGui.GetWindowDrawList();
            if (aimBotCircle) { DrawAimLockCircle(); }
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
