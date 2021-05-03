# 偽装ちゃん

偽装ちゃん(FakeChan)は、棒読みちゃん(BouyomiChan)のフリをしてテキストを受け取りAssistantSeika経由で音声合成製品に渡します。  
コメントビュアーが棒読みちゃん呼び出しをサポートしていれば、棒読みちゃん替わりに利用できます。

…つまり、棒読みちゃんを停止して偽装ちゃんを実行すれば、棒読みちゃんから呼び出せなかった音声合成製品を呼び出せます。

![偽装ちゃん](https://hgotoh.jp/wiki/lib/exe/fetch.php/documents/tools/pasted/20210331-165343.png "ダイアグラム")


## 使い方

事前に[AssistantSeika](https://hgotoh.jp/wiki/doku.php/documents/voiceroid/assistantseika/assistantseika-001a) をダウンロード、インストール、実行してください。

AssistantSeikaで利用したい音声合成製品を認識している状態になったら、偽装ちゃん(FakeChan.exe)を起動してください。  
コメントビュアーなどでは自動でBouyomiChan.exeを呼び出すようになっている場合があるので、
その際は呼び出すプログラムをFakeChan.exeに変更するか、FakeChan.exeをBouyomiChan.exeにリネームして入れ替えてください。  
※もちろんオリジナルのBouyomiChan.exeはバックアップしておいてください。

偽装ちゃんが起動すると、通信を受け付けてコメントを読み上げ可能になります。

![偽装ちゃん](https://hgotoh.jp/wiki/lib/exe/fetch.php/documents/tools/pasted/20210503-141939.png "偽装ちゃん起動直後")

話者設定タブで使用するインタフェース（棒読みちゃん連携方法）の設定を行います。

![偽装ちゃん](https://hgotoh.jp/wiki/lib/exe/fetch.php/documents/tools/pasted/20210503-142343.png "話者マップ")

話者マップで使用するインタフェースのタブを選択し、発声方法と棒読みちゃんのボイス0～8に音声合成製品を紐付けします。  
棒読みちゃん連携のソフトウエアのほとんどはボイス0のみ使っているように見受けられます。ツイキャスコメントビューアー（閲覧君）は1番（女性1）を使用するようです。

音声パラメタ設定を展開すると紐付けした音声合成製品のパラメタを編集できます。

![偽装ちゃん](https://hgotoh.jp/wiki/lib/exe/fetch.php/documents/tools/pasted/20210503-142423.png "音声パラメタ編集")

発声方法は以下があります。
- 同期を選ぶと、メッセージはキューに入り、順次AssistantSeikaに発声指示を出します。
- 非同期を選ぶと、キューを使用せずすぐAssistantSeikaに発声指示を出します。

再生に使う音声合成製品は、同期と非同期で異なるものを指定してください。同じ音声合成製品を指定するのは無意味です。

テスト再生ボタンを押すと、再生テキストにあるテキストを設定したパラメタを適用して再生します。パラメタのテストで利用してください。

音声合成製品へ渡す前にテキスト情報を編集できます。

![偽装ちゃん](https://hgotoh.jp/wiki/lib/exe/fetch.php/documents/tools/pasted/20210503-144218.png "置換設定")

正規表現による置換が優先順位1～7の順に実施されます。適用する置換にはチェックを入れてください。  
最後に指定の文字数で切捨てが行われます。 

## その他の使い方

[偽装ちゃんと棒読みちゃんの混在方法](https://hgotoh.jp/wiki/doku.php/documents/tools/tools-206a)もどうぞ。


## 自分のアプリから連携

IPC, Socket, HTTP による接続のサンプルは棒読みちゃんに同梱されていますのでそちらをご覧ください。

## socket(50002)、HTTP(50081)の有効化

設定タブの丸をクリック。ON/OFF トグルスイッチになっています。

## 棒読みちゃん使えばいいんじゃないの？

64bit版の音声合成製品を利用できます……棒読みちゃんでCeVIO AI使うのがまだ困難なようですし… ※2021/03/16現在

## バイナリはどこ？

私のサイトの[ダウンロードページ](https://hgotoh.jp/wiki/doku.php/documents/tools/tools-206)からダウンロードしてください。
