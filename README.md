# Unicode-Input: Agda and HTML-style Unicode character input in PowerToys Run
![Build Pipeline](https://github.com/nathancartlidge/powertoys-run-unicode/actions/workflows/build.yml/badge.svg)

## Introduction
**TLDR:** this plugin lets you type `\ne` and get `≠` in your clipboard, `&times;` and get `×`, `&#x1f61d;` to get
`😝`, or thousands of other combinations!

> Do you often need to type unicode symbols like `™` or `≤`? Do you have the wikipedia page for
> [box drawing characters](https://en.wikipedia.org/wiki/Box-drawing_character#Unicode) saved as a bookmark? Have you
> ever tried to write digital notes about Agda outside of Emacs? This might just be the plugin for you!

**Unicode-Input** is a [PowerToys Run](https://learn.microsoft.com/en-gb/windows/powertoys/run) plugin that emulates the input
capabilities of both [HTML entities](https://developer.mozilla.org/en-US/docs/Glossary/Entity) and Emacs in [agda-mode](https://agda.readthedocs.io/en/v2.6.4.3/tools/emacs-mode.html#unicode-input).

> If you are unfamiliar, Agda's input method allows you to type the Latex form of (almost) any character - this will
> then be replaced with the equivalent unicode character in your document, allowing for very the construction of very
> dense programs!
>
> For example, you can type `\lambda` and the plugin will copy `λ` into your clipboard. For some longer latex strings,
> such as `\mathbb{N}` (for `ℕ`), a shorter version is often - in this case, you would type `\bN`. It's not just
> limited to mathematics - for example, you could type `\'a` to get the accented character `á`
> 
> HTML entities attempts to accomplish a broadly similar task (representing unicode characters with text), but are
> slightly more verbose in cases.

This plugin is based upon a complete export of Agda's input mapping library and WHATWG's entity mappings - as such, any
command that works in Agda or in your browser should also work here!

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
- Auto-typing implementation from [InputTyper](https://github.com/CoreyHayward/PowerToys-Run-InputTyper) by
- `CoreyHayward`