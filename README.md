# Glyph Provider

### Abstract

The **Glyph Provider** is a portable identity resolver built on **embedded resources**.

Each glyph family ships with a `config.json` alongside its `.ttf`. The JSON is embedded into the assembly, ensuring the entire mapping of *friendly name → Unicode codepoint* is available without relying on platform APIs or file paths. At runtime, the `GlyphProvider` loads this configuration once per family and caches it, exposing glyphs by declarative name or enum attribute.

This is what makes the scheme portable: the source of truth for glyph identity lives in embedded JSON, not in platform-specific font mechanics.

⚠️ The `.ttf` file itself is still subject to platform import rules.  
MAUI, WPF, WinForms, etc. each require you to register or package fonts according to their own conventions. The `GlyphProvider` does not replace that step—it complements it by providing a uniform, declarative way to resolve glyphs across platforms.

---


---

### Table of Contents

1. [Concept](#concept)  
2. [How It Works](#how-it-works)  
   - [Singleton Family Lookup](#singleton-family-lookup)  
   - [JSON Configuration](#json-configuration)  
   - [Friendly Name Mapping](#friendly-name-mapping)  
3. [Usage](#usage)  
   - [From Glyph Family](#from-glyph-family)  
   - [Lookup by Enum](#lookup-by-enum)  
   - [XAML Integration](#xaml-integration)  
4. [Why Glyph Provider (Not Font)](#why-glyph-provider-not-font)  
5. [Relation to OnePage Tenets](#relation-to-onepage-tenets)

---
