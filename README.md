# MIT Hackathon 2026 Template

Unity VR template project for Meta Quest with 360/180 video playback support.

## Features

- **360 Video Playback** - Full sphere rendering for equirectangular 360 videos
- **180 Video Support** - Equirectangular and fisheye projection support
- **Flat Video** - Standard flat screen video playback
- **Stereo 3D** - Top-bottom and left-right stereo packing support
- **File Explorer** - Browse and select video files on device
- **XR Interaction** - Full VR controller support with Meta Quest

## Screen Types

- **Sphere** - 360 degree equirectangular projection
- **Equirect** - 180 degree equirectangular projection
- **Fisheye** - 180 degree fisheye projection
- **Flat** - Standard flat screen

## Requirements

- Unity 6 (6000.3.4f1)
- Meta Quest 2/3/Pro
- AVPro Video (free version with watermark included)
- XR Interaction Toolkit

## Setup

1. Open project in Unity
2. Build and deploy to Meta Quest

## Project Structure

```
Assets/
├── Scripts/
│   ├── AppStateManager.cs      - Application state management
│   ├── VideoScreen.cs          - Video screen mesh control
│   ├── FileExplorer.cs         - File browser UI
│   ├── XRMediaPlayerController.cs - VR media controls
│   └── ScreenMesh/             - Mesh generators
├── Prefabs/
│   ├── MediaPlayerUI.prefab    - Video player controls
│   ├── Files.prefab            - File explorer UI
│   └── FileEntry.prefab        - File list item
└── Scenes/
    └── TemplateScene.unity     - Main scene
```
