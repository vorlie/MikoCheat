using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikoCheat
{
    public static class Offsets
    {
        // buttons.cs
        public static int attack = 0x185D4C0;
        public static int jump = 0x185D9D0;

        // offsets.cs
        public static int dwViewAngles = 0x1A6DD10;
        public static int dwLocalPlayerPawn = 0x1864CC8;
        public static int dwEntityList = 0x1A01AA8;
        public static int dwViewMatrix = 0x1A63C10;

        // client.dll.cs
        public static int m_iIDEntIndex = 0x1458;
        public static int m_iszPlayerName = 0x660;
        public static int m_flFlashBangTime = 0x13F8;
        public static int m_hPlayerPawn = 0x80C;
        public static int m_iHealth = 0x344;
        public static int m_vOldOrigin = 0x1324;
        public static int m_iTeamNum = 0x3E3;
        public static int m_vecViewOffset = 0xCB0;
        public static int m_lifeState = 0x348;
        public static int m_modelState = 0x170;
        public static int m_pGameSceneNode = 0x328;
        public static int m_fFlags = 0x3EC;
        public static int m_entitySpottedState = 0x23D0;
        public static int m_bSpotted = 0x8;
    }
}
