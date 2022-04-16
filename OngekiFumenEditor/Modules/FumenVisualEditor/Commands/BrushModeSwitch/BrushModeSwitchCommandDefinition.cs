﻿using Gemini.Framework.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OngekiFumenEditor.Modules.FumenVisualEditor.Commands.BrushModeSwitch
{
    [CommandDefinition]
    public class BrushModeSwitchCommandDefinition : CommandDefinition
    {
        public override string Name => "Toolbar.BrushModeSwitch";

        public override string Text => "切换笔刷模式";

        public override string ToolTip => "如果开启笔刷模式,避免大部分物件拖拉操作";

        public override Uri IconSource => new Uri("pack://application:,,,/OngekiFumenEditor;component/Resources/Icons/icons8-paint-brush-16.png");
    }
}