using Caliburn.Micro;
using Gemini.Framework;
using Gemini.Modules.Toolbox.Services;
using OngekiFumenEditor.Base;
using OngekiFumenEditor.Base.OngekiObjects;
using OngekiFumenEditor.Modules.FumenMetaInfoBrowser;
using OngekiFumenEditor.Modules.FumenVisualEditor.Controls;
using OngekiFumenEditor.Modules.FumenVisualEditor.Controls.OngekiObjects;
using OngekiFumenEditor.Modules.FumenVisualEditor.ViewModels.OngekiObjects;
using OngekiFumenEditor.Modules.FumenVisualEditor.Views;
using OngekiFumenEditor.Parser;
using OngekiFumenEditor.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OngekiFumenEditor.Modules.FumenVisualEditor.ViewModels
{
    [Export(typeof(FumenVisualEditorViewModel))]
    public class FumenVisualEditorViewModel : PersistedDocument
    {
        public struct XGridUnitLineViewModel : INotifyPropertyChanged
        {
            public double X { get; set; }
            public double Unit { get; set; }
            public bool IsCenterLine { get; set; }

            public event PropertyChangedEventHandler PropertyChanged;

            public override string ToString() => $"{X:F4} {Unit} {(IsCenterLine ? "Center" : string.Empty)}";
        }

        private OngekiFumen fumen;
        public OngekiFumen Fumen
        {
            get
            {
                return fumen;
            }
            set
            {
                fumen = value;
                OnFumenObjectLoaded();
                NotifyOfPropertyChange(() => Fumen);
            }
        }

        public double XUnitSize => CanvasWidth / (24 * 2) * UnitCloseSize;
        public double CanvasWidth => VisualDisplayer?.ActualWidth ?? 0;
        public double CanvasHeight => VisualDisplayer?.ActualHeight ?? 0;
        public FumenVisualEditorView View { get; private set; }
        public ObservableCollection<XGridUnitLineViewModel> XGridUnitLineLocations { get; } = new ObservableCollection<XGridUnitLineViewModel>();
        public Panel VisualDisplayer => View?.VisualDisplayer;

        private string errorMessage;
        public string ErrorMessage
        {
            get
            {
                return errorMessage;
            }
            set
            {
                errorMessage = value;
                if (!string.IsNullOrWhiteSpace(value))
                    Log.LogError("Current error message : " + value);
                NotifyOfPropertyChange(() => ErrorMessage);
            }
        }

        private TGrid currentDisplayTimePosition;
        public TGrid CurrentDisplayTimePosition
        {
            get
            {
                return currentDisplayTimePosition;
            }
            set
            {
                currentDisplayTimePosition = value;
                NotifyOfPropertyChange(() => CurrentDisplayTimePosition);
            }
        }

        public override string DisplayName
        {
            get { return base.DisplayName; }
            set
            {
                base.DisplayName = value;
                if (IoC.Get<WindowTitleHelper>() is WindowTitleHelper title)
                {
                    title.TitleContent = base.DisplayName;
                }
            }
        }

        private bool isPreventXAutoClose;
        public bool IsPreventXAutoClose
        {
            get
            {
                return isPreventXAutoClose;
            }
            set
            {
                isPreventXAutoClose = value;
                NotifyOfPropertyChange(() => IsPreventTimelineAutoClose);
            }
        }

        private bool isPreventTimelineAutoClose;
        public bool IsPreventTimelineAutoClose
        {
            get
            {
                return isPreventTimelineAutoClose;
            }
            set
            {
                isPreventTimelineAutoClose = value;
                NotifyOfPropertyChange(() => IsPreventTimelineAutoClose);
            }
        }

        private double unitCloseSize = 4;
        public double UnitCloseSize
        {
            get
            {
                return unitCloseSize;
            }
            set
            {
                unitCloseSize = value;
                RedrawUnitCloseXLines();
                NotifyOfPropertyChange(() => UnitCloseSize);
            }
        }

        private void RedrawUnitCloseXLines()
        {
            XGridUnitLineLocations.Clear();

            var width = CanvasWidth;
            var unitSize = XUnitSize;
            var totalUnitValue = 0d;

            for (double totalLength = width / 2 + unitSize; totalLength < width; totalLength += unitSize)
            {
                totalUnitValue += UnitCloseSize;

                XGridUnitLineLocations.Add(new XGridUnitLineViewModel()
                {
                    X = totalLength,
                    Unit = totalUnitValue,
                    IsCenterLine = false
                });
                XGridUnitLineLocations.Add(new XGridUnitLineViewModel()
                {
                    X = (width / 2) - (totalLength - (width / 2)),
                    Unit = -totalUnitValue,
                    IsCenterLine = false
                });
            }
            XGridUnitLineLocations.Add(new XGridUnitLineViewModel()
            {
                X = width / 2,
                IsCenterLine = true
            });
        }

        protected override void OnViewLoaded(object v)
        {
            base.OnViewLoaded(v);
            var view = v as FumenVisualEditorView;

            View = view;
            RedrawUnitCloseXLines();
        }

        private Task InitalizeVisualData()
        {
            var displayableObjects = fumen.GetAllDisplayableObjects();
            //add all displayable object.
            foreach (var obj in displayableObjects)
            {
                var displayObject = Activator.CreateInstance(obj.ModelViewType) as DisplayObjectViewModelBase;
                if (ViewCreateHelper.CreateView(displayObject) is OngekiObjectViewBase view && obj is IOngekiObject o)
                {
                    view.ViewModel.ReferenceOngekiObject = o;
                    view.ViewModel.EditorViewModel = this;
                    VisualDisplayer.Children.Add(view);
                }
            }
            return Task.CompletedTask;
        }

        protected override async Task DoLoad(string filePath)
        {
            using var _ = StatusNotifyHelper.BeginStatus("Fumen loading : " + filePath);
            Log.LogInfo($"FumenVisualEditorViewModel DoLoad() : {filePath}");
            using var fileStream = File.OpenRead(filePath);
            Fumen = await IoC.Get<IOngekiFumenParser>().ParseAsync(fileStream);
            await InitalizeVisualData();
            IsDirty = true;
        }

        private void OnFumenObjectLoaded()
        {
            IoC.Get<IFumenMetaInfoBrowser>().Fumen = Fumen;   
        }

        protected override async Task DoNew()
        {
            Fumen = new OngekiFumen();
            await InitalizeVisualData();
            Log.LogInfo($"FumenVisualEditorViewModel DoNew()");
        }

        protected override async Task DoSave(string filePath)
        {
            using var _ = StatusNotifyHelper.BeginStatus("Fumen saving : " + filePath);
            await File.WriteAllTextAsync(filePath, fumen.Serialize());
            Log.LogInfo($"FumenVisualEditorViewModel DoSave() : {filePath}");
        }

        public void OnNewObjectAdd(DisplayObjectViewModelBase viewModel)
        {
            var view = ViewCreateHelper.CreateView(viewModel);
            fumen.AddObject(viewModel.ReferenceOngekiObject);

            VisualDisplayer.Children.Add(view);
            viewModel.EditorViewModel = this;

            Log.LogInfo($"create new display object: {viewModel.ReferenceOngekiObject.Name}");
        }

        public void DeleteSelectedObjects()
        {
            var selectedObject = VisualDisplayer.Children.OfType<OngekiObjectViewBase>().Where(x => x.IsSelected).ToArray();
            foreach (var obj in selectedObject)
            {
                VisualDisplayer.Children.Remove(obj);
                fumen.AddObject(obj.ViewModel?.ReferenceOngekiObject);
            }
            Log.LogInfo($"deleted {selectedObject.Length} objects.");
        }

        public void CopySelectedObjects()
        {

        }

        public void PasteCopiesObjects()
        {

        }

        public void OnKeyDown(ActionExecutionContext e)
        {
            if (e.EventArgs is KeyEventArgs arg)
            {
                if (arg.Key == Key.Delete)
                {
                    DeleteSelectedObjects();
                }
            }
        }
    }
}
