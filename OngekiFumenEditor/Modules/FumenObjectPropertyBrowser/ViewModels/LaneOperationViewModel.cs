﻿using Caliburn.Micro;
using Gemini.Modules.Toolbox;
using OngekiFumenEditor.Base;
using OngekiFumenEditor.Base.OngekiObjects.Beam;
using OngekiFumenEditor.Base.OngekiObjects.ConnectableObject;
using OngekiFumenEditor.Base.OngekiObjects.Lane;
using OngekiFumenEditor.Base.OngekiObjects.Wall;
using OngekiFumenEditor.Modules.FumenVisualEditor.Base;
using OngekiFumenEditor.Modules.FumenVisualEditor.ViewModels;
using OngekiFumenEditor.Modules.FumenVisualEditor.ViewModels.OngekiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace OngekiFumenEditor.Modules.FumenObjectPropertyBrowser.ViewModels
{
    public class LaneOperationViewModel : ConnectableObjectOperationViewModel
    {
        public char LaneChar => ConnectableObject.IDShortName[1];
        public char LaneTypeChar => ConnectableObject.IDShortName[0];

        public LaneOperationViewModel(ConnectableObjectBase obj) : base(obj)
        {

        }

        public override ConnectableChildObjectBase GenerateChildObject(bool needNext)
        {
            if (LaneTypeChar == 'W')
            {
                return LaneChar switch
                {
                    'L' => needNext ? new WallLeftNext() : new WallLeftEnd(),
                    'R' => needNext ? new WallRightNext() : new WallRightEnd(),
                    _ => default
                };
            }

            return LaneChar switch
            {
                'L' => needNext ? new LaneLeftNext() : new LaneLeftEnd(),
                'C' => needNext ? new LaneCenterNext() : new LaneCenterEnd(),
                'R' => needNext ? new LaneRightNext() : new LaneRightEnd(),
                _ => default
            };
        }

        public override DisplayObjectViewModelBase GenerateChildObjectViewModel(bool needNext)
        {
            if (LaneTypeChar == 'W')
            {
                return LaneChar switch
                {
                    'L' => needNext ? new WallLeftNextViewModel() : new WallLeftEndViewModel(),
                    'R' => needNext ? new WallRightNextViewModel() : new WallRightEndViewModel(),
                    _ => default
                };
            }

            return LaneChar switch
            {
                'L' => needNext ? new LaneLeftNextViewModel() : new LaneLeftEndViewModel(),
                'C' => needNext ? new LaneCenterNextViewModel() : new LaneCenterEndViewModel(),
                'R' => needNext ? new LaneRightNextViewModel() : new LaneRightEndViewModel(),
                _ => default
            };
        }
    }
}