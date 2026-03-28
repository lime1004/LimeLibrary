# CLAUDE.md

## コミットメッセージ規約

### 形式

```
Prefix: 変更内容の説明（日本語）
```

### プレフィックス一覧

| Prefix | 用途 | 例 |
|--------|------|-----|
| `Add` | 新機能・新クラス・新関数の追加 | `Add: UIAnimatorにPause関数を追加` |
| `Fix` | バグ修正・不具合修正 | `Fix: AnimationComponentPlayerの挙動修正` |
| `Modify` | 既存機能の小規模な調整・インターフェース変更 | `Modify: UIButtonにTextを外から指定できるようにする` |
| `Change` | 既存機能の仕様変更・動作変更 | `Change: SceneUpdaterのOnSceneStartのタイミングをCreate前に変更` |
| `Refactor` | 動作を変えないコードの整理・リファクタリング | `Refactor: InputMode switching logic` |

### ルール

1. **プレフィックスは英語**で記述し、コロン+半角スペースの後に説明を書く
2. **説明は日本語**で記述する
3. **何を変更したか**が分かるように、対象クラス名や機能名を含める
4. 1行で簡潔にまとめる（長くなりすぎない）
5. `Fix: エラー修正` のような曖昧な説明は避け、具体的な内容を書く
