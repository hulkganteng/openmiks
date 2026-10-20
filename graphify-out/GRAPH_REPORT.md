# Graph Report - vmix clone  (2026-10-20)

## Corpus Check
- 125 files · ~364,234 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 7 file(s) not represented in the graph (top: .xml 3, (none) 1, .ico 1)

## Summary
- 1937 nodes · 3376 edges · 145 communities (129 shown, 16 thin omitted)
- Extraction: 95% EXTRACTED · 5% INFERRED · 0% AMBIGUOUS · INFERRED: 178 edges (avg confidence: 0.85)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `f86e38f8`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- VMixConfiguration
- system_windows
- VirtualSetLayer
- Window
- InputItem
- ProductionEngine
- Window
- Button
- UserControl
- Window
- Window
- Window
- StreamingEngine
- PlayoutEngine
- InstantReplayEngine
- TcpServerService
- AudioMeterService
- SettingsWindow
- NdiDiscoveryService
- .RecReplay_Click
- InputDisplay
- AudioMixerEngine
- ReplayChannel
- .Prop_Changed
- .RunAll
- StreamDeckAndTallyService
- RoutedEventArgs
- ScriptingAndTriggersEngine
- VMixApiService
- CameraCaptureService
- InputType
- VirtualSetView
- TitleTemplate
- NdiSrtService
- ReplayEvent
- InputDisplay
- VirtualSet
- UserControl
- UserControl
- TransitionEffect
- ChromaKeyEffect
- ReplayWindow
- VirtualSetCameraPreset
- AudioBus
- Vst3EffectSlot
- SrtStreamConfig
- StingerTransition
- TriggerRule
- PhotoSlideshowModel
- StreamStatus
- PreviewObsEditToggle
- UserControl
- UserControl
- UserControl
- PropLayerName
- PropTypeCombo
- CameraPresetBar
- SetPreviewView
- StatusText
- UserControl
- Window
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
- ObsTransformGizmo
- DataSourceBase
- .SetTBar
- 🏛️ Arsitektur & Roadmap Fitur Lengkap (8 Phase)
- UserControl
- UserControl
- UserControl
- UserControl
- UserControl
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
- INotifyPropertyChanged
- InputCategory
- AGENTS.md
- .ExecuteApiFunction
- ProgramSurface
- MultiViewWindow
- 🔌 Backend Capture Nyata (bukan simulasi)
- StackPanel
- TextBox
- ProgressBar
- PART_Track
- AutoAudioMixingCheck
- Bd
- WebUrlDisplay

## God Nodes (most connected - your core abstractions)
1. `ProductionEngine` - 116 edges
2. `Window` - 114 edges
3. `InputItem` - 91 edges
4. `MainWindow` - 70 edges
5. `Window` - 53 edges
6. `UserControl` - 50 edges
7. `Window` - 39 edges
8. `VMixConfiguration` - 38 edges
9. `PlayoutEngine` - 36 edges
10. `Window` - 36 edges

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

## Communities (145 total, 16 thin omitted)

### Community 0 - "VMixConfiguration"
Cohesion: 0.06
Nodes (30): VMixConfiguration, AspectRatio, AudioBusADevice, AudioMasterDevice, AudioSampleRate, AutoAudioMixing, EnableNdi, EnableSrt (+22 more)

### Community 1 - "system_windows"
Cohesion: 0.08
Nodes (13): Openmiks, Openmiks.Services, Openmiks.Tests, Openmiks.Views, Openmiks.Effects, Openmiks.Controls, Openmiks.TitleLibrary, Openmiks.Models (+5 more)

### Community 2 - "VirtualSetLayer"
Cohesion: 0.11
Nodes (17): VirtualSetLayer, AssignedInputNumber, ChromaKeyColor, ChromaSimilarity, ChromaSmoothness, ChromaSpill, EnableChromaKey, FileName (+9 more)

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
Nodes (50): AudioLevelL, AudioLevelR, CategoryHex, IsOverlay1, IsOverlay2, IsOverlay3, IsOverlay4, IsPreview (+42 more)

### Community 7 - "Button"
Cohesion: 0.07
Nodes (16): BusAMuteBtn, CatAllBtn, ExternalBtn, FtbBtn, MasterMuteBtn, Overlay1Btn, Overlay2Btn, Overlay3Btn (+8 more)

### Community 8 - "UserControl"
Cohesion: 0.05
Nodes (60): AudioPlayer, AudioRoot, AudioTimeText, AudioTitleText, BuiltInTitle, CameraImage, CameraNameText, CameraPlaceholder (+52 more)

### Community 9 - "Window"
Cohesion: 0.10
Nodes (25): CameraNumber, Duration, Id, InPoint, OutPoint, Speed, Text, CameraAnglesList (+17 more)

### Community 10 - "Window"
Cohesion: 0.21
Nodes (16): AudioBusACombo, AudioMasterCombo, FrameRateCombo, MicAux1Combo, MicAux2Combo, MonitorCombo, OutputResCombo, RecAudioBitrateCombo (+8 more)

### Community 11 - "Window"
Cohesion: 0.05
Nodes (51): AudioChannelCombo, AudioDeviceCombo, AudioDevicePanel, AudioLoopCheck, AudioPanel, AudioPathBox, Bd, CameraDeviceCombo (+43 more)

### Community 12 - "StreamingEngine"
Cohesion: 0.08
Nodes (17): ProgramFrameSource, IFrameSource, RecordingEngine, CurrentFilePath, Duration, FileSizeFormatted, IsRecording, TotalBytesWritten (+9 more)

### Community 13 - "PlayoutEngine"
Cohesion: 0.08
Nodes (19): PlayoutMode, Slideshow, VideoList, VideoListModel, CurrentIndex, IsLoop, Items, PlayoutEngine (+11 more)

### Community 14 - "InstantReplayEngine"
Cohesion: 0.12
Nodes (8): InstantReplayEngine, Cameras, ChannelA, ChannelB, CurrentListIndex, Events, IsRecording, SessionTime

### Community 15 - "TcpServerService"
Cohesion: 0.16
Nodes (3): TcpServerService, IsRunning, Port

### Community 16 - "AudioMeterService"
Cohesion: 0.17
Nodes (6): AudioMeterService, DeviceName, IsRunning, LastError, LevelL, LevelR

### Community 18 - "NdiDiscoveryService"
Cohesion: 0.21
Nodes (6): NdiDiscoveryService, IsScanning, LastError, QueryBytes, RuntimePresent, Sources

### Community 19 - ".RecReplay_Click"
Cohesion: 0.25
Nodes (3): PlayPauseBtn, RecReplayBtn, Button

### Community 20 - "InputDisplay"
Cohesion: 0.20
Nodes (3): InputDisplay, IsOverlayMode, Item

### Community 21 - "AudioMixerEngine"
Cohesion: 0.14
Nodes (12): AudioMixerEngine, BusA, BusB, BusC, BusD, BusE, BusF, BusG (+4 more)

### Community 22 - "ReplayChannel"
Cohesion: 0.13
Nodes (12): PlaylistItem, Duration, FilePath, Title, ReplayChannel, ActiveCamera, ChannelName, CurrentPosition (+4 more)

### Community 23 - ".Prop_Changed"
Cohesion: 0.29
Nodes (10): PropChromaCheck, PropOpacitySlider, PropSimilaritySlider, PropSmoothnessSlider, PropSpillSlider, PropXSlider, PropYSlider, PropZoomSlider (+2 more)

### Community 24 - ".RunAll"
Cohesion: 0.14
Nodes (4): PresetService, ProductionEngineTests, SyntheticFrameSource, FramesServed

### Community 25 - "StreamDeckAndTallyService"
Cohesion: 0.20
Nodes (4): StreamDeckAndTallyService, ActiveComPort, DeckPort, IsRunning

### Community 27 - "ScriptingAndTriggersEngine"
Cohesion: 0.15
Nodes (8): ScriptingAndTriggersEngine, Scripts, Triggers, ScriptItem, Code, IsRunning, LastRun, Name

### Community 28 - "VMixApiService"
Cohesion: 0.24
Nodes (5): IvMixAPI, VMixApiService, vMixState, vMixTally, vMixTallys

### Community 29 - "CameraCaptureService"
Cohesion: 0.13
Nodes (10): CameraCaptureService, Bitmap, DeviceName, Height, IsRunning, LastError, Width, BITMAPINFO (+2 more)

### Community 30 - "InputType"
Cohesion: 0.13
Nodes (15): InputType, AudioDevice, AudioFile, Camera, Clock, ColorBars, Colour, DesktopCapture (+7 more)

### Community 31 - "VirtualSetView"
Cohesion: 0.27
Nodes (3): VirtualSetView, AvailableInputs, VirtualSet

### Community 32 - "TitleTemplate"
Cohesion: 0.18
Nodes (8): TitleTemplate, Fields, LastError, LastLoadError, Name, NamedFields, Root, TemplatePath

### Community 33 - "NdiSrtService"
Cohesion: 0.15
Nodes (10): NdiSourceInfo, IpAddress, Name, Port, NdiSrtService, DiscoveredNdiSources, IsNdiOutput1Enabled, IsNdiOutput2Enabled (+2 more)

### Community 34 - "ReplayEvent"
Cohesion: 0.15
Nodes (10): ReplayEvent, CameraNumber, Duration, Id, InPoint, ListNumber, OutPoint, Speed (+2 more)

### Community 35 - "InputDisplay"
Cohesion: 0.17
Nodes (12): Overlay1Display, Overlay2Display, Overlay3Display, Overlay4Display, Overlay5Display, Overlay6Display, Overlay7Display, Overlay8Display (+4 more)

### Community 36 - "VirtualSet"
Cohesion: 0.17
Nodes (9): VirtualSet, ActiveCameraX, ActiveCameraY, ActiveCameraZoom, CameraPresets, CurrentPreset, FolderPath, Layers (+1 more)

### Community 37 - "UserControl"
Cohesion: 0.19
Nodes (14): AwayScore, AwayTitle, Border1, Border2, Border3, Border4, HomeScore, HomeTitle (+6 more)

### Community 38 - "UserControl"
Cohesion: 0.17
Nodes (14): AwayScore, AwayTitle, Canvas1, Canvas2, HomeScore, HomeTitle, Image1, ScoreBoardHD2 (+6 more)

### Community 39 - "TransitionEffect"
Cohesion: 0.18
Nodes (11): TransitionEffect, BarnDoor, CubeZoom, Cut, Fade, Fly, Merge, Slide (+3 more)

### Community 40 - "ChromaKeyEffect"
Cohesion: 0.15
Nodes (8): ChromaKeyEffect, Input, KeyColor, Similarity, Smoothness, SpillSuppression, TextBlockDesign, Fill

### Community 41 - "ReplayWindow"
Cohesion: 0.30
Nodes (3): EventsDataGrid, ReplayWindow, DataGrid

### Community 42 - "VirtualSetCameraPreset"
Cohesion: 0.20
Nodes (6): VirtualSetCameraPreset, Name, Rotation, X, Y, Zoom

### Community 43 - "AudioBus"
Cohesion: 0.20
Nodes (9): AudioBus, AssignedOutputDevice, DbLevel, IsMuted, MeterLevelL, MeterLevelR, Name, Tag (+1 more)

### Community 45 - "Vst3EffectSlot"
Cohesion: 0.25
Nodes (7): Vst3EffectSlot, IsBypassed, Param1, Param2, Param3, PluginName, WetDryMix

### Community 46 - "SrtStreamConfig"
Cohesion: 0.25
Nodes (7): SrtStreamConfig, Host, IsActive, LatencyMs, Mode, Passphrase, Port

### Community 47 - "StingerTransition"
Cohesion: 0.29
Nodes (6): StingerTransition, AssignedInputNumber, CutPointMs, DurationMs, Name, StingerNumber

### Community 48 - "TriggerRule"
Cohesion: 0.29
Nodes (6): TriggerRule, DelayMs, Function, Input, Trigger, Value

### Community 49 - "PhotoSlideshowModel"
Cohesion: 0.33
Nodes (4): PhotoSlideshowModel, CurrentIndex, ImagePaths, SlideDurationSeconds

### Community 50 - "StreamStatus"
Cohesion: 0.40
Nodes (5): StreamStatus, Connecting, Error, Live, Offline

### Community 52 - "UserControl"
Cohesion: 0.25
Nodes (10): DescriptionLeft, DescriptionRight, Grid1, Grid2, Grid3, Headline, MultiViewHD, UserControl (+2 more)

### Community 53 - "UserControl"
Cohesion: 0.25
Nodes (10): DescriptionLeft, DescriptionRight, Grid1, Grid2, Grid3, Headline, MultiViewHD2, UserControl (+2 more)

### Community 54 - "UserControl"
Cohesion: 0.29
Nodes (10): Away, AwayScore, Clock, Divider, Home, HomeScore, Scoreboard1, ScoreDivider (+2 more)

### Community 55 - "PropLayerName"
Cohesion: 0.67
Nodes (3): PropLayerName, SetNameBox, TextBox

### Community 67 - "UserControl"
Cohesion: 0.20
Nodes (9): CameraScale, CameraTranslate, CameraViewport, LayersCanvas, UserControl, Canvas, Grid, ScaleTransform (+1 more)

### Community 68 - "Window"
Cohesion: 0.07
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

### Community 85 - "ObsTransformGizmo"
Cohesion: 0.10
Nodes (26): BoxBorder, BoxCanvas, CenterDot, GizmoCanvas, HandleE, HandleN, HandleNE, HandleNW (+18 more)

### Community 86 - "DataSourceBase"
Cohesion: 0.08
Nodes (16): CsvDataSource, FirstRowHasHeaders, DataSourceBase, IsRunning, Name, PollIntervalMs, SourcePathOrUrl, DataSourceManager (+8 more)

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

### Community 125 - "WebServerService"
Cohesion: 0.22
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

### Community 155 - "Window"
Cohesion: 0.19
Nodes (7): PlaceholderType, LayersList, Window, Name, VirtualSetEditorWindow, VirtualSetResult, ListBox

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

### Community 243 - "INotifyPropertyChanged"
Cohesion: 0.29
Nodes (5): ReplayCamera, AssignedInputNumber, CameraNumber, IsRecording, Name

### Community 244 - "InputCategory"
Cohesion: 0.20
Nodes (8): InputCategory, All, Aqua, Blue, Green, Orange, Purple, Red

### Community 247 - "ProgramSurface"
Cohesion: 0.36
Nodes (3): Fbo, ProgramSurface, LastError

### Community 248 - "MultiViewWindow"
Cohesion: 0.08
Nodes (20): Application, Arrow, Border, ContentSite, DropDown, DropDownBorder, Popup, ToggleButton (+12 more)

### Community 249 - "🔌 Backend Capture Nyata (bukan simulasi)"
Cohesion: 0.25
Nodes (4): 🔌 Backend Capture Nyata (bukan simulasi), CameraDeviceService, AudioDevices, VideoDevices

### Community 251 - "StackPanel"
Cohesion: 0.25
Nodes (8): AboutPanel, AudioPanel, DisplayPanel, RecordingPanel, ShortcutsPanel, StreamingPanel, WebPanel, StackPanel

### Community 252 - "TextBox"
Cohesion: 0.33
Nodes (6): RecFolderBox, StreamKeyBox, StreamUrlBox, WebPasswordBox, WebPortBox, TextBox

### Community 253 - "ProgressBar"
Cohesion: 0.40
Nodes (5): BusAMeterL, BusAMeterR, MasterMeterL, MasterMeterR, ProgressBar

### Community 257 - "PART_Track"
Cohesion: 0.50
Nodes (4): PART_Indicator, PART_Track, Border, Track

### Community 260 - "AutoAudioMixingCheck"
Cohesion: 0.67
Nodes (3): AutoAudioMixingCheck, WebEnabledCheck, CheckBox

## Knowledge Gaps
- **552 isolated node(s):** `Border`, `TextBlock`, `ResultItem`, `ToggleButton`, `IsDropDownOpen` (+547 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 708 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **16 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `ProductionEngine` connect `ProductionEngine` to `VMixConfiguration`, `system_windows`, `InputItem`, `Button`, `StreamingEngine`, `PlayoutEngine`, `InstantReplayEngine`, `TcpServerService`, `AudioMeterService`, `SettingsWindow`, `NdiDiscoveryService`, `InputDisplay`, `AudioMixerEngine`, `ReplayChannel`, `.RunAll`, `StreamDeckAndTallyService`, `RoutedEventArgs`, `ScriptingAndTriggersEngine`, `VMixApiService`, `CameraCaptureService`, `VirtualSetView`, `NdiSrtService`, `TransitionEffect`, `ReplayWindow`, `VirtualSetCameraPreset`, `EventArgs`, `StingerTransition`, `PhotoSlideshowModel`, `DataSourceBase`, `INotifyPropertyChanged`, `InputCategory`, `.ExecuteApiFunction`, `MultiViewWindow`, `🔌 Backend Capture Nyata (bukan simulasi)`, `WebServerService`?**
  _High betweenness centrality (0.157) - this node is a cross-community bridge._
- **Are the 2 inferred relationships involving `ProductionEngine` (e.g. with `VMixApiService` and `.RunAll()`) actually correct?**
  _`ProductionEngine` has 2 INFERRED edges - model-reasoned connections that need verification._
- **What connects `Border`, `TextBlock`, `ResultItem` to the rest of the system?**
  _552 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `VMixConfiguration` be split into smaller, more focused modules?**
  _Cohesion score 0.062388591800356503 - nodes in this community are weakly interconnected._
- **Why does `InputItem` connect `InputItem` to `system_windows`, `Window`, `ProductionEngine`, `Window`, `PlayoutEngine`, `AudioMeterService`, `InputDisplay`, `ReplayChannel`, `.RunAll`, `RoutedEventArgs`, `CameraCaptureService`, `InputType`, `VirtualSetView`, `VirtualSet`, `ChromaKeyEffect`, `VirtualSetCameraPreset`, `.ApplyPreset`, `Window`, `ObsTransformGizmo`, `INotifyPropertyChanged`, `InputCategory`, `MultiViewWindow`?**
  _High betweenness centrality (0.150) - this node is a cross-community bridge._
- **Are the 5 inferred relationships involving `InputItem` (e.g. with `.Ok_Click()` and `.NewPreset_Click()`) actually correct?**
  _`InputItem` has 5 INFERRED edges - model-reasoned connections that need verification._
- **Should `system_windows` be split into smaller, more focused modules?**
  _Cohesion score 0.07563025210084033 - nodes in this community are weakly interconnected._