# FC2Editor-Refactored

This repository contains a fully refactored, compilable C# source code project for the Far Cry 2 Map Editor. The original application was decompiled using ILSpy and has been meticulously reorganized into a clean, maintainable, and editable Visual Studio solution targeting the .NET 2.0 Framework.

The primary goal of this project is to provide a stable source code base for the Far Cry 2 modding community, enabling developers to learn from, debug, and extend the original editor's capabilities.

## Project Status

-   [x] **Fully Compilable:** The project builds without errors in Visual Studio.
-   [x] **Fully Runnable:** The application launches and is functional.
-   [x] **Refactored:** Decompiler artifacts have been cleaned, and the code has been organized into a logical folder structure.
-   [x] **Dependencies Managed:** All required native game DLLs are handled by the project for automatic copying on build.

## Features

*   Complete source code for all editor forms, UI controls, and logic.
*   P/Invoke wrappers for interacting with the native `Dunia.dll` game engine.
*   Full implementation of all terrain editing, object placement, and map property tools.
*   A working project structure that can be easily opened and built in modern versions of Visual Studio.

## Getting Started

### Prerequisites

1.  **Visual Studio:** A version that supports .NET Framework 2.0 development (VS 2010 or newer recommended).
2.  **Far Cry 2:** A legitimate copy of the game is required to obtain the necessary engine DLLs.
3.  **.NET Framework 2.0:** Ensure the runtime is installed on your machine.

### Build Instructions

1.  **Clone the repository:**
    ```
    git clone https://github.com/your-username/FC2Editor-Refactored.git
    ```

2.  **Open in Visual Studio:**
    *   Open the `FC2Editor.sln` solution file in Visual Studio.
    *   The project is configured to automatically copy the game DLLs to the output directory upon building.

3.  **Build the Solution:**
    *   From the top menu, select **Build > Rebuild Solution**.
    *   The project should compile without any errors.

4.  **Run:**
    *   Press **F5** or click the **Start** button to launch the editor.

## Project Goals & Future Work

This project provides a foundation for many exciting possibilities:

*   **Adding Custom Content:** Investigating how to add new models, textures, and objects to the editor's library.
*   **Improving UI/UX:** Modernizing the user interface for better usability.
*   **New Tools:** Developing new terrain or object placement tools to enhance the mapping workflow.
*   **Engine Exploration:** Using this C# host application as a tool to further reverse-engineer the Dunia engine.

## Disclaimer

This project is an unofficial, community-driven effort created for educational and interoperability purposes. It is the result of reverse-engineering the original, publicly available map editor. All original copyrights for Far Cry 2, the Dunia Engine, and related assets belong to Ubisoft Entertainment. This project makes no claim to ownership of the original intellectual property. The provided source code is for the C# application layer only.

## License

This project is licensed under the **MIT License**. See the `LICENSE` file for details.
