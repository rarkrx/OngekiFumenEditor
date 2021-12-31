﻿using OngekiFumenEditor.Base.OngekiObjects.Beam;
using OngekiFumenEditor.Base.OngekiObjects.Wall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OngekiFumenEditor.Modules.FumenVisualEditor.ViewModels.OngekiObjects
{
    public abstract class WallChildBaseViewModel<T> : DisplayObjectViewModelBase<T> where T : WallChildBase, new()
    {
        public override double CheckAndAdjustY(double y)
        {
            y = base.CheckAndAdjustY(y);

            if (ReferenceOngekiObject is WallChildBase childBase && childBase.PrevWall is WallBase prevWall)
            {
                var prevY = TGridCalculator.ConvertTGridToY(prevWall.TGrid, EditorViewModel) ?? y;
                if (prevY > y)
                {
                    y = prevY;
                }
            }

            return y;
        }
    }
}