# InstantProjectInitializer

言語を指定してプロジェクト初期化ファイルを生成するCLIツール（コマンド名: `ipi`）。

## 開発環境

devenv + direnvで管理。作業前に `devenv shell` で環境に入ること。

## コマンド

```sh
test-unit          # 単体テスト
test-integration   # 結合テスト（外部API呼び出しあり・ネットワーク必須）
test-all           # 両方

nix build          # バイナリ生成 → result/bin/ipi

update-deps        # deps.json更新（NuGetパッケージ追加・更新時）
```

## アーキテクチャ

```
src/InstantProjectInitializer/
├── Validation.fs  # AsyncValidation ユーティリティ
├── IO.fs          # 外部API呼び出し（gitignore.io, devenv.new）
├── UseCase.fs     # 各ファイル取得ロジック + 全体ワークフロー定義（run関数）
└── Program.fs     # CLIエントリポイント（Argu）
```

IO → UseCase → Program の依存方向。
UseCase はIOを関数引数で受け取るため、テスト時はスタブに差し替え可能。
