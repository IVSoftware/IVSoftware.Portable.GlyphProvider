# Glyph Provider

### Abstract

The **Glyph Provider** is a portable identity resolver built on **embedded resources**.

Typically, when creating custom fonts (for example, with [Fontello](https://www.fontello.com)), the tool produces both a `.ttf` font file and a corresponding `config.json`. When the config file is embedded into the assembly, the entire mapping of *friendly name -> Unicode codepoint* is available without reliance on platform APIs or file paths. At runtime, the `GlyphProvider` discovers and caches all of the available information, exposing glyphs by declarative name or enum attribute.

This is what makes the scheme portable: the source of truth for glyph identity lives in embedded JSON, not in platform-specific font mechanics.

⚠️ The `.ttf` file itself is still subject to platform import rules.  
MAUI, WPF, WinForms, etc. each require you to register or package fonts according to their own conventions. The `GlyphProvider` does not replace that step—it complements it by providing a uniform, declarative way to resolve glyphs across platforms.

---

### Table of Contents

1. [Concept](#concept)  
   - [Discover and Catalog](#discover-and-catalog)  
   - [GlyphProvider with Indexing](#glyphprovider-with-indexing)  
   - [String vs. Enum keys](#string-vs-enum-keys)  
   - [Declarative Bindings using GlyphAttribute](#declarative-bindings-using-glyphattribute)  
2. [Usage](#usage)  
   - [ToGlyph extensions](#toglyph-extensions)  
   - [Return formats](#return-formats)  
   - [XAML Integration](#xaml-integration)  
3. [Why Glyph Provider (Not Font)](#why-glyph-provider-not-font)  

---

## Concept

### Discover and Catalog
Glyph families are discovered once by enumerating embedded resources across the current app domain. Each `config.json` is deserialized into a `GlyphProvider` and cached in a lookup keyed by its declared name.

### GlyphProvider with Indexing
Glyphs can be retrieved by friendly name or enum member:

```csharp
// From string key
string g1 = "basics-icons".ToGlyph("search");

// From enum member
string g2 = "basics-icons".ToGlyph(StdBasicsGlyph.Trash);
```

### String vs. Enum keys
- String keys allow quick fuzzy lookups.  
- Enums provide compile-time safety and serve as canonical contracts between code and glyphs.

### Declarative Bindings using GlyphAttribute
Annotate models or properties directly:

```csharp
public class NavBar
{
    [Glyph(FontFamily = "basics-icons", Key = "search")]
    public string? SearchIcon { get; set; }

    [Glyph(FontFamily = "basics-icons", StdEnum = StdBasicsGlyph.Trash)]
    public string? DeleteIcon { get; set; }
}
```

---

## Usage

### ToGlyph extensions
Extension methods resolve glyphs from either string keys or enums.

```csharp
string u = "basics-icons".ToGlyph("filter");                   // "\uE806"
string d = "basics-icons".ToGlyph(StdBasicsGlyph.Filter);      // "\uE806"
```

### Return formats
Glyphs can be retrieved in multiple formats:

```csharp
// Unicode character
"basics-icons".ToGlyph("search", GlyphFormat.Unicode);

// Unicode display form
"basics-icons".ToGlyph("search", GlyphFormat.UnicodeDisplay); // U+E807

// XAML entity
"basics-icons".ToGlyph("search", GlyphFormat.Xaml);           // &#xE807;
```

### XAML Integration
Fonts still must be registered with the platform (MAUI, WPF, WinForms). Once imported, glyph strings returned from `ToGlyph` can be bound directly to text elements or used in control templates.

---

## Why Glyph Provider (Not Font)

Fonts are platform-specific, with their own rules for import and registration. The **Glyph Provider** does not manage those mechanics. Instead, it provides a **portable identity layer**:

- Glyphs are resolved by friendly name, not magic numbers.  
- Enums and attributes canonize those names for maintainability.  
- Applications stay declarative and testable, regardless of platform.  

By separating glyph identity from font rendering, the Glyph Provider keeps your code portable and future-proof.

---
