# InstantProjectInitializer

[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](LICENSE)

プロジェクト初期化に必要なファイルを一発生成するCLIツール。

A CLI tool that instantly generates project initialization files.

---

## 目次 / Table of Contents

- [概要 / Overview](#概要--overview)
- [前提条件 / Prerequisites](#前提条件--prerequisites)
- [インストール / Installation](#インストール--installation)
- [使い方 / Usage](#使い方--usage)
- [注意点 / Notes](#注意点--notes)
- [ライセンス / License](#ライセンス--license)

---

## 概要 / Overview

### 日本語

`ipi` は使用する言語を指定するだけで、新しいプロジェクトに必要な設定ファイルをまとめて生成するCLIツールです。

生成されるファイル：

| ファイル | 生成元 |
|---|---|
| `.gitignore` | [gitignore.io](https://www.toptal.com/developers/gitignore) |
| `devenv.nix` | [devenv.new](https://devenv.new) |
| `devenv.yaml` | [devenv.new](https://devenv.new) |
| `.envrc` | 固定テンプレート |

### English

`ipi` is a CLI tool that generates project configuration files all at once by simply specifying the languages you use.

Generated files:

| File | Source |
|---|---|
| `.gitignore` | [gitignore.io](https://www.toptal.com/developers/gitignore) |
| `devenv.nix` | [devenv.new](https://devenv.new) |
| `devenv.yaml` | [devenv.new](https://devenv.new) |
| `.envrc` | Fixed template |

---

## 前提条件 / Prerequisites

### 日本語

以下がインストール済みであることが必要です。

- [Nix](https://nixos.org/download/) （flakes有効化済み）
- [devenv](https://devenv.sh/)
- [direnv](https://direnv.net/)

Nix flakesの有効化：`~/.config/nix/nix.conf` または `/etc/nix/nix.conf` に以下を追加してください。

```
experimental-features = nix-command flakes
```

### English

The following must be installed:

- [Nix](https://nixos.org/download/) (with flakes enabled)
- [devenv](https://devenv.sh/)
- [direnv](https://direnv.net/)

To enable Nix flakes, add the following to `~/.config/nix/nix.conf` or `/etc/nix/nix.conf`:

```
experimental-features = nix-command flakes
```

---

## インストール / Installation

### 日本語

**Nixで直接実行（インストール不要）：**

```sh
nix run github:ne0san/InstantProjectInitializer -- <言語名>
```

**Home Managerでインストール：**

`flake.nix` の `inputs` に追加します：

```nix
inputs = {
  ipi.url = "github:ne0san/InstantProjectInitializer";
};
```

`home.packages` に追加します：

```nix
home.packages = [
  inputs.ipi.packages.${pkgs.system}.default
];
```

### English

**Run directly with Nix (no installation required):**

```sh
nix run github:ne0san/InstantProjectInitializer -- <language>
```

**Install via Home Manager:**

Add to `inputs` in your `flake.nix`:

```nix
inputs = {
  ipi.url = "github:ne0san/InstantProjectInitializer";
};
```

Add to `home.packages`:

```nix
home.packages = [
  inputs.ipi.packages.${pkgs.system}.default
];
```

**Supported platforms:** `x86_64-linux` / `aarch64-linux` / `aarch64-darwin` / `x86_64-darwin`

---

## 使い方 / Usage

```sh
ipi <lang1> [lang2] [lang3] ...
```

### 例 / Examples

```sh
# Rustプロジェクトの初期化 / Initialize a Rust project
ipi rust

# PythonとTypeScriptの複合プロジェクト / Python + TypeScript project
ipi python node

# 複数言語 / Multiple languages
ipi rust go python
```

### 実行例 / Example output

```
Start Retrieving Target Files...
All files were successfully retrieved
Start Exporting File...
File Export Complete
```

実行後、カレントディレクトリに `.gitignore`, `devenv.nix`, `devenv.yaml`, `.envrc` が生成されます。

After execution, `.gitignore`, `devenv.nix`, `devenv.yaml`, and `.envrc` are generated in the current directory.

---

## 注意点 / Notes

### 日本語

- **ネットワーク接続が必要です。** gitignore.io および devenv.new の外部APIを呼び出します。
- **既存ファイルは上書きされます。** 実行前に `.gitignore`, `devenv.nix`, `devenv.yaml`, `.envrc` をバックアップしてください。
- 言語名は gitignore.io および devenv.new が認識できる名前を使用してください。

### English

- **Network connection is required.** The tool calls external APIs from gitignore.io and devenv.new.
- **Existing files will be overwritten.** Back up `.gitignore`, `devenv.nix`, `devenv.yaml`, and `.envrc` before running.
- Use language names recognized by gitignore.io and devenv.new.

---

## ライセンス / License

[Apache License 2.0](LICENSE)
