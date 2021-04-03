# 偽装ちゃん

偽装ちゃん(FakeChan)は、棒読みちゃん(BouyomiChan)のフリをしてテキストを受け取りAssistantSeika経由で音声合成製品に渡します。  
コメントビュアーが棒読みちゃん呼び出しをサポートしていれば、棒読みちゃん替わりに利用できます。

![偽装ちゃん](https://hgotoh.jp/wiki/lib/exe/fetch.php/documents/tools/pasted/20210331-165343.png "ダイアグラム")

## 偽装方法

### 偽装IPCインタフェース

棒読みちゃんが起動するIPCインタフェースを偽装して起動します。  
コメントビュアー NCV, MultiCommentViewer で動作することを確認しました。棒読みちゃんと思い込んでます。(ΦωΦ)ﾌﾌﾌ…  

### 偽装Socketインタフェース

棒読みちゃんが起動するSocketインタフェースを偽装して起動します。  
棒読みちゃんとは異なり、ポートの違うSocket接続を１つ追加しています。

### 偽装HTTPインタフェース

棒読みちゃんβ版でサポートするHTTPインタフェースを追加しました。  
棒読みちゃんβ版とは異なり、ポートの違うHTTP接続を１つ追加しています。
また、Ｗｅｂサーバの機能はありません。

## 使い方

事前に[AssistantSeika](https://hgotoh.jp/wiki/doku.php/documents/voiceroid/assistantseika/assistantseika-001a) をダウンロード、インストール、実行してください。

AssistantSeikaで利用したい音声合成製品を認識している状態になったら、偽装ちゃん(FakeChan.exe)を起動してください。  
コメントビュアーなどでは自動でBouyomiChan.exeを呼び出すようになっている場合があるので、
その際は呼び出すプログラムをFakeChan.exeに変更するか、FakeChan.exeをBouyomiChan.exeにリネームして入れ替えてください。  
※もちろんオリジナルのBouyomiChan.exeはバックアップしておいてください。

偽装ちゃんが起動すると、通信を受け付けてコメントを読み上げ可能になります。

![偽装ちゃん](https://hgotoh.jp/wiki/lib/exe/fetch.php/documents/tools/pasted/20210404-054646.png "偽装ちゃん起動直後")

テスト再生ボタンを押すと、再生テキストにあるテキストを再生します。各話者のパラメタはここで変更できます。  
その時に適用される話者とパラメタは、音声パラメタ設定で選択されている話者と表示されているパラメタになります。パラメタのテストで利用してください。

話者マップタブで、棒読みちゃんのボイス0～8に話者を割り付けできます。  
棒読みちゃん連携のソフトウエアのほとんどはボイス0のみ使っているように見受けられます。  
![偽装ちゃん](https://hgotoh.jp/wiki/lib/exe/fetch.php/documents/tools/pasted/20210330-212504.png "話者マップ")

## 発声方法

発声方法は以下があります。
- 同期を選ぶと、メッセージはキューに入り、順次AssistantSeikaに発声指示を出します。
- 非同期を選ぶと、キューを使用せずすぐAssistantSeikaに発声指示を出します。

再生に使う音声合成製品は、同期と非同期で異なるものを指定してください。同じ音声合成製品を指定するのは無意味です。

## 自分のアプリから連携

IPC, Socket, HTTP による接続のサンプルは棒読みちゃんに同梱されていますのでそちらをご覧ください。

## socket(50002)、HTTP(50081)の有効化

設定タブの丸をクリック。ON/OFF トグルスイッチになっています。

## 棒読みちゃん使えばいいんじゃないの？

64bit版の音声合成製品を利用できます……棒読みちゃんでCeVIO AI使うのがまだ困難なようですし… ※2021/03/16現在

## バイナリはどこ？

私のサイトの[ダウンロードページ](https://hgotoh.jp/wiki/doku.php/documents/tools/tools-206)からダウンロードしてください。
