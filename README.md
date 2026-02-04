# Virtual Reality Showroom for Artisan Jewelry

<p align="center">
  <img src="Assets/_Models/HOME/MATERIAL/ARTISAN  XPERIENCE LOGO V2.jpg" alt="Project Logo" width='600' height='600'/>
</p>

## 📖 Overview

This project was developed as part of a Master's Thesis at **labLENI, Universitat Politècnica de València (UPV), Spain**.

The primary objective was to create an immersive **Virtual Reality (VR) showroom application** to promote the artisan jewelry creations of the renowned Valencian artist **José Marín**. The project aimed to demonstrate the superiority of immersive VR experiences over traditional web-based e-commerce platforms for artisan product showcasing.

### Key Achievements

- ✅ Developed a fully immersive VR showroom for artisan jewelry exploration
- ✅ Integrated a **conversational virtual agent** powered by **Generative AI**
- ✅ Conducted **40 experimental sessions** comparing VR vs. traditional web experiences
- ✅ Validated VR superiority through objective metrics and user behavior analysis
- ✅ Provided IT consulting and completed the **ViewShop** project in collaboration with Politecnico di Milano

---

## 🎯 Research Objectives

1. **Promote Artisan Heritage**: Showcase Valencian artisan jewelry in an innovative, engaging format
2. **Unified User Experience**: Create a seamless interaction between users and products through immersive VR
3. **AI Integration**: Introduce a conversational virtual agent to enhance user guidance and engagement
4. **Empirical Validation**: Quantify and validate the effectiveness of VR experiences compared to traditional webpages
5. **Metrics Collection**: Gather objective behavioral data from user sessions for analysis

---

## 🏗️ Technical Architecture

### Platform & Engine
- **Unity Engine**: Version 2022.3.20f1 (LTS)
- **Target Platform**: Meta Quest VR headsets
- **Rendering Pipeline**: Standard Pipeline with Shader Graph support

### VR Framework
- **Meta XR SDK**: Complete suite for VR development
  - `com.meta.xr.sdk.all`: v65.0.0
  - `com.meta.xr.sdk.core`: v62.0.0
  - `com.meta.xr.sdk.interaction.ovr`: v64.0.0
- **Unity XR Management**: v4.4.1
- **Oculus Integration**: v4.1.2

### AI & Communication
- **Socket.IO Unity**: Real-time bidirectional communication with AI backend
- **Meta Wit.ai**: Voice-to-text transcription for natural language input
- **NAudio**: Advanced audio processing for AI voice responses
- **Generative AI Backend**: External server for conversational agent processing

### User Interface
- **TextMesh Pro**: High-quality text rendering
- **Unity UI System**: Canvas-based interfaces
- **3D Interaction System**: Custom hand tracking and controller interactions

---

## 📁 Project Structure

```
├── Assets/
│   ├── _Animation/          # Animation clips and controllers
│   ├── _Files/              # Configuration and data files
│   ├── _Font/               # Typography assets
│   ├── _Materials/          # Material definitions
│   ├── _Media/              # Audio, video, and image assets
│   │   ├── Audio/           # Sound effects and music
│   │   ├── Backgrounds/     # Environment textures
│   │   ├── Jewels-img/      # Jewelry product images
│   │   └── Video/           # 180° and 2D video content
│   ├── _Models/             # 3D models
│   │   ├── HOME/            # Home environment assets
│   │   ├── JOYA/            # Jewelry 3D models
│   │   └── Beach/, Pier/    # Environment models
│   ├── _Prefabs/            # Reusable prefab objects
│   ├── _Scenes/             # Unity scenes
│   │   ├── Intro.unity      # Introduction/landing scene
│   │   ├── Home.unity       # Main hub/home scene
│   │   ├── Jewel1-4.unity   # Individual jewelry showcase scenes
│   │   └── ConversationalAgent.unity  # AI agent testing scene
│   ├── _Scripts/            # C# source code
│   │   ├── Interactions/    # Object interaction logic
│   │   ├── Menu/            # UI and menu systems
│   │   ├── Metrics/         # Data collection for experiments
│   │   ├── SceneControllers/# Scene-specific controllers
│   │   ├── SceneManagers/   # Scene lifecycle management
│   │   ├── SocketIO/        # AI communication layer
│   │   └── Video/           # Video playback systems
│   └── _Shaders/            # Custom shaders
├── Packages/                # Unity package dependencies
├── ProjectSettings/         # Unity project configuration
└── README.md               # This file
```

---

## 🎨 Features

### Immersive Jewelry Showcase
- **360° Product Exploration**: Users can examine jewelry pieces from all angles
- **Multiple Environments**: Beach, pier, and custom showroom settings
- **High-Fidelity 3D Models**: Detailed jewelry models with realistic materials

### Conversational AI Agent
- **Voice Recognition**: Natural speech input using Meta Wit.ai
- **Real-time AI Responses**: Socket.IO-based communication with AI backend
- **Audio Playback**: High-quality voice synthesis for agent responses
- **Context-Aware Interaction**: Agent understands scene context and product information

### Interaction System
- **Hand Tracking**: Natural hand-based object manipulation
- **Controller Support**: Traditional VR controller interactions
- **3D Buttons**: Intuitive 3D UI elements
- **Grabbable Objects**: Pick up and examine jewelry pieces

### Metrics Collection
- **User Behavior Tracking**: Head position, gaze direction, interaction times
- **Product Engagement**: Time spent with each item, interaction frequency
- **Session Recording**: Complete session data for research analysis
- **CSV Export**: Data export for statistical analysis

---

## 🔧 Installation & Setup

### Prerequisites
- Unity Hub with Unity 2022.3.20f1 installed
- Meta Quest Link (for development)
- Git LFS for large file support

### Setup Steps

1. **Clone the Repository**
   ```bash
   git clone https://github.com/Alebrex99/Thesis-Artigiani-StandardPipeline.git
   cd Thesis-Artigiani-StandardPipeline
   ```

2. **Open in Unity**
   - Launch Unity Hub
   - Click "Open" and select the project folder
   - Wait for Unity to import all assets

3. **Configure Build Settings**
   - Go to `File > Build Settings`
   - Select "Android" platform
   - Enable "Texture Compression: ASTC"
   - Click "Switch Platform"

4. **Configure XR Settings**
   - Go to `Edit > Project Settings > XR Plug-in Management`
   - Enable "Oculus" for Android platform

5. **Build & Deploy**
   - Connect Meta Quest device
   - Click `File > Build and Run`

### AI Backend Setup (Optional)
The conversational agent requires an external AI server. Configure the server endpoint in the Socket.IO manager settings.

---

## 🔬 Research Methodology

### Experimental Design
- **Sample Size**: 40 participants across experimental sessions
- **Comparison Groups**: VR showroom vs. traditional artisan webpage
- **Metrics Collected**: 
  - Task completion time
  - Product exploration depth
  - User engagement indicators
  - Head tracking and gaze data
  - Interaction frequency

### Data Collection
The `Metrics.cs` script captures:
- User head position and rotation (every 10 frames)
- Object grab/release events with timestamps
- UI interaction events
- Session start/end times
- Product-specific engagement metrics

### Results
**VR demonstrated superiority** in user engagement and product exploration compared to the traditional web experience. Detailed analysis confirmed the effectiveness of immersive technology for artisan product showcasing.

---

## 🤝 Collaboration

### ViewShop Project
In addition to the main thesis work, collaboration with **Politecnico di Milano** enabled the completion of the **ViewShop** project, which focused on collecting objective metrics for e-commerce experiences. IT consulting was provided within labLENI to support ongoing research initiatives.

---

## 🛠️ Technologies Used

| Category | Technology | Version |
|----------|-----------|---------|
| Game Engine | Unity | 2022.3.20f1 |
| VR Platform | Meta Quest | - |
| VR SDK | Meta XR SDK | 65.0.0 |
| Networking | Socket.IO Unity | Latest |
| Voice Recognition | Meta Wit.ai | - |
| Audio Processing | NAudio | 1.8.0 |
| Text Rendering | TextMesh Pro | 3.0.9 |
| Animation | Unity Timeline | 1.7.6 |
| Visual Scripting | Unity Visual Scripting | 1.9.4 |

---

## 📜 Credits & Acknowledgments

### Development
- **Author**: Alessandro Bresciani
- **Institution**: Universitat Politècnica de València (UPV)
- **Laboratory**: labLENI
- **Location**: Valencia, Spain

### Collaboration
- **Partner Institution**: Politecnico di Torino, Italy

### Artisan
- **José Marín**: Valencian artisan jeweler whose creations are featured in the VR showroom

### Supervision
- labLENI Research Team, UPV
- Prof. Mariano Alcaniz Raya
- Dott. Jaime Provinciale
- Prof. Lamberti Fabrizio, Politecnico di Torino

---

## 📧 Contact

For inquiries about this research project, please contact Alessandro Bresciani.

---

<p align="center">
  <i>Developed with ❤️ at labLENI, Universitat Politècnica de València</i>
</p>
