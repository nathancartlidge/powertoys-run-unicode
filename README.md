# Unicode-Input: Agda-style Unicode character search in PowerToys Run
## Introduction
**TLDR:** this plugin lets you type `\ne` and get `≠` in your clipboard, or thousands of other combinations!

> Do you often need to type unicode symbols like `™` or `≤`? Do you have the wikipedia page for
> [box drawing characters](https://en.wikipedia.org/wiki/Box-drawing_character#Unicode) saved as a bookmark? Have you
> ever tried to write digital notes about Agda outside of Emacs? This might just be the plugin for you!

**Unicode-Input** is a [PowerToys Run](https://learn.microsoft.com/en-gb/windows/powertoys/run) plugin that emulates the input
capabilities of Emacs in [agda-mode](https://agda.readthedocs.io/en/v2.6.4.3/tools/emacs-mode.html#unicode-input).

If you are unfamiliar, this input method allows you to type the Latex form of (almost) any character - this will then be
replaced with the equivalent unicode character in your document, allowing for very the construction of very dense
programs!

For example, you can type `\lambda` and the plugin will insert `λ` into your document. For longer latex strings, such as
`\mathbb{N}`, you can type `\bN` - this will result in inserting `ℕ` into your document.

This plugin is based upon a complete export of Agda's input mappings - as such, any command that works in agda should
also work here!

## Installation
### Automatic Installation (`winget`)
*This doesn't work yet, sorry!*

### Manual Installation
1. Go to the [latest release](https://github.com/nathancartlidge/powertoys-run-unicode/releases/latest) and download the
   `.zip` file that matches your architecture - this is probably `x64` if you are unsure.
2. Close PowerToys
3. Locate your plugin installation folder: for me, this was `~\AppData\Local\Microsoft\PowerToys\PowerToys Run\Plugins`
4. Copy the plugin folder (`UnicodeInput`) from the release into this folder (such that the path
   `...\PowerToys Run\Plugins\UnicodeInput\` exists)
5. Open PowerToys and enable the plugin!
6. 🥳

## Development / Contributing
- This project is based upon dotnet version 8.0.x - to work on it, you will likely want a similar configuration.
- You may wish to update the libraries in `src/libs` with copies from your own machine - these can be found in the root
  directory of your PowerToys installation.
- Please write unit tests for any functionality you add

## Attribution
- Initial project structure based upon [ptrun-guid](https://github.com/skttl/ptrun-guid) by `skttl`
- GitHub CI pipeline based upon [PowerToys Run: GitKraken](https://github.com/davidegiacometti/PowerToys-Run-GitKraken) 
  by `davidegiacometti`