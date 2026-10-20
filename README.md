<p align="center">
  <img src="openmiks_trimmed.png" alt="Openmiks Logo" width="450" />
</p>

<h1 align="center">Openmiks 29 Pro</h1>

<p align="center">
  <strong>Native Windows (.NET 10 WPF) High-Performance Live Video Production Switcher</strong>
</p>

---

**Openmiks** adalah software live video production dan video mixer switcher berbasis **Native Windows (.NET 10 WPF)** yang ultra-ringan, berkinerja tinggi, dan kompatibel penuh dengan ekosistem vMix.

Dibangun dengan arsitektur C# murni tanpa Electron/WebView, Openmiks berjalan sangat efisien dengan jejak memori kecil (**~38 MB RAM**) dan penggunaan CPU idle **< 3%**.

---

## 🏛️ Arsitektur & Roadmap Fitur Lengkap (8 Phase)

### ════ Phase 1: Virtual Sets & GPU Chroma Key ════
- **GPU Chroma Keying**: Shader HLSL Pixel Shader 3.0 (`Effects/ChromaKey.hlsl`) terkompilasi ke Direct3D bytecode (`ChromaKey.ps`). Mengatur *Similarity Threshold*, *Smoothness/Feathering*, dan *Green Spill Suppression* langsung di hardware GPU tanpa membebani CPU.
- **Sistem Virtual Set**: Parser dan loader format asli `config.xml` vMix (`<virtualSet><input ...><zoom ...></virtualSet>`). Mendukung layer statis (Background, Frame, Foreground) dan layer dinamis (Talent/Presenter & Screen).
- **Virtual Set Editor (VSE)**: GUI terdedikasi (`🎨 VSE`) untuk menyusun layer grafis, mengubah ukuran, rotasi, Z-order, dan mengatur shot preset kamera.
- **Preset Kamera PTZ**: Perpindahan shot kamera (*Wide*, *Medium*, *Close Up*) dengan animasi *cubic ease-out* mulus.
- **Template Bawaan**: Termasuk set studio resmi yang disalin langsung dari vMix (`NewsStudio`, `LateNightNews`, `MultiView1`).

### ════ Phase 2: Instant Replay (8 Kamera) & VAR ════
- **8-Camera Continuous Ingestion**: Engine perekaman multi-sudut simultan untuk hingga 8 kamera.
- **Dual Playback Channels**: Dua channel playback terpisah (`Channel A` dan `Channel B`).
- **Slow-Motion & Transport**: Kecepatan variabel (`100%`, `75%`, `50%`, `33%`, `25%`, `10%`), frame stepping, dan jog/shuttle.
- **Event List & Auto-Highlight**: Fitur `[ MARK IN ]`, `[ MARK OUT ]`, tombol pintas `⚡ LAST 5s`, `⚡ LAST 10s`, `⚡ LAST 20s`, serta 20 daftar event replay dengan deskripsi dan tagging.

### ════ Phase 3: NDI & SRT Transport ════
- **NDI Discovery & I/O**: Penemuan otomatis sumber NDI di jaringan lokal (mDNS/IP) dan output NDI Program serta Preview.
- **SRT Caller & Listener**: Streaming latensi rendah point-to-point terenkripsi passphrase dengan konfigurasi buffer latency (20ms – 8000ms).

### ════ Phase 4: Audio Mixer 8-Bus + VST3 ════
- **8 Audio Busses**: Routing independen untuk `Master (M)` dan bus `A`, `B`, `C`, `D`, `E`, `F`, `G`.
- **Fader & Metering**: Fader volume presisi dB (-inf s/d +6 dB), stereo peak PPM bar, tombol `MUTE`, dan `SOLO` (PFL/AFL).
- **VST3 Effect Slot**: Arsitektur slot plugin audio (EQ, Kompresor, Noise Gate, Reverb, Delay) dengan kendali Bypass dan Dry/Wet mix.

### ════ Phase 5: 8 Channel Overlay & Stinger ════
- **8 Channel Overlay**: Ekspansi dari 4 ke 8 channel overlay grafis independen (`Overlay 1` s/d `Overlay 8`).
- **Stinger Transitions**: Transisi animasi video overlay dengan transparansi Alpha channel dan konfigurasi *Cut Point* (waktu potong pergantian background).

### ════ Phase 6: Scripting & Automation Triggers ════
- **C# Scripting Engine**: Runner skrip berbasis API (`API.Function("Cut")`, `API.Sleep(1000)`, `API.Function("Fade", "Duration=500")`).
- **Automation Triggers**: Pemetaan otomatis event siaran (`OnRecordingStart`, `OnStreamStart`, `OnTransitionComplete`, `OnCountdownCompleted`) ke aksi switcher.

### ════ Phase 7: Input Tambahan & Codec ════
- **Playlist & Video List**: Pemutaran video berurutan otomatis dengan transisi antar item dan loop.
- **Photo Slideshow**: Slideshow gambar dengan interval detik yang dapat disesuaikan.
- **Tipe Input Native**: Video Player (MP4/MKV/AVI/WMV), Live Camera (DirectShow), Desktop Capture (GDI BitBlt 60fps), Image, Audio File, Title/XAML Lower Third, SMPTE Color Bars, Studio Clock, Web Browser, dan Virtual Set.

### ════ Phase 8: MultiView Output, PTZ, & Hardware ════
- **Dedicated MultiView Window (`⛶ MULTIVIEW`)**: Mendukung tata letak `Studio` (Preview + Program besar di atas + 8 input di bawah), `Quad (2x2 VAR)`, `3x3`, dan `4x4` dengan indikator Tally border (Merah = Program, Hijau = Preview).
- **VISCA over IP PTZ Controller**: Kontrol kamera robotik Pan, Tilt, Zoom, Speed, dan Recall/Store Preset 1–10 via UDP port 52381.
- **Dual External API**:
  - **HTTP Web API (Port 8088)**: Format XML vMix kompatibel & Web Controller HTML5 (`/controller`).
  - **TCP Broadcast API (Port 8099)**: Protokol vMix TCP native (`TALLY`, `SUBSCRIBE TALLY`, `FUNCTION Cut`, `FUNCTION Fade`, dll) untuk integrasi langsung dengan **Bitfocus Companion**, **Elgato Stream Deck**, dan hardware Tally lights.

---

## 🔌 Backend Capture Nyata (bukan simulasi)

Kamera, audio meter, dan transport ride di atas `ffmpeg6.exe` bawaan vMix (`dshow` + `gdigrab` tersedia di build itu).

| Service | Yang dikerjakan | Catatan batas |
| :--- | :--- | :--- |
| `CameraDeviceService` | Enumerasi device DirectShow asli (`ffmpeg -list_devices`) | Cache per proses; `Refresh()` untuk pindai ulang |
| `CameraCaptureService` | Graph capture hidup: `dshow → rawvideo bgr24 → WriteableBitmap` | Pipe + 1 thread baca; frame drop saat UI sibuk (bukan buffer) |
| `AudioMeterService` | Level PPM nyata dari `astats`/`ametadata` device audio DirectShow | Jitter fallback dipakai hanya bila tak ada device audio |
| `NdiDiscoveryService` | Discovery NDI via mDNS `_ndi._tcp.local` | **Receive/send NDI belum ada** — butuh runtime NewTek NDI |
| `ProgramSurface` | Render canvas Program → frame BGRA (RenderTargetBitmap, double-buffer) | Sumber frame untuk encoder; alpha canvas opaque |
| `PlayoutEngine` | Playlist & foto slideshow: baca file nyata, maju berdasarkan waktu nyata | Non-media diabaikan; mode video/slideshow |
| `TitleTemplate` | Load `Titles\*.xaml` asli vMix + set field bernama (`Heading`, `Description`, …) | Namespace `vMixTitleLibrary` di-rewrite ke shim lokal |
| `ProductionEngine.ExecuteApiFunction` | Satu pintu dispatch untuk UI, HTTP 8088, TCP 8099, script, trigger | Overlay 1–8, `SetVolume`, `AudioOn/Off`, `SetTBar` |

**Jujur soal batasnya**: `vMix\ndi\x64\Processing.NDI.Lib.dll` yang ikut terinstall adalah host runtime NDI 6, bukan SDK yang bisa di-link aplikasi lain; tidak ada NDI Runtime system-wide di mesin ini, jadi NDI tetap discovery-only. `ffmpeg6.exe` vMix tidak punya writer `srt://` (hanya `rtmp`/`udp`), jadi output SRT butuh build ffmpeg ber-`libsrt` (mis. `C:\Users\BHY14\scoop\shims\ffmpeg`). Recording & streaming sekarang merekam **canvas Program** (`ProgramSurface`) bila `ProgramFrames` tersedia, dan jatuh ke `gdigrab desktop` bila tidak.

Bukti struktur filter vMix asli (REA/Ghidra atas `filters64\vMixGrabber.ax`, sha256 `15ab7c15…6cc`): DirectShow transform filter `CSampleGrabber` turunan `CTransInPlaceFilter`/`CTransformFilter` dengan pin `vMixGrabberInputPin`/`CSampleGrabberInPin` — hanya string export & nama kelas, tanpa PDB.

---

## ⌨️ Shortcut Keyboard

| Tombol | Fungsi |
| :--- | :--- |
| `Space` | **CUT** (Instant Swap Preview & Program) |
| `Enter` | **FADE** (Transisi Fade otomatis) |
| `1` – `9` | Pilih Input 1 s/d 9 ke **Preview** |
| `Shift + 1` – `9` | Pindah langsung Input 1 s/d 9 ke **Program (LIVE)** |
| `F1` – `F8` | Toggle **Overlay 1 s/d 8** |
| `F11` | Toggle **Fullscreen Output** |

---

## 📦 Menjalankan & Membangun

### 1. Menjalankan Tes Otomatis (27 Test Suite)
Memvalidasi seluruh 8 phase plus backend nyata (enumerasi DirectShow, graph kamera hidup, konversi dBFS meter audio, query mDNS NDI, dispatch API tunggal, trigger otomatis, render canvas Program, rekam program surface, template judul vMix, playout playlist/slideshow):
```bash
dotnet run -- --test
```

### 2. Menjalankan Aplikasi
```bash
dotnet run
```

### 3. Membangun Binary Standalone Release
```bash
dotnet build -c Release
```
File binary executable:
`bin\Release\net10.0-windows\Openmiks.exe`
