# Changelog
All notable changes to VIRNECT Track unity package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)

## [1.11.0] - 2024-07-04

### Added

- Added AR-Foundation fusion tracker for Android
- Added AR-Foundation fusion tracker for iOS
- Added event trigger UI for target tracking state
- Added sample project for fusion tracking
- Added support for Universal Rendering Pipeline (URP)

### Changed
- Enabled dynamic calibration update during runtime
- Improved error feedback during tracking
- Improved overall tracking performance
- Improved camera access on Android


## [1.10.0] - 2024-03-28

### Changed
- improved target visibility handling
- improved overall tracking performance
- improved .track file compatibility
- optimized multi threading


## [1.10.0] - 2024-03-28

### Changed
- improved target visibility handling
- improved overall tracking performance
- improved .track file compatibility
- optimized multi threading

## [1.9.0] - 2023-11-09
### Added
- added generic Object3D target tracking support
- added relocation functionality
- added drop-down to select USB camera by name
- added check box to set target as static

### Changed
- fixed bug with linux library linking
- improved overall tracking performance
- optimized multi threading

## [1.8.0] - 2023-06-29
### Changed
- improved Image tracking performance

## [1.7.0] - 2023-04-20
### Added
- device orientation interface

### Changed
- improved QR tracking performance
- improved Image tracking performance
- improved Shape tracking performance
- various minor bug fixes

## [1.6.0] - 2022-12-01
### Added
- shape targets that enable usage of cylinder, cube and planar objects as targets

### Changed
- improved QR tracking performance
- improved Image tracking performance
- various minor bug fixes

## [1.5.0] - 2022-08-18
### Added
- extended calibration format to support multiple resolutions and cameras
- extended UI with resolution dropdown menu

### Changed
- separated QR code target name from encoded content
- removed openMP dependency on all platforms
- general tracking improvements for all target types

## [1.4.0] - 2022-04-14
### Added 
- external image source input
- multiple CAD target tracking 

### Changed
- updated to '.track' target file format
- target placeholder visualization material

### Removed
- automatic CAD model preview

### Fixed
- Z-axis direction inversion of Target GameObject

## [1.3.0] - 2021-12-17
### Changed 
- improved CAD tracking performance
- target visualization UX

## [1.2.1] - 2021-09-10
### Changed 
- improved CAD tracking performance

## [1.2.0] - 2021-08-27
### Added
- CAD model tracking 
- MAP target tracking
- MAP target recording

### Removed
- extended tracker experimental feature

## [1.1.0] - 2021-05-06
### Added
- Record and replay functionality
- RecoderUI prefabs
- ActiveTargetManagerUI prefabs

## [1.0.0] - 2021-04-25
### Added
- Enable usage of *Virnect Track* framework with Unity Native (Windows / Linux) and Android
- Tracking for Image & QR-Code targets
- TrackSettings asset
- TrackManager- and TrackTarget- prefabs
- Scene sanitization logic
- Prebuild check

### This is the first release of *VIRNECT Track*.