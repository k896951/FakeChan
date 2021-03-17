# 偽装ちゃん

偽装ちゃん(fakeChan)は、棒読みちゃん(BouyomiChan)の替わりにAssistantSeika経由で音声合成製品に渡します。
コメントビュアーが棒読みちゃん呼び出しをサポートしていれば、棒読みちゃん替わりに利用できます。

## 偽装IPCインタフェースをサポートしました

コメントビュアー NCV, MultiCommentViewer で動作することを確認しました。棒読みちゃんと思い込んでます。(ΦωΦ)ﾌﾌﾌ…

## 使い方

事前に[AssistantSeika](https://hgotoh.jp/wiki/doku.php/documents/voiceroid/assistantseika/assistantseika-001a) をダウンロード、インストール、実行してください。

AssistantSeikaで利用したい音声合成製品を認識している状態になったら、偽装ちゃん(fakeChan.exe)を実行してください。  
コメントビュアーなどでは自動でBouyomiChan.exeを呼び出すようになっている場合があるようですので、その際は呼び出すプログラムをfakeChan.exeに変更するか、
fakeChan.exeをBouyomiChan.exeにリネームして入れ替えてください。  
※もちろんオリジナルのBouyomiChan.exeはバックアップしておいてください。

偽装ちゃんが起動し（必要ならポート番号を変更し）たら、読み上げが始まります。

TCP/IPでの接続をする場合は「TCP/IP待ち受け開始」ボタンを押します。待ち受けポートの変更等無ければ、通信を受け付けてコメントを読み上げ可能になります。

待ち受けポートの番号は 1024 ～ 65535 の範囲で指定します。その場合、呼び出す側もポート番号を変更して下さいね。

## 棒読みちゃん使えばいいんじゃないの？

64bit版の音声合成製品を利用できます……棒読みちゃんでCeVIO AI使うのがまだ困難なようですし… ※2021/03/16現在

## バイナリはどこ？

私のサイトの[ダウンロードページ](https://hgotoh.jp/wiki/doku.php/documents/tools/tools-206)からダウンロードしてください。
