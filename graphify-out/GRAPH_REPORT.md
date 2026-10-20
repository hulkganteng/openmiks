# Graph Report - vmix clone  (2026-10-17)

## Corpus Check
- 478 files · ~1,031,640 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 359 file(s) not represented in the graph (top: .cache 187, .editorconfig 90, .baml 24)

## Summary
- 3207 nodes · 5845 edges · 265 communities (254 shown, 11 thin omitted)
- Extraction: 97% EXTRACTED · 3% INFERRED · 0% AMBIGUOUS · INFERRED: 180 edges (avg confidence: 0.85)
- Token cost: 0 input · 0 output

## Community Hubs (Navigation)
- system_windows
- DebuggerNonUserCodeAttribute
- Window
- InputItem
- ProductionEngine
- Window
- RoutedEventArgs
- UserControl
- Window
- Window
- Window
- StreamingEngine
- PlayoutEngine
- VMixConfiguration
- CameraCaptureService
- AddInputDialog
- NdiSrtService
- InstantReplayEngine
- InputDisplay
- AudioMixerEngine
- ReplayEvent
- Window
- .RunAll
- StreamDeckAndTallyService
- RoutedEventArgs
- INotifyPropertyChanged
- StingerTransition
- ScreenCaptureService
- VirtualSetLayer
- MultiViewWindow
- TitleTemplate
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- InputType
- ChromaKeyEffect
- ReplayWindow
- VirtualSet
- CsvDataSource
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- ReplayEvent
- TransitionEffect
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- Window
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- IDataSource
- .SetTBar
- 🏛️ Arsitektur & Roadmap Fitur Lengkap (8 Phase)
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- ObsTransformGizmo
- WebServerService
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- SelectionChangedEventArgs
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- VMixClone.csproj
- DataSourceBase
- INotifyPropertyChanged
- InputCategory
- AGENTS.md
- .ExecuteApiFunction
- ProgramSurface
- Application
- 🔌 Backend Capture Nyata (bukan simulasi)
- SrtStreamConfig
- StackPanel
- TextBox
- ProgressBar
- PtzProtocol
- StreamStatus
- PART_Track
- RecDot
- AutoAudioMixingCheck
- FtbMask
- PreviewGizmo
- Bd
- WebUrlDisplay

## God Nodes (most connected - your core abstractions)
1. `ProductionEngine` - 116 edges
2. `Window` - 114 edges
3. `MainWindow` - 96 edges
4. `InputItem` - 91 edges
5. `Window` - 53 edges
6. `UserControl` - 50 edges
7. `InputDisplay` - 39 edges
8. `Window` - 39 edges
9. `VMixConfiguration` - 38 edges
10. `AddInputDialog` - 37 edges

## Surprising Connections (you probably didn't know these)
- `🔌 Backend Capture Nyata (bukan simulasi)` --references--> `AudioMeterService`  [INFERRED]
  README.md → Services/AudioMeterService.cs
- `🔌 Backend Capture Nyata (bukan simulasi)` --references--> `CameraCaptureService`  [INFERRED]
  README.md → Services/CameraCaptureService.cs
- `🔌 Backend Capture Nyata (bukan simulasi)` --references--> `NdiDiscoveryService`  [INFERRED]
  README.md → Services/NdiDiscoveryService.cs
- `🔌 Backend Capture Nyata (bukan simulasi)` --references--> `PlayoutEngine`  [INFERRED]
  README.md → Services/PlayoutEngine.cs
- `🔌 Backend Capture Nyata (bukan simulasi)` --references--> `ProgramSurface`  [INFERRED]
  README.md → Services/ProgramSurface.cs

## Import Cycles
- None detected.

## Communities (265 total, 11 thin omitted)

### Community 1 - "system_windows"
Cohesion: 0.13
Nodes (8): VMixClone.Services, VMixClone.Models, VMixClone, VMixClone.Effects, VMixClone.TitleLibrary, VMixClone.Tests, VMixClone.Views, VMixClone.Controls

### Community 2 - "DebuggerNonUserCodeAttribute"
Cohesion: 0.07
Nodes (4): App, XamlGeneratedNamespace, GeneratedInternalTypeHelper, GeneratedInternalTypeHelper

### Community 3 - "Window"
Cohesion: 0.08
Nodes (28): CategoryCombo, ChromaKeyCheck, HeaderTitle, HeadlineBox, MuteCheck, PtzIpBox, PtzPanel, PtzStatusText (+20 more)

### Community 4 - "InputItem"
Cohesion: 0.03
Nodes (61): InputItem, AudioBusses, AudioCapture, AudioDeviceName, AudioLevelL, AudioLevelR, CameraDeviceName, CameraFps (+53 more)

### Community 5 - "ProductionEngine"
Cohesion: 0.04
Nodes (51): ProductionEngine, Api, AudioMeter, AudioMixer, Automation, BusALevelL, BusALevelR, BusAMuted (+43 more)

### Community 6 - "Window"
Cohesion: 0.06
Nodes (55): AudioLevelL, AudioLevelR, CategoryHex, IsOverlay1, IsOverlay2, IsOverlay3, IsOverlay4, IsPreview (+47 more)

### Community 7 - "RoutedEventArgs"
Cohesion: 0.06
Nodes (16): BusAMuteBtn, CatAllBtn, ExternalBtn, FtbBtn, MasterMuteBtn, Overlay1Btn, Overlay2Btn, Overlay3Btn (+8 more)

### Community 8 - "UserControl"
Cohesion: 0.05
Nodes (60): AudioPlayer, AudioRoot, AudioTimeText, AudioTitleText, BuiltInTitle, CameraImage, CameraNameText, CameraPlaceholder (+52 more)

### Community 9 - "Window"
Cohesion: 0.10
Nodes (25): CameraNumber, Duration, Id, InPoint, OutPoint, Speed, Text, CameraAnglesList (+17 more)

### Community 10 - "Window"
Cohesion: 0.17
Nodes (16): AudioBusACombo, AudioMasterCombo, FrameRateCombo, MicAux1Combo, MicAux2Combo, MonitorCombo, OutputResCombo, RecAudioBitrateCombo (+8 more)

### Community 11 - "Window"
Cohesion: 0.07
Nodes (49): AudioChannelCombo, AudioDeviceCombo, AudioDevicePanel, AudioLoopCheck, AudioPanel, AudioPathBox, Bd, CameraDeviceCombo (+41 more)

### Community 12 - "StreamingEngine"
Cohesion: 0.08
Nodes (17): ProgramFrameSource, IFrameSource, RecordingEngine, CurrentFilePath, Duration, FileSizeFormatted, IsRecording, TotalBytesWritten (+9 more)

### Community 13 - "PlayoutEngine"
Cohesion: 0.07
Nodes (23): PhotoSlideshowModel, CurrentIndex, ImagePaths, SlideDurationSeconds, PlayoutMode, Slideshow, VideoList, VideoListModel (+15 more)

### Community 14 - "VMixConfiguration"
Cohesion: 0.07
Nodes (30): VMixConfiguration, AspectRatio, AudioBusADevice, AudioMasterDevice, AudioSampleRate, AutoAudioMixing, EnableNdi, EnableSrt (+22 more)

### Community 15 - "CameraCaptureService"
Cohesion: 0.11
Nodes (13): AudioMeterService, DeviceName, IsRunning, LastError, LevelL, LevelR, CameraCaptureService, Bitmap (+5 more)

### Community 17 - "AddInputDialog"
Cohesion: 0.10
Nodes (5): AddInputDialog, ResultItem, SettingsWindow, VirtualSetEditorWindow, VirtualSetResult

### Community 18 - "NdiSrtService"
Cohesion: 0.10
Nodes (16): NdiDiscoveryService, IsScanning, LastError, QueryBytes, RuntimePresent, Sources, NdiSourceInfo, IpAddress (+8 more)

### Community 19 - "InstantReplayEngine"
Cohesion: 0.09
Nodes (11): InstantReplayEngine, Cameras, ChannelA, ChannelB, CurrentListIndex, Events, IsRecording, SessionTime (+3 more)

### Community 20 - "InputDisplay"
Cohesion: 0.10
Nodes (6): InputDisplay, IsOverlayMode, Item, VirtualSetView, AvailableInputs, VirtualSet

### Community 21 - "AudioMixerEngine"
Cohesion: 0.09
Nodes (21): AudioBus, AssignedOutputDevice, DbLevel, IsMuted, MeterLevelL, MeterLevelR, Name, Tag (+13 more)

### Community 22 - "ReplayEvent"
Cohesion: 0.13
Nodes (12): PlaylistItem, Duration, FilePath, Title, ReplayChannel, ActiveCamera, ChannelName, CurrentPosition (+4 more)

### Community 23 - "Window"
Cohesion: 0.11
Nodes (22): PlaceholderType, CameraPresetBar, PropChromaCheck, PropLayerName, PropOpacitySlider, PropSimilaritySlider, PropSmoothnessSlider, PropSpillSlider (+14 more)

### Community 24 - ".RunAll"
Cohesion: 0.15
Nodes (4): PresetService, ProductionEngineTests, SyntheticFrameSource, FramesServed

### Community 25 - "StreamDeckAndTallyService"
Cohesion: 0.06
Nodes (12): StreamDeckAndTallyService, ActiveComPort, DeckPort, IsRunning, TcpServerService, IsRunning, Port, IvMixAPI (+4 more)

### Community 26 - "RoutedEventArgs"
Cohesion: 0.10
Nodes (3): PreviewObsEditToggle, MainWindow, ToggleButton

### Community 27 - "INotifyPropertyChanged"
Cohesion: 0.10
Nodes (14): ScriptingAndTriggersEngine, Scripts, Triggers, ScriptItem, Code, IsRunning, LastRun, Name (+6 more)

### Community 28 - "StingerTransition"
Cohesion: 0.29
Nodes (6): StingerTransition, AssignedInputNumber, CutPointMs, DurationMs, Name, StingerNumber

### Community 29 - "ScreenCaptureService"
Cohesion: 0.29
Nodes (3): BITMAPINFO, BITMAPINFOHEADER, ScreenCaptureService

### Community 30 - "VirtualSetLayer"
Cohesion: 0.11
Nodes (17): VirtualSetLayer, AssignedInputNumber, ChromaKeyColor, ChromaSimilarity, ChromaSmoothness, ChromaSpill, EnableChromaKey, FileName (+9 more)

### Community 31 - "MultiViewWindow"
Cohesion: 0.18
Nodes (4): MultiViewRoot, Window, MultiViewWindow, Grid

### Community 32 - "TitleTemplate"
Cohesion: 0.17
Nodes (8): TitleTemplate, Fields, LastError, LastLoadError, Name, NamedFields, Root, TemplatePath

### Community 33 - "UserControl"
Cohesion: 0.19
Nodes (14): AwayScore, AwayTitle, Border1, Border2, Border3, Border4, HomeScore, HomeTitle (+6 more)

### Community 34 - "UserControl"
Cohesion: 0.17
Nodes (14): AwayScore, AwayTitle, Canvas1, Canvas2, HomeScore, HomeTitle, Image1, ScoreBoardHD2 (+6 more)

### Community 35 - "UserControl"
Cohesion: 0.19
Nodes (14): AwayScore, AwayTitle, Border1, Border2, Border3, Border4, HomeScore, HomeTitle (+6 more)

### Community 36 - "UserControl"
Cohesion: 0.17
Nodes (14): AwayScore, AwayTitle, Canvas1, Canvas2, HomeScore, HomeTitle, Image1, ScoreBoardHD2 (+6 more)

### Community 37 - "UserControl"
Cohesion: 0.19
Nodes (14): AwayScore, AwayTitle, Border1, Border2, Border3, Border4, HomeScore, HomeTitle (+6 more)

### Community 38 - "UserControl"
Cohesion: 0.17
Nodes (14): AwayScore, AwayTitle, Canvas1, Canvas2, HomeScore, HomeTitle, Image1, ScoreBoardHD2 (+6 more)

### Community 39 - "InputType"
Cohesion: 0.13
Nodes (15): InputType, AudioDevice, AudioFile, Camera, Clock, ColorBars, Colour, DesktopCapture (+7 more)

### Community 40 - "ChromaKeyEffect"
Cohesion: 0.15
Nodes (8): ChromaKeyEffect, Input, KeyColor, Similarity, Smoothness, SpillSuppression, TextBlockDesign, Fill

### Community 41 - "ReplayWindow"
Cohesion: 0.18
Nodes (3): EventsDataGrid, ReplayWindow, DataGrid

### Community 42 - "VirtualSet"
Cohesion: 0.09
Nodes (15): VirtualSet, ActiveCameraX, ActiveCameraY, ActiveCameraZoom, CameraPresets, CurrentPreset, FolderPath, Layers (+7 more)

### Community 43 - "CsvDataSource"
Cohesion: 0.21
Nodes (4): CsvDataSource, FirstRowHasHeaders, JsonDataSource, RssDataSource

### Community 44 - "UserControl"
Cohesion: 0.25
Nodes (10): DescriptionLeft, DescriptionRight, Grid1, Grid2, Grid3, Headline, MultiViewHD, UserControl (+2 more)

### Community 45 - "UserControl"
Cohesion: 0.25
Nodes (10): DescriptionLeft, DescriptionRight, Grid1, Grid2, Grid3, Headline, MultiViewHD2, UserControl (+2 more)

### Community 46 - "UserControl"
Cohesion: 0.29
Nodes (10): Away, AwayScore, Clock, Divider, Home, HomeScore, Scoreboard1, ScoreDivider (+2 more)

### Community 47 - "UserControl"
Cohesion: 0.25
Nodes (10): DescriptionLeft, DescriptionRight, Grid1, Grid2, Grid3, Headline, MultiViewHD, UserControl (+2 more)

### Community 48 - "UserControl"
Cohesion: 0.25
Nodes (10): DescriptionLeft, DescriptionRight, Grid1, Grid2, Grid3, Headline, MultiViewHD2, UserControl (+2 more)

### Community 49 - "UserControl"
Cohesion: 0.29
Nodes (10): Away, AwayScore, Clock, Divider, Home, HomeScore, Scoreboard1, ScoreDivider (+2 more)

### Community 50 - "ReplayEvent"
Cohesion: 0.15
Nodes (10): ReplayEvent, CameraNumber, Duration, Id, InPoint, ListNumber, OutPoint, Speed (+2 more)

### Community 51 - "TransitionEffect"
Cohesion: 0.18
Nodes (11): TransitionEffect, BarnDoor, CubeZoom, Cut, Fade, Fly, Merge, Slide (+3 more)

### Community 52 - "UserControl"
Cohesion: 0.25
Nodes (10): DescriptionLeft, DescriptionRight, Grid1, Grid2, Grid3, Headline, MultiViewHD, UserControl (+2 more)

### Community 53 - "UserControl"
Cohesion: 0.25
Nodes (10): DescriptionLeft, DescriptionRight, Grid1, Grid2, Grid3, Headline, MultiViewHD2, UserControl (+2 more)

### Community 54 - "UserControl"
Cohesion: 0.29
Nodes (10): Away, AwayScore, Clock, Divider, Home, HomeScore, Scoreboard1, ScoreDivider (+2 more)

### Community 55 - "UserControl"
Cohesion: 0.27
Nodes (9): AnimatedHD, Description, Headline, rDescription, Rectangle1, rHeadline, UserControl, Rectangle (+1 more)

### Community 56 - "UserControl"
Cohesion: 0.27
Nodes (9): AnimatedInOutHD, Description, Headline, rDescription, Rectangle1, rHeadline, UserControl, Rectangle (+1 more)

### Community 57 - "UserControl"
Cohesion: 0.27
Nodes (9): Description, Headline, NewsHD, Rectangle1, Rectangle2, Rectangle3, UserControl, Rectangle (+1 more)

### Community 58 - "UserControl"
Cohesion: 0.27
Nodes (9): Description, Headline, NewsHD6, Rectangle1, Rectangle2, Rectangle3, UserControl, Rectangle (+1 more)

### Community 59 - "UserControl"
Cohesion: 0.31
Nodes (9): Away, AwayScore, Clock, Home, HomeScore, Scoreboard2, ScoreDivider, UserControl (+1 more)

### Community 60 - "UserControl"
Cohesion: 0.22
Nodes (9): AnimatedTicker, BackgroundImage, TickerHD2, Title, UserControl, ActualWidth, DoubleReverseConverter, Image (+1 more)

### Community 61 - "UserControl"
Cohesion: 0.27
Nodes (9): AnimatedHD, Description, Headline, rDescription, Rectangle1, rHeadline, UserControl, Rectangle (+1 more)

### Community 62 - "UserControl"
Cohesion: 0.27
Nodes (9): AnimatedInOutHD, Description, Headline, rDescription, Rectangle1, rHeadline, UserControl, Rectangle (+1 more)

### Community 63 - "UserControl"
Cohesion: 0.27
Nodes (9): Description, Headline, NewsHD, Rectangle1, Rectangle2, Rectangle3, UserControl, Rectangle (+1 more)

### Community 64 - "UserControl"
Cohesion: 0.27
Nodes (9): Description, Headline, NewsHD6, Rectangle1, Rectangle2, Rectangle3, UserControl, Rectangle (+1 more)

### Community 65 - "UserControl"
Cohesion: 0.31
Nodes (9): Away, AwayScore, Clock, Home, HomeScore, Scoreboard2, ScoreDivider, UserControl (+1 more)

### Community 66 - "UserControl"
Cohesion: 0.22
Nodes (9): AnimatedTicker, BackgroundImage, TickerHD2, Title, UserControl, ActualWidth, DoubleReverseConverter, Image (+1 more)

### Community 67 - "UserControl"
Cohesion: 0.20
Nodes (9): CameraScale, CameraTranslate, CameraViewport, LayersCanvas, UserControl, Canvas, Grid, ScaleTransform (+1 more)

### Community 68 - "Window"
Cohesion: 0.06
Nodes (40): CanvasCol, CoordsStatusText, CropBSlider, CropBText, CropLSlider, CropLText, CropRSlider, CropRText (+32 more)

### Community 69 - "UserControl"
Cohesion: 0.27
Nodes (9): AnimatedHD, Description, Headline, rDescription, Rectangle1, rHeadline, UserControl, Rectangle (+1 more)

### Community 70 - "UserControl"
Cohesion: 0.27
Nodes (9): AnimatedInOutHD, Description, Headline, rDescription, Rectangle1, rHeadline, UserControl, Rectangle (+1 more)

### Community 71 - "UserControl"
Cohesion: 0.27
Nodes (9): Description, Headline, NewsHD, Rectangle1, Rectangle2, Rectangle3, UserControl, Rectangle (+1 more)

### Community 72 - "UserControl"
Cohesion: 0.27
Nodes (9): Description, Headline, NewsHD6, Rectangle1, Rectangle2, Rectangle3, UserControl, Rectangle (+1 more)

### Community 73 - "UserControl"
Cohesion: 0.31
Nodes (9): Away, AwayScore, Clock, Home, HomeScore, Scoreboard2, ScoreDivider, UserControl (+1 more)

### Community 74 - "UserControl"
Cohesion: 0.22
Nodes (9): AnimatedTicker, BackgroundImage, TickerHD2, Title, UserControl, ActualWidth, DoubleReverseConverter, Image (+1 more)

### Community 75 - "UserControl"
Cohesion: 0.28
Nodes (8): BackgroundImage, Description, Headline, NewsHD17, SubDescription, UserControl, Image, TextBlockDesign

### Community 76 - "UserControl"
Cohesion: 0.28
Nodes (8): BackgroundImage, Description, Headline, NewsHD2, TitleTime, UserControl, Image, TextBlockDesign

### Community 77 - "UserControl"
Cohesion: 0.33
Nodes (8): Away, AwayScore, Clock, Home, HomeScore, Scoreboard3, UserControl, TextBlockDesign

### Community 78 - "UserControl"
Cohesion: 0.33
Nodes (8): Away, AwayScore, Clock, Home, HomeScore, Scoreboard5, UserControl, TextBlockDesign

### Community 79 - "UserControl"
Cohesion: 0.22
Nodes (8): AnimatedTicker, Rectangle1, TickerHD, UserControl, ActualWidth, DoubleReverseConverter, Rectangle, TextBlock

### Community 80 - "UserControl"
Cohesion: 0.28
Nodes (8): BackgroundImage, Description, Headline, NewsHD17, SubDescription, UserControl, Image, TextBlockDesign

### Community 81 - "UserControl"
Cohesion: 0.28
Nodes (8): BackgroundImage, Description, Headline, NewsHD2, TitleTime, UserControl, Image, TextBlockDesign

### Community 82 - "UserControl"
Cohesion: 0.33
Nodes (8): Away, AwayScore, Clock, Home, HomeScore, Scoreboard3, UserControl, TextBlockDesign

### Community 83 - "UserControl"
Cohesion: 0.33
Nodes (8): Away, AwayScore, Clock, Home, HomeScore, Scoreboard5, UserControl, TextBlockDesign

### Community 84 - "UserControl"
Cohesion: 0.22
Nodes (8): AnimatedTicker, Rectangle1, TickerHD, UserControl, ActualWidth, DoubleReverseConverter, Rectangle, TextBlock

### Community 85 - "UserControl"
Cohesion: 0.19
Nodes (20): BoxCanvas, CenterDot, GizmoCanvas, HandleE, HandleN, HandleNE, HandleNW, HandleS (+12 more)

### Community 86 - "IDataSource"
Cohesion: 0.18
Nodes (7): DataSourceManager, Sources, IDataSource, IsRunning, Name, PollIntervalMs, SourcePathOrUrl

### Community 87 - ".SetTBar"
Cohesion: 0.32
Nodes (4): BusAFader, MasterFader, TBarSlider, Slider

### Community 88 - "🏛️ Arsitektur & Roadmap Fitur Lengkap (8 Phase)"
Cohesion: 0.12
Nodes (15): 1. Menjalankan Tes Otomatis (27 Test Suite), 2. Menjalankan Aplikasi, 3. Membangun Binary Standalone Release, 🏛️ Arsitektur & Roadmap Fitur Lengkap (8 Phase), 📦 Menjalankan & Membangun, Openmix 29 Pro, ════ Phase 1: Virtual Sets & GPU Chroma Key ════, ════ Phase 2: Instant Replay (8 Kamera) & VAR ════ (+7 more)

### Community 89 - "UserControl"
Cohesion: 0.28
Nodes (8): BackgroundImage, Description, Headline, NewsHD17, SubDescription, UserControl, Image, TextBlockDesign

### Community 90 - "UserControl"
Cohesion: 0.28
Nodes (8): BackgroundImage, Description, Headline, NewsHD2, TitleTime, UserControl, Image, TextBlockDesign

### Community 91 - "UserControl"
Cohesion: 0.33
Nodes (8): Away, AwayScore, Clock, Home, HomeScore, Scoreboard3, UserControl, TextBlockDesign

### Community 92 - "UserControl"
Cohesion: 0.33
Nodes (8): Away, AwayScore, Clock, Home, HomeScore, Scoreboard5, UserControl, TextBlockDesign

### Community 93 - "UserControl"
Cohesion: 0.22
Nodes (8): AnimatedTicker, Rectangle1, TickerHD, UserControl, ActualWidth, DoubleReverseConverter, Rectangle, TextBlock

### Community 94 - "UserControl"
Cohesion: 0.29
Nodes (7): BackgroundImage, Description, Headline, NewsHD10, UserControl, Image, TextBlockDesign

### Community 95 - "UserControl"
Cohesion: 0.29
Nodes (7): BackgroundImage, Description, Headline, NewsHD11, UserControl, Image, TextBlockDesign

### Community 96 - "UserControl"
Cohesion: 0.29
Nodes (7): BackgroundImage, Description, Headline, NewsHD12, UserControl, Image, TextBlockDesign

### Community 97 - "UserControl"
Cohesion: 0.29
Nodes (7): BackgroundImage, Description, Headline, NewsHD13, UserControl, Image, TextBlockDesign

### Community 98 - "UserControl"
Cohesion: 0.29
Nodes (7): BackgroundImage, Description, Headline, NewsHD14, UserControl, Image, TextBlockDesign

### Community 99 - "UserControl"
Cohesion: 0.29
Nodes (7): BackgroundImage, Description, Headline, NewsHD15, UserControl, Image, TextBlockDesign

### Community 100 - "UserControl"
Cohesion: 0.29
Nodes (7): BackgroundImage, Description, Headline, NewsHD16, UserControl, Image, TextBlockDesign

### Community 101 - "UserControl"
Cohesion: 0.29
Nodes (7): BackgroundImage, Description, Headline, NewsHD3, UserControl, Image, TextBlockDesign

### Community 102 - "UserControl"
Cohesion: 0.29
Nodes (7): BackgroundImage, Description, Headline, NewsHD5, UserControl, Image, TextBlockDesign

### Community 103 - "UserControl"
Cohesion: 0.29
Nodes (7): BackgroundImage, Description, Headline, NewsHD7, UserControl, Image, TextBlockDesign

### Community 104 - "UserControl"
Cohesion: 0.29
Nodes (7): BackgroundImage, Description, Headline, NewsHD8, UserControl, Image, TextBlockDesign

### Community 105 - "UserControl"
Cohesion: 0.29
Nodes (7): BackgroundImage, Description, Headline, NewsHD9, UserControl, Image, TextBlockDesign

### Community 106 - "UserControl"
Cohesion: 0.36
Nodes (7): AwayScore, Clock, HomeAway, HomeScore, Scoreboard4, UserControl, TextBlockDesign

### Community 107 - "UserControl"
Cohesion: 0.36
Nodes (7): Description, Heading, News, Time, Title1, UserControl, TextBlockDesign

### Community 108 - "UserControl"
Cohesion: 0.29
Nodes (7): Description, Heading, Image1, Title3Aqua, UserControl, Image, TextBlockDesign

### Community 109 - "UserControl"
Cohesion: 0.29
Nodes (7): BackgroundImage, Description, Headline, NewsHD10, UserControl, Image, TextBlockDesign

### Community 110 - "UserControl"
Cohesion: 0.29
Nodes (7): BackgroundImage, Description, Headline, NewsHD11, UserControl, Image, TextBlockDesign

### Community 111 - "UserControl"
Cohesion: 0.29
Nodes (7): BackgroundImage, Description, Headline, NewsHD12, UserControl, Image, TextBlockDesign

### Community 112 - "UserControl"
Cohesion: 0.29
Nodes (7): BackgroundImage, Description, Headline, NewsHD13, UserControl, Image, TextBlockDesign

### Community 113 - "UserControl"
Cohesion: 0.29
Nodes (7): BackgroundImage, Description, Headline, NewsHD14, UserControl, Image, TextBlockDesign

### Community 114 - "UserControl"
Cohesion: 0.29
Nodes (7): BackgroundImage, Description, Headline, NewsHD15, UserControl, Image, TextBlockDesign

### Community 115 - "UserControl"
Cohesion: 0.29
Nodes (7): BackgroundImage, Description, Headline, NewsHD16, UserControl, Image, TextBlockDesign

### Community 116 - "UserControl"
Cohesion: 0.29
Nodes (7): BackgroundImage, Description, Headline, NewsHD3, UserControl, Image, TextBlockDesign

### Community 117 - "UserControl"
Cohesion: 0.29
Nodes (7): BackgroundImage, Description, Headline, NewsHD5, UserControl, Image, TextBlockDesign

### Community 118 - "UserControl"
Cohesion: 0.29
Nodes (7): BackgroundImage, Description, Headline, NewsHD7, UserControl, Image, TextBlockDesign

### Community 119 - "UserControl"
Cohesion: 0.29
Nodes (7): BackgroundImage, Description, Headline, NewsHD8, UserControl, Image, TextBlockDesign

### Community 120 - "UserControl"
Cohesion: 0.29
Nodes (7): BackgroundImage, Description, Headline, NewsHD9, UserControl, Image, TextBlockDesign

### Community 121 - "UserControl"
Cohesion: 0.36
Nodes (7): AwayScore, Clock, HomeAway, HomeScore, Scoreboard4, UserControl, TextBlockDesign

### Community 122 - "UserControl"
Cohesion: 0.36
Nodes (7): Description, Heading, News, Time, Title1, UserControl, TextBlockDesign

### Community 123 - "UserControl"
Cohesion: 0.29
Nodes (7): Description, Heading, Image1, Title3Aqua, UserControl, Image, TextBlockDesign

### Community 124 - "ObsTransformGizmo"
Cohesion: 0.15
Nodes (7): Border, BoxBorder, RotHandle, ObsTransformGizmo, IsCropMode, Item, Rectangle

### Community 125 - "WebServerService"
Cohesion: 0.23
Nodes (3): WebServerService, IsRunning, Port

### Community 126 - "UserControl"
Cohesion: 0.29
Nodes (7): BackgroundImage, Description, Headline, NewsHD10, UserControl, Image, TextBlockDesign

### Community 127 - "UserControl"
Cohesion: 0.29
Nodes (7): BackgroundImage, Description, Headline, NewsHD11, UserControl, Image, TextBlockDesign

### Community 128 - "UserControl"
Cohesion: 0.29
Nodes (7): BackgroundImage, Description, Headline, NewsHD12, UserControl, Image, TextBlockDesign

### Community 129 - "UserControl"
Cohesion: 0.29
Nodes (7): BackgroundImage, Description, Headline, NewsHD13, UserControl, Image, TextBlockDesign

### Community 130 - "UserControl"
Cohesion: 0.29
Nodes (7): BackgroundImage, Description, Headline, NewsHD14, UserControl, Image, TextBlockDesign

### Community 131 - "UserControl"
Cohesion: 0.29
Nodes (7): BackgroundImage, Description, Headline, NewsHD15, UserControl, Image, TextBlockDesign

### Community 132 - "UserControl"
Cohesion: 0.29
Nodes (7): BackgroundImage, Description, Headline, NewsHD16, UserControl, Image, TextBlockDesign

### Community 133 - "UserControl"
Cohesion: 0.29
Nodes (7): BackgroundImage, Description, Headline, NewsHD3, UserControl, Image, TextBlockDesign

### Community 134 - "UserControl"
Cohesion: 0.29
Nodes (7): BackgroundImage, Description, Headline, NewsHD5, UserControl, Image, TextBlockDesign

### Community 135 - "UserControl"
Cohesion: 0.29
Nodes (7): BackgroundImage, Description, Headline, NewsHD7, UserControl, Image, TextBlockDesign

### Community 136 - "UserControl"
Cohesion: 0.29
Nodes (7): BackgroundImage, Description, Headline, NewsHD8, UserControl, Image, TextBlockDesign

### Community 137 - "UserControl"
Cohesion: 0.29
Nodes (7): BackgroundImage, Description, Headline, NewsHD9, UserControl, Image, TextBlockDesign

### Community 138 - "UserControl"
Cohesion: 0.36
Nodes (7): AwayScore, Clock, HomeAway, HomeScore, Scoreboard4, UserControl, TextBlockDesign

### Community 139 - "UserControl"
Cohesion: 0.36
Nodes (7): Description, Heading, News, Time, Title1, UserControl, TextBlockDesign

### Community 140 - "UserControl"
Cohesion: 0.29
Nodes (7): Description, Heading, Image1, Title3Aqua, UserControl, Image, TextBlockDesign

### Community 141 - "UserControl"
Cohesion: 0.29
Nodes (6): BackgroundImage, Headline, NewsHD4, UserControl, Image, TextBlockDesign

### Community 142 - "UserControl"
Cohesion: 0.29
Nodes (6): AnimatedTicker, TickerHD3, UserControl, ActualWidth, DoubleReverseConverter, TextBlock

### Community 143 - "UserControl"
Cohesion: 0.38
Nodes (6): Description, Heading, SubHeading, Title14, UserControl, TextBlockDesign

### Community 144 - "UserControl"
Cohesion: 0.38
Nodes (6): Description, Heading, Live, Title2Blue, UserControl, TextBlockDesign

### Community 145 - "UserControl"
Cohesion: 0.38
Nodes (6): Description, Heading, Live, Title2Green, UserControl, TextBlockDesign

### Community 146 - "UserControl"
Cohesion: 0.38
Nodes (6): Description, Heading, Live, Title2Red, UserControl, TextBlockDesign

### Community 147 - "UserControl"
Cohesion: 0.38
Nodes (6): Description, Heading, SubHeading, Title7, UserControl, TextBlockDesign

### Community 148 - "UserControl"
Cohesion: 0.29
Nodes (6): BackgroundImage, Headline, NewsHD4, UserControl, Image, TextBlockDesign

### Community 149 - "UserControl"
Cohesion: 0.29
Nodes (6): AnimatedTicker, TickerHD3, UserControl, ActualWidth, DoubleReverseConverter, TextBlock

### Community 150 - "UserControl"
Cohesion: 0.38
Nodes (6): Description, Heading, SubHeading, Title14, UserControl, TextBlockDesign

### Community 151 - "UserControl"
Cohesion: 0.38
Nodes (6): Description, Heading, Live, Title2Blue, UserControl, TextBlockDesign

### Community 152 - "UserControl"
Cohesion: 0.38
Nodes (6): Description, Heading, Live, Title2Green, UserControl, TextBlockDesign

### Community 153 - "UserControl"
Cohesion: 0.38
Nodes (6): Description, Heading, Live, Title2Red, UserControl, TextBlockDesign

### Community 154 - "UserControl"
Cohesion: 0.38
Nodes (6): Description, Heading, SubHeading, Title7, UserControl, TextBlockDesign

### Community 155 - "SelectionChangedEventArgs"
Cohesion: 0.22
Nodes (4): LayersList, PropTypeCombo, ComboBox, ListBox

### Community 156 - "UserControl"
Cohesion: 0.29
Nodes (6): BackgroundImage, Headline, NewsHD4, UserControl, Image, TextBlockDesign

### Community 157 - "UserControl"
Cohesion: 0.29
Nodes (6): AnimatedTicker, TickerHD3, UserControl, ActualWidth, DoubleReverseConverter, TextBlock

### Community 158 - "UserControl"
Cohesion: 0.38
Nodes (6): Description, Heading, SubHeading, Title14, UserControl, TextBlockDesign

### Community 159 - "UserControl"
Cohesion: 0.38
Nodes (6): Description, Heading, Live, Title2Blue, UserControl, TextBlockDesign

### Community 160 - "UserControl"
Cohesion: 0.38
Nodes (6): Description, Heading, Live, Title2Green, UserControl, TextBlockDesign

### Community 161 - "UserControl"
Cohesion: 0.38
Nodes (6): Description, Heading, Live, Title2Red, UserControl, TextBlockDesign

### Community 162 - "UserControl"
Cohesion: 0.38
Nodes (6): Description, Heading, SubHeading, Title7, UserControl, TextBlockDesign

### Community 163 - "UserControl"
Cohesion: 0.40
Nodes (5): Message, TextHD, Title, UserControl, TextBlockDesign

### Community 164 - "UserControl"
Cohesion: 0.40
Nodes (5): Description, Heading, Title10, UserControl, TextBlockDesign

### Community 165 - "UserControl"
Cohesion: 0.40
Nodes (5): Description, Heading, Title11, UserControl, TextBlockDesign

### Community 166 - "UserControl"
Cohesion: 0.40
Nodes (5): Description, Heading, Title13, UserControl, TextBlockDesign

### Community 167 - "UserControl"
Cohesion: 0.40
Nodes (5): Description, Heading, Title3AquaAlt, UserControl, TextBlockDesign

### Community 168 - "UserControl"
Cohesion: 0.40
Nodes (5): Description, Heading, Title3Blue, UserControl, TextBlockDesign

### Community 169 - "UserControl"
Cohesion: 0.40
Nodes (5): Description, Heading, Title3BlueAlt, UserControl, TextBlockDesign

### Community 170 - "UserControl"
Cohesion: 0.40
Nodes (5): Description, Heading, Title3Red, UserControl, TextBlockDesign

### Community 171 - "UserControl"
Cohesion: 0.40
Nodes (5): Description, Heading, Title3RedAlt, UserControl, TextBlockDesign

### Community 172 - "UserControl"
Cohesion: 0.40
Nodes (5): Description, Heading, Title4, UserControl, TextBlockDesign

### Community 173 - "UserControl"
Cohesion: 0.40
Nodes (5): Heading, Time, Title5Blue, UserControl, TextBlockDesign

### Community 174 - "UserControl"
Cohesion: 0.40
Nodes (5): Heading, Time, Title5Orange, UserControl, TextBlockDesign

### Community 175 - "UserControl"
Cohesion: 0.40
Nodes (5): Heading, Time, Title5Red, UserControl, TextBlockDesign

### Community 176 - "UserControl"
Cohesion: 0.40
Nodes (5): Heading, Time, Title5Yellow, UserControl, TextBlockDesign

### Community 177 - "UserControl"
Cohesion: 0.40
Nodes (5): Description, Heading, Title6, UserControl, TextBlockDesign

### Community 178 - "UserControl"
Cohesion: 0.40
Nodes (5): Message, TextHD, Title, UserControl, TextBlockDesign

### Community 179 - "UserControl"
Cohesion: 0.40
Nodes (5): Description, Heading, Title10, UserControl, TextBlockDesign

### Community 180 - "UserControl"
Cohesion: 0.40
Nodes (5): Description, Heading, Title11, UserControl, TextBlockDesign

### Community 181 - "UserControl"
Cohesion: 0.40
Nodes (5): Description, Heading, Title13, UserControl, TextBlockDesign

### Community 182 - "UserControl"
Cohesion: 0.40
Nodes (5): Description, Heading, Title3AquaAlt, UserControl, TextBlockDesign

### Community 183 - "UserControl"
Cohesion: 0.40
Nodes (5): Description, Heading, Title3Blue, UserControl, TextBlockDesign

### Community 184 - "UserControl"
Cohesion: 0.40
Nodes (5): Description, Heading, Title3BlueAlt, UserControl, TextBlockDesign

### Community 185 - "UserControl"
Cohesion: 0.40
Nodes (5): Description, Heading, Title3Red, UserControl, TextBlockDesign

### Community 186 - "UserControl"
Cohesion: 0.40
Nodes (5): Description, Heading, Title3RedAlt, UserControl, TextBlockDesign

### Community 187 - "UserControl"
Cohesion: 0.40
Nodes (5): Description, Heading, Title4, UserControl, TextBlockDesign

### Community 188 - "UserControl"
Cohesion: 0.40
Nodes (5): Heading, Time, Title5Blue, UserControl, TextBlockDesign

### Community 189 - "UserControl"
Cohesion: 0.40
Nodes (5): Heading, Time, Title5Orange, UserControl, TextBlockDesign

### Community 190 - "UserControl"
Cohesion: 0.40
Nodes (5): Heading, Time, Title5Red, UserControl, TextBlockDesign

### Community 191 - "UserControl"
Cohesion: 0.40
Nodes (5): Heading, Time, Title5Yellow, UserControl, TextBlockDesign

### Community 192 - "UserControl"
Cohesion: 0.40
Nodes (5): Description, Heading, Title6, UserControl, TextBlockDesign

### Community 193 - "UserControl"
Cohesion: 0.40
Nodes (5): Message, TextHD, Title, UserControl, TextBlockDesign

### Community 194 - "UserControl"
Cohesion: 0.40
Nodes (5): Description, Heading, Title10, UserControl, TextBlockDesign

### Community 195 - "UserControl"
Cohesion: 0.40
Nodes (5): Description, Heading, Title11, UserControl, TextBlockDesign

### Community 196 - "UserControl"
Cohesion: 0.40
Nodes (5): Description, Heading, Title13, UserControl, TextBlockDesign

### Community 197 - "UserControl"
Cohesion: 0.40
Nodes (5): Description, Heading, Title3AquaAlt, UserControl, TextBlockDesign

### Community 198 - "UserControl"
Cohesion: 0.40
Nodes (5): Description, Heading, Title3Blue, UserControl, TextBlockDesign

### Community 199 - "UserControl"
Cohesion: 0.40
Nodes (5): Description, Heading, Title3BlueAlt, UserControl, TextBlockDesign

### Community 200 - "UserControl"
Cohesion: 0.40
Nodes (5): Description, Heading, Title3Red, UserControl, TextBlockDesign

### Community 201 - "UserControl"
Cohesion: 0.40
Nodes (5): Description, Heading, Title3RedAlt, UserControl, TextBlockDesign

### Community 202 - "UserControl"
Cohesion: 0.40
Nodes (5): Description, Heading, Title4, UserControl, TextBlockDesign

### Community 203 - "UserControl"
Cohesion: 0.40
Nodes (5): Heading, Time, Title5Blue, UserControl, TextBlockDesign

### Community 204 - "UserControl"
Cohesion: 0.40
Nodes (5): Heading, Time, Title5Orange, UserControl, TextBlockDesign

### Community 205 - "UserControl"
Cohesion: 0.40
Nodes (5): Heading, Time, Title5Red, UserControl, TextBlockDesign

### Community 206 - "UserControl"
Cohesion: 0.40
Nodes (5): Heading, Time, Title5Yellow, UserControl, TextBlockDesign

### Community 207 - "UserControl"
Cohesion: 0.40
Nodes (5): Description, Heading, Title6, UserControl, TextBlockDesign

### Community 208 - "UserControl"
Cohesion: 0.40
Nodes (4): Clock, Timer1, UserControl, TextBlockDesign

### Community 209 - "UserControl"
Cohesion: 0.40
Nodes (4): Clock, Timer2, UserControl, TextBlockDesign

### Community 210 - "UserControl"
Cohesion: 0.40
Nodes (4): txtClock, UserControl, UserControl1, TextBlockDesign

### Community 211 - "UserControl"
Cohesion: 0.40
Nodes (4): Heading, Title12, UserControl, TextBlockDesign

### Community 212 - "UserControl"
Cohesion: 0.40
Nodes (4): Heading, Title8AmericasBlue, UserControl, TextBlockDesign

### Community 213 - "UserControl"
Cohesion: 0.40
Nodes (4): Heading, Title8AmericasRed, UserControl, TextBlockDesign

### Community 214 - "UserControl"
Cohesion: 0.40
Nodes (4): Heading, Title8AsiaBlue, UserControl, TextBlockDesign

### Community 215 - "UserControl"
Cohesion: 0.40
Nodes (4): Heading, Title8AsiaRed, UserControl, TextBlockDesign

### Community 216 - "UserControl"
Cohesion: 0.40
Nodes (4): Heading, Title8EuropeBlue, UserControl, TextBlockDesign

### Community 217 - "UserControl"
Cohesion: 0.40
Nodes (4): Heading, Title8EuropeRed, UserControl, TextBlockDesign

### Community 218 - "UserControl"
Cohesion: 0.40
Nodes (4): Heading, Title9, UserControl, TextBlockDesign

### Community 219 - "UserControl"
Cohesion: 0.40
Nodes (4): Clock, Timer1, UserControl, TextBlockDesign

### Community 220 - "UserControl"
Cohesion: 0.40
Nodes (4): Clock, Timer2, UserControl, TextBlockDesign

### Community 221 - "UserControl"
Cohesion: 0.40
Nodes (4): txtClock, UserControl, UserControl1, TextBlockDesign

### Community 222 - "UserControl"
Cohesion: 0.40
Nodes (4): Heading, Title12, UserControl, TextBlockDesign

### Community 223 - "UserControl"
Cohesion: 0.40
Nodes (4): Heading, Title8AmericasBlue, UserControl, TextBlockDesign

### Community 224 - "UserControl"
Cohesion: 0.40
Nodes (4): Heading, Title8AmericasRed, UserControl, TextBlockDesign

### Community 225 - "UserControl"
Cohesion: 0.40
Nodes (4): Heading, Title8AsiaBlue, UserControl, TextBlockDesign

### Community 226 - "UserControl"
Cohesion: 0.40
Nodes (4): Heading, Title8AsiaRed, UserControl, TextBlockDesign

### Community 227 - "UserControl"
Cohesion: 0.40
Nodes (4): Heading, Title8EuropeBlue, UserControl, TextBlockDesign

### Community 228 - "UserControl"
Cohesion: 0.40
Nodes (4): Heading, Title8EuropeRed, UserControl, TextBlockDesign

### Community 229 - "UserControl"
Cohesion: 0.40
Nodes (4): Heading, Title9, UserControl, TextBlockDesign

### Community 230 - "UserControl"
Cohesion: 0.40
Nodes (4): Clock, Timer1, UserControl, TextBlockDesign

### Community 231 - "UserControl"
Cohesion: 0.40
Nodes (4): Clock, Timer2, UserControl, TextBlockDesign

### Community 232 - "UserControl"
Cohesion: 0.40
Nodes (4): txtClock, UserControl, UserControl1, TextBlockDesign

### Community 233 - "UserControl"
Cohesion: 0.40
Nodes (4): Heading, Title12, UserControl, TextBlockDesign

### Community 234 - "UserControl"
Cohesion: 0.40
Nodes (4): Heading, Title8AmericasBlue, UserControl, TextBlockDesign

### Community 235 - "UserControl"
Cohesion: 0.40
Nodes (4): Heading, Title8AmericasRed, UserControl, TextBlockDesign

### Community 236 - "UserControl"
Cohesion: 0.40
Nodes (4): Heading, Title8AsiaBlue, UserControl, TextBlockDesign

### Community 237 - "UserControl"
Cohesion: 0.40
Nodes (4): Heading, Title8AsiaRed, UserControl, TextBlockDesign

### Community 238 - "UserControl"
Cohesion: 0.40
Nodes (4): Heading, Title8EuropeBlue, UserControl, TextBlockDesign

### Community 239 - "UserControl"
Cohesion: 0.40
Nodes (4): Heading, Title8EuropeRed, UserControl, TextBlockDesign

### Community 240 - "UserControl"
Cohesion: 0.40
Nodes (4): Heading, Title9, UserControl, TextBlockDesign

### Community 242 - "DataSourceBase"
Cohesion: 0.21
Nodes (5): DataSourceBase, IsRunning, Name, PollIntervalMs, SourcePathOrUrl

### Community 243 - "INotifyPropertyChanged"
Cohesion: 0.13
Nodes (12): Vst3EffectSlot, IsBypassed, Param1, Param2, Param3, PluginName, WetDryMix, ReplayCamera (+4 more)

### Community 244 - "InputCategory"
Cohesion: 0.20
Nodes (8): InputCategory, All, Aqua, Blue, Green, Orange, Purple, Red

### Community 247 - "ProgramSurface"
Cohesion: 0.36
Nodes (3): Fbo, ProgramSurface, LastError

### Community 248 - "Application"
Cohesion: 0.13
Nodes (14): Application, Arrow, ContentSite, DropDown, DropDownBorder, Popup, ToggleButton, Border (+6 more)

### Community 249 - "🔌 Backend Capture Nyata (bukan simulasi)"
Cohesion: 0.25
Nodes (4): 🔌 Backend Capture Nyata (bukan simulasi), CameraDeviceService, AudioDevices, VideoDevices

### Community 250 - "SrtStreamConfig"
Cohesion: 0.25
Nodes (7): SrtStreamConfig, Host, IsActive, LatencyMs, Mode, Passphrase, Port

### Community 251 - "StackPanel"
Cohesion: 0.25
Nodes (8): AboutPanel, AudioPanel, DisplayPanel, RecordingPanel, ShortcutsPanel, StreamingPanel, WebPanel, StackPanel

### Community 252 - "TextBox"
Cohesion: 0.33
Nodes (6): RecFolderBox, StreamKeyBox, StreamUrlBox, WebPasswordBox, WebPortBox, TextBox

### Community 253 - "ProgressBar"
Cohesion: 0.40
Nodes (5): BusAMeterL, BusAMeterR, MasterMeterL, MasterMeterR, ProgressBar

### Community 254 - "PtzProtocol"
Cohesion: 0.40
Nodes (5): PtzProtocol, PtzOpticsHttp, SonyCgi, ViscaTcp, ViscaUdp

### Community 255 - "StreamStatus"
Cohesion: 0.40
Nodes (5): StreamStatus, Connecting, Error, Live, Offline

### Community 257 - "PART_Track"
Cohesion: 0.50
Nodes (4): PART_Indicator, PART_Track, Border, Track

### Community 259 - "RecDot"
Cohesion: 0.67
Nodes (3): RecDot, StreamDot, Ellipse

### Community 260 - "AutoAudioMixingCheck"
Cohesion: 0.67
Nodes (3): AutoAudioMixingCheck, WebEnabledCheck, CheckBox

## Knowledge Gaps
- **760 isolated node(s):** `Border`, `TextBlock`, `ResultItem`, `ToggleButton`, `IsDropDownOpen` (+755 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 1044 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **11 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `ProductionEngine` connect `ProductionEngine` to `system_windows`, `InputItem`, `RoutedEventArgs`, `StreamingEngine`, `PlayoutEngine`, `VMixConfiguration`, `CameraCaptureService`, `AddInputDialog`, `NdiSrtService`, `InstantReplayEngine`, `AudioMixerEngine`, `ReplayEvent`, `.RunAll`, `StreamDeckAndTallyService`, `RoutedEventArgs`, `INotifyPropertyChanged`, `StingerTransition`, `MultiViewWindow`, `ReplayWindow`, `VirtualSet`, `TransitionEffect`, `IDataSource`, `INotifyPropertyChanged`, `InputCategory`, `.ExecuteApiFunction`, `🔌 Backend Capture Nyata (bukan simulasi)`, `WebServerService`?**
  _High betweenness centrality (0.096) - this node is a cross-community bridge._
- **Are the 2 inferred relationships involving `ProductionEngine` (e.g. with `VMixApiService` and `.RunAll()`) actually correct?**
  _`ProductionEngine` has 2 INFERRED edges - model-reasoned connections that need verification._
- **What connects `Border`, `TextBlock`, `ResultItem` to the rest of the system?**
  _760 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `System` be split into smaller, more focused modules?**
  _Cohesion score 0.04209563029055136 - nodes in this community are weakly interconnected._
- **Why does `MainWindow` connect `RoutedEventArgs` to `System`, `system_windows`, `DebuggerNonUserCodeAttribute`, `ProductionEngine`, `Window`, `RoutedEventArgs`, `StreamingEngine`, `AddInputDialog`, `InputDisplay`, `.RunAll`, `StreamDeckAndTallyService`, `MultiViewWindow`, `ReplayWindow`, `Window`, `.SetTBar`, `ProgramSurface`, `Application`, `ObsTransformGizmo`, `WebServerService`?**
  _High betweenness centrality (0.084) - this node is a cross-community bridge._
- **Are the 3 inferred relationships involving `MainWindow` (e.g. with `.UpdateFromUI()` and `.Start()`) actually correct?**
  _`MainWindow` has 3 INFERRED edges - model-reasoned connections that need verification._
- **Should `system_windows` be split into smaller, more focused modules?**
  _Cohesion score 0.12694512694512694 - nodes in this community are weakly interconnected._