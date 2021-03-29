# 偽装ちゃん

偽装ちゃん(FakeChan)は、棒読みちゃん(BouyomiChan)のフリをしてテキストを受け取りAssistantSeika経由で音声合成製品に渡します。  
コメントビュアーが棒読みちゃん呼び出しをサポートしていれば、棒読みちゃん替わりに利用できます。

## 偽装IPCインタフェースをサポートしました

コメントビュアー NCV, MultiCommentViewer で動作することを確認しました。棒読みちゃんと思い込んでます。(ΦωΦ)ﾌﾌﾌ…

## 使い方

事前に[AssistantSeika](https://hgotoh.jp/wiki/doku.php/documents/voiceroid/assistantseika/assistantseika-001a) をダウンロード、インストール、実行してください。

AssistantSeikaで利用したい音声合成製品を認識している状態になったら、偽装ちゃん(FakeChan.exe)を起動してください。  
コメントビュアーなどでは自動でBouyomiChan.exeを呼び出すようになっている場合があるので、
その際は呼び出すプログラムをFakeChan.exeに変更するか、FakeChan.exeをBouyomiChan.exeにリネームして入れ替えてください。  
※もちろんオリジナルのBouyomiChan.exeはバックアップしておいてください。

偽装ちゃんが起動すると、通信を受け付けてコメントを読み上げ可能になります。

テスト再生ボタンを押すと、再生テキストにあるテキストを再生します。  
その時に適用される話者とパラメタは、音声パラメタ設定で選択されている話者とパラメタになります。パラメタのテストで利用してください。

話者マップタブで、棒読みちゃんのボイス0～8に話者を割り付けできます。  
各話者のパラメタは設定タブの音声パラメタ設定で変更できます。

## できない事

- 現在の版では、話者1名に対して1組の音声パラメタしか割り当てできません。  ※例えばボイス0と女性2に異なるパラメタを持ったCeVIO ONEさんを割り当てするようなことができません。

- 設定値は記憶されません。起動するたびに初期値に戻ります。


## 棒読みちゃん使えばいいんじゃないの？

64bit版の音声合成製品を利用できます……棒読みちゃんでCeVIO AI使うのがまだ困難なようですし… ※2021/03/16現在

## バイナリはどこ？

私のサイトの[ダウンロードページ](https://hgotoh.jp/wiki/doku.php/documents/tools/tools-206)からダウンロードしてください。
