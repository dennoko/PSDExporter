# PSD Exporter User Manual

**PSD Exporter** is a Unity Editor extension that automatically splits baked textures into layers based on specified mask textures and exports them directly into **PSD (Adobe Photoshop)** files.

It is designed for 3D creators and avatar/asset developers (such as those using Substance 3D Painter, Blender, or other baking pipelines) who only have flat baked textures and need to distribute cleanly organized, layered PSDs for texture customization.

---

## Table of Contents

1. [Key Features](#key-features)
2. [How to Launch](#how-to-launch)
3. [Quick Start (3 Basic Steps)](#quick-start-3-basic-steps)
4. [Interface & Detailed Features](#interface--detailed-features)
   - [1. Input Textures (Slot List)](#1-input-textures-slot-list)
   - [2. Layer Structure (Tree View)](#2-layer-structure-tree-view)
   - [3. Selected Item Details](#3-selected-item-details)
   - [4. Export Options](#4-export-options)
   - [5. Output Settings](#5-output-settings)
   - [6. Preview and Export Execution](#6-preview-and-export-execution)
5. [Advanced Workflows](#advanced-workflows)
   - [Batch Processing BaseColor, Normal, and Metallic Maps](#batch-processing-basecolor-normal-and-metallic-maps)
   - [Extracting Layers from RGBA Channel-Packed Masks](#extracting-layers-from-rgba-channel-packed-masks)
   - [Preserving Layer Masks for Non-Destructive Editing](#preserving-layer-masks-for-non-destructive-editing)
6. [Safety & Technical Details](#safety--technical-details)
7. [Troubleshooting & FAQ](#troubleshooting--faq)

---

## Key Features

* **Zero External Dependencies**: Fast, native C# binary PSD generation within the standard Unity Editor environment—no third-party DLLs or external tools required.
* **Batch Multi-Texture Export**: Apply a single layer/mask configuration across multiple maps (e.g., BaseColor, Normal, Metallic/Smoothness) and export corresponding PSD files simultaneously.
* **Flexible Cutout Modes**: Choose between "Transparent Cutout" (clearing pixels outside masks) and "Preserve Layer Mask" (attaching masks as Photoshop layer mask channels for non-destructive editing).
* **Layer Group (Folder) Support**: Organize layers into nested folder hierarchies that are preserved upon opening in Photoshop, CLIP STUDIO PAINT, etc.
* **Per-Channel Mask Extraction**: Extract mask data not only from grayscale luminance but also from isolated R, G, B, or Alpha channels.
* **Automatic & Safe Texture Importer Restoration**: Even if `Read/Write Enabled` is turned off, textures are temporarily made readable and automatically restored to their exact original settings when finished or if an error occurs.
* **Full Undo / Redo Integration**: All window operations fully support Unity's native Undo system (`Ctrl + Z` / `Ctrl + Y`).

---

## How to Launch

From the top Unity menu bar, select:

> **dennokoworks → PSD Exporter**

The editor window will appear. You can resize it (recommended width: 520px or higher) or dock it anywhere within the Unity Editor layout.

---

## Quick Start (3 Basic Steps)

Here is the quickest way to create a layered PSD from a flat BaseColor texture:

1. **Register the Input Texture**:
   * Drag & drop your completed texture (e.g., BaseColor PNG) into the **"Input Textures"** drop area.
2. **Register Mask Textures**:
   * Drag & drop your mask textures into the **"Drop mask textures here to add layers"** area.
   * Each dropped mask immediately becomes a layer.
3. **Export**:
   * Check your output directory and click the **"Export"** button at the bottom.
   * A layered PSD file will be generated in the specified folder.

---

## Interface & Detailed Features

### 1. Input Textures (Slot List)

Register the source baked textures to be exported.

| Control | Description |
| :--- | :--- |
| **Checkbox** | Toggles whether this texture slot is included in the export. |
| **Texture Field** | The source texture asset (Texture2D). |
| **Suffix Field** | The suffix appended to the output filename (e.g., `BaseColor`, `Normal`). |
| **▼ Button** | Opens a preset dropdown with standard suffixes (`BaseColor`, `Normal`, `Metallic`, `Roughness`, `Emission`, `Alpha`, `AO`, etc.). |
| **↑ / ↓ Buttons** | Reorders the texture slots. |
| **× Button** | Removes the slot. |
| **Drop Area** | Dragging and dropping textures here adds them as new slots. |
| **+ Add Slot** | Manually adds an empty texture slot. |

---

### 2. Layer Structure (Tree View)

Configure the layer and folder hierarchy for the exported PSD. **Top items are in the foreground (front); bottom items are in the background (back).**

* **+ Layer**: Adds a new mask layer.
* **+ Group**: Adds a new folder/group.
* **Drop Area**: Dropping mask images here creates layers automatically (supports multi-selection).

#### Per-Row Action Buttons:
* **Eye Icon**: Sets whether the layer is initially visible or hidden when opened in an image editing program.
* **Checkbox**: Toggles inclusion in the export (unchecking skips this layer).
* **↑ / ↓**: Moves the layer up or down in the stack.
* **← (Outdent)**: Moves a nested layer out of its parent group.
* **→ (Indent)**: Moves the layer into the group directly above it.
* **×**: Deletes the layer or group.

---

### 3. Selected Item Details

Selecting a layer or group in the tree displays its detailed configuration in the inspector panel below.

#### When a Layer is Selected:
* **Name**: The layer name in the PSD.
* **Opacity**: Layer opacity percentage (0% to 100%).
* **Include in Export**: Enables or disables this layer during export.
* **Visible**: Initial visibility state in the PSD.
* **Mask**: The mask texture (Texture2D).
* **Mask Channel**: Specifies which channel to extract as the mask:
  * `Luminance (Grayscale)`: Uses RGB brightness (ideal for standard black-and-white masks).
  * `R`, `G`, `B`: Uses the selected color channel.
  * `Alpha`: Uses the texture's alpha transparency.
* **Invert Mask**: Inverts the black and white values of the mask.

#### When a Group is Selected:
* **Name**: The folder name.
* **Opacity**: The overall opacity of the group.
* **Expand in PSD**: When checked, the group folder is expanded (opened) by default when viewed in Photoshop.

---

### 4. Export Options

Expand the **"Export Options"** foldout to configure generation settings.

* **Cutout Mode**:
  * **Transparent Cutout (Default)**: Keeps only pixels corresponding to the white mask areas and sets the rest to transparent. Works out of the box in virtually all image software.
  * **Preserve Layer Mask**: Retains the full source image pixels and attaches the mask as a native Photoshop layer mask channel. Recommended for creators who want non-destructive mask painting in Photoshop or CLIP STUDIO PAINT.
* **Original Background Layer**:
  * When enabled, adds an uncut, full-size source texture layer at the very bottom of the PSD stack (default name: `Background`).
* **Unassigned Difference Layer**:
  * When enabled, automatically generates an extra base layer containing **all pixels not covered by any defined mask** (default name: `Base`).
* **Trim Layer Bounds (Reduce Size)**:
  * When enabled, automatically crops transparent margins of each layer to its minimal bounding box, dramatically reducing the resulting PSD file size.
* **Read Quality**:
  * **Source File Quality (Recommended)**: Temporarily bypasses Unity texture compression (DXT/BC7) and resolution limits to read the original raw image at full fidelity.
  * **As Imported**: Uses the texture exactly as imported in the Unity Editor.

---

### 5. Output Settings

* **Output Folder**: Destination directory for exported PSD files (click `...` to browse).
* **File Prefix**: Prefix for generated filenames.
  * *Example:* If Prefix is `Costume` and the slot suffix is `BaseColor`, the output file will be `Costume_BaseColor.psd`.
* **Existing Files (Overwrite Policy)**:
  * `Ask Before Overwrite`: Displays a confirmation prompt if files already exist.
  * `Overwrite`: Automatically overwrites existing files.
  * `Save as Copy`: Appends a numbered suffix (e.g., `_1`, `_2`).
* **Import into AssetDatabase**: Automatically triggers an AssetDatabase import if the output path is inside the project's `Assets/` directory.
* **Planned Output Files**: Displays a live list of the filenames that will be generated.

---

### 6. Preview and Export Execution

#### Preview
* **Target Slot**: Selects which texture slot to preview.
* **Refresh Preview Button**: Generates a composite preview directly inside the window based on the current layers and options.
  * *Note:* If you modify layers or options, a notice indicates that the preview is stale until refreshed.

#### Running the Export
* When there are no validation errors, the **"Export"** button at the bottom becomes enabled.
* Clicking it opens a progress dialog while each texture slot is processed and encoded into a PSD file.
* Once complete, if the file was written to the `Assets/` directory, Unity will highlight (ping) the generated file in the Project window.

---

## Advanced Workflows

### Batch Processing BaseColor, Normal, and Metallic Maps

When preparing texture modification packages for 3D avatars or outfits:

1. Add 3 slots under **"Input Textures"**:
   * Slot 1: `outfit_BaseColor.png` (Suffix: `BaseColor`)
   * Slot 2: `outfit_Normal.png` (Suffix: `Normal`)
   * Slot 3: `outfit_Metallic.png` (Suffix: `Metallic`)
2. Define your mask layers (e.g., "Jacket", "Shirt", "Buttons", "Lining").
3. Click **Export**. Three synchronized PSD files are produced:
   * `outfit_BaseColor.psd`
   * `outfit_Normal.psd`
   * `outfit_Metallic.psd`
   All files share the identical layer names, folder structures, and mask boundaries, making multi-map editing seamless.

---

### Extracting Layers from RGBA Channel-Packed Masks

If your workflow packs multiple masks into the four channels of a single texture:

1. Create 4 layers and assign the same packed texture to each.
2. In the Details panel, set the **Mask Channel** for each:
   * Layer 1: `R`
   * Layer 2: `G`
   * Layer 3: `B`
   * Layer 4: `Alpha`
3. Each channel will extract its corresponding region into a separate layer.

---

### Preserving Layer Masks for Non-Destructive Editing

When distributing PSDs to end users for recoloring:

* Set **Cutout Mode** to **"Preserve Layer Mask"**.
* Instead of destructive transparent cutouts, full image pixels are retained with black-and-white layer masks attached.
* Users can use standard brushes or selection tools in Photoshop or CLIP STUDIO PAINT to expand or soften mask borders without losing original texture data.

---

## Safety & Technical Details

* **Non-Destructive Texture Importer Restoration**:
  * To read uncompressed, unscaled pixels, the tool temporarily adjusts `TextureImporter` settings.
  * A journal-based restoration system guarantees that all textures are restored to their original importer configurations upon completion, cancellation, or error.
* **Automatic Resolution Synchronization**:
  * If source textures (e.g., 4096×4096) and mask textures (e.g., 2048×2048) have different resolutions, the masks are automatically resampled using bilinear filtering to match the base texture. Manual pre-scaling is not required.

---

## Troubleshooting & FAQ

### Q: The "Export" button is disabled (grayed out).
**A:** Check the validation messages displayed above the button:
* Ensure at least one texture slot is enabled and has a valid texture assigned.
* Ensure all layers have a mask texture assigned.
* Ensure layer and group names are not blank.
* Ensure the output directory is valid and output filenames do not conflict.

### Q: The exported PSD layer is completely black or transparent.
**A:** Check the **Mask Channel** setting on the layer. For instance, if you select `Alpha` on a standard RGB PNG mask that has no alpha channel, the alpha may be read as zero. For standard black-and-white mask images, ensure **"Luminance (Grayscale)"** is selected.

### Q: The resulting PSD file is huge.
**A:** Enable **"Trim Layer Bounds (Reduce Size)"** in Export Options. This trims empty transparent margins from every layer, significantly reducing file size.

### Q: Can I open the exported files in CLIP STUDIO PAINT, Paint Tool SAI, or GIMP?
**A:** Yes. The output strictly conforms to the standard Adobe Photoshop PSD specification (8-bit RGB, RLE PackBits compression) and is compatible with any image editing software that supports PSD files.
