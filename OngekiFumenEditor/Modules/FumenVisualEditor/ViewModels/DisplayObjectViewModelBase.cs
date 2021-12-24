﻿using Caliburn.Micro;
using OngekiFumenEditor.Base;
using OngekiFumenEditor.Modules.FumenVisualEditor.Converters;
using OngekiFumenEditor.Modules.FumenVisualEditor.Views;
using OngekiFumenEditor.Utils;
using OngekiFumenEditor.Utils.Attributes;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace OngekiFumenEditor.Modules.FumenVisualEditor.ViewModels
{
    public abstract class DisplayObjectViewModelBase : PropertyChangedBase, IViewAware
    {
        protected OngekiObjectBase referenceOngekiObject;

        public virtual OngekiObjectBase ReferenceOngekiObject
        {
            get { return referenceOngekiObject; }
            set
            {
                referenceOngekiObject = value;
                NotifyOfPropertyChange(() => ReferenceOngekiObject);
                NotifyOfPropertyChange(() => CanMoveX);
                NotifyOfPropertyChange(() => IsTimelineObject);
            }
        }

        public event EventHandler<ViewAttachedEventArgs> ViewAttached;

        public object View { get; private set; }

        public object Context { get; private set; }

        /// <summary>
        /// 表示此物件是否能设置水平位置(即此物件是否支持XGrid)
        /// </summary>
        public bool CanMoveX => ReferenceOngekiObject is IHorizonPositionObject;

        /// <summary>
        /// 表示此物件是否能设置时间轴位置(即此物件是否支持TGrid)
        /// </summary>
        public bool IsTimelineObject => ReferenceOngekiObject is ITimelineObject;

        private FumenVisualEditorViewModel editorViewModel;
        public FumenVisualEditorViewModel EditorViewModel
        {
            get
            {
                return editorViewModel;
            }
            set
            {
                editorViewModel = value;
                NotifyOfPropertyChange(() => EditorViewModel);
            }
        }

        public virtual void MoveCanvas(Point relativePoint)
        {
            if (EditorViewModel is FumenVisualEditorViewModel hostModelView)
            {
                if (ReferenceOngekiObject is ITimelineObject timeObj)
                {
                    var ry = CheckAndAdjustY(relativePoint.Y);
                    if (TGridCalculator.ConvertYToTGrid(ry, hostModelView) is TGrid tGrid)
                    {
                        timeObj.TGrid = (tGrid);
                        //Log.LogInfo($"Y: {ry} , TGrid: {timeObj.TGrid}");
                    }
                }

                if (ReferenceOngekiObject is IHorizonPositionObject posObj)
                {
                    var x = CheckAndAdjustX(relativePoint.X);
                    var xgridValue = (x - hostModelView.CanvasWidth / 2) / (hostModelView.XUnitSize / hostModelView.UnitCloseSize);
                    var near = xgridValue > 0 ? Math.Floor(xgridValue + 0.5) : Math.Ceiling(xgridValue - 0.5);
                    posObj.XGrid.Unit = Math.Abs(xgridValue - near) < 0.00001 ? (int)near : (float)xgridValue;
                    //Log.LogInfo($"xgridValue : {xgridValue:F4} , posObj.XGrid.Unit : {posObj.XGrid.Unit}");
                }
            }
            else
            {
                Log.LogInfo("Can't move object in canvas because it's not ready.");
            }
        }

        public double CheckAndAdjustY(double y)
        {
            var s = y;
            y = EditorViewModel.CanvasHeight - y;
            var enableMagneticAdjust = !(editorViewModel?.IsPreventTimelineAutoClose ?? false);
            var mid = enableMagneticAdjust ? editorViewModel?.TGridUnitLineLocations?.Select(z => new
            {
                distance = Math.Abs(z.Y - s),
                y = z.Y
            })?.Where(z => z.distance < 4)?.OrderBy(x => x.distance)?.ToList() : default;
            var nearestUnitLine = mid?.FirstOrDefault();
            var fin = nearestUnitLine != null ? (EditorViewModel.CanvasHeight - nearestUnitLine.y) : y;
            //Log.LogInfo($"before y={y:F2} ,select:({nearestUnitLine?.y:F2}) ,fin:{fin:F2}");
            return fin;
        }

        public double CheckAndAdjustX(double x)
        {
            //todo 基于二分法查询最近
            var enableMagneticAdjust = !(editorViewModel?.IsPreventXAutoClose ?? false);
            var mid = enableMagneticAdjust ? editorViewModel?.XGridUnitLineLocations?.Select(z => new
            {
                distance = Math.Abs(z.X - x),
                x = z.X
            })?.Where(z => z.distance < 4)?.OrderBy(x => x.distance)?.ToList() : default;
            var nearestUnitLine = mid?.FirstOrDefault();
            //Log.LogInfo($"nearestUnitLine in:{x:F2} distance:{nearestUnitLine?.distance:F2} x:{nearestUnitLine?.x:F2}");
            return nearestUnitLine != null ? nearestUnitLine.x : x;
        }

        protected virtual void OnAttachedView(object view)
        {
            var element = view as FrameworkElement;

            if (ReferenceOngekiObject is IHorizonPositionObject)
            {
                var xBinding = new MultiBinding()
                {
                    Converter = new XGridCanvasConverter(),
                };
                xBinding.Bindings.Add(new Binding("ReferenceOngekiObject.XGrid.Unit"));
                xBinding.Bindings.Add(new Binding("EditorViewModel"));
                element.SetBinding(Canvas.LeftProperty, xBinding);
            }

            if (ReferenceOngekiObject is ITimelineObject)
            {
                var xBinding = new MultiBinding()
                {
                    Converter = new TGridCanvasConverter(),
                };
                xBinding.Bindings.Add(new Binding("ReferenceOngekiObject.TGrid.Grid"));
                xBinding.Bindings.Add(new Binding("ReferenceOngekiObject.TGrid.Unit"));
                xBinding.Bindings.Add(new Binding("ReferenceOngekiObject.TGrid"));
                xBinding.Bindings.Add(new Binding("EditorViewModel"));
                element.SetBinding(Canvas.TopProperty, xBinding);
            }

            Refresh();
        }

        public void AttachView(object view, object context = null)
        {
            View = view;
            Context = context;

            OnAttachedView(View);
        }

        public object GetView(object context = null) => View;
    }

    public abstract class DisplayObjectViewModelBase<T> : DisplayObjectViewModelBase where T : OngekiObjectBase, new()
    {
        public override OngekiObjectBase ReferenceOngekiObject
        {
            get
            {
                if (referenceOngekiObject is null)
                {
                    ReferenceOngekiObject = new T();
                }
                return base.ReferenceOngekiObject;
            }
            set
            {
                base.ReferenceOngekiObject = value;
            }
        }
    }

    [MapToView(ViewType = typeof(DisplayTextLineObjectViewBase))]
    public abstract class DisplayTextLineObjectViewModelBase<T> : DisplayObjectViewModelBase<T> where T : OngekiObjectBase, new()
    {
        public new T ReferenceOngekiObject
        {
            get
            {
                return base.ReferenceOngekiObject as T;
            }
            set
            {
                base.ReferenceOngekiObject = value;
            }
        }

        public abstract Brush DisplayBrush { get; }
        public virtual string DisplayName => ReferenceOngekiObject.IDShortName;
        public abstract BindingBase DisplayValueBinding { get; }

        protected override void OnAttachedView(object v)
        {
            base.OnAttachedView(v);

            if (v is DisplayTextLineObjectViewBase view)
                view.displayValueTextBlock.SetBinding(TextBlock.TextProperty, DisplayValueBinding);
        }
    }
}
