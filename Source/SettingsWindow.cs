using System;
using UnityEngine;
using Verse;

namespace FactionControl
{
    class SettingsWindow : Window
    {
        private Mod mod;

        public override Vector2 InitialSize => new Vector2(900f, 740f);

        public SettingsWindow()
        {
            this.mod = LoadedModManager.GetMod(typeof(Controller));
            doCloseButton = true;
            doCloseX = true;
            forcePause = true;
            absorbInputAroundWindow = true;
        }

        public override void PostClose()
        {
            base.PostClose();
            this.mod.WriteSettings();
        }

        public override void DoWindowContents(Rect canvas)
        {
            this.mod.DoSettingsWindowContents(new Rect(0f, 0f, canvas.width, canvas.height - Window.CloseButSize.y));
        }
    }
}
