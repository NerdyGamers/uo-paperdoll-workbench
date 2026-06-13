# uo-paperdoll-workbench

A WPF desktop application for AI-assisted classification of Ultima Online gump and paperdoll wearable assets using local [Ollama](https://ollama.com) vision models.

## Features

- **Two-pass classification pipeline**
  - **Pass 1** — Coarse gump taxonomy: identifies asset class, shape, style, function, and whether the asset is paperdoll-related
  - **Pass 2** — Paperdoll wearable detail: identifies slot, material, gender, body type, state, hueable flag, covered regions, and theme
- **Local AI only** — No cloud API keys required; runs entirely via Ollama on your machine
- **Deterministic outputs** — Temperature 0 enforced; `Unknown` used for all uncertain fields
- **Non-destructive** — Original files are never moved or modified; sorted copies go to output folders
- **Manifest export** — CSV and JSON manifests written after each pass
- **Modern dark UI** — Queue, image preview, inspector pane, and batch progress bar

## Prerequisites

| Requirement | Version |
|---|---|
| .NET SDK | 9.0+ |
| Ollama | Latest |
| Ollama vision model | `llava` (default) or any multimodal model |

## Setup

### 1. Install Ollama

Download and install from https://ollama.com/download

### 2. Pull a vision model

```bash
ollama pull llava
```

Or use any other multimodal model (e.g. `llava:13b`, `moondream`, `bakllava`).

### 3. Start Ollama

Ollama starts automatically on most systems. Verify it is running:

```bash
curl http://localhost:11434/api/tags
```

### 4. Clone and build

```bash
git clone https://github.com/NerdyGamers/uo-paperdoll-workbench.git
cd uo-paperdoll-workbench
dotnet build
```

### 5. Run

```bash
dotnet run
```

Or open `UOGumpClassifier.csproj` in Visual Studio 2022+ and press F5.

## Usage

1. Click **Load Folder** and select a directory containing PNG gump assets
2. Click **Start Pass 1** to run coarse classification on all queued files
   - Results are written to `output/pass1_manifest.json` and `output/pass1_manifest.csv`
3. After Pass 1 completes, click **Start Pass 2** to classify paperdoll-related items
   - Results are written to `output/pass2_manifest.json` and `output/pass2_manifest.csv`
   - Files are sorted into `output/sorted/` by slot and material (non-destructive copies)
4. Use the **Inspector** panel on the right to review classification details and raw JSON for each asset
5. Click **Cancel** at any time to abort the current pass

## Project Structure

```
uo-paperdoll-workbench/
├── Models/
│   ├── GumpAssetItem.cs       # Queue item model with file metadata
│   ├── Pass1Result.cs         # Pass 1 classification output schema
│   └── PaperdollResult.cs     # Pass 2 paperdoll classification schema
├── Services/
│   ├── OllamaService.cs       # Ollama HTTP client, Pass1 + Pass2 prompts
│   ├── ClassifierService.cs   # Batch orchestration for both passes
│   ├── ImageMetadataService.cs# SHA-256, dimensions, alpha detection
│   ├── ManifestExporter.cs    # JSON + CSV manifest writer
│   └── FileSorter.cs          # Non-destructive copy into sorted folders
├── Schemas/
│   ├── pass1_schema.json      # JSON Schema for Pass 1 output validation
│   └── pass2_schema.json      # JSON Schema for Pass 2 output validation
├── Themes/
│   └── Dark.xaml              # Dark color palette resource dictionary
├── ViewModels/
│   └── MainViewModel.cs       # MVVM ViewModel with commands and state
├── MainWindow.xaml            # UI layout: queue, preview, inspector, progress
├── MainWindow.xaml.cs         # Code-behind: status log auto-scroll
├── App.xaml                   # Application startup and theme loading
└── UOGumpClassifier.csproj    # Project file (.NET 9, WPF)
```

## Classification Schemas

### Pass 1 — Gump Asset

| Field | Type | Notes |
|---|---|---|
| `file_id` | string | SHA-256 prefix |
| `asset_class` | enum | `gump`, `paperdoll`, `icon`, `cursor`, `background`, `unknown` |
| `gump_type` | string | Button, Frame, Container, etc. |
| `shape` | string | Rectangular, Circular, Irregular |
| `style` | string | Stone, Wood, Metal, Ornate |
| `function_guess` | string | Inferred UI function |
| `has_text` | bool | Visible text present |
| `visible_text` | string | Verbatim text content |
| `is_paperdoll_related` | bool | Flagged for Pass 2 |
| `confidence` | float 0–1 | Model confidence |

### Pass 2 — Paperdoll Wearable

| Field | Type | Notes |
|---|---|---|
| `base_name` | string | e.g. `Shirt`, `PlateChest` |
| `slot` | enum | Head, Chest, Arms, Hands, Legs, Feet, Waist, Back, Neck, Ring, Earring, Shield, Weapon |
| `material` | enum | Cloth, Leather, Chain, Plate, Bone, Wood |
| `gender` | enum | Male, Female, Neutral |
| `body_type` | enum | Human, Elf, Gargoyle |
| `state` | enum | Base, Equipped, Partial |
| `is_hueable` | bool | Supports dye/hue |
| `is_partial_piece` | bool | One layer of multi-part item |
| `covers_regions` | string[] | e.g. `["torso", "shoulders"]` |
| `theme` | string | Medieval, Fantasy, Tribal, etc. |
| `confidence` | float 0–1 | Model confidence |

## Notes

- All uncertain fields are set to `Unknown` — the model is instructed **not** to guess game lore
- Anchor offset data (`offset_x`, `offset_y`) is computed from image metadata, not inferred by AI
- The Ollama endpoint defaults to `http://localhost:11434` and can be changed in `OllamaService.cs`
- Tested with `llava` 7B and 13B; larger models produce higher confidence outputs
