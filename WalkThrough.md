# ActuatorApp Codebase Walkthrough

This document guides you through the structure, features, and usage of the ActuatorApp codebase.

## 1. Getting Started

### Prerequisites
- Visual Studio 2019/2022
- .NET Core 3.1 SDK
- .NET Standard 2.0 support

### Running the App
1. Open `NodeNetwork.sln`.
2. Set `ActuatorApp` as the **Startup Project**.
3. Build and Run (F5).
4. The main window will launch with the node editor canvas.

## 2. Codebase Tour

The solution is divided into three main layers:

### 2.1 The Core (ActuatorApp.Core)
*Path: `ActuatorApp.Core/`*
This is the heart of the system. Start here to understand the *Business Domain*.
- **`Entities/`**: Database models (e.g., `ProductEntities.cs`).
- **`Interfaces/`**: Contracts like `IProductRepository` and `INodeFactory`.
- **`Validation/`**: `ConnectionRuleEngine.cs` contains the rules for what can connect to what (e.g., "A Battery cannot connect to another Battery").
- **`Products/`**: Definitions of what a "Node" is in our business terms (`ProductDefinition`).

### 2.2 The Infrastructure (ActuatorApp.Infrastructure)
*Path: `ActuatorApp.Infrastructure/`*
Handlings data persistence.
- **`Repositories/ProductRepository.cs`**: Implements SQLite access using Dapper. This is where data is loaded from `Assets/Database.db`.

### 2.3 The UI (ActuatorApp)
*Path: `ActuatorApp/`*
The WPF application that ties everything together using the NodeNetwork library.
- **`App.xaml.cs`**: The composition root. Instances of Repositories and ViewModels are created here.
- **`ViewModels/MainViewModel.cs`**: The orchestrator. Manages the `NetworkViewModel` (the canvas).
- **`ViewModels/Nodes/`**: Specific ViewModel for each node type (e.g., `TCNodeViewModel`, `TANodeViewModel`). They implement the logic for specific node behaviors.
- **`Adapters/NodeNetworkNodeFactory.cs`**: The bridge that converts a Core `ProductDefinition` into a UI `NodeViewModel`.

## 3. Key Features Walkthrough

### 3.1 creating Nodes
- **User Action**: Drag items from the left sidebar or right-click the canvas.
- **Code Path**: `MainViewModel` receives the command -> calls `NodeFactory.CreateNode` -> adds to `Network.Nodes`.

### 3.2 Connecting Nodes
- **User Action**: Drag from an Output (Right) to an Input (Left).
- **Code Path**: `ConnectionRules.ValidateConnection` is triggered. It checks `PortType` compatibility and runs `ConnectionRuleEngine` logic.
- **Features**: 
  - **Auto-Connect**: Dropping a node triggers `AutoConnectService` to try and link it to existing compatible ports automatically.

### 3.3 Grouping
- **User Action**: Select multiple nodes -> Right Click -> "Group Selected Nodes".
- **Code Path**: `GroupNodeViewModel` wraps the selected nodes, creating a nested `NetworkViewModel`. Proxy ports are generated to maintain external connections.

### 3.4 Visual Grouping (Frames)
- **User Action**: Select nodes -> Right Click -> "Create Frame".
- **Code Path**: `FrameNodeViewModel` creates a visual container (like a sticky note box) behind the nodes within `ActuatorApp/Views/FrameNodeView.xaml`.

## 4. Tips for Developers

- **Adding a new Product**: You don't always need to code! Check `Assets/products.json`. You can add new models there, and they will appear in the system if their category is supported.
- **Changing validation logic**: Go to `ActuatorApp.Core/Validation/ConnectionRuleEngine.cs`. Do not modify the ViewModels for business rule changes.
- **Customizing Node Appearance**: Look at `ActuatorApp/Views/`. Most nodes use the standard `NodeView`, but specific ones like `GroupNode` have custom templates.

## 5. Artifacts & Data
- **Database**: Located at `Assets/Database.db`.
- **Images**: Product images are loaded from `Assets/Products/`.
